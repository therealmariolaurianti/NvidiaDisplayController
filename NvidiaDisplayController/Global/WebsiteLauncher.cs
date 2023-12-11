using System;
using System.ComponentModel;
using System.Diagnostics;

namespace NvidiaDisplayController.Global;

public static class WebsiteLauncher
{
    private static bool IsValidUri(string uri)
    {
        if (!Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            return false;
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var tmp))
            return false;
        return tmp.Scheme == Uri.UriSchemeHttp || tmp.Scheme == Uri.UriSchemeHttps;
    }

    public static void OpenWebsite(string uri)
    {
        if (string.IsNullOrWhiteSpace(uri))
            return;

        if (!uri.StartsWith("https://"))
            uri = uri.Insert(0, "https://");

        if (!IsValidUri(uri))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(uri)
            {
                UseShellExecute = true
            });
        }
        catch (Win32Exception noBrowser)
        {
        }
        catch (Exception other)
        {
        }
    }
}