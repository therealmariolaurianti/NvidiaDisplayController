using NvidiaDisplayController.Interface.Monitors;
using NvidiaDisplayController.Interface.Profiles;
using NvidiaDisplayController.Objects.Entities;

namespace NvidiaDisplayController.Objects.Factories.Interfaces;

public interface IProfileViewModelFactory : IFactory
{
    ProfileViewModel Create(Profile profile, MonitorViewModel monitorViewModel);
}