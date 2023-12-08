using NvidiaDisplayController.Interface.Monitors;

namespace NvidiaDisplayController.Objects.Factories;

public class MonitorViewModelFactory
{
    private readonly IProfileViewModelFactory _profileViewModelFactory;

    public MonitorViewModelFactory(IProfileViewModelFactory profileViewModelFactory)
    {
        _profileViewModelFactory = profileViewModelFactory;
    }

    public MonitorViewModel Create(Monitor monitor)
    {
        var monitorViewModel = new MonitorViewModel(monitor);
        foreach (var profile in monitor.Profiles)
        {
            var profileViewModel = _profileViewModelFactory.Create(profile, monitorViewModel);
            monitorViewModel.Profiles.Add(profileViewModel);
        }

        return monitorViewModel;
    }
}