using NvidiaDisplayController.Interface.ProfileSettings;
using NvidiaDisplayController.Objects.Entities;

namespace NvidiaDisplayController.Objects.Factories.Interfaces;

public interface IProfileSettingViewModelFactory : IFactory
{
    ProfileSettingViewModel Create(ProfileSetting profileSetting, bool isDefault);
}