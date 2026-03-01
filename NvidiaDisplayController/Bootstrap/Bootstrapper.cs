using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using System.Runtime.InteropServices;
using System.Text;

namespace NvidiaDisplayController.Bootstrap;

public class Bootstrapper : BootstrapperBase
{
    private readonly IKernel _kernel;

    // Used for Window Management
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    // Used for cross process communication 
    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

    // custom message
    private const int WM_SHOWME = 0x0400 + 1; // WM_USER + 1
    
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private const int SW_RESTORE = 9;

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
        var currentProcess = Process.GetCurrentProcess();
        var existingProcess = Process.GetProcessesByName(currentProcess.ProcessName)
                                    .FirstOrDefault(p => p.Id != currentProcess.Id);

        // if there's no other process of this running, we continue
        if (existingProcess == null) return Result.Ok();

        // if we find an existing process, find the window and bring it to the front
        IntPtr foundHandle = IntPtr.Zero;

        // we need to partial match since the window title changes based on GPU
        EnumWindows((hWnd, lParam) =>
        {
            StringBuilder sb = new StringBuilder(256);
            GetWindowText(hWnd, sb, sb.Capacity);
            string title = sb.ToString();

            if (title.StartsWith("Adjust Displays")) 
            {
                foundHandle = hWnd;
                return false; 
            }
            return true;
        }, IntPtr.Zero);

        if (foundHandle != IntPtr.Zero)
        {
            _fileLogger.Info("Found existing window via partial match. Restoring...");
            ShowWindow(foundHandle, SW_RESTORE);
            SetForegroundWindow(foundHandle);

            // we can't force the other instance to bring itself to the front
            // tell it to do it itself
            PostMessage(foundHandle, WM_SHOWME, IntPtr.Zero, IntPtr.Zero);

            Application.Current.Shutdown();
            return Result.Fail("Focused existing instance.");
        }

    return Result.Ok();
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