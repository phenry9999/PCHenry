using System.Diagnostics;
using Microsoft.Win32;

namespace SkySpec.ScreenSaver;

internal static class ScreenSaverInstaller
{
    private const string DesktopRegistryPath = @"Control Panel\Desktop";
    private const string ScreenSaverValueName = "SCRNSAVE.EXE";

    public static bool IsSelected()
    {
        try
        {
            using var desktopKey = Registry.CurrentUser.OpenSubKey(DesktopRegistryPath);
            var selectedPath = desktopKey?.GetValue(ScreenSaverValueName) as string;
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return false;
            }

            return string.Equals(
                Path.GetFullPath(Environment.ExpandEnvironmentVariables(selectedPath.Trim('"'))),
                Path.GetFullPath(GetScreenSaverPath()),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or System.Security.SecurityException
                or ArgumentException
                or NotSupportedException)
        {
            return false;
        }
    }

    public static void InstallAndSelect()
    {
        var screenSaverPath = GetScreenSaverPath();

        if (!File.Exists(screenSaverPath))
        {
            throw new FileNotFoundException(
                "The SkySpec screen saver file could not be found beside the application.",
                screenSaverPath);
        }

        var process = Process.Start(
            new ProcessStartInfo
            {
                FileName = screenSaverPath,
                Verb = "install",
                UseShellExecute = true
            });

        if (process is null)
        {
            throw new InvalidOperationException(
                "Windows could not open the screen saver installer.");
        }
    }

    private static string GetScreenSaverPath() =>
        Path.Combine(
            AppContext.BaseDirectory,
            $"{typeof(ScreenSaverInstaller).Assembly.GetName().Name}.scr");
}
