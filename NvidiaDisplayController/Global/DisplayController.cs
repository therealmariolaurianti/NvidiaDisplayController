using NvidiaDisplayController.Objects;
using WindowsDisplayAPI;

namespace NvidiaDisplayController.Global;

public class DisplayController
{
    public void UpdateColorSettings(Display display, ProfileSetting profileSetting,
        NvAPIWrapper.Display.Display? nvidiaMonitor)
    {
        display.GammaRamp =
            new DisplayGammaRamp(profileSetting.Brightness, profileSetting.Contrast, profileSetting.Gamma);
        if (nvidiaMonitor is not null)
            nvidiaMonitor.DigitalVibranceControl.NormalizedLevel = profileSetting.DigitalVibrance - .3;
    }
}