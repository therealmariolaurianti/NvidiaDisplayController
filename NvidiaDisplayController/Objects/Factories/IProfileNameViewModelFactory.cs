using NvidiaDisplayController.Interface.ProfileNames;

namespace NvidiaDisplayController.Objects.Factories;

public interface IProfileNameViewModelFactory : IFactory
{
    ProfileNameViewModel Create();
}