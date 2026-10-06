using System;
using System.Collections.Generic; // Added for IList/List
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.DependencyInjection; // Added for [Resolved]
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Platform.Keyboard;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin;

namespace Kuuube_s_Chatter_Exterminator;

[PluginName("Velocity Controls")]
public class Kuuube_s_CHATTER_EXTERMINATOR_RAW : IPositionedPipelineElement<IDeviceReport> 
{

    private readonly IList<string> rightAppsKey = new List<string> { "ContextMenu" };
    public bool pressed = false;

    [Resolved]
    public IVirtualKeyboard Keyboard { set; get; } = null!;

public event Action<IDeviceReport> Emit;
    public void Consume(IDeviceReport value) 
    {
        if (value is IAbsolutePositionReport report) 
        {

            if (report.Position.Y < Edge_Mapping_Y)
            {
                
                if (!pressed)
                {
                    Keyboard.Release(rightAppsKey);
                    pressed = true;
                }
            }
            else
            {
                if (pressed)
                {
                    
                    Keyboard.Press(rightAppsKey);
                    pressed = false;
                }
            }
            
        }

        Emit?.Invoke(value);
    }

    public PipelinePosition Position => PipelinePosition.PostTransform;

    [Property("Max Y value"), DefaultPropertyValue(2f)]
    [ToolTip("Any number past this will send a LeftMenu input.")]
    public float Edge_Mapping_Y { set; get; }
}