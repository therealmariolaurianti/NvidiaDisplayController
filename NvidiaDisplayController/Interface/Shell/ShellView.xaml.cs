using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using Ninject;
using NvidiaDisplayController.Global;
using NvidiaDisplayController.Global.Controllers;
using Application = System.Windows.Application;

namespace NvidiaDisplayController.Interface.Shell;

public partial class ShellView
{
    private NotifyIcon? _notifyIcon;

    public ShellView()
    {
        InitializeComponent();
        Start();
    }

    [Inject] public DataController DataController { get; set; } = null!;

    private void Start()
    {
        IoC.BuildUp(this);

        CreateSystemTrayIcon();

        GlobalEvents.UpdateToolTip += OnUpdateToolTip;
    }

    private void OnUpdateToolTip()
    {
        BuildToolTip();
    }

    private void CreateSystemTrayIcon()
    {
        _notifyIcon = new NotifyIcon();
        _notifyIcon.Icon = new Icon("Resources/desktop.ico");
        _notifyIcon.Visible = true;

        _notifyIcon.ContextMenuStrip = new ContextMenuStrip();
        _notifyIcon.ContextMenuStrip.Items.Add("Open", null, OpenEvent);
        _notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
        _notifyIcon.ContextMenuStrip.Items.Add("Exit", null, ExitEvent);

        BuildToolTip();
    }

    private void BuildToolTip()
    {
        var data = DataController.Load();
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("Nvidia Display Controller");
        foreach (var monitor in data!.Monitors)
        {
            var activeProfile = monitor.Profiles.Single(p => p.IsActive);
            stringBuilder.AppendLine($"{monitor.Name} - {activeProfile.Name}");
        }

        _notifyIcon!.Text = stringBuilder.ToString();
    }

    private void ExitEvent(object? sender, EventArgs args)
    {
        Application.Current.Shutdown();
    }

    private void OpenEvent(object? sender, EventArgs args)
    {
        DoShow();
    }

    private void DoShow()
    {
        Show();
        WindowState = WindowState.Normal;
    }

    protected override void OnStateChanged(EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
            Hide();

        base.OnStateChanged(e);
    }
}