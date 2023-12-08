using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Ninject;
using Ninject.Extensions.Conventions;
using NvidiaDisplayController.Data;
using NvidiaDisplayController.Interface.Shell;
using NvidiaDisplayController.Objects;
using NvidiaDisplayController.Objects.Factories;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;

namespace NvidiaDisplayController.Bootstrap;

public class Bootstrapper : BootstrapperBase
{
    private readonly IKernel _kernel;

    public Bootstrapper()
    {
        _kernel = new StandardKernel();
        _kernel.Load(Assembly.GetExecutingAssembly());

        Initialize();
    }

    private MonitorFactory _monitorFactory => _kernel.Get<MonitorFactory>();
    private DataController _dataController => _kernel.Get<DataController>();

    protected override void BuildUp(object instance)
    {
        _kernel.Inject(instance);
    }

    protected override IEnumerable<object> GetAllInstances(Type service)
    {
        return _kernel.GetAll(service);
    }

    protected override object GetInstance(Type service, string key)
    {
        if (service == null)
            throw new ArgumentNullException(nameof(service));

        return _kernel.Get(service);
    }

    protected override void OnStartup(object sender, StartupEventArgs e)
    {
        _kernel.Bind<IWindowManager>().To<WindowManager>();
        _kernel.Bind<IEventAggregator>().To<EventAggregator>();

        _kernel.Bind(x => x.FromThisAssembly()
            .SelectAllInterfaces()
            .InheritedFrom<IFactory>()
            .BindToFactory());

        Load();
        DisplayRootViewForAsync<ShellViewModel>();
    }

    private void Load()
    {
        var monitors = _dataController.Load();
        if (monitors is null)
            StartupMonitors();
    }

    private void StartupMonitors()
    {
        var displays = Display.GetDisplays();
        var pathDisplayTargets = PathDisplayTarget.GetDisplayTargets();

        var monitors = new List<Monitor>();
        foreach (var display in displays)
        {
            var resolution = display.DisplayScreen.CurrentSetting.Resolution;
            var frequency = display.DisplayScreen.CurrentSetting.Frequency;
            var displaySource = pathDisplayTargets.Single(pds => pds.DevicePath == display.DevicePath);

            var monitor = _monitorFactory.CreateDefault(display.DevicePath, displaySource.FriendlyName,
                resolution, frequency);

            monitors.Add(monitor);
        }

        _dataController.Write(monitors);
    }
}