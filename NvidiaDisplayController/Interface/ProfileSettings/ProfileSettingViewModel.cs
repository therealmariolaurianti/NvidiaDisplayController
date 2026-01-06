using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using NvidiaDisplayController.Objects.Entities;
using NvidiaDisplayController.Objects.HandleEvents;

namespace NvidiaDisplayController.Interface.ProfileSettings;

public class ProfileSettingViewModel : Screen, IHandle<RevertEvent>
{
    private readonly IEventAggregator _eventAggregator;
    private readonly Profile _profile;
    private ProfileSetting _originalSettings = null!;
    private bool _resetting;
    private bool _isCtrlChecked;
    private bool _isAltChecked;
    private bool _isShiftChecked;
    private Key? _selectedKey;

    public ProfileSettingViewModel(ProfileSetting profileSetting, bool isDefault, IEventAggregator eventAggregator, Profile profile)
    {
        _eventAggregator = eventAggregator;
        _profile = profile;
        ProfileSetting = profileSetting;
        IsDefault = isDefault;

        SetOriginalSettings(profileSetting);
        LoadHotkeySettings();
        _eventAggregator.SubscribeOnPublishedThread(this);
    }

    public ProfileSetting ProfileSetting { get; }

    public bool IsDefault { get; }
    
    public List<Key> AvailableKeys { get; } = new()
    {
        Key.F1, Key.F2, Key.F3, Key.F4, Key.F5, Key.F6, Key.F7, Key.F8, Key.F9, Key.F10, Key.F11, Key.F12,
        Key.D0, Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9,
        Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J, Key.K, Key.L, Key.M,
        Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T, Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z
    };

    public bool IsCtrlChecked
    {
        get => _isCtrlChecked;
        set
        {
            if (value == _isCtrlChecked) return;
            _isCtrlChecked = value;
            NotifyOfPropertyChange();
            UpdateHotkey();
        }
    }

    public bool IsAltChecked
    {
        get => _isAltChecked;
        set
        {
            if (value == _isAltChecked) return;
            _isAltChecked = value;
            NotifyOfPropertyChange();
            UpdateHotkey();
        }
    }

    public bool IsShiftChecked
    {
        get => _isShiftChecked;
        set
        {
            if (value == _isShiftChecked) return;
            _isShiftChecked = value;
            NotifyOfPropertyChange();
            UpdateHotkey();
        }
    }

    public Key? SelectedKey
    {
        get => _selectedKey;
        set
        {
            if (Equals(value, _selectedKey)) return;
            _selectedKey = value;
            NotifyOfPropertyChange();
            UpdateHotkey();
        }
    }

    public ICommand ClearHotkeyCommand => new RelayCommand(ClearHotkey);

    public double Brightness
    {
        get => ProfileSetting.Brightness;
        set
        {
            if (value.Equals(ProfileSetting.Brightness)) return;
            ProfileSetting.Brightness = value;
            NotifyOfPropertyChange();
            Publish();
        }
    }

    public double Contrast
    {
        get => ProfileSetting.Contrast;
        set
        {
            if (value.Equals(ProfileSetting.Contrast)) return;
            ProfileSetting.Contrast = value;
            NotifyOfPropertyChange();
            Publish();
        }
    }

    public double Gamma
    {
        get => ProfileSetting.Gamma;
        set
        {
            if (value.Equals(ProfileSetting.Gamma)) return;
            ProfileSetting.Gamma = value;
            NotifyOfPropertyChange();
            Publish();
        }
    }

    public double DigitalVibrance
    {
        get => ProfileSetting.DigitalVibrance;
        set
        {
            if (value.Equals(ProfileSetting.DigitalVibrance)) return;
            ProfileSetting.DigitalVibrance = value;
            NotifyOfPropertyChange();
            Publish();
        }
    }

    public Task HandleAsync(RevertEvent message, CancellationToken cancellationToken)
    {
        _resetting = true;
        {
            Brightness = _originalSettings.Brightness;
            Contrast = _originalSettings.Contrast;
            Gamma = _originalSettings.Gamma;
        }
        _resetting = false;

        Publish(false);

        return Task.CompletedTask;
    }

    private void LoadHotkeySettings()
    {
        if (_profile.HotkeyModifiers.HasValue)
        {
            _isCtrlChecked = (_profile.HotkeyModifiers.Value & ModifierKeys.Control) == ModifierKeys.Control;
            _isAltChecked = (_profile.HotkeyModifiers.Value & ModifierKeys.Alt) == ModifierKeys.Alt;
            _isShiftChecked = (_profile.HotkeyModifiers.Value & ModifierKeys.Shift) == ModifierKeys.Shift;
        }
        _selectedKey = _profile.HotkeyKey;
        
        NotifyOfPropertyChange(nameof(IsCtrlChecked));
        NotifyOfPropertyChange(nameof(IsAltChecked));
        NotifyOfPropertyChange(nameof(IsShiftChecked));
        NotifyOfPropertyChange(nameof(SelectedKey));
    }

    private void UpdateHotkey()
    {
        if (_resetting) return;

        if (_selectedKey.HasValue && (IsCtrlChecked || IsAltChecked || IsShiftChecked))
        {
            var modifiers = ModifierKeys.None;
            if (IsCtrlChecked) modifiers |= ModifierKeys.Control;
            if (IsAltChecked) modifiers |= ModifierKeys.Alt;
            if (IsShiftChecked) modifiers |= ModifierKeys.Shift;

            _profile.HotkeyModifiers = modifiers;
            _profile.HotkeyKey = _selectedKey;
        }
        else
        {
            _profile.HotkeyModifiers = null;
            _profile.HotkeyKey = null;
        }

        Publish();
    }

    private void ClearHotkey()
    {
        _resetting = true;
        IsCtrlChecked = false;
        IsAltChecked = false;
        IsShiftChecked = false;
        SelectedKey = null;
        _resetting = false;

        _profile.HotkeyModifiers = null;
        _profile.HotkeyKey = null;

        Publish();
    }

    private void SetOriginalSettings(ProfileSetting profileSetting)
    {
        _originalSettings = new ProfileSetting(profileSetting.Brightness, profileSetting.Contrast,
            profileSetting.Gamma, profileSetting.DigitalVibrance);
    }

    private void Publish(bool value = true)
    {
        if (!_resetting)
            Task.Run(async () => await _eventAggregator.PublishOnCurrentThreadAsync(new ProfileSettingsEvent(value)));
    }

    public void IsUpdated()
    {
        SetOriginalSettings(ProfileSetting);
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;

    public RelayCommand(Action execute)
    {
        _execute = execute;
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter)
    {
        _execute();
    }

    public event EventHandler? CanExecuteChanged;
}
