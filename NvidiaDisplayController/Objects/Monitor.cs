using System.Collections.Generic;
using System.Drawing;

namespace NvidiaDisplayController.Objects;

public class Monitor
{
    public Monitor(string name, Size resolution, int frequency)
    {
        Name = name;
        Resolution = resolution;
        Frequency = frequency;
    }

    public string Name { get; }
    public Size Resolution { get; }
    public int Frequency { get; }
    public List<Profile> Profiles { get; set; } = new();
}