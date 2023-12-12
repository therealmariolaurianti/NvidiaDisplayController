using System.Diagnostics;
using Microsoft.Win32;

namespace NvidiaDisplayController.Global;

public class RegistryController
{
    private static string NvidiaDisplayController => "NvidiaDisplayController";

    public void RegisterForStartWithWindows(bool isStartWithWindows)
    {
        var registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
        if (isStartWithWindows)
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
                registryKey?.SetValue(NvidiaDisplayController, processModule.FileName);
        }
        else
        {
            registryKey?.DeleteValue(NvidiaDisplayController);
        }
    }
}