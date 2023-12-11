using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using Microsoft.Win32;
using NvidiaDisplayController.Data;
using NvidiaDisplayController.Global;
using NvidiaDisplayController.Interface.Help;
using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.Profiles;
using NvidiaDisplayController.Objects;
using NvidiaDisplayController.Objects.Factories;
using WindowsDisplayAPI;
using Display = NvAPIWrapper.Display.Display;

namespace NvidiaDisplayController.Interface.Shell;

public class ShellViewModel : Conductor<IScreen>, IHandle<ProfileSettingsEvent>
{
    private readonly DataController _dataController;
    private readonly IEventAggregator _eventAggregator;

    private readonly IHelpViewModelFactory _helpViewModelFactory;
    private readonly MonitorViewModelFactory _monitorViewModelFactory;
    private readonly ProfileFactory _profileFactory;
    private readonly IProfileNameViewModelFactory _profileNameViewModelFactory;
    private readonly IProfileViewModelFactory _profileViewModelFactory;
    private readonly WindowManager _windowManager;
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
        DataController dataController, IProfileViewModelFactory profileViewModelFactory, ProfileFactory profileFactory,
        WindowManager windowManager, IProfileNameViewModelFactory profileNameViewModelFactory,
        IHelpViewModelFactory helpViewModelFactory)
    {
        _eventAggregator = eventAggregator;
        _monitorViewModelFactory = monitorViewModelFactory;
        _dataController = dataController;
        _profileViewModelFactory = profileViewModelFactory;
        _profileFactory = profileFactory;
        _windowManager = windowManager;
        _profileNameViewModelFactory = profileNameViewModelFactory;
        _helpViewModelFactory = helpViewModelFactory;

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

    private static string NvidiaDisplayController => "NvidiaDisplayController";

    public async Task HandleAsync(ProfileSettingsEvent message, CancellationToken cancellationToken)
    {
        ProfileSettingsIsDirty = message.IsDirty;
        await Task.CompletedTask;
    }

    private void OnIsStartWithWindowsChanged()
    {
        var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (IsStartWithWindows)
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
                registryKey?.SetValue(NvidiaDisplayController, processModule.FileName);
        }
        else
        {
            registryKey?.DeleteValue(NvidiaDisplayController);
        }
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

        _nvidiaDisplays = Display.GetDisplays().ToList();
        Computer = computer;

        ApplySettingsOnStart();
    }

    private void ApplySettingsOnStart()
    {
        if (IsApplySettingsOnStart)
            foreach (var monitorViewModel in Monitors)
            {
                var activeProfile = monitorViewModel.Profiles.SingleOrDefault(p => p.IsActive);
                var nvidiaDisplay =
                    _nvidiaDisplays.SingleOrDefault(d => d.Name == monitorViewModel.Display.DisplayScreen.ScreenName);
                if (activeProfile is not null && nvidiaDisplay is not null)
                    UpdateColorSettings(monitorViewModel.Display, activeProfile.ProfileSettings.ProfileSetting,
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
            SelectedProfile.IsSelected = true;

            NotifyOfPropertyChange(nameof(CanAddNewProfile));
        }
    }

    public void Apply()
    {
        UpdateColorSettings(SelectedMonitor!.Display, SelectedProfile!.ProfileSettings.ProfileSetting,
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

    private void UpdateColorSettings(WindowsDisplayAPI.Display display, ProfileSetting profileSetting,
        Display? nvidiaMonitor)
    {
        display.GammaRamp =
            new DisplayGammaRamp(profileSetting.Brightness, profileSetting.Contrast, profileSetting.Gamma);
        nvidiaMonitor!.DigitalVibranceControl.NormalizedLevel = profileSetting.DigitalVibrance - .3;
    }

    public void OpenHelp()
    {
        var viewModel = _helpViewModelFactory.Create();
        _windowManager.ShowDialogAsync(viewModel);
    }

    public void OpenDonation()
    {
        WebsiteLauncher.OpenWebsite("https://www.paypal.com/donate/?hosted_button_id=FT6HS8V8R4XYC");
    }
}