using System.Windows.Forms;
using Caliburn.Micro;
using NvidiaDisplayController.Interface.Help;
using NvidiaDisplayController.Interface.ProfileNames;
using NvidiaDisplayController.Objects.Factories;

namespace NvidiaDisplayController.Global;

public class NvidiaDisplayWindowManager
{
    private readonly IHelpViewModelFactory _helpViewModelFactory;
    private readonly IProfileNameViewModelFactory _profileNameViewModelFactory;
    private readonly IWindowManager _windowManager;

    public NvidiaDisplayWindowManager(
        IWindowManager windowManager,
        IHelpViewModelFactory helpViewModelFactory,
        IProfileNameViewModelFactory profileNameViewModelFactory)
    {
        _windowManager = windowManager;
        _helpViewModelFactory = helpViewModelFactory;
        _profileNameViewModelFactory = profileNameViewModelFactory;
    }

    public void OpenHelp()
    {
        var viewModel = _helpViewModelFactory.Create();
        _windowManager.ShowDialogAsync(viewModel);
    }

    public void OpenWebsite(string urlString)
    {
        WebsiteLauncher.OpenWebsite(urlString);
    }

    public ProfileNameViewModel? OpenProfileNameViewModel()
    {
        var viewModel = _profileNameViewModelFactory.Create();
        return _windowManager.ShowDialogAsync(viewModel).Result is true ? viewModel : null;
    }

    public void ShowMessageBox(string message)
    {
        MessageBox.Show(message);
    }
}