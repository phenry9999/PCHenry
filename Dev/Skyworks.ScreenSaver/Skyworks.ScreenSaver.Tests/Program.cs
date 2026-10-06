using Skyworks.ScreenSaver.Core;
using System.Text.Json;

var tests = new List<(string Name, Action Body)>();
void Test(string name, Action body) => tests.Add((name, body));
void Equal<T>(T expected, T actual) {
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new Exception($"Expected '{expected}', got '{actual}'.");
}
void Throws<T>(Action body) where T : Exception {
    try { body(); }
    catch (T) { return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}
void WithDirectory(Action<string> body) {
    string path = Path.Combine(AppContext.BaseDirectory, ".test-data", Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(path);
    try { body(path); }
    finally {
        Directory.Delete(path, recursive: true);
        string parent = Path.GetDirectoryName(path)!;
        if (!Directory.EnumerateFileSystemEntries(parent).Any())
            Directory.Delete(parent);
    }
}

Test("Settings defaults", () => {
    var settings = new SaverSettings();
    settings.Validate();
    Equal(2, settings.RefreshMinutes);
    Equal(30, settings.RotationSeconds);
    Equal(45, settings.CaptureTimeoutSeconds);
    Equal(null, typeof(SaverSettings).GetProperty("LinksPath"));
    Equal(null, typeof(SaverSettings).GetProperty("AssetsPath"));
});

foreach (var (name, set, minimum, maximum) in new (string, Action<SaverSettings, int>, int, int)[]
{
    ("RefreshMinutes", (s, v) => s.RefreshMinutes = v, 1, 1440),
    ("RotationSeconds", (s, v) => s.RotationSeconds = v, 5, 3600),
    ("CaptureTimeoutSeconds", (s, v) => s.CaptureTimeoutSeconds = v, 10, 180)
}) {
    foreach (int value in new[] { minimum, maximum })
        Test($"{name} accepts {value}", () => { var s = new SaverSettings(); set(s, value); s.Validate(); });
    foreach (int value in new[] { int.MinValue, minimum - 1, maximum + 1, int.MaxValue })
        Test($"{name} rejects {value}", () => {
            var s = new SaverSettings();
            set(s, value);
            Throws<ArgumentOutOfRangeException>(s.Validate);
        });
}

Test("Default settings store location", () => {
    var store = new SettingsStore();
    Equal(Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Skyworks.ScreenSaver")), store.DirectoryPath);
    Equal(Path.Combine(store.DirectoryPath, "settings.json"), store.SettingsPath);
});

Test("Missing settings return fresh defaults without creating data", () => WithDirectory(root => {
    string missing = Path.Combine(root, "missing");
    var store = new SettingsStore(missing);
    Equal(2, store.Load().RefreshMinutes);
    Equal(false, Directory.Exists(missing));
    Directory.CreateDirectory(missing);
    store.Load().RefreshMinutes = 999;
    Equal(2, store.Load().RefreshMinutes);
    Equal(false, File.Exists(store.SettingsPath));
}));

Test("Settings save, load and replacement round trip", () => WithDirectory(root => {
    var store = new SettingsStore(Path.Combine(root, "settings"));
    var s = new SaverSettings {
        RefreshMinutes = 120, RotationSeconds = 50, CaptureTimeoutSeconds = 90
    };
    store.Save(s);
    SaverSettings actual = new SettingsStore(store.DirectoryPath).Load();
    Equal(s.RefreshMinutes, actual.RefreshMinutes);
    Equal(s.RotationSeconds, actual.RotationSeconds);
    Equal(s.CaptureTimeoutSeconds, actual.CaptureTimeoutSeconds);
    s.RefreshMinutes = 1440;
    store.Save(s);
    Equal(1440, store.Load().RefreshMinutes);
    Equal(1, Directory.GetFiles(store.DirectoryPath).Length);
}));

Test("Invalid save preserves previously saved settings", () => WithDirectory(root => {
    var store = new SettingsStore(root);
    store.Save(new());
    string before = File.ReadAllText(store.SettingsPath);
    Throws<ArgumentException>(() => store.Save(new() { RefreshMinutes = 0 }));
    Throws<ArgumentNullException>(() => store.Save(null!));
    Equal(before, File.ReadAllText(store.SettingsPath));
    Equal(1, Directory.GetFiles(root).Length);
}));

Test("Invalid settings are not rewritten by migration", () => WithDirectory(root => {
    var store = new SettingsStore(root);
    string invalid = "{\"LinksPath\":\"Link.txt\",\"RefreshMinutes\":0}";
    File.WriteAllText(store.SettingsPath, invalid);
    Throws<InvalidDataException>(() => store.Load());
    Equal(invalid, File.ReadAllText(store.SettingsPath));
}));

foreach (string json in new[]
{
    "", "{", "null", "[]", "true", "{\"RefreshMinutes\":\"sixty\"}",
    "{\"RefreshMinutes\":0}", "{\"RotationSeconds\":3601}", "{\"CaptureTimeoutSeconds\":9}",
    "{\"Unknown\":1}",
    "{\"RefreshMinutes\":999999999999999999999999999}"
})
    Test($"Malformed settings report error: {json}", () => WithDirectory(root => {
        var store = new SettingsStore(root);
        File.WriteAllText(store.SettingsPath, json);
        try { store.Load(); }
        catch (InvalidDataException error) {
            if (!error.Message.Contains(store.SettingsPath) || error.InnerException is null)
                throw new Exception("Malformed settings error must include path and cause.");
            Equal(json, File.ReadAllText(store.SettingsPath));
            return;
        }
        throw new Exception("Malformed settings silently accepted.");
    }));

Test("Partial settings keep defaults and accept case-insensitive names", () => WithDirectory(root => {
    var store = new SettingsStore(root);
    File.WriteAllText(store.SettingsPath, "{\"refreshminutes\":20}");
    Equal(20, store.Load().RefreshMinutes);
    Equal(30, store.Load().RotationSeconds);
}));

foreach (string oldAsset in new[] { "\"C:\\\\old-assets\"", "\" \"", "null" })
    Test($"Retired AssetsPath is removed without losing other settings: {oldAsset}", () => WithDirectory(root => {
        var store = new SettingsStore(root);
        File.WriteAllText(store.SettingsPath,
            "{\"RefreshMinutes\":90,\"RotationSeconds\":45,\"LinksPath\":\"custom.json\",\"aSsetsPath\":" + oldAsset + "}");
        var settings = store.Load();
        Equal(90, settings.RefreshMinutes);
        Equal(45, settings.RotationSeconds);
        Equal(null, typeof(SaverSettings).GetProperty("LinksPath"));
        Equal(false, File.ReadAllText(store.SettingsPath).Contains("AssetsPath", StringComparison.OrdinalIgnoreCase));
        Equal(false, File.ReadAllText(store.SettingsPath).Contains("LinksPath", StringComparison.OrdinalIgnoreCase));
        Equal(90, store.Load().RefreshMinutes);
    }));

Test("No arguments selects settings", () => Equal(new LaunchOptions(SaverMode.Settings, 0), LaunchOptions.Parse([])));
foreach (string prefix in new[] { "/", "-" }) {
    foreach (string mode in new[] { "s", "S" })
        Test($"Run switch {prefix}{mode}", () => Equal(new LaunchOptions(SaverMode.Run, 0), LaunchOptions.Parse([prefix + mode])));
    foreach (string mode in new[] { "c", "C", "p", "P" }) {
        SaverMode expected = mode.Equals("c", StringComparison.OrdinalIgnoreCase) ? SaverMode.Settings : SaverMode.Preview;
        foreach (string number in new[] { "123", "0x7b", "0X7B" }) {
            Test($"Switch {prefix}{mode}:{number}", () =>
                Equal(new LaunchOptions(expected, 123), LaunchOptions.Parse([$"{prefix}{mode}:{number}"])));
            Test($"Switch {prefix}{mode} {number}", () =>
                Equal(new LaunchOptions(expected, 123), LaunchOptions.Parse([prefix + mode, number])));
        }
        if (expected == SaverMode.Settings)
            Test($"Settings without handle {prefix}{mode}", () =>
                Equal(new LaunchOptions(expected, 0), LaunchOptions.Parse([prefix + mode])));
    }
}
Test("Settings may have zero parent handle", () => Equal(new LaunchOptions(SaverMode.Settings, 0), LaunchOptions.Parse(["/c:0"])));
Test("Native-sized handle maximum accepted", () =>
    Equal(new LaunchOptions(SaverMode.Preview, nint.MaxValue), LaunchOptions.Parse(["/p", nint.MaxValue.ToString()])));
Test("Null arguments rejected", () => Throws<ArgumentNullException>(() => LaunchOptions.Parse(null!)));
foreach (string[] arguments in new string[][]
{
    [""], [null!], ["s"], ["/"], ["/unknown"], ["/ss"], ["/p"], ["-P"], ["/p:0"], ["/p", "0"],
    ["/p:0x0"], ["/c:"], ["/p:"], ["/c", ""], ["/p", "123", "extra"], ["/s", "123"],
    ["/s:123"], ["/s:"], ["/c:123", "123"], ["/p:123:456"], ["/c", "/s"],
    ["/p", "-1"], ["/p", "+1"], ["/p", " 1"], ["/p", "1.0"], ["/p", "0x"],
    ["/p", "18446744073709551616"], ["/p", "0xffffffffffffffff"], ["/p", null!],
    ["/c", "1", "extra"], ["/S", "/C"]
})
    Test($"Invalid arguments [{string.Join(", ", arguments)}]", () => Throws<ArgumentException>(() => LaunchOptions.Parse(arguments)));

Test("JSON URL array trims entries, preserving order", () => {
    var result = JsonLinks.Deserialize("""[" https://example.com/a ", "http://example.org"]""");
    Equal(2, result.Urls.Count);
    Equal("https://example.com/a", result.Urls[0].AbsoluteUri);
    Equal("http://example.org/", result.Urls[1].AbsoluteUri);
    Equal(0, result.Diagnostics.Count);
});

Test("Invalid JSON URL entries have zero-based index diagnostics", () => {
    string?[] entries = [null, "", " ", "relative/path", "# comment", "; comment", "ftp://example.com",
        "https://user:password@example.com", "http://", "file:///data", "https:///missinghost", "https://"];
    var result = JsonLinks.Deserialize(JsonSerializer.Serialize(entries));
    Equal(0, result.Urls.Count);
    Equal(entries.Length, result.Diagnostics.Count);
    for (int i = 0; i < entries.Length; i++)
        Equal(true, result.Diagnostics[i].StartsWith($"Index {i}:"));
    Equal(false, result.Diagnostics.Any(d => d.Contains("password")));
});

Test("Duplicate normalized URLs are skipped", () => {
    var result = JsonLinks.Deserialize(JsonSerializer.Serialize(new[] {
        "https://EXAMPLE.com", "https://example.com:443/", "https://example.com/a",
        "https://example.com/A", "http://example.com", "https://example.com/a?q=1" }));
    Equal(5, result.Urls.Count);
    Equal(1, result.Diagnostics.Count);
    Equal(true, result.Diagnostics[0].StartsWith("Index 1:"));
});

Test("HTTP URLs with IPv6, ports, fragments and queries are valid", () => {
    var result = JsonLinks.Deserialize("""["HTTP://localhost:8080/a?q=1#section", "https://[::1]/"]""");
    Equal(2, result.Urls.Count);
    Equal(0, result.Diagnostics.Count);
});
Test("Scheme-less host URLs preserve fallback policy and ports", () => {
    var result = JsonLinks.Deserialize("""["example.com", "www.example.com/path?x=1", "example.com:8080/path", "//example.org/news", "http://example.net/a", "https://secure.example.org/"]""");
    Equal(6, result.Targets.Count);
    Equal(0, result.Diagnostics.Count);
    foreach (int index in new[] { 0, 1, 2, 3 })
    {
        Equal(true, result.Targets[index].AllowHttpFallback);
        Equal(2, result.Targets[index].Attempts.Count);
        Equal("https", result.Targets[index].Attempts[0].Scheme);
        Equal("http", result.Targets[index].Attempts[1].Scheme);
    }
    Equal(8080, result.Targets[2].Attempts[0].Port);
    Equal(8080, result.Targets[2].Attempts[1].Port);
    Equal(false, result.Targets[4].AllowHttpFallback);
    Equal(false, result.Targets[5].AllowHttpFallback);
});
Test("Explicit HTTPS takes precedence over equivalent scheme-less duplicate", () => {
    var result = JsonLinks.Deserialize("""["example.com/path", "https://example.com/path"]""");
    Equal(1, result.Targets.Count);
    Equal(false, result.Targets[0].AllowHttpFallback);
});
Test("Scheme-less local/relative paths and non-HTTP schemes are rejected", () => {
    var result = JsonLinks.Deserialize("""["relative/path", "C:\\images\\background.jpg", "file:///tmp/x", "ftp://example.com", "mailto:a@example.com", "data:text/plain,x"]""");
    Equal(0, result.Targets.Count);
    Equal(6, result.Diagnostics.Count);
});
Test("Empty JSON array is legitimate", () => {
    var result = JsonLinks.Deserialize("[]");
    Equal(0, result.Urls.Count);
    Equal(0, result.Diagnostics.Count);
});
Test("Null JSON argument rejected", () => Throws<ArgumentNullException>(() => JsonLinks.Deserialize(null!)));

Test("Valid entries survive invalid entries in their original order", () => {
    var result = JsonLinks.Deserialize("""["https://example.com/a", null, "relative/path", "http://example.org/b", ""]""");
    Equal(2, result.Urls.Count);
    Equal("https://example.com/a", result.Urls[0].AbsoluteUri);
    Equal("http://example.org/b", result.Urls[1].AbsoluteUri);
    Equal(3, result.Diagnostics.Count);
    Equal(true, result.Diagnostics[0].StartsWith("Index 1:"));
    Equal(true, result.Diagnostics[1].StartsWith("Index 2:"));
    Equal(true, result.Diagnostics[2].StartsWith("Index 4:"));
});

foreach (string json in new[] {
    "", " ", "[", "{", "null", "{}", """{"urls":["https://example.com"]}""",
    "123", "true", "\"https://example.com\"", "[123]", "[true]", "[{}]", "[[]]",
    """["https://example.com", 123]""", """["https://example.com", {}]""",
    """["https://example.com",]""", """["https://example.com"] trailing""",
    "# comment\nhttps://example.com"
})
    Test($"Malformed JSON links fail explicitly: {json}", () => {
        try { JsonLinks.Deserialize(json); }
        catch (InvalidDataException error) {
            if (error.InnerException is not JsonException cause)
                throw new Exception("Expected JsonException as the cause.");
            Equal(true, error.Message.Contains(cause.Message));
            Equal(true, cause.Path is not null);
            Equal(true, error.Message.Contains(cause.Path!));
            Equal(true, cause.LineNumber.HasValue);
            Equal(true, cause.BytePositionInLine.HasValue);
            return;
        }
        throw new Exception("Malformed JSON links silently accepted.");
    });

Test("Mixed element failure includes array index and JSON line information", () => {
    try { JsonLinks.Deserialize("[\n  \"https://example.com\",\n  42\n]"); }
    catch (InvalidDataException error) when (error.InnerException is JsonException cause) {
        Equal("$[1]", cause.Path);
        Equal(2L, cause.LineNumber);
        Equal(true, cause.BytePositionInLine.HasValue);
        return;
    }
    throw new Exception("Expected a JSON conversion error for numeric entry.");
});

Test("JSON serialization preserves URL data and URI order", () => {
    string[] source = [
        "https://example.com/path?q=one&other=two#section",
        "https://example.org/quoted?value=\"hello\"",
        "http://localhost:8080/ü",
        "https://[::1]/a"
    ];
    string json = JsonSerializer.Serialize(source);
    Equal(true, source.SequenceEqual(JsonSerializer.Deserialize<string[]>(json)!));
    var result = JsonLinks.Deserialize(json);
    Equal(0, result.Diagnostics.Count);
    Equal(true, source.Select(value => new Uri(value)).SequenceEqual(result.Urls));
    string roundTrip = JsonSerializer.Serialize(result.Urls.Select(uri => uri.AbsoluteUri).ToArray());
    Equal(true, result.Urls.SequenceEqual(JsonLinks.Deserialize(roundTrip).Urls));
});

Test("JSON links file round trip leaves source data intact", () => WithDirectory(root => {
    string path = Path.Combine(root, "Links.json");
    string json = JsonSerializer.Serialize(new[] { "https://example.com/a", "http://example.org/b" });
    File.WriteAllText(path, json);
    var result = JsonLinks.Deserialize(File.ReadAllText(path));
    Equal(2, result.Urls.Count);
    Equal(0, result.Diagnostics.Count);
    Equal(json, File.ReadAllText(path));
}));

foreach (int count in new[] { 0, -1, int.MinValue })
    Test($"Invalid monitor count {count}", () => Throws<ArgumentOutOfRangeException>(() => new MonitorRotation(count)));
foreach (int count in new[] { 1, 2, 5 })
    Test($"Monitor rotation cycles {count} monitors", () => {
        var rotation = new MonitorRotation(count);
        for (int i = 0; i < count * 10; i++)
            Equal(i % count, rotation.Next());
    });
Test("Independent rotation instances", () => {
    var first = new MonitorRotation(2);
    var second = new MonitorRotation(2);
    Equal(0, first.Next());
    Equal(1, first.Next());
    Equal(0, second.Next());
});
Test("Maximum monitor count starts without overflow", () => {
    var rotation = new MonitorRotation(int.MaxValue);
    Equal(0, rotation.Next());
    Equal(1, rotation.Next());
});

foreach (int size in new[] { 0, 1, 2, 7, 25, 101 }) {
    foreach (int seed in new[] { 0, 17, 321 }) {
        Test($"Shuffled cycles preserve every item once: size {size}, seed {seed}", () => {
            int[] source = Enumerable.Range(0, size).ToArray();
            var cycle = new ShuffledCycle<int>(new Random(seed));
            cycle.Replace(source);
            Equal(size, cycle.Count);
            for (int round = 0; round < 10; round++) {
                var shown = new HashSet<int>();
                for (int item = 0; item < size; item++) {
                    Equal(true, cycle.TryNext(out int next));
                    Equal(true, shown.Add(next));
                }
                Equal(true, shown.SetEquals(source));
            }
            Equal(size != 0, cycle.TryNext(out _));
        });
    }
}
Test("Shuffle uses a private snapshot and removes duplicate entries", () => {
    var source = new List<string> { "A", "B", "a" };
    var cycle = new ShuffledCycle<string>(new Random(7), StringComparer.OrdinalIgnoreCase);
    cycle.Replace(source);
    source.Clear();
    Equal(2, cycle.Count);
    var shown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < 2; i++) { Equal(true, cycle.TryNext(out string? value)); Equal(true, shown.Add(value!)); }
    Equal(true, shown.SetEquals(new[] { "A", "B" }));
    cycle.Replace([]);
    Equal(false, cycle.TryNext(out _));
});
Test("Seeded presentation is randomized and reshuffled on the next cycle", () => {
    var cycle = new ShuffledCycle<int>(new Random(42));
    int[] source = Enumerable.Range(0, 12).ToArray();
    cycle.Replace(source);
    int[] first = new int[12], second = new int[12];
    for (int i = 0; i < 12; i++) { Equal(true, cycle.TryNext(out first[i])); }
    for (int i = 0; i < 12; i++) { Equal(true, cycle.TryNext(out second[i])); }
    Equal(false, first.SequenceEqual(source));
    Equal(false, first.SequenceEqual(second));
});
Test("Removing a broken image preserves the remainder without early repeats", () => {
    var cycle = new ShuffledCycle<int>(new Random(13));
    cycle.Replace(Enumerable.Range(0, 8));
    Equal(true, cycle.TryNext(out int first));
    int broken = cycle.Items.First(value => value != first);
    cycle.Remove(broken);
    var shown = new HashSet<int> { first };
    for (int i = 0; i < 6; i++) { Equal(true, cycle.TryNext(out int value)); Equal(true, shown.Add(value)); }
    Equal(true, shown.SetEquals(Enumerable.Range(0, 8).Where(value => value != broken)));
    Equal(7, cycle.Count);
});
Test("Refresh snapshot replaces stale pages without resetting monitor rotation", () => {
    var cycle = new ShuffledCycle<string>(new Random(11));
    var monitors = new MonitorRotation(3);
    cycle.Replace(new[] { "old-valid-a", "old-valid-b" });
    for (int i = 0; i < 2; i++) { Equal(true, cycle.TryNext(out _)); Equal(i, monitors.Next()); }
    cycle.Replace(new[] { "successful-a", "successful-b", "successful-c", "successful-d" });
    var shown = new HashSet<string>();
    for (int i = 0; i < 4; i++) {
        Equal(true, cycle.TryNext(out string? page));
        Equal(true, shown.Add(page!));
        Equal((i + 2) % 3, monitors.Next());
    }
    Equal(true, shown.SetEquals(new[] { "successful-a", "successful-b", "successful-c", "successful-d" }));
    cycle.Replace([]);
    Equal(false, cycle.TryNext(out _));
    Equal(0, monitors.Next());
});
Test("Null shuffle source is rejected", () => Throws<ArgumentNullException>(() => new ShuffledCycle<int>().Replace(null!)));

int failures = 0;
foreach (var (name, body) in tests) {
    try { body(); Console.WriteLine($"PASS {name}"); }
    catch (Exception error) {
        failures++;
        Console.Error.WriteLine($"FAIL {name}: {error}");
    }
}
Console.WriteLine($"{tests.Count - failures}/{tests.Count} tests passed.");
return failures == 0 ? 0 : 1;
