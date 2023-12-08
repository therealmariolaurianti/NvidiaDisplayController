using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.Profiles;

namespace NvidiaDisplayController.Objects.Factories;

public class ProfileViewModelFactory
{
    public ProfileViewModel Create(Profile profile, MonitorViewModel monitorViewModel)
    {
        return new ProfileViewModel(profile)
        {
            MonitorViewModel = monitorViewModel
        };
    } 
}