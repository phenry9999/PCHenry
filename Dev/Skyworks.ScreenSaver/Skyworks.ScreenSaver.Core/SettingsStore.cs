using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;

namespace Skyworks.ScreenSaver.Core;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };

    public string DirectoryPath { get; }
    public string SettingsPath { get; }

    public SettingsStore(string? directory = null)
    {
        DirectoryPath = Path.GetFullPath(directory ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Skyworks.ScreenSaver"));
        SettingsPath = Path.Combine(DirectoryPath, "settings.json");
    }

    public SaverSettings Load()
    {
        string json;
        try
        {
            json = File.ReadAllText(SettingsPath);
        }
        catch (FileNotFoundException)
        {
            return new();
        }
        catch (DirectoryNotFoundException)
        {
            return new();
        }

        SaverSettings settings;
        bool retiredPaths;
        try
        {
            var document = JsonNode.Parse(json) as JsonObject
                ?? throw new JsonException("Settings must be a JSON object.");
            var retired = document.Select(pair => pair.Key)
                .Where(key => key.Equals("AssetsPath", StringComparison.OrdinalIgnoreCase)
                    || key.Equals("LinksPath", StringComparison.OrdinalIgnoreCase)).ToArray();
            retiredPaths = retired.Length > 0;
            foreach (string key in retired) document.Remove(key);
            settings = document.Deserialize<SaverSettings>(JsonOptions)
                ?? throw new JsonException("Settings must be a JSON object, not null.");
            settings.Validate();
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw new InvalidDataException($"Invalid screensaver settings in '{SettingsPath}': {exception.Message}",
                exception);
        }

        if (retiredPaths) Save(settings);
        return settings;
    }

    public void Save(SaverSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        settings.Validate();
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        Directory.CreateDirectory(DirectoryPath);
        string temporaryPath = Path.Combine(DirectoryPath, $".settings.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }
            // A same-directory rename publishes a complete file, including when replacing existing settings.
            File.Move(temporaryPath, SettingsPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }
}
