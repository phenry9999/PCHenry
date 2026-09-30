using Microsoft.Win32;

namespace SkySpec.ScreenSaver;

internal static class WidgetStartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "SkySpecStatusWidget";

    public static void Apply(bool enabled)
    {
        using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath);
        if (enabled)
        {
            var executablePath = Path.Combine(
                AppContext.BaseDirectory,
                $"{typeof(WidgetStartupRegistration).Assembly.GetName().Name}.exe");
            runKey.SetValue(ValueName, $"\"{executablePath}\"");
        }
        else
        {
            runKey.DeleteValue(ValueName, false);
        }
    }
}
