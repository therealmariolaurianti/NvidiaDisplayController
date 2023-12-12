using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NLog;
using NvAPIWrapper.Display;
using NvidiaDisplayController.Data;
using NvidiaDisplayController.Global;
using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.Profiles;
using NvidiaDisplayController.Objects;
using NvidiaDisplayController.Objects.Factories;

namespace NvidiaDisplayController.Interface.Shell;

public class ShellViewModel : Conductor<IScreen>, IHandle<ProfileSettingsEvent>
{
    private readonly DataController _dataController;
    private readonly DisplayController _displayController;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger _logger;
    private readonly MonitorViewModelFactory _monitorViewModelFactory;

    private readonly NvidiaDisplayWindowManager _nvidiaDisplayWindowManager;
    private readonly ProfileFactory _profileFactory;
    private readonly IProfileViewModelFactory _profileViewModelFactory;

    private readonly RegistryController _registryController;
    private Computer _computer;
    private ObservableCollection<MonitorViewModel> _monitors;
    private List<Display> _nvidiaDisplays;
    private bool _profileSettingsIsDirty;
    private MonitorViewModel? _selectedMonitor;
    private Display? _selectedNvidiaMonitor;
    private ProfileViewModel? _selectedProfile;

    public ShellViewModel(
        IEventAggregator eventAggregator,
        MonitorViewModelFactory monitorViewModelFactory,
        DataController dataController,
        IProfileViewModelFactory profileViewModelFactory,
        ProfileFactory profileFactory,
        ILogger logger,
        DisplayController displayController,
        NvidiaDisplayWindowManager nvidiaDisplayWindowManager,
        RegistryController registryController)
    {
        _eventAggregator = eventAggregator;
        _monitorViewModelFactory = monitorViewModelFactory;
        _dataController = dataController;
        _profileViewModelFactory = profileViewModelFactory;
        _profileFactory = profileFactory;
        _logger = logger;
        _displayController = displayController;
        _nvidiaDisplayWindowManager = nvidiaDisplayWindowManager;
        _registryController = registryController;

        _eventAggregator.SubscribeOnPublishedThread(this);

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
        get => "Adjust Displays";
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

    public bool ProfileSettingsIsDirty
    {
        get => _profileSettingsIsDirty;
        set
        {
            if (value == _profileSettingsIsDirty) return;
            _profileSettingsIsDirty = value;
            NotifyOfPropertyChange();
            NotifyOfPropertyChange(nameof(CanApply));
        }
    }

    public bool CanApply => SelectedProfile is not null;
    public bool CanAddNewProfile => SelectedMonitor is not null && SelectedMonitor.Profiles.Count < 5;

    public bool IsStartWithWindows
    {
        get => Computer.IsStartWithWindows;
        set
        {
            if (value == Computer.IsStartWithWindows) return;
            Computer.IsStartWithWindows = value;
            NotifyOfPropertyChange();
            Write();
            OnIsStartWithWindowsChanged();
        }
    }

    public bool IsApplySettingsOnStart
    {
        get => Computer.IsApplySettingsOnStart;
        set
        {
            if (value == Computer.IsApplySettingsOnStart) return;
            Computer.IsApplySettingsOnStart = value;
            NotifyOfPropertyChange();
            Write();
        }
    }

    public Display? SelectedNvidiaMonitor
    {
        get => _selectedNvidiaMonitor;
        set
        {
            if (Equals(value, _selectedNvidiaMonitor)) return;
            _selectedNvidiaMonitor = value;
            NotifyOfPropertyChange();
        }
    }

    public Computer Computer
    {
        get => _computer;
        set
        {
            if (Equals(value, _computer)) return;
            _computer = value;
            NotifyOfPropertyChange();
        }
    }

    public async Task HandleAsync(ProfileSettingsEvent message, CancellationToken cancellationToken)
    {
        ProfileSettingsIsDirty = message.IsDirty;
        await Task.CompletedTask;
    }

    private void OnIsStartWithWindowsChanged()
    {
        _registryController.RegisterForStartWithWindows(IsStartWithWindows);
    }

    private void Start()
    {
        _monitors = new ObservableCollection<MonitorViewModel>();

        var computer = _dataController.Load();
        foreach (var monitor in computer!.Monitors)
        {
            var monitorViewModel = _monitorViewModelFactory.Create(monitor);
            foreach (var profileViewModel in monitorViewModel.Profiles)
                WireProfileEvents(profileViewModel);

            monitorViewModel.IsSelectedChanged += OnMonitorViewModelIsSelectedChanged;

            Monitors.Add(monitorViewModel);
        }

        Computer = computer;

        LoadNvidiaDisplays();
        ApplySettingsOnStart();
    }

    private void LoadNvidiaDisplays()
    {
        try
        {
            _nvidiaDisplays = Display.GetDisplays().ToList();
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private void ApplySettingsOnStart()
    {
        if (IsApplySettingsOnStart)
            foreach (var monitorViewModel in Monitors)
            {
                var activeProfile = monitorViewModel.Profiles.SingleOrDefault(p => p.IsActive);
                var nvidiaDisplay =
                    _nvidiaDisplays.SingleOrDefault(d => d.Name == monitorViewModel.Display.DisplayScreen.ScreenName);
                if (activeProfile is not null)
                    _displayController.UpdateColorSettings(monitorViewModel.Display,
                        activeProfile.ProfileSettings.ProfileSetting,
                        nvidiaDisplay);
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

        ProfileSettingsIsDirty = false;
    }

    private void OnProfileRemoved(Guid guid)
    {
        var profileViewModel = SelectedMonitor!.Profiles.Single(p => p.Guid == guid);

        SelectedMonitor.Monitor.Profiles.Remove(profileViewModel.Profile);
        SelectedMonitor?.Profiles.Remove(profileViewModel);

        NotifyOfPropertyChange(nameof(CanAddNewProfile));

        Write();
    }

    private void Write()
    {
        _dataController.Write(Computer);
    }

    private void OnMonitorViewModelIsSelectedChanged(bool isSelected, Guid selectedMonitor)
    {
        SetMonitorState(selectedMonitor, !isSelected);
        SelectedMonitor = isSelected ? Monitors.Single(m => m.Guid == selectedMonitor) : null;
        SelectedNvidiaMonitor =
            _nvidiaDisplays.SingleOrDefault(d => d.Name == SelectedMonitor?.Display.DisplayScreen.ScreenName);

        SetSelectedProfile();
    }

    private void SetSelectedProfile()
    {
        SelectedProfile = SelectedMonitor?.Profiles.SingleOrDefault(p => p.IsActive);
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
        var result = _nvidiaDisplayWindowManager.OpenProfileNameViewModel();
        if (result is null)
            return;

        var profileViewModel =
            _profileViewModelFactory.Create(_profileFactory.Create(SelectedMonitor!.Monitor,
                result.ProfileName), SelectedMonitor);

        WireProfileEvents(profileViewModel);

        SelectedMonitor?.Profiles.Add(profileViewModel);
        SelectedProfile = profileViewModel;
        SelectedProfile.IsSelected = true;
        
        NotifyOfPropertyChange(nameof(CanAddNewProfile));
        
        Write();
    }

    public void Apply()
    {
        _displayController.UpdateColorSettings(
            SelectedMonitor!.Display,
            SelectedProfile!.ProfileSettings.ProfileSetting,
            SelectedNvidiaMonitor);
        
        SetActiveProfile();
        Write();

        ProfileSettingsIsDirty = false;
        GlobalEvents.UpdateToolTip.Invoke();
    }

    private void SetActiveProfile()
    {
        SelectedProfile!.IsActive = true;
        foreach (var profileViewModel in SelectedMonitor!.Profiles.Where(p => p.Guid != SelectedProfile.Guid))
            profileViewModel.Deactivate();
    }

    public void Revert()
    {
        Task.Run(async () => await _eventAggregator.PublishOnCurrentThreadAsync(new RevertEvent()));
    }

    public void Update()
    {
        Write();

        SelectedProfile!.IsUpdated();
        ProfileSettingsIsDirty = false;
    }

    public void OpenHelp()
    {
        _nvidiaDisplayWindowManager.OpenHelp();
    }

    public void OpenDonation()
    {
        _nvidiaDisplayWindowManager.OpenWebsite("https://www.paypal.com/donate/?hosted_button_id=FT6HS8V8R4XYC");
    }
}