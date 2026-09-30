using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SkySpec.ScreenSaver;

internal static class BrowserLauncher
{
    public static void OpenNewWindow(string url)
    {
        var browserPath = GetDefaultBrowserPath();
        if (browserPath is null)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            return;
        }

        var processStartInfo = new ProcessStartInfo(browserPath)
        {
            UseShellExecute = false
        };
        processStartInfo.ArgumentList.Add(
            Path.GetFileName(browserPath).Contains("firefox", StringComparison.OrdinalIgnoreCase)
                ? "-new-window"
                : "--new-window");
        processStartInfo.ArgumentList.Add(url);
        Process.Start(processStartInfo);
    }

    private static string? GetDefaultBrowserPath()
    {
        var length = 1024u;
        var result = new StringBuilder((int)length);
        var status = AssocQueryString(
            AssociationFlags.None,
            AssociationString.Executable,
            "https",
            "open",
            result,
            ref length);

        return status == 0 && result.Length > 0 ? result.ToString() : null;
    }

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern uint AssocQueryString(
        AssociationFlags flags,
        AssociationString associationString,
        string association,
        string extra,
        StringBuilder output,
        ref uint outputLength);

    private enum AssociationFlags : uint
    {
        None = 0
    }

    private enum AssociationString
    {
        Executable = 2
    }
}
