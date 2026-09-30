namespace SkySpec.ScreenSaver;

internal enum ScreenSaverMode
{
    None,
    Configure,
    FullScreen,
    Widget
}

internal sealed record ScreenSaverCommand(ScreenSaverMode Mode)
{
    public static ScreenSaverCommand Parse(string[] args)
    {
        if (args.Length == 0)
        {
            return new(ScreenSaverMode.Widget);
        }

        var option = args[0].Trim().ToLowerInvariant().Split(':', 2)[0];

        return option switch
        {
            "/s" or "-s" => new(ScreenSaverMode.FullScreen),
            "/w" or "-w" or "/widget" or "-widget" => new(ScreenSaverMode.Widget),
            "/p" or "-p" => new(ScreenSaverMode.None),
            "/c" or "-c" => new(ScreenSaverMode.Configure),
            _ => new(ScreenSaverMode.Configure)
        };
    }
}
