using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NvidiaDisplayController.Global.Controllers;

public class RegistryController
{
    private static string NvidiaDisplayController => "NvidiaDisplayController";

    public void RegisterForStartWithWindows(bool isStartWithWindows)
    {
        var registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        if (registryKey is null)
            throw new Exception();

        if (isStartWithWindows)
            registryKey.SetValue(NvidiaDisplayController, Application.ExecutablePath);
        else
            registryKey.DeleteValue(NvidiaDisplayController);
    }
}