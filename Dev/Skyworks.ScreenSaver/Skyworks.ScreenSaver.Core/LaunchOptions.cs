using System.Globalization;

namespace Skyworks.ScreenSaver.Core;

public enum SaverMode
{
    Run,
    Preview,
    Settings
}

public sealed record LaunchOptions(SaverMode Mode, nint ParentHandle)
{
    public static LaunchOptions Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length == 0)
            return new(SaverMode.Settings, 0);
        if (args.Length > 2 || string.IsNullOrEmpty(args[0]))
            throw new ArgumentException("Expected a screensaver switch and an optional window handle.", nameof(args));

        string argument = args[0];
        if (argument.Length < 2 || (argument[0] != '/' && argument[0] != '-'))
            throw new ArgumentException("Expected /s, /c, or /p.", nameof(args));

        string[] parts = argument[1..].Split(':');
        SaverMode mode = parts[0].ToLowerInvariant() switch
        {
            "s" => SaverMode.Run,
            "c" => SaverMode.Settings,
            "p" => SaverMode.Preview,
            _ => throw new ArgumentException("Unknown screensaver switch.", nameof(args))
        };
        if (parts.Length > 2 || (parts.Length == 2 && args.Length == 2) ||
            (mode == SaverMode.Run && (parts.Length != 1 || args.Length != 1)))
            throw new ArgumentException("Unexpected screensaver arguments.", nameof(args));

        string? handleText = parts.Length == 2 ? parts[1] : args.Length == 2 ? args[1] : null;
        nint handle = 0;
        if (handleText is not null)
        {
            bool hex = handleText.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
            string digits = hex ? handleText[2..] : handleText;
            if (string.IsNullOrEmpty(digits) ||
                !ulong.TryParse(digits, hex ? NumberStyles.AllowHexSpecifier : NumberStyles.None,
                    CultureInfo.InvariantCulture, out ulong value) ||
                value > (ulong)nint.MaxValue)
                throw new ArgumentException("The window handle must be a nonnegative native-sized integer.", nameof(args));
            handle = (nint)value;
        }
        if (mode == SaverMode.Preview && handle == 0)
            throw new ArgumentException("Preview mode requires a nonzero window handle.", nameof(args));

        return new(mode, handle);
    }
}
