﻿using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using Caliburn.Micro;
using NvidiaDisplayController.Interface.Profiles;
using NvidiaDisplayController.Objects;

namespace NvidiaDisplayController.Interface.Monitors;

public class MonitorViewModel : Screen
{
    private bool _isEnabled;
    private bool _isSelected;
    private ObservableCollection<ProfileViewModel> _profiles;

    public MonitorViewModel(Monitor monitor)
    {
        Monitor = monitor;
        _isEnabled = true;
        _profiles = new ObservableCollection<ProfileViewModel>();
        Guid = Guid.NewGuid();
    }

    public Monitor Monitor { get; }

    public string Name => Monitor.Name;
    public Size Resolution => Monitor.Resolution;
    public int Frequency => Monitor.Frequency;

    public Action<bool, Guid> IsSelectedChanged { get; set; }
    public Guid Guid { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value == _isSelected) return;
            _isSelected = value;
            NotifyOfPropertyChange();
            IsSelectedChanged.Invoke(value, Guid);
        }
    }

    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (value == _isEnabled) return;
            _isEnabled = value;
            NotifyOfPropertyChange();
        }
    }

    public ObservableCollection<ProfileViewModel> Profiles
    {
        get => _profiles;
        set
        {
            if (Equals(value, _profiles)) return;
            _profiles = value;
            NotifyOfPropertyChange();
        }
    }
}