using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.Profiles;

namespace NvidiaDisplayController.Objects.Factories;

public interface IProfileViewModelFactory : IFactory
{
    ProfileViewModel Create(Profile profile, MonitorViewModel monitorViewModel);
}