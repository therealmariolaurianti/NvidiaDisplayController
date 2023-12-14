using System;
using System.Collections.Generic;
using System.Linq;
using FluentResults;
using NLog;
using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Objects.Entities;
using NvidiaDisplayController.Objects.Factories.Interfaces;
using WindowsDisplayAPI;

namespace NvidiaDisplayController.Objects.Factories;

public class MonitorViewModelFactory
{
    private readonly ILogger _logger;
    private readonly IProfileViewModelFactory _profileViewModelFactory;

    public MonitorViewModelFactory(IProfileViewModelFactory profileViewModelFactory, ILogger logger)
    {
        _profileViewModelFactory = profileViewModelFactory;
        _logger = logger;
    }

    private IEnumerable<Display> _displays
    {
        get
        {
            try
            {
                var enumerable = Display.GetDisplays();
                return enumerable;
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return Enumerable.Empty<Display>();
            }
        }
    }

    public Result<MonitorViewModel> Create(Monitor monitor)
    {
        var display = _displays.SingleOrDefault(d => d.DevicePath == monitor.DisplayDevicePath);
        if (display is null)
            return Result.Fail("Can't find display.");

        var monitorViewModel = new MonitorViewModel(monitor, display);

        foreach (var profile in monitor.Profiles)
        {
            var profileViewModel = _profileViewModelFactory.Create(profile, monitorViewModel);
            monitorViewModel.Profiles.Add(profileViewModel);
        }

        return monitorViewModel;
    }
}