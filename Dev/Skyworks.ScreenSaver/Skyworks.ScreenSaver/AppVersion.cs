namespace Skyworks.ScreenSaver;

internal static class AppVersion
{
    public static Version Number { get; } = typeof(AppVersion).Assembly.GetName().Version
        ?? throw new InvalidOperationException("The screensaver assembly has no version metadata.");

    public static string DisplayText { get; } = $"Version: {Number.ToString(4)}";
}
