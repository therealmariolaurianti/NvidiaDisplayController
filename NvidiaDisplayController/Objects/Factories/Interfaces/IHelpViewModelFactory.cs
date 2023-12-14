using NvidiaDisplayController.Interface.Help;

namespace NvidiaDisplayController.Objects.Factories.Interfaces;

public interface IHelpViewModelFactory : IFactory
{
    HelpViewModel Create();
}