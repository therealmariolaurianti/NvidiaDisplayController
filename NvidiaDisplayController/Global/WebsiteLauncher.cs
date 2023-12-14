using System;
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

    public static void OpenWebsite(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return;

        if (!url.StartsWith("https://"))
            url = url.Insert(0, "https://");

        if (!IsValidUri(url))
            return;

        try
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }
}