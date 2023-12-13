using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;
using Caliburn.Micro;
using NvidiaDisplayController.Global;
using NvidiaDisplayController.Global.Controllers;
using NvidiaDisplayController.Objects.Factories;
using Prism.Commands;

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
        _dataController.Write(null);

        var process = new ProcessStartInfo
        {
            Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetEntryAssembly()!.Location + "\"",
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            FileName = "cmd.exe"
        };
        
        Process.Start(process);
        Process.GetCurrentProcess().Kill();
    }
}