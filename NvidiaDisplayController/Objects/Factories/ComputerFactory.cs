using System.Collections.Generic;
using System.Linq;
using FluentResults;
using NvidiaDisplayController.Objects.Entities;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;

namespace NvidiaDisplayController.Objects.Factories;

public class ComputerFactory
{
    private readonly MonitorFactory _monitorFactory;
    private readonly IEnumerable<Display> _displays;
    private readonly PathDisplayTarget[] _pathDisplayTargets;

    public ComputerFactory(MonitorFactory monitorFactory)
    {
        _monitorFactory = monitorFactory;

        _displays = Display.GetDisplays();
        _pathDisplayTargets = PathDisplayTarget.GetDisplayTargets();
    }

    public Result<Computer> Create()
    {
        var computer = new Computer();
        var monitors = new List<Monitor>();

        foreach (var display in _displays)
        {
            var resolution = display.DisplayScreen.CurrentSetting.Resolution;
            var frequency = display.DisplayScreen.CurrentSetting.Frequency;
            var displaySource = _pathDisplayTargets.Single(pds => pds.DevicePath == display.DevicePath);

            var monitor = _monitorFactory
                .CreateDefault(display.DevicePath, displaySource.FriendlyName, resolution, frequency);

            monitors.Add(monitor);
        }

        computer.Monitors.AddRange(monitors);

        return computer;
    }
}