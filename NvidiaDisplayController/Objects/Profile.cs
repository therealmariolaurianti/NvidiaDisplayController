namespace NvidiaDisplayController.Objects;

public class Profile
{
    public Profile(Monitor monitor, string name, ProfileSetting profileSetting, bool isActive,
        bool isDefault = false)
    {
        Monitor = monitor;
        Name = name;
        ProfileSetting = profileSetting;
        IsActive = isActive;
        IsDefault = isDefault;
    }

    public Monitor Monitor { get; }
    public string Name { get; set; }
    public ProfileSetting ProfileSetting { get; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}