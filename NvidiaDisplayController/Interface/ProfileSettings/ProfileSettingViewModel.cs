using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using NvidiaDisplayController.Objects;

namespace NvidiaDisplayController.Interface.ProfileSettings;

public class ProfileSettingViewModel : Screen, IHandle<RevertEvent>
{
    private readonly IEventAggregator _eventAggregator;
    private ProfileSetting _originalSettings = null!;
    private bool _resetting;

    public ProfileSettingViewModel(ProfileSetting profileSetting, bool isDefault, IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        ProfileSetting = profileSetting;
        IsDefault = isDefault;

        SetOriginalSettings(profileSetting);
        _eventAggregator.SubscribeOnPublishedThread(this);
    }

    public ProfileSetting ProfileSetting { get; }

    public bool IsDefault { get; }

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

    public async Task HandleAsync(RevertEvent message, CancellationToken cancellationToken)
    {
        _resetting = true;
        {
            Brightness = _originalSettings.Brightness;
            Contrast = _originalSettings.Contrast;
            Gamma = _originalSettings.Gamma;
        }
        _resetting = false;

        Publish(false);

        await Task.CompletedTask;
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