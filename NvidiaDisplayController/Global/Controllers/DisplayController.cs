using System;
using NLog;
using NvidiaDisplayController.Objects;
using WindowsDisplayAPI;

namespace NvidiaDisplayController.Global.Controllers;

public class DisplayController
{
    private readonly ILogger _logger;
    private readonly NvidiaDisplayWindowManager _windowManager;

    public DisplayController(ILogger logger, NvidiaDisplayWindowManager windowManager)
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
            var message = "Failed to update color settings.";

            _logger.Error(message);
            _logger.Error(e);

            _windowManager.ShowMessageBox(message);
        }
    }
}