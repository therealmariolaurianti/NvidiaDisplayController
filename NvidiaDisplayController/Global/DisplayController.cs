using System;
using Caliburn.Micro;
using NLog;
using NvidiaDisplayController.Objects;
using WindowsDisplayAPI;

namespace NvidiaDisplayController.Global;

public class DisplayController
{
    private readonly ILogger _logger;
    private IWindowManager _windowManager;

    public DisplayController(ILogger logger, IWindowManager windowManager)
    {
        _logger = logger;
        _windowManager = windowManager;
    }

    public void UpdateColorSettings(Display display, ProfileSetting profileSetting,
        NvAPIWrapper.Display.Display? nvidiaMonitor)
    {
        try
        {
            display.GammaRamp =
                new DisplayGammaRamp(profileSetting.Brightness, profileSetting.Contrast, profileSetting.Gamma);
            if (nvidiaMonitor is not null)
                nvidiaMonitor.DigitalVibranceControl.NormalizedLevel = profileSetting.DigitalVibrance - .3;
        }
        catch (Exception e)
        {
            _logger.Error("Failed to update color settings.");
            _logger.Error(e);
        }
    }
}