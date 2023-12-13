using System;
using System.Diagnostics;
using System.Windows;
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
        {
            var directoryName = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
                registryKey.SetValue(NvidiaDisplayController, directoryName);
        }
        else
        {
            registryKey.DeleteValue(NvidiaDisplayController);
        }
    }
}