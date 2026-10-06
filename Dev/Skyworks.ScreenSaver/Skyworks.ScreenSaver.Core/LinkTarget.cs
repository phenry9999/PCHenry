namespace Skyworks.ScreenSaver.Core;

public sealed class LinkTarget
{
    public Uri Url { get; }
    public bool AllowHttpFallback { get; }
    public string Source { get; }
    public IReadOnlyList<Uri> Attempts { get; }

    public LinkTarget(Uri url, bool allowHttpFallback, string source, bool hasExplicitPort = false)
    {
        ArgumentNullException.ThrowIfNull(url);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        if (!url.IsAbsoluteUri || (url.Scheme != Uri.UriSchemeHttp && url.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("A target must be an absolute HTTP/HTTPS URL.", nameof(url));
        if (allowHttpFallback && url.Scheme != Uri.UriSchemeHttps)
            throw new ArgumentException("HTTP fallback requires an HTTPS-first target.", nameof(allowHttpFallback));
        Url = url;
        AllowHttpFallback = allowHttpFallback;
        Source = source;
        Attempts = allowHttpFallback
            ? Array.AsReadOnly(new[] { url, new UriBuilder(url) { Scheme = Uri.UriSchemeHttp, Port = hasExplicitPort ? url.Port : -1 }.Uri })
            : Array.AsReadOnly(new[] { url });
    }
}

public sealed class LinkLoadResult
{
    public IReadOnlyList<LinkTarget> Targets { get; }
    public IReadOnlyList<Uri> Urls { get; }
    public IReadOnlyList<string> Diagnostics { get; }

    public LinkLoadResult(IEnumerable<LinkTarget> targets, IEnumerable<string> diagnostics)
    {
        var snapshot = targets.ToArray();
        Targets = Array.AsReadOnly(snapshot);
        Urls = Array.AsReadOnly(snapshot.Select(target => target.Url).ToArray());
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }
}
