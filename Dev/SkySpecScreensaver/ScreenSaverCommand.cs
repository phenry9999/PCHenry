using System.Globalization;

namespace SkySpec.ScreenSaver;

internal enum ScreenSaverMode
{
    Configure,
    Preview,
    FullScreen,
    Widget
}

internal sealed record ScreenSaverCommand(ScreenSaverMode Mode, IntPtr PreviewHandle)
{
    public static ScreenSaverCommand Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new(ScreenSaverMode.Widget, IntPtr.Zero);
        }

        var parts = args[0].Trim().ToLowerInvariant().Split(':', 2);
        var option = parts[0];
        var handleText = parts.Length == 2
            ? parts[1]
            : args.Length > 1 ? args[1] : string.Empty;

        return option switch
        {
            "/s" or "-s" => new(ScreenSaverMode.FullScreen, IntPtr.Zero),
            "/w" or "-w" or "/widget" or "-widget" => new(ScreenSaverMode.Widget, IntPtr.Zero),
            "/p" or "-p" => new(
                ScreenSaverMode.Preview,
                long.TryParse(handleText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var handle)
                    ? new IntPtr(handle)
                    : IntPtr.Zero),
            "/c" or "-c" => new(ScreenSaverMode.Configure, IntPtr.Zero),
            _ => new(ScreenSaverMode.Configure, IntPtr.Zero)
        };
    }
}
