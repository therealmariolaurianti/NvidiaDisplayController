using NvidiaDisplayController.Interface.ProfileNames;

namespace NvidiaDisplayController.Objects.Factories.Interfaces;

public interface IProfileNameViewModelFactory : IFactory
{
    ProfileNameViewModel Create();
}