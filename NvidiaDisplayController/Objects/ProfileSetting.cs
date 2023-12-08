namespace NvidiaDisplayController.Objects;

public class ProfileSetting
{
    public ProfileSetting(double brightness, double contrast, double gamma)
    {
        Brightness = brightness;
        Contrast = contrast;
        Gamma = gamma;
    }
    
    public double Brightness { get; set; }
    public double Contrast { get; set; }
    public double Gamma { get; set; }
}