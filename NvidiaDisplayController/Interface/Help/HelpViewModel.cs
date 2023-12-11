using System.Windows.Input;
using Caliburn.Micro;
using NvidiaDisplayController.Global;
using NvidiaDisplayController.Objects.Factories;
using Prism.Commands;

namespace NvidiaDisplayController.Interface.Help;

public interface IHelpViewModelFactory : IFactory
{
    HelpViewModel Create();
}

public class HelpViewModel : Screen
{
    public HelpViewModel()
    {
        OpenWebsiteCommand = new DelegateCommand<object>(MyAction, o => true);
    }

    public ICommand OpenWebsiteCommand { get; }

    public override string DisplayName
    {
        get => "About";
        set { }
    }

    private void MyAction(object website)
    {
        if(website is string websiteValue)
            WebsiteLauncher.OpenWebsite(websiteValue);
    }
}