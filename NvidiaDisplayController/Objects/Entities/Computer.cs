﻿using System.Collections.Generic;

namespace NvidiaDisplayController.Objects.Entities;

public class Computer
{
    public bool IsStartWithWindows { get; set; }
    public bool IsApplySettingsOnStart { get; set; }
    public List<Monitor> Monitors { get; set; } = new();
}