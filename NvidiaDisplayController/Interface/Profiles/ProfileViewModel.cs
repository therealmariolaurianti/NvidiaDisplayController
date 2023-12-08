using System;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using NvidiaDisplayController.Data;
using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.ProfileSettings;
using NvidiaDisplayController.Objects;
using NvidiaDisplayController.Objects.Factories;

namespace NvidiaDisplayController.Interface.Profiles;

public class ProfileViewModel : Screen
{
    private readonly IProfileNameViewModelFactory _profileNameViewModelFactory;
    private readonly IWindowManager _windowManager;
    private bool _callEvent;
    private DataController _dataController;
    private bool _isSelected;

    public ProfileViewModel(Profile profile, MonitorViewModel monitorViewModel,
        IProfileSettingViewModelFactory profileSettingViewModelFactory,
        IProfileNameViewModelFactory profileNameViewModelFactory, IWindowManager windowManager,
        DataController dataController)
    {
        Profile = profile;
        MonitorViewModel = monitorViewModel;
        _profileNameViewModelFactory = profileNameViewModelFactory;
        _windowManager = windowManager;
        _dataController = dataController;
        Guid = Guid.NewGuid();
        _callEvent = true;

        CreateContextMenu();
        ProfileSettings = profileSettingViewModelFactory.Create(Profile.ProfileSetting, Profile.IsDefault);
    }

    public Profile Profile { get; }
    public Action<Guid> ProfileRemoved { get; set; }
    public string Name => Profile.Name;
    public Guid Guid { get; }
    public ProfileSettingViewModel ProfileSettings { get; set; }

    public ContextMenu ContextMenu { get; set; }
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
                IsSelectedChanged.Invoke(Guid);
        }
    }

    public Action<Guid> IsSelectedChanged { get; set; }
    public bool IsDefault => Profile.IsDefault;

    public void IsUpdated()
    {
        ProfileSettings.IsUpdated();
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
        }
        _callEvent = true;
    }
}