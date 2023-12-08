using NvidiaDisplayController.Interface.Monitors;

namespace NvidiaDisplayController.Objects;

public class Profile
{
    public Profile(Monitor monitor, string name, ProfileSetting profileSetting,
        bool isDefault = false)
    {
        Monitor = monitor;
        Name = name;
        ProfileSetting = profileSetting;
        IsDefault = isDefault;
    }

    public Monitor Monitor { get; }
    public string Name { get; }
    public ProfileSetting ProfileSetting { get; }
    public bool IsDefault { get; set; }
}