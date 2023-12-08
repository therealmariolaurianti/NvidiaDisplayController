namespace NvidiaDisplayController.Objects.Factories;

public class ProfileFactory
{
    public Profile CreateDefault(Monitor monitor)
    {
        return new Profile(monitor, "Default",
            new ProfileSetting(0.5, 0.5, 1.0, 0.5), true);
    }

    public Profile Create(Monitor monitor, string name)
    {
        var profile = new Profile(monitor, name, new ProfileSetting(0.5, 0.5, 1.0, 0.5));
        monitor.Profiles.Add(profile);
        return profile;
    }
}