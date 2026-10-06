using System;
using System.Collections.Generic;
using System.Numerics;
using System.Timers; // Required for Timer
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin;
using System.Threading.Tasks;

namespace Gesture_Swipe;

[PluginName("Gesture Swipe")]
public class Gesture_Swipe : IPositionedPipelineElement<IDeviceReport>, IDisposable
{
    public readonly IList<string> ScrollLockKey = new List<string> { "ScrollLock" };
    public bool pressed = false;
    private bool ran = false;

    // ai - Watchdog variables
    private readonly Timer _watchdogTimer;
    private long _lastReportTimestamp;
    private const long TimeoutMs = 100; // 100ms silence = pen is gone
    private readonly object _lock = new object();

    public event Action<IDeviceReport> Emit;


    [Resolved]
    public IVirtualKeyboard Keyboard { set; get; } = null!;

    public Gesture_Swipe()
    {
        // ai generated 
        // The timer runs steadily in the background every 50ms 
        // without being constantly spammed, stopped, or started.
        _watchdogTimer = new Timer(50);
        _watchdogTimer.AutoReset = true;
        _watchdogTimer.Elapsed += CheckForTimeout;
        _watchdogTimer.Start();
    }

    public void Consume(IDeviceReport value) 
    {
        if (value is ITabletReport)
        {
            // also part of the ai generated timer
            // EXTREMELY FAST: Just updates a single primitive number in memory
            _lastReportTimestamp = Environment.TickCount64;

            lock (_lock)
            {
                Stuff(value);
            }
        }
        Emit?.Invoke(value);
    }

    private void CheckForTimeout(object? sender, ElapsedEventArgs e)
    {
        // If Alt is pressed, check if the time since the last packet exceeds our threshold
        if (pressed && (Environment.TickCount64 - _lastReportTimestamp > TimeoutMs))
        {
            lock (_lock)
            {
                // Double-check inside the lock to prevent multi-threading race conditions
                if (pressed) 
                {
                    
                    pressed = false;
                    ran = false;
                    Keyboard.Release(ScrollLockKey);
                }
            }
        }
    }

    public void Stuff(IDeviceReport value)
    {
        if (value is IAbsolutePositionReport report) 
        {
            if ((report.Position.Y < Trigger_Y) || (report.Position.Y > Trigger_mY))
            {
                if (!ran)
                {
                    ran = true;
                    if (!pressed)
                    {
                        Keyboard.Press(ScrollLockKey);
                        pressed = true;
                    }
                }
            }
            else
            {
                ran = false;
            }
        }
    }
    // ai
    public void Dispose()
    {
        _watchdogTimer.Stop();
        _watchdogTimer.Dispose();
    }

    public PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Max Y value"), DefaultPropertyValue(2f)]
    [ToolTip("Any number past this will send a Scroll Lock input when you get close to the top edge of your tablet")]
    public float Trigger_Y { set; get; }
    
    [Property("negative Max Y value"), DefaultPropertyValue(2f)]
    [ToolTip("Any number past this will send a Scroll Lock input at the bottom of your tablet")]
    public float Trigger_mY { set; get; }
}