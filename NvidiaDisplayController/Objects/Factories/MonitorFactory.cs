using System.Drawing;

namespace NvidiaDisplayController.Objects.Factories;

public class MonitorFactory
{
    private readonly ProfileFactory _profileFactory;

    public MonitorFactory(ProfileFactory profileFactory)
    {
        _profileFactory = profileFactory;
    }

    public Monitor CreateDefault(string name, Size resolution, int frequency)
    {
        var monitor = new Monitor(name, resolution, frequency);
        monitor.Profiles.Add(_profileFactory.CreateDefault(monitor));
        return monitor;
    }
}