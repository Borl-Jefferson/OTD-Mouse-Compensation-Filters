using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;


namespace DirectionalBinds
{
    [PluginName("DirectionalBinds")]
    public class DragThreshold : IPositionedPipelineElement<IDeviceReport>, IDisposable
    {
            public readonly IList<string> ScrollLockKey = new List<string> { "ScrollLock" };
        public event Action<IDeviceReport> Emit;
        public PipelinePosition Position => PipelinePosition.PostTransform;

        [Property("Threshold"), Unit("px"), DefaultPropertyValue(5f), ToolTip(
            "DragThreshold:\n\n" +
            "Threshold: The distance in pixels the pen must move before it is considered a drag. " +
            "Movement below this threshold will keep the cursor stationary, helping prevent accidental drags during clicks.")]
        public float Threshold { get; set; }

        // --- Plugin State ---
        private Vector2 _track = new Vector2(0, 0);
        private const int ReportThreshold = 20;
        private int _reportCounter = 0;
        private int _fin = 0;
        private bool _clicked = false;
        
        // ai - Cached Input Size to prevent Marshal allocations on every report
        private static readonly int _inputSize = Marshal.SizeOf(typeof(INPUT));

        // ai - --- Hook State ---
        private static volatile bool _isAltHeld = false;
        private Thread _hookThread;
        private CancellationTokenSource _cts;
        private IntPtr _hookId = IntPtr.Zero;
        private LowLevelKeyboardProc _hookProc;
        bool penDown;
        bool wasDown = false;

        bool somethingHappened = false;
         [Resolved]
    public IVirtualKeyboard Keyboard { set; get; } = null!;



        public void Consume(IDeviceReport value)
        {
            if (value is ITabletReport tabletReport)
            {
                ProcessTabletReport(tabletReport);
            }

            Emit?.Invoke(value);
        }

        private void ProcessTabletReport(ITabletReport tabletReport)
        {
            penDown = tabletReport.Pressure > 0;
                // ai too, i used it here to figure out how to detect if scroll lock was active instead of just being held down
            if ((GetKeyState(0x91) & 1) != 0)
            {
                if (penDown)
                {
                    wasDown = true;
                    _track += tabletReport.Position; 

                    if (_track.X > Threshold)
                    {
                        _reportCounter++;
                        if (_reportCounter >= ReportThreshold && (_fin == 0 || _fin == 1))
                        {
                            somethingHappened = true;
                            SendScrollWheel(-ScrollSpeed);
                            _reportCounter = 0;
                            _fin = 1;
                        }
                    }
                    else if (_track.X < -Threshold)
                    {
                        _reportCounter++;
                        if (_reportCounter >= ReportThreshold && (_fin == 0 || _fin == 2))
                        {
                            somethingHappened = true;
                            SendScrollWheel(ScrollSpeed);
                            _reportCounter = 0;
                            _fin = 2;
                        }
                    }

                    else if (_track.Y > Threshold && (_fin == 0 || _fin == 3))
                    {
                        if (!_clicked)
                        {
                            somethingHappened = true;
                            _clicked = true;
                            SendLeftClickDown();
                            _fin = 3;
                        }
                    }
                    else if (_track.Y < -Threshold && (_fin == 0 || _fin == 4))
                    {
                        if (!_clicked)
                        {
                            somethingHappened = true;
                            _clicked = true;
                            SendRightClickDown();
                            _fin = 4;
                        }
                    }
                    tabletReport.Pressure = 0;
                }
                else
                {

                    if(wasDown)
                    { 
                    if(!somethingHappened)
                    {
                        //i think i used ai here too originally, but I ended up using it as a template and changing it a lot
                        Task.Run(async () => { Keyboard.Press(ScrollLockKey); await Task.Delay(40); Keyboard.Release(ScrollLockKey); });
                        
                        somethingHappened = false;
                        }
                        else
                        {
                            if (_fin == 3)
                            {
                                SendLeftClickUp();
                            }
                            else if (_fin == 4)
                            {
                                SendRightClickUp();
                            }
                        }
                    }
                    wasDown = false;
                    somethingHappened = false;
                    _track = new Vector2(0, 0);
                    _clicked = false;
                    _fin = 0;
                    _reportCounter = ReportThreshold; // ai - Reset so next drag triggers instantly
                }

                tabletReport.Position = new Vector2(0, 0);
            }
            else
            {
                _track = new Vector2(0, 0);
                _clicked = false;
                _fin = 0;
            }
        }

        // all ai for interacting with windows inputs --- P/INVOKE BOILERPLATE ---
[DllImport("user32.dll")]
private static extern short GetKeyState(int vKey);
        private const int WH_KEYBOARD_LL = 13;
        private const int VK_SCRLK = 0x91;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int INPUT_MOUSE = 0;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd; public uint message; public IntPtr wParam;
            public IntPtr lParam; public uint time; public int pt_x; public int pt_y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx; public int dy; public int mouseData;
            public uint dwFlags; public uint time; public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public MOUSEINPUT mi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int type; public InputUnion u;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        private static void SendScrollWheel(int delta)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT { type = INPUT_MOUSE, u = new InputUnion { mi = new MOUSEINPUT { mouseData = delta, dwFlags = MOUSEEVENTF_WHEEL } } };
            SendInput(1, inputs, _inputSize);
        }

        private static void SendLeftClickDown()
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT { type = INPUT_MOUSE, u = new InputUnion { mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTDOWN } } };
            SendInput(1, inputs, _inputSize);
        }
        private static void SendLeftClickUp()
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT { type = INPUT_MOUSE, u = new InputUnion { mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_LEFTUP } } };
            SendInput(1, inputs, _inputSize);
        }

        private static void SendRightClickDown()
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT { type = INPUT_MOUSE, u = new InputUnion { mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_RIGHTDOWN } } };
            SendInput(1, inputs, _inputSize);
        }
        private static void SendRightClickUp()
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0] = new INPUT { type = INPUT_MOUSE, u = new InputUnion { mi = new MOUSEINPUT { dwFlags = MOUSEEVENTF_RIGHTUP } } };
            SendInput(1, inputs, _inputSize);
        }

       public void Dispose()
{
}

[Property("Scrolling rate"), DefaultPropertyValue(2f)]
    [ToolTip("How fast the thing scrolls (80 default)")]
    public int ScrollSpeed { set; get; }
    }
}