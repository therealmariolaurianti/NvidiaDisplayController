using System.Collections.Generic;
using System.Drawing;

namespace NvidiaDisplayController.Objects;

public class Monitor
{
    public Monitor(string displayDevicePath, string name, Size resolution, int frequency)
    {
        DisplayDevicePath = displayDevicePath;
        Name = name;
        Resolution = resolution;
        Frequency = frequency;
    }

    public string DisplayDevicePath { get; }
    public string Name { get; }
    public Size Resolution { get; }
    public int Frequency { get; }
    public List<Profile> Profiles { get; set; } = new();
}