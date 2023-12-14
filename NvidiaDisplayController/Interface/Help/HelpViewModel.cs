using System.Windows.Forms;
using System.Windows.Input;
using NvidiaDisplayController.Global;
using NvidiaDisplayController.Global.Controllers;
using NvidiaDisplayController.Global.Extensions;
using NvidiaDisplayController.Objects.Factories;
using Prism.Commands;
using Screen = Caliburn.Micro.Screen;

namespace NvidiaDisplayController.Interface.Help;

public class HelpViewModel : Screen
{
    private readonly ComputerFactory _computerFactory;
    private readonly DataController _dataController;

    public HelpViewModel(DataController dataController, ComputerFactory computerFactory)
    {
        _dataController = dataController;
        _computerFactory = computerFactory;

        OpenWebsiteCommand = new DelegateCommand<object>(MyAction, _ => true);
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
        _computerFactory.Create()
            .IfSuccess(computer =>
            {
                _dataController.Write(computer);

                Application.Restart();
                System.Windows.Application.Current.Shutdown();
            });
    }
}