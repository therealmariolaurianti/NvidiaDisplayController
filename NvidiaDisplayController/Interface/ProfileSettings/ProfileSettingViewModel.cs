using Caliburn.Micro;
using NvidiaDisplayController.Objects;

namespace NvidiaDisplayController.Interface.ProfileSettings;

public class ProfileSettingViewModel : Screen
{
    private readonly ProfileSetting _profileSetting;
    private readonly IEventAggregator _eventAggregator;

    public ProfileSettingViewModel(ProfileSetting profileSetting, bool isDefault, IEventAggregator eventAggregator)
    {
        IsDefault = isDefault;
        _eventAggregator = eventAggregator;
        _profileSetting = profileSetting;
    }

    public bool IsDefault { get; }

    public double Brightness
    {
        get => _profileSetting.Brightness;
        set
        {
            if (value.Equals(_profileSetting.Brightness)) return;
            _profileSetting.Brightness = value;
            NotifyOfPropertyChange();
            
            _eventAggregator.PublishOnCurrentThreadAsync(new ProfileSettingsEvent(true));
        }
    }

    public double Contrast
    {
        get => _profileSetting.Contrast;
        set
        {
            if (value.Equals(_profileSetting.Contrast)) return;
            _profileSetting.Contrast = value;
            NotifyOfPropertyChange();
            
            _eventAggregator.PublishOnCurrentThreadAsync(new ProfileSettingsEvent(true));
        }
    }

    public double Gamma
    {
        get => _profileSetting.Gamma;
        set
        {
            if (value.Equals(_profileSetting.Gamma)) return;
            _profileSetting.Gamma = value;
            NotifyOfPropertyChange();
            
            _eventAggregator.PublishOnCurrentThreadAsync(new ProfileSettingsEvent(true));
        }
    }
}