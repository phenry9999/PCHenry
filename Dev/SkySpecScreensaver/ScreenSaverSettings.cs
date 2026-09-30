using System.Text.Json;

namespace SkySpec.ScreenSaver;

internal sealed class ScreenSaverSettings {
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string SettingsFileName =
        $"{typeof(ScreenSaverSettings).Assembly.GetName().Name}.settings.json";

    public int RefreshIntervalMinutes { get; set; } = 1;

    public bool StartWidgetWithWindows { get; set; }

    public bool WidgetAlwaysOnTop { get; set; }

    public bool ShowWidgetOnTaskbar { get; set; } = true;

    public int? WidgetLeft { get; set; }

    public int? WidgetTop { get; set; }

    public int WidgetWidth { get; set; } = 820;

    public int WidgetHeight { get; set; } = 300;

    public List<EnvironmentEndpoint> Endpoints { get; set; } =
    [
        new("Local", "https://localhost:7163/") { Include = false },
        new("Dev", "https://skyspec2-dev.skyworksinc.com/"),
        new("Stage", "https://skyspec2-stage.skyworksinc.com/"),
        new("Prod", "https://skyspec2.skyworksinc.com/")
    ];

    private static string UserSettingsPath =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SkySpec",
            "StatusScreenSaver",
            SettingsFileName);

    private static string ApplicationSettingsPath =>
        Path.Combine(AppContext.BaseDirectory, SettingsFileName);

    public static ScreenSaverSettings Load() {
        var settingsPath = File.Exists(UserSettingsPath)
            ? UserSettingsPath
            : ApplicationSettingsPath;

        if (!File.Exists(settingsPath)) {
            throw new FileNotFoundException(
                "Neither a user settings file nor the application settings file could be found.",
                settingsPath);
        }

        try {
            var settings = JsonSerializer.Deserialize<ScreenSaverSettings>(
                File.ReadAllText(settingsPath),
                JsonOptions);

            if (settings is not { Endpoints.Count: 4 }) {
                throw new InvalidDataException(
                    $"Settings file '{settingsPath}' must define exactly four endpoints.");
            }

            return settings;
        }
        catch (JsonException exception) {
            throw new InvalidDataException(
                $"Settings file '{settingsPath}' contains invalid JSON.",
                exception);
        }
    }

    public void Save() {
        var directory = Path.GetDirectoryName(UserSettingsPath)!;
        Directory.CreateDirectory(directory);
        File.WriteAllText(UserSettingsPath, JsonSerializer.Serialize(this, JsonOptions));
    }
}

internal sealed record EnvironmentEndpoint(string Name, string Url)
{
    public bool Include { get; init; } = true;
}
