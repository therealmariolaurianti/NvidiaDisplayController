using Caliburn.Micro;

namespace NvidiaDisplayController.Interface.ProfileNames;

public class ProfileNameViewModel : Screen
{
    private string _profileName;

    public override string DisplayName
    {
        get => "New Profile Name";
        set { }
    }

    public string ProfileName
    {
        get => _profileName;
        set
        {
            if (value == _profileName) return;
            _profileName = value;
            NotifyOfPropertyChange();
        }
    }

    public void Save()
    {
        TryCloseAsync(true);
    }

    public void Cancel()
    {
        TryCloseAsync(false);
    }
}