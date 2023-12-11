using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace NvidiaDisplayController.Interface.Shell;

public partial class ShellView
{
    private NotifyIcon? _notifyIcon;

    public ShellView()
    {
        InitializeComponent();
        CreateSystemTrayIcon();
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