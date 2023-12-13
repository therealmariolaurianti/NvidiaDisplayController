using System;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.ProfileSettings;
using NvidiaDisplayController.Objects;
using NvidiaDisplayController.Objects.Factories;

namespace NvidiaDisplayController.Interface.Profiles;

public class ProfileViewModel : Screen
{
    private readonly IProfileSettingViewModelFactory _profileSettingViewModelFactory;
    private bool _callEvent;
    private bool _isSelected;
    private ProfileSettingViewModel? _profileSettings;

    public ProfileViewModel(Profile profile, MonitorViewModel monitorViewModel,
        IProfileSettingViewModelFactory profileSettingViewModelFactory)
    {
        _profileSettingViewModelFactory = profileSettingViewModelFactory;
        Profile = profile;
        MonitorViewModel = monitorViewModel;

        Start();
    }

    public Profile Profile { get; }
    public Action<Guid> ProfileRemoved { get; set; } = null!;
    public string Name => Profile.Name;
    public Guid Guid { get; set; }

    public ProfileSettingViewModel? ProfileSettings
    {
        get => _profileSettings;
        set
        {
            if (Equals(value, _profileSettings)) return;
            _profileSettings = value;
            NotifyOfPropertyChange();
        }
    }

    public ContextMenu ContextMenu { get; set; } = null!;
    public MonitorViewModel MonitorViewModel { get; set; }

    public new bool IsActive
    {
        get => Profile.IsActive;
        set
        {
            if (value == Profile.IsActive) return;
            Profile.IsActive = value;
            NotifyOfPropertyChange();
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (value == _isSelected) return;
            _isSelected = value;
            NotifyOfPropertyChange();
            if (_callEvent)
            {
                BuildProfileSettings();
                IsSelectedChanged.Invoke(Guid, value);
            }
        }
    }

    public Action<Guid, bool> IsSelectedChanged { get; set; } = null!;
    public bool IsDefault => Profile.IsDefault;

    private void Start()
    {
        Guid = Guid.NewGuid();
        _callEvent = true;

        CreateContextMenu();
        BuildProfileSettings();
    }

    private void BuildProfileSettings()
    {
        ProfileSettings = _profileSettingViewModelFactory
            .Create(Profile.ProfileSetting, Profile.IsDefault);
    }

    public void IsUpdated()
    {
        ProfileSettings?.IsUpdated();
    }

    private void CreateContextMenu()
    {
        if (Profile.IsDefault)
            return;

        ContextMenu = new ContextMenu();
        var menuItem1 = new MenuItem
        {
            Header = "Remove"
        };
        menuItem1.Click += OnRemoveClicked;
        ContextMenu.Items.Add(menuItem1);
    }

    private void OnRemoveClicked(object sender, RoutedEventArgs e)
    {
        ProfileRemoved.Invoke(Guid);
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void UnSelect()
    {
        _callEvent = false;
        {
            IsSelected = false;
            ProfileSettings = null;
        }
        _callEvent = true;
    }
}