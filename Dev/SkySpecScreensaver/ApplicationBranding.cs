using System.Reflection;

namespace SkySpec.ScreenSaver;

internal static class ApplicationBranding
{
    public static string Title { get; } =
        Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyTitleAttribute>()?
            .Title
        ?? "SkySpec";

    public static Icon Icon { get; } =
        System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath)
        ?? SystemIcons.Application;
}
