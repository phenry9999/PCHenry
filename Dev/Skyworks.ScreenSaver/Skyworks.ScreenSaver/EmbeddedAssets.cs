namespace Skyworks.ScreenSaver;

internal static class EmbeddedAssets
{
    private const string ResourcePrefix = "Skyworks.ScreenSaver.Assets.";
    private const string ImagePrefix = "embedded:";

    internal static IReadOnlyList<string> Images { get; } = typeof(EmbeddedAssets).Assembly
        .GetManifestResourceNames()
        .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
        .Order(StringComparer.Ordinal)
        .Select(name => ImagePrefix + name)
        .ToArray();

    internal static Skyworks.ScreenSaver.Core.LinkLoadResult LoadLinks()
    {
        using var stream = typeof(EmbeddedAssets).Assembly.GetManifestResourceStream("Skyworks.ScreenSaver.Links.json")
            ?? throw new FileNotFoundException("The compiled Assets\\Links.json resource is missing.");
        using var reader = new StreamReader(stream);
        return Skyworks.ScreenSaver.Core.JsonLinks.Deserialize(reader.ReadToEnd());
    }

    internal static Stream OpenImage(string path) =>
        path.StartsWith(ImagePrefix, StringComparison.Ordinal)
            ? typeof(EmbeddedAssets).Assembly.GetManifestResourceStream(path[ImagePrefix.Length..])
                ?? throw new FileNotFoundException($"Embedded background is missing: {path}")
            : File.OpenRead(path);
}
