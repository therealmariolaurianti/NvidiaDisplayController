using System;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.ProfileSettings;
using NvidiaDisplayController.Objects;
using Action = System.Action;

namespace NvidiaDisplayController.Interface.Profiles;

public class ProfileViewModel : Screen
{
    private bool _isSelected;
    private bool _callEvent;

    public ProfileViewModel(Profile profile)
    {
        Profile = profile;
        Guid = Guid.NewGuid();
        _callEvent = true;

        CreateContextMenu();
    }

    public Profile Profile { get; }
    public Action<Guid> ProfileRemoved { get; set; }
    public string Name => Profile.Name;
    public Guid Guid { get; }
    public ProfileSettingViewModel ProfileSettings => new(Profile.ProfileSetting, Profile.IsDefault);
    public ContextMenu ContextMenu { get; set; }
    public MonitorViewModel MonitorViewModel { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value == _isSelected) return;
            _isSelected = value;
            NotifyOfPropertyChange();
            if(_callEvent)
                IsSelectedChanged.Invoke(Guid);
        }
    }

    public Action<Guid> IsSelectedChanged { get; set; }

    private void CreateContextMenu()
    {
        if (Profile.IsDefault)
            return;

        ContextMenu = new ContextMenu();
        var menuItem = new MenuItem
        {
            Header = "Remove"
        };
        menuItem.Click += OnRemoveClicked;
        ContextMenu.Items.Add(menuItem);
    }

    private void OnRemoveClicked(object sender, RoutedEventArgs e)
    {
        ProfileRemoved.Invoke(Guid);
    }

    public void UnSelect()
    {
        _callEvent = false;
        IsSelected = false;
        _callEvent = true;
    }
}