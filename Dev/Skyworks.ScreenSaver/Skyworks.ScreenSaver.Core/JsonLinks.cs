using System.Text.Json;
using System.Text.RegularExpressions;

namespace Skyworks.ScreenSaver.Core;

public static class JsonLinks
{
    public static LinkLoadResult Deserialize(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        string?[] entries;
        try
        {
            entries = JsonSerializer.Deserialize<string?[]>(json)
                ?? throw new JsonException("Links must be a JSON string array, not null.", "$", 0, 0);
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException(
                $"Invalid links JSON at {exception.Path}, line {exception.LineNumber}, byte {exception.BytePositionInLine}: {exception.Message}",
                exception);
        }

        var targets = new List<LinkTarget>();
        var diagnostics = new List<string>();
        var seen = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int index = 0; index < entries.Length; index++)
        {
            string? text = entries[index]?.Trim();
            if (!TryNormalize(text, index, diagnostics, out Uri? uri, out bool allowFallback, out bool explicitPort) || uri is null)
            {
                continue;
            }
            string key = uri.AbsoluteUri;
            if (seen.TryGetValue(key, out int prior))
            {
                if (!allowFallback && targets[prior].AllowHttpFallback)
                    targets[prior] = new LinkTarget(uri, false, text!, explicitPort);
                else diagnostics.Add($"Index {index}: duplicate normalized URL skipped.");
                continue;
            }
            seen.Add(key, targets.Count);
            targets.Add(new LinkTarget(uri, allowFallback, text!, explicitPort));
        }
        return new LinkLoadResult(targets, diagnostics);
    }

    private static bool TryNormalize(string? text, int index, List<string> diagnostics,
        out Uri? uri, out bool allowFallback, out bool explicitPort)
    {
        uri = null;
        allowFallback = false;
        explicitPort = false;
        bool requiresPublicHost = false;
        if (string.IsNullOrWhiteSpace(text))
        {
            diagnostics.Add($"Index {index}: URL entry is empty.");
            return false;
        }

        string candidate = text;
        if (candidate.StartsWith("//", StringComparison.Ordinal))
        {
            allowFallback = true;
            requiresPublicHost = true;
            candidate = "https:" + candidate;
            explicitPort = TryGetExplicitPort(candidate);
        }
        else
        {
            Match scheme = Regex.Match(candidate, @"^([A-Za-z][A-Za-z0-9+.-]*):");
            bool hostPort = Regex.IsMatch(candidate, @"^[^/\\?#:]+:\d+(?:[/\\?#]|$)");
            if (scheme.Success && !hostPort)
            {
                if (!candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !candidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    diagnostics.Add($"Index {index}: unsupported URL scheme '{scheme.Groups[1].Value}'; only HTTP and HTTPS are allowed.");
                    return false;
                }
                explicitPort = TryGetExplicitPort(candidate);
            }
            else
            {
                if (candidate.StartsWith("/", StringComparison.Ordinal) ||
                    candidate.StartsWith(@"\", StringComparison.Ordinal) ||
                    candidate.Contains('\\') || candidate.Any(char.IsControl))
                {
                    diagnostics.Add($"Index {index}: local paths and malformed URL values are not allowed.");
                    return false;
                }
                allowFallback = true;
                requiresPublicHost = true;
                candidate = "https://" + candidate;
                explicitPort = TryGetExplicitPort(candidate);
            }
        }

        if (!Uri.TryCreate(candidate, UriKind.Absolute, out Uri? parsed) || parsed is null ||
            (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrWhiteSpace(parsed.Host) || !string.IsNullOrEmpty(parsed.UserInfo) ||
            (requiresPublicHost && parsed.HostNameType == UriHostNameType.Dns &&
                !parsed.Host.Contains('.') && !parsed.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase)))
        {
            diagnostics.Add($"Index {index}: expected a valid HTTP/HTTPS URL with a host and no user information.");
            uri = null;
            return false;
        }
        uri = parsed;
        return true;
    }

    private static bool TryGetExplicitPort(string value)
    {
        int authorityStart = value.IndexOf("://", StringComparison.Ordinal);
        if (authorityStart < 0) return false;
        string authority = value[(authorityStart + 3)..].Split(['/', '?', '#'], 2)[0];
        int colon = authority.LastIndexOf(':');
        return colon >= 0 && (authority.IndexOf(']') < colon || !authority.StartsWith('['))
            && int.TryParse(authority[(colon + 1)..], out int port) && port is >= 0 and <= 65535;
    }
}
