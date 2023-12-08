using System;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using NvidiaDisplayController.Data;
using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.Profiles;
using NvidiaDisplayController.Objects.Factories;
using WindowsDisplayAPI;

namespace NvidiaDisplayController.Interface.Shell;

public class ShellViewModel : Screen
{
    private readonly DataController _dataController;
    private readonly MonitorViewModelFactory _monitorViewModelFactory;
    private readonly ProfileFactory _profileFactory;
    private readonly ProfileNameViewModelFactory _profileNameViewModelFactory;
    private readonly ProfileViewModelFactory _profileViewModelFactory;

    private readonly WindowManager _windowManager;

    private Guid _lastSelectedProfile;
    private ObservableCollection<MonitorViewModel> _monitors;
    private MonitorViewModel? _selectedMonitor;
    private ProfileViewModel? _selectedProfile;

    public ShellViewModel(MonitorViewModelFactory monitorViewModelFactory,
        DataController dataController, ProfileViewModelFactory profileViewModelFactory, ProfileFactory profileFactory,
        WindowManager windowManager, ProfileNameViewModelFactory profileNameViewModelFactory)
    {
        _monitorViewModelFactory = monitorViewModelFactory;
        _dataController = dataController;
        _profileViewModelFactory = profileViewModelFactory;
        _profileFactory = profileFactory;
        _windowManager = windowManager;
        _profileNameViewModelFactory = profileNameViewModelFactory;

        Start();
    }

    public ObservableCollection<MonitorViewModel> Monitors
    {
        get => _monitors;
        set
        {
            if (Equals(value, _monitors)) return;
            _monitors = value;
            NotifyOfPropertyChange();
        }
    }

    public override string DisplayName
    {
        get => "Monitor Color Adjuster";
        set { }
    }

    public MonitorViewModel? SelectedMonitor
    {
        get => _selectedMonitor;
        set
        {
            if (Equals(value, _selectedMonitor)) return;
            _selectedMonitor = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(SelectedProfile));
            NotifyOfPropertyChange(nameof(CanAddNewProfile));
        }
    }

    public ProfileViewModel? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (Equals(value, _selectedProfile)) return;
            _selectedProfile = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanApply));
        }
    }

    public bool CanApply => SelectedProfile is not null;
    public bool CanAddNewProfile => SelectedMonitor is not null;

    private void Start()
    {
        _monitors = new ObservableCollection<MonitorViewModel>();

        var monitors = _dataController.Load();
        foreach (var monitor in monitors!)
        {
            var monitorViewModel = _monitorViewModelFactory.Create(monitor);
            foreach (var profileViewModel in monitorViewModel.Profiles)
                WireProfileEvents(profileViewModel);

            monitorViewModel.IsSelectedChanged += OnMonitorViewModelIsSelectedChanged;

            Monitors.Add(monitorViewModel);
        }
    }

    private void WireProfileEvents(ProfileViewModel profileViewModel)
    {
        profileViewModel.IsSelectedChanged += OnProfileViewModelSelectedChanged;
        profileViewModel.ProfileRemoved += OnProfileRemoved;
    }

    private void OnProfileViewModelSelectedChanged(Guid guid)
    {
        SelectedProfile = SelectedMonitor!.Profiles.Single(p => p.Guid == guid);
        foreach (var profileViewModel in SelectedMonitor.Profiles.Where(p => p.Guid != guid))
            profileViewModel.UnSelect();
    }

    private void OnProfileRemoved(Guid guid)
    {
        var profileViewModel = SelectedMonitor!.Profiles.Single(p => p.Guid == guid);

        SelectedMonitor.Monitor.Profiles.Remove(profileViewModel.Profile);
        SelectedMonitor?.Profiles.Remove(profileViewModel);

        Write();
    }

    private void Write()
    {
        _dataController.Write(Monitors.Select(m => m.Monitor).ToList());
    }

    private void OnMonitorViewModelIsSelectedChanged(bool isSelected, Guid selectedMonitor)
    {
        SetMonitorState(selectedMonitor, !isSelected);
        SelectedMonitor = isSelected ? Monitors.Single(m => m.Guid == selectedMonitor) : null;

        SelectedProfile = SelectedMonitor?.Profiles.Single(p => p.IsDefault);
        if (SelectedProfile is not null)
            SelectedProfile.IsSelected = true;
    }

    private void SetMonitorState(Guid selectedMonitor, bool isEnabled)
    {
        foreach (var monitor in Monitors.Where(m => m.Guid != selectedMonitor))
            monitor.IsEnabled = isEnabled;
    }

    public void AddNewProfile()
    {
        var viewModel = _profileNameViewModelFactory.Create();
        var result = _windowManager.ShowDialogAsync(viewModel).Result;
        if (result == true)
        {
            var profileViewModel =
                _profileViewModelFactory.Create(_profileFactory.Create(SelectedMonitor!.Monitor,
                    viewModel.ProfileName), SelectedMonitor);
            WireProfileEvents(profileViewModel);
            SelectedMonitor?.Profiles.Add(profileViewModel);
            Write();
            SelectedProfile = profileViewModel;
        }
    }

    public void Apply()
    {
        UpdateColorSettings(SelectedMonitor!.Display, SelectedProfile!.ProfileSettings.Brightness,
            SelectedProfile.ProfileSettings.Contrast, SelectedProfile.ProfileSettings.Gamma);

        Write();
    }

    private void UpdateColorSettings(Display display,
        double brightness = 0.5, double contrast = 0.5, double gamma = 1)
    {
        var newGamma = new DisplayGammaRamp(brightness, contrast, gamma);
        display.GammaRamp = newGamma;
    }
}