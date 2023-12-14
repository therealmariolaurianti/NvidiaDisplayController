using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using FluentResults;
using Ninject;
using Ninject.Extensions.Conventions;
using NLog;
using NvAPIWrapper;
using NvidiaDisplayController.Global.Controllers;
using NvidiaDisplayController.Global.Extensions;
using NvidiaDisplayController.Interface.Shell;
using NvidiaDisplayController.Objects.Factories;
using NvidiaDisplayController.Objects.Factories.Interfaces;
using LogManager = NLog.LogManager;

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

    private ComputerFactory _computerFactory => _kernel.Get<ComputerFactory>();
    private DataController _dataController => _kernel.Get<DataController>();
    private ILogger _fileLogger => _kernel.Get<ILogger>();

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
        _kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
        _kernel.Bind<ILogger>().ToConstant(LogManager.GetCurrentClassLogger()).InSingletonScope();

        _kernel.Bind(x => x.FromThisAssembly()
            .SelectAllInterfaces()
            .InheritedFrom<IFactory>()
            .BindToFactory());

        CheckIfApplicationIsRunning()
            .IfSuccess(() => TryStartNvidia()
                .IfSuccess(() => TryLoad()
                    .IfSuccess(() =>
                    {
                        DisplayRootViewForAsync<ShellViewModel>();
                        _fileLogger.Info("Loaded root.");
                    })));
    }

    private Result CheckIfApplicationIsRunning()
    {
        if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length <= 1)
        {
            _fileLogger.Info("Starting Application.");
            return Result.Ok();
        }

        var message = "Application is already running.";

        _fileLogger.Info(message);
        MessageBox.Show(message);

        Application.Current.Shutdown();
        return Result.Fail(message);
    }

    private Result TryStartNvidia()
    {
        try
        {
            NVIDIA.Initialize();
            _fileLogger.Info("Starting Nvidia.");
            return Result.Ok();
        }
        catch (Exception e)
        {
            return Log(e, "Nvidia device not found.");
        }
    }

    private Result Log(Exception e, string message)
    {
        _fileLogger.Error(e);
        Execute.OnUIThread(() => MessageBox.Show(message));
        Application.Shutdown();

        return Result.Fail(message);
    }

    private Result TryLoad()
    {
        try
        {
            return _dataController.Load()
                .IfFail(Start)
                .Bind(_ => Result.Ok());
        }
        catch (Exception e)
        {
            return Log(e, "Failed to load data.");
        }
    }

    private Result Start()
    {
        try
        {
            _fileLogger.Info("Loading data.");
            return _computerFactory
                .Create()
                .IfSuccess(computer => _dataController.Write(computer))
                .ToResult();
        }
        catch (Exception e)
        {
            return Log(e, "Failed to load data.");
        }
    }

    protected override void PrepareApplication()
    {
        AppDomain.CurrentDomain.UnhandledException += OnError;
        base.PrepareApplication();
    }

    private void OnError(object sender, UnhandledExceptionEventArgs e)
    {
        Log((Exception)e.ExceptionObject, "An unexpected error has occured.");
    }

    protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _fileLogger.Error(e);
    }
}