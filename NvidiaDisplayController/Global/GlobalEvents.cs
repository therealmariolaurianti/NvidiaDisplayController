using System;

namespace NvidiaDisplayController.Global;

public static class GlobalEvents
{
    public static Action UpdateToolTip { get; set; } = null!;
}