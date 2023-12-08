using NvidiaDisplayController.Interface.ProfileSettings;

namespace NvidiaDisplayController.Objects.Factories;

public interface IProfileSettingViewModelFactory : IFactory
{
    ProfileSettingViewModel Create(ProfileSetting profileSetting, bool isDefault);
}