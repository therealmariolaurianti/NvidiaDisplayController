using System.Drawing;
using WindowsDisplayAPI;

namespace NvidiaDisplayController.Objects.Factories;

public class MonitorFactory
{
    private readonly ProfileFactory _profileFactory;

    public MonitorFactory(ProfileFactory profileFactory)
    {
        _profileFactory = profileFactory;
    }

    public Monitor CreateDefault(string displayDevicePath, string name, Size resolution, int frequency)
    {
        var monitor = new Monitor(displayDevicePath, name, resolution, frequency);
        monitor.Profiles.Add(_profileFactory.CreateDefault(monitor));
        return monitor;
    }
}