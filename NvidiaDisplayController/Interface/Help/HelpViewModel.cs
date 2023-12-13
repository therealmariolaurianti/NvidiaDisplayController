using System.Windows.Forms;
using System.Windows.Input;
using NvidiaDisplayController.Global;
using NvidiaDisplayController.Global.Controllers;
using NvidiaDisplayController.Objects.Factories;
using Prism.Commands;
using Screen = Caliburn.Micro.Screen;

namespace NvidiaDisplayController.Interface.Help;

public interface IHelpViewModelFactory : IFactory
{
    HelpViewModel Create();
}

public class HelpViewModel : Screen
{
    private readonly DataController _dataController;

    public HelpViewModel(DataController dataController)
    {
        _dataController = dataController;
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
        if (website is string websiteValue)
            WebsiteLauncher.OpenWebsite(websiteValue);
    }

    public void Reset()
    {
        _dataController.Write(string.Empty);

        Application.Restart();
        System.Windows.Application.Current.Shutdown();
    }
}