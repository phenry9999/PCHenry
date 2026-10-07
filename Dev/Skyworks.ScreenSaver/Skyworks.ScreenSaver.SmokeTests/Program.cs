using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Skyworks.ScreenSaver;
using Skyworks.ScreenSaver.Core;

internal static class Program
{
    private static readonly Color Pink = Color.FromArgb(240, 40, 160);
    private static readonly Color Cyan = Color.FromArgb(20, 210, 220);
    private static readonly Color Yellow = Color.FromArgb(240, 220, 30);
    private static int failures;
    private static string artifacts = "";
    private static DiagnosticLog log = null!;
    private static Exception? expectedThreadException;
    private static TaskCompletionSource<Exception>? threadExceptionObserved;

    [STAThread]
    private static int Main(string[] args)
    {
        artifacts = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Skyworks.ScreenSaver.SmokeTests", DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(artifacts);
        Environment.SetEnvironmentVariable("TEMP", artifacts);
        Environment.SetEnvironmentVariable("TMP", artifacts);
        Require(Path.GetTempPath().StartsWith(artifacts, StringComparison.OrdinalIgnoreCase), "Test process temporary files must be isolated.");
        log = new DiagnosticLog(artifacts);
        Console.WriteLine($"Windows GUI/WebView2 smoke tests. Artifacts: {artifacts}");
        using var watchdog = new System.Threading.Timer(_ =>
        {
            Console.Error.WriteLine("FAIL global watchdog: runner exceeded 115 seconds.");
            Environment.Exit(2);
        }, null, TimeSpan.FromSeconds(115), Timeout.InfiniteTimeSpan);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) =>
        {
            log.Write("Smoke runner UI thread exception.", e.Exception);
            Exception exception = e.Exception is TargetInvocationException { InnerException: Exception inner }
                ? inner : e.Exception;
            if (ReferenceEquals(exception, expectedThreadException))
            {
                threadExceptionObserved!.TrySetResult(exception);
                return;
            }
            failures++;
            Console.Error.WriteLine($"FAIL unhandled UI thread exception: {e.Exception}");
            Application.ExitThread();
        };
        using var host = new Form
        {
            Text = "Skyworks focused smoke test host",
            StartPosition = FormStartPosition.CenterScreen,
            ClientSize = new Size(420, 280),
            TopMost = false
        };
        host.Shown += async (_, _) =>
        {
            try
            {
                if (args.SequenceEqual(new[] { "--actual-settings-only" }))
                {
                    await Test("actual migrated installed JSON and settings Save/Load (no registration)", () => ActualSettingsAsync(host));
                    return;
                }
                if (args.SequenceEqual(new[] { "--overlay-only" }))
                {
                    await Test("adaptive version overlay contrast and 50 percent larger font", VersionOverlayAsync);
                    return;
                }
                if (args.SequenceEqual(new[] { "--transition-only" }))
                {
                    await Test("two-second per-monitor image crossfade", ImageTransitionAsync);
                    return;
                }
                if (args.SequenceEqual(new[] { "--assets-only" }))
                {
                    await Test("shuffled startup Assets, complete cycles and preview lifetime", PreviewAsync);
                    return;
                }
                if (args.SequenceEqual(new[] { "--embedded-only" }))
                {
                    await Test("embedded backgrounds match source bytes and render without external files", EmbeddedAssetsAsync);
                    await Test("embedded Links.json preserves source data and fallback policy", EmbeddedLinksAsync);
                    await Test("settings no longer expose Assets or URL paths", () => SettingsAsync(host));
                    await Test("installation does not require external Assets or URL data", RegistrationHelpersAsync);
                    await Test("embedded startup Assets shuffle and preview lifetime", PreviewAsync);
                    await Test("refresh still stages and shuffles only successful main HTTP 200 captures", RefreshPresentationAsync);
                    await Test("published .scr preview without external Assets or Links.json", PublishedPreviewAsync);
                    return;
                }
                await Test("UI thread exception handler logs without modal dialogs", () => ThreadExceptionAsync(host));
                await Test("version file parsing, assembly metadata and publish packaging", VersionMetadataAsync);
                await Test("settings GUI omits background and URL data paths", () => SettingsAsync(host));
                await Test("registration helpers copy/preserve deployment and prepare shell command (no install)", RegistrationHelpersAsync);
                await Test("static image rendering, unlocked file, negative-coordinate bounds", StaticImageAsync);
                await Test("contrasting version overlay placement, letterboxing, DPI and preview sizing", VersionOverlayAsync);
                await Test("real WebView2 capture, status isolation, redirect, cancellation, timeout", () => CaptureAsync(host));
                await Test("scheme-less HTTPS-first HTTP fallback, explicit HTTPS policy and final diagnostics", FallbackAsync);
                await Test("completed refresh publishes only successful pages after loading, not request order", RefreshPresentationAsync);
                await Test("embedded preview, shuffled webpage cycles, Assets fallback, input and shutdown", PreviewAsync);
                await Test("published renamed .scr cross-process /p embedding and lifetime", PublishedPreviewAsync);
            }
            catch (Exception ex)
            {
                failures++;
                Console.Error.WriteLine($"FAIL runner: {ex}");
            }
            finally { host.Close(); }
        };
        Application.Run(host);
        Console.WriteLine($"RESULT: {(failures == 0 ? "PASS" : "FAIL")} ({failures} failed groups). Diagnostics: {log.FilePath}");
        Console.WriteLine("Coverage excludes full /s on the desktop, real multi-monitor layout, and mixed-DPI transitions.");
        return failures == 0 ? 0 : 1;
    }

    private static async Task ThreadExceptionAsync(Form host)
    {
        expectedThreadException = new InvalidOperationException("Expected smoke runner UI exception sentinel.");
        threadExceptionObserved = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        try
        {
            host.BeginInvoke((Action)(() => throw expectedThreadException!));
            await threadExceptionObserved.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Require(File.ReadAllText(log.FilePath).Contains("Expected smoke runner UI exception sentinel."),
                "UI exception handler did not persist diagnostics.");
        }
        finally
        {
            expectedThreadException = null;
            threadExceptionObserved = null;
        }
    }

    private static async Task VersionMetadataAsync()
    {
        string project = Path.GetFullPath(Path.Combine("Skyworks.ScreenSaver", "Skyworks.ScreenSaver.csproj"));
        string prefix = XDocument.Load(project).Descendants("VersionPrefix").Single().Value;
        string revisionFile = Path.Combine(AppContext.BaseDirectory, "build-revision.txt");
        Require(File.Exists(revisionFile), "Built deployment lacks its revision file.");
        string revision = File.ReadAllText(revisionFile).Trim();
        Require(int.TryParse(revision, out int number) && number is >= 0 and <= 65534, "Invalid packaged revision.");
        var expected = Version.Parse($"{prefix}.{number}");
        Require(AppVersion.Number == expected && AppVersion.DisplayText == $"Version: {expected.ToString(4)}",
            "Displayed version differs from the version file and project prefix.");
        var assembly = typeof(AppVersion).Assembly;
        Require(assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()!.Version == expected.ToString(4)
            && assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion == expected.ToString(4),
            "Assembly/file/product versions do not agree.");
        string publishedRevision = File.ReadAllText(Path.Combine("publish", "build-revision.txt")).Trim();
        Require(int.TryParse(publishedRevision, out int publishedNumber) && publishedNumber is >= 0 and <= 65534,
            "Invalid published revision.");
        var publishedVersion = Version.Parse($"{prefix}.{publishedNumber}");
        foreach (var (path, version) in new[]
        {
            (assembly.Location, expected.ToString(4)),
            (Path.GetFullPath(Path.Combine("publish", "Skyworks.ScreenSaver.scr")), publishedVersion.ToString(4))
        })
        {
            var information = FileVersionInfo.GetVersionInfo(path);
            Require(information.FileVersion == version && information.ProductVersion == version,
                $"Version metadata was not propagated to {path}");
        }
        Require(FileVersionInfo.GetVersionInfo(Path.GetFullPath(Path.Combine("publish", "Skyworks.ScreenSaver.scr"))).FileVersion
            == publishedVersion.ToString(4), "Published revision file differs from the packaged screensaver version.");
        foreach (var (text, valid) in new (string?, bool)[] { ("0", true), ("4", true), ("00004", true), ("65534", true),
                     ("", false), ("wrong", false), ("-1", false), ("65535", false), ("4\n5", false), (null, false) })
        {
            string directory = Path.Combine(artifacts, "version-parser", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string file = Path.Combine(directory, "revision.txt");
            if (text is not null) File.WriteAllText(file, text);
            var start = new ProcessStartInfo("dotnet") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            foreach (string argument in new[] { "msbuild", project, "-target:GenerateBuildVersion", $"-property:BuildRevisionFile={file}",
                         $"-property:IntermediateOutputPath={directory}{Path.DirectorySeparatorChar}", "-getProperty:Version", "-verbosity:quiet" })
                start.ArgumentList.Add(argument);
            using var process = Process.Start(start)!;
            Task<string> output = process.StandardOutput.ReadToEndAsync(), error = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(12));
            string diagnostics = await output + await error;
            Require((process.ExitCode == 0) == valid, $"Version parser accepted/rejected wrong fixture '{text}': {diagnostics}");
            if (valid) Require(diagnostics.Trim() == $"{prefix}.{int.Parse(text!)}", "Version parser lost canonical numeric revision.");
            else Require(diagnostics.Contains("revision", StringComparison.OrdinalIgnoreCase), "Invalid/missing revision lacked an explicit diagnostic.");
        }
    }

    private static Task VersionOverlayAsync()
    {
        foreach (Color background in new[] { Color.Black, Color.White, Color.Transparent })
        {
            using var source = new Bitmap(800, 600);
            using (var fill = Graphics.FromImage(source))
            {
                fill.Clear(background == Color.Transparent ? Color.Black : background);
                if (background == Color.Transparent)
                    for (int x = 0; x < 800; x += 16) fill.FillRectangle(Brushes.White, x, 0, 8, 600);
            }
            using var rendered = new Bitmap(800, 600);
            RectangleF layout;
            using (var graphics = Graphics.FromImage(rendered))
            {
                graphics.DrawImageUnscaled(source, 0, 0);
                layout = VersionOverlay.Draw(graphics, new Rectangle(0, 0, 800, 600), 96, source);
            }
            Color expected = background == Color.Black ? Color.White : Color.Black;
            Require(background == Color.Transparent || VersionOverlay.SelectTextColor(source, new Rectangle(0, 0, 800, 600), layout) == expected,
                "Version contrast did not adapt to the actual image region.");
            Require(layout.Height >= 21 && layout.Right <= 792 && layout.Bottom <= 592,
                "21px version font or bottom-right inset is incorrect.");
            bool glyphFound = false;
            for (int y = (int)layout.Top; y < Math.Min(600, (int)Math.Ceiling(layout.Bottom)); y++)
                for (int x = (int)layout.Left; x < Math.Min(800, (int)Math.Ceiling(layout.Right)); x++)
                    glyphFound |= rendered.GetPixel(x, y).ToArgb() == expected.ToArgb();
            Require(glyphFound, "Contrasting version glyphs were not painted.");
            if (background == Color.Transparent)
                Require(VersionOverlay.SelectTextColor(source, new Rectangle(0, 0, 800, 600), layout) is var chosen &&
                    (chosen == Color.Black || chosen == Color.White), "Mixed background selected a noncontrasting grey foreground.");
        }
        using (var source = new Bitmap(800, 600))
        using (var rendered = new Bitmap(800, 600))
        using (var graphics = Graphics.FromImage(rendered))
        {
            graphics.Clear(Color.White);
            RectangleF layout = VersionOverlay.Draw(graphics, new Rectangle(0, 0, 800, 600), 96);
            using (var fill = Graphics.FromImage(source))
            using (var darkPatch = new SolidBrush(Color.FromArgb(90, 90, 90)))
            {
                fill.Clear(Color.White);
                fill.FillRectangle(darkPatch, layout.Left, layout.Top, layout.Width * 0.75f, layout.Height);
            }
            Require(VersionOverlay.SelectTextColor(source, new Rectangle(0, 0, 800, 600), layout) == Color.White,
                "A bright minority overwhelmed contrast on the predominantly dark label region.");
            graphics.DrawImageUnscaled(source, 0, 0);
            VersionOverlay.Draw(graphics, new Rectangle(0, 0, 800, 600), 96, source);
            int darkHaloPixels = 0, whiteGlyphPixels = 0;
            for (int y = (int)layout.Top; y < (int)layout.Bottom; y++)
                for (int x = (int)layout.Left; x < (int)(layout.Left + layout.Width * 0.7f); x++)
                {
                    Color pixel = rendered.GetPixel(x, y);
                    if (pixel.ToArgb() == Color.Black.ToArgb()) darkHaloPixels++;
                    if (pixel.ToArgb() == Color.White.ToArgb()) whiteGlyphPixels++;
                }
            Require(darkHaloPixels > 100 && whiteGlyphPixels > 100,
                "Mixed-background version text lacks a solid contrasting foreground and strong opposite halo.");
        }
        foreach (var (size, dpi) in new[] { (new Size(800, 600), 96), (new Size(800, 600), 192), (new Size(144, 96), 144), (new Size(64, 48), 192) })
        {
            using var surface = new Bitmap(size.Width, size.Height);
            var imageBounds = new Rectangle(0, size.Height / 8, size.Width, size.Height * 3 / 4);
            RectangleF text;
            using (var graphics = Graphics.FromImage(surface))
            {
                graphics.Clear(Color.Black);
                using var imageBrush = new SolidBrush(Cyan);
                graphics.FillRectangle(imageBrush, imageBounds);
                using var source = new Bitmap(size.Width, size.Height);
                using (var fill = Graphics.FromImage(source)) fill.Clear(Cyan);
                text = VersionOverlay.Draw(graphics, imageBounds, dpi, source);
            }
            Require(imageBounds.Contains(Rectangle.Ceiling(text)), $"Version text exceeded the fitted image at {size}/{dpi} DPI: {text}");
            VerifyOverlayPixels(surface, imageBounds, dpi, text);
        }
        string asset = MakeImage("overlay-wide-asset.png", Cyan, new Size(1600, 900));
        VerifyImageWindowOverlay(asset, new Size(800, 600), new Rectangle(0, 75, 800, 450));
        return Task.CompletedTask;
    }

    private static Task EmbeddedAssetsAsync()
    {
        string[] sources = Directory.GetFiles(Path.GetFullPath("Assets"), "*", SearchOption.AllDirectories)
            .Where(path => new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" }
                .Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase)).ToArray();
        Require(sources.Length == EmbeddedAssets.Images.Count && sources.Length > 0,
            "The compiled background count differs from the project's source images.");
        foreach (string path in EmbeddedAssets.Images)
        {
            using var stream = EmbeddedAssets.OpenImage(path);
            byte[] hash = System.Security.Cryptography.SHA256.HashData(stream);
            Require(sources.Any(source => System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(source)).SequenceEqual(hash)),
                $"Embedded image does not match any original source: {path}");
            using var window = new ImageWindow(new Rectangle(0, 0, 800, 600), true);
            window.ShowImage(path);
            using var rendered = new Bitmap(800, 600);
            window.DrawToBitmap(rendered, new Rectangle(0, 0, 800, 600));
            VerifyEmbeddedCenter(rendered, path);
        }
        Console.WriteLine($"  {sources.Length} original images embedded and rendered; no Assets setting or external folder required.");
        return Task.CompletedTask;
    }

    private static Task EmbeddedLinksAsync()
    {
        string source = File.ReadAllText(Path.Combine("Assets", "Links.json"));
        using var stream = typeof(AppVersion).Assembly.GetManifestResourceStream("Skyworks.ScreenSaver.Links.json")
            ?? throw new InvalidOperationException("Embedded Links.json is missing.");
        using var reader = new StreamReader(stream);
        Require(reader.ReadToEnd() == source, "Embedded Links.json differs from the original source data.");
        var expected = JsonLinks.Deserialize(source);
        var actual = EmbeddedAssets.LoadLinks();
        Require(actual.Targets.Select(target => (target.Source, target.Url, target.AllowHttpFallback))
            .SequenceEqual(expected.Targets.Select(target => (target.Source, target.Url, target.AllowHttpFallback))),
            "Embedded JSON deserialization changed URL order or scheme-less fallback policy.");
        Require(actual.Targets.Count == 25 && !File.Exists("Links.json"), "Canonical Assets\\Links.json was not moved intact or scheme-less URLs were dropped.");
        Console.WriteLine($"  {actual.Targets.Count} URLs embedded in their original order.");
        return Task.CompletedTask;
    }

    private static void VerifyEmbeddedCenter(Bitmap rendered, string path)
    {
        using var stream = EmbeddedAssets.OpenImage(path);
        using var original = new Bitmap(stream);
        using var expected = new Bitmap(rendered.Width, rendered.Height);
        using (var graphics = Graphics.FromImage(expected))
        {
            graphics.Clear(Color.Black);
            double scale = Math.Min((double)rendered.Width / original.Width, (double)rendered.Height / original.Height);
            int width = (int)(original.Width * scale), height = (int)(original.Height * scale);
            graphics.DrawImage(original, new Rectangle((rendered.Width - width) / 2, (rendered.Height - height) / 2, width, height));
        }
        Require(Near(rendered.GetPixel(rendered.Width / 2, rendered.Height / 2),
            expected.GetPixel(rendered.Width / 2, rendered.Height / 2)),
            $"Embedded background did not render from its resource: {path}");
    }

    private static void VerifyImageWindowOverlay(string path, Size size, Rectangle imageBounds)
    {
        byte[] original = File.ReadAllBytes(path);
        using var window = new ImageWindow(new Rectangle(Point.Empty, size), true);
        window.ShowImage(path);
        using var surface = new Bitmap(size.Width, size.Height);
        window.DrawToBitmap(surface, new Rectangle(Point.Empty, size));
        VerifyOverlayPixels(surface, imageBounds, window.DeviceDpi, null);
        Require(original.SequenceEqual(File.ReadAllBytes(path)), "Painting a version changed the original image/screenshot file.");
    }

    private static void VerifyOverlayPixels(Bitmap surface, Rectangle imageBounds, int dpi, RectangleF? textBounds)
    {
        var grey = new List<Point>();
        for (int y = imageBounds.Top; y < imageBounds.Bottom; y++)
            for (int x = imageBounds.Left; x < imageBounds.Right; x++)
                if (surface.GetPixel(x, y).ToArgb() == Color.White.ToArgb() || surface.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                    grey.Add(new Point(x, y));
        Require(grey.Count > 0, "Contrasting version glyphs/halo were not rendered.");
        Require(grey.All(imageBounds.Contains), "Version text was drawn into the letterbox/outside the image.");
        if (textBounds is not null)
            Require(grey.All(point => RectangleF.Inflate(textBounds.Value, 2, 2).Contains(point)), "Version pixels escaped the measured text layout.");
        Require((imageBounds.Width < 300 || grey.Min(point => point.X) > imageBounds.Left + imageBounds.Width / 3)
            && grey.Min(point => point.Y) > imageBounds.Bottom - Math.Max(40, (int)(50 * dpi / 96f)),
            "Version glyphs are not in the bottom-right image corner.");
        Require(surface.GetPixel(imageBounds.Left + imageBounds.Width / 2, imageBounds.Top + imageBounds.Height / 2).ToArgb() != Color.Gray.ToArgb(),
            "Version overlay added an unwanted central panel.");
    }

    private static async Task ActualSettingsAsync(Form host)
    {
        var store = new SettingsStore();
        var settings = store.Load();
        var parsed = EmbeddedAssets.LoadLinks();
        Require(parsed.Urls.Count > 0, "Actual authoritative JSON list did not deserialize to usable URLs.");
        using var dialog = new SettingsDialog(settings, store, log, null);
        dialog.Show(host);
        await Task.Delay(50);
        Require(typeof(SettingsDialog).GetField("links", BindingFlags.NonPublic | BindingFlags.Instance) is null,
            "Settings still exposes a URL path.");
        ((Button)dialog.AcceptButton!).PerformClick();
        Require(!dialog.Visible, "Actual settings Save did not succeed.");
        var reopened = store.Load();
        Require(reopened.RefreshSeconds == settings.RefreshSeconds, "Save/Load changed refresh settings.");
        Console.WriteLine($"  embedded Assets\\Links.json: {parsed.Urls.Count} eligible URL entries; Save/Load verified");
    }

    private static async Task SettingsAsync(Form host)
    {
        var store = new SettingsStore(Path.Combine(artifacts, "settings-gui"));
        Require(store.SettingsPath.StartsWith(artifacts, StringComparison.OrdinalIgnoreCase),
            "Settings GUI store is not isolated.");
        using (var dialog = new SettingsDialog(new SaverSettings(), store, log, null))
        {
            dialog.Show(host);
            await Task.Delay(50);
            Require(typeof(SettingsDialog).GetField("links", BindingFlags.NonPublic | BindingFlags.Instance) is null
                && typeof(SettingsDialog).GetField("assets", BindingFlags.NonPublic | BindingFlags.Instance) is null
                && !Descendants(dialog).OfType<Label>().Any(label => label.Text == "Assets directory"),
                "Settings still exposes a configurable Assets directory.");
            var settingLabels = Descendants(dialog).OfType<Label>().Select(label => label.Text).ToArray();
            Require(settingLabels.Contains("Refresh interval (seconds)")
                && !settingLabels.Contains("Webpage refresh (minutes)")
                && !settingLabels.Contains("Image rotation (seconds)"),
                "Settings GUI does not expose one shared image/webpage refresh interval.");
            Require(Descendants(dialog).OfType<Button>().Any(button => button.Text == "Set as Windows screensaver"),
                "Settings GUI lacks the Set as Windows screensaver button.");
            ((Button)dialog.AcceptButton!).PerformClick();
            Require(File.Exists(store.SettingsPath), "Settings GUI Save did not persist isolated settings.");
            Require(store.Load().RefreshSeconds == 30, "Settings GUI did not save the default shared refresh interval.");
            Require(!dialog.Visible, "Successful settings Save did not close the dialog.");
        }
        Require(!File.ReadAllText(store.SettingsPath).Contains("LinksPath"), "Settings still persists a URL path.");
        using var reopened = new SettingsDialog(store.Load(), store, log, null);
        reopened.Show(host);
        await Task.Delay(50);
        Require(typeof(SaverSettings).GetProperty("LinksPath") is null, "Settings still supports a configurable URL path.");
        reopened.Close();
    }

    private static Task RegistrationHelpersAsync()
    {
        string root = Path.Combine(artifacts, "registration fixtures with spaces");
        string deployment = Path.Combine(root, "published application");
        string executable = Path.Combine(deployment, "Skyworks.ScreenSaver.exe");
        string saver = Path.Combine(deployment, "Skyworks.ScreenSaver.scr");
        void CreateDeployment(string directory, bool published = true)
        {
            Directory.CreateDirectory(Path.Combine(directory, "Assets", "nested"));
            File.WriteAllText(Path.Combine(directory, "Skyworks.ScreenSaver.exe"), "fixture, never execute");
            File.WriteAllText(Path.Combine(directory, "Skyworks.ScreenSaver.scr"), "fixture, never execute");
            if (published)
                File.WriteAllText(Path.Combine(directory, ScreenSaverRegistration.DeploymentMarker),
                    ScreenSaverRegistration.DeploymentSignature);
            File.WriteAllText(Path.Combine(directory, "Links.json"), JsonSerializer.Serialize(new[] { "http://127.0.0.1/" }));
            File.WriteAllText(Path.Combine(directory, "build-revision.txt"), "4");
            File.WriteAllText(Path.Combine(directory, "Assets", "nested", "asset.png"), "asset fixture");
            Directory.CreateDirectory(Path.Combine(directory, "native", "nested"));
            File.WriteAllText(Path.Combine(directory, "native", "nested", "dependency.dll"), "recursive deployment fixture");
        }
        void Rejected(string name, Action action)
        {
            bool rejected = false;
            try { action(); }
            catch (Exception ex) when (ex is InvalidOperationException or IOException or InvalidDataException) { rejected = true; }
            Require(rejected, $"Registration helper accepted {name}.");
        }
        try
        {
            CreateDeployment(deployment);
            Require(ScreenSaverRegistration.ResolveDeployment(deployment, executable) == saver,
                "Direct exe did not resolve the exact sibling .scr path with spaces.");
            Require(ScreenSaverRegistration.ResolveDeployment(deployment, saver) == saver,
                "Direct .scr did not resolve itself.");
            Rejected("dotnet host", () => ScreenSaverRegistration.ResolveDeployment(deployment, Path.Combine(deployment, "dotnet.exe")));
            Rejected("different application directory", () => ScreenSaverRegistration.ResolveDeployment(root, executable));
            File.Delete(saver);
            Rejected("missing .scr", () => ScreenSaverRegistration.ResolveDeployment(deployment, executable));
            File.WriteAllText(saver, "fixture, never execute");
            string marker = Path.Combine(deployment, ScreenSaverRegistration.DeploymentMarker);
            File.Delete(marker);
            Rejected("missing marker and development dependencies", () => ScreenSaverRegistration.ResolveDeployment(deployment, executable));
            File.WriteAllText(marker, "incorrect signature");
            Rejected("incorrect marker without development dependencies", () => ScreenSaverRegistration.ResolveDeployment(deployment, executable));
            File.WriteAllText(marker, ScreenSaverRegistration.DeploymentSignature);
            var command = ScreenSaverRegistration.InstallationCommand(saver);
            Require(command.FileName == saver && command.UseShellExecute && command.Verb == "install"
                && command.ArgumentList.Count == 0 && command.Arguments.Length == 0,
                "Shell installation command must preserve the full spaced path unquoted, install verb, and no arguments.");
            Rejected("non-.scr shell command", () => ScreenSaverRegistration.InstallationCommand(executable));
            Rejected("missing shell command file", () => ScreenSaverRegistration.InstallationCommand(Path.Combine(root, "missing.scr")));

            string development = Path.Combine(root, "bin", "Debug");
            CreateDeployment(development, published: false);
            string devExe = Path.Combine(development, "Skyworks.ScreenSaver.exe");
            string[] dependencies = ["Skyworks.ScreenSaver.dll", "Skyworks.ScreenSaver.deps.json",
                "Skyworks.ScreenSaver.runtimeconfig.json", "Skyworks.ScreenSaver.Core.dll",
                "Microsoft.Web.WebView2.Core.dll", "Microsoft.Web.WebView2.WinForms.dll"];
            foreach (string dependency in dependencies) File.WriteAllText(Path.Combine(development, dependency), "dependency fixture");
            string loader = Path.Combine(development, "runtimes", "win-x64", "native", "WebView2Loader.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(loader)!);
            File.WriteAllText(loader, "loader fixture");
            Require(ScreenSaverRegistration.ResolveDeployment(development, devExe) == Path.Combine(development, "Skyworks.ScreenSaver.scr"),
                "Complete bin/temp development deployment was rejected.");
            foreach (string dependency in dependencies)
            {
                string path = Path.Combine(development, dependency);
                File.Delete(path);
                Rejected($"missing {dependency}", () => ScreenSaverRegistration.ResolveDeployment(development, devExe));
                File.WriteAllText(path, "dependency fixture");
            }
            File.Delete(loader);
            Rejected("missing native loader", () => ScreenSaverRegistration.ResolveDeployment(development, devExe));
            File.WriteAllText(Path.Combine(development, "WebView2Loader.dll"), "root loader fixture");
            Require(ScreenSaverRegistration.ResolveDeployment(development, devExe).EndsWith(".scr"), "Root WebView2 loader was rejected.");

            string installationRoot = Path.Combine(root, "stable installation with spaces");
            var installed = ScreenSaverRegistration.PrepareInstallation(development, devExe, new SaverSettings(), installationRoot);
            string installedDirectory = Path.GetDirectoryName(installed.ScreenSaverPath)!;
            Require(installedDirectory.StartsWith(Path.Combine(installationRoot, "Deployments") + Path.DirectorySeparatorChar),
                "Installation did not create a stable Deployments child.");
            Require(Guid.TryParse(Path.GetFileName(installedDirectory), out _), "Installation directory is not uniquely GUID-named.");
            foreach (string sourceFile in Directory.GetFiles(development, "*", SearchOption.AllDirectories))
            {
                if (Path.GetRelativePath(development, sourceFile).StartsWith("Assets" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (Path.GetFileName(sourceFile) == "Links.json") continue;
                string copy = Path.Combine(installedDirectory, Path.GetRelativePath(development, sourceFile));
                Require(File.Exists(copy) && File.ReadAllBytes(copy).SequenceEqual(File.ReadAllBytes(sourceFile)),
                    $"Incomplete full deployment copy: {Path.GetRelativePath(development, sourceFile)}.");
            }
            Require(!Directory.Exists(Path.Combine(installedDirectory, "Assets")),
                "Installation copied obsolete external background files.");
            Require(!File.Exists(Path.Combine(installedDirectory, "Links.json")),
                "Installation copied an obsolete external URL list.");
            string installedAsset = Path.Combine(installationRoot, "Data", "Assets", "nested", "asset.png");
            Directory.CreateDirectory(Path.GetDirectoryName(installedAsset)!);
            string userJson = JsonSerializer.Serialize(new[] { "https://user.example/original" });
            string oldLinks = Path.Combine(installationRoot, "Data", "Links.json");
            File.WriteAllText(oldLinks, userJson);
            File.WriteAllText(installedAsset, "user image must survive");
            File.WriteAllText(Path.Combine(development, "Links.json"), JsonSerializer.Serialize(new[] { "https://source.example/new" }));
            File.WriteAllText(Path.Combine(development, "Assets", "nested", "asset.png"), "new source image");
            var second = ScreenSaverRegistration.PrepareInstallation(development, devExe, new SaverSettings(), installationRoot);
            Require(second.ScreenSaverPath != installed.ScreenSaverPath, "Second installation overwrote a prior deployment.");
            Require(File.ReadAllText(oldLinks) == userJson
                && File.ReadAllText(installedAsset) == "user image must survive", "Reinstallation overwrote installed user data.");
            int deploymentCount = Directory.GetDirectories(Path.Combine(installationRoot, "Deployments")).Length;
            var reused = ScreenSaverRegistration.PrepareInstallation(installedDirectory, installed.ScreenSaverPath,
                installed.Settings, installationRoot);
            Require(reused.ScreenSaverPath == installed.ScreenSaverPath
                && Directory.GetDirectories(Path.Combine(installationRoot, "Deployments")).Length == deploymentCount,
                "Already installed deployment was unnecessarily recopied.");

            Rejected("installation inside source", () => ScreenSaverRegistration.PrepareInstallation(deployment, executable,
                new SaverSettings(), Path.Combine(deployment, "inside-source")));
            File.Delete(Path.Combine(deployment, "Links.json"));
            Directory.Delete(Path.Combine(deployment, "Assets"), true);
            var withoutAssets = ScreenSaverRegistration.PrepareInstallation(deployment, executable,
                new SaverSettings(), installationRoot);
            Require(File.Exists(withoutAssets.ScreenSaverPath), "Installation still requires external Assets or Links.json.");
            return Task.CompletedTask;
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (Control descendant in Descendants(child)) yield return descendant;
        }
    }

    private static async Task Test(string name, Func<Task> body)
    {
        var watch = Stopwatch.StartNew();
        try
        {
            await body();
            Console.WriteLine($"PASS {name} ({watch.Elapsed.TotalSeconds:F1}s)");
        }
        catch (Exception ex)
        {
            failures++;
            Console.Error.WriteLine($"FAIL {name} ({watch.Elapsed.TotalSeconds:F1}s): {ex}");
        }
    }

    private static Task StaticImageAsync()
    {
        var bounds = new Rectangle(-1600, -900, 800, 600);
        using var window = new ImageWindow(bounds, true);
        Require(window.Bounds == bounds && window.ClientSize == bounds.Size, "Negative-coordinate construction changed requested bounds.");
        string path = MakeImage("static.png", Cyan);
        window.ShowImage(path);
        string moved = Path.Combine(artifacts, "static-moved.png");
        File.Move(path, moved);
        File.Delete(moved);
        using var pixels = new Bitmap(800, 600);
        window.DrawToBitmap(pixels, new Rectangle(Point.Empty, pixels.Size));
        VerifyPixels(pixels, Cyan, checkBottomRight: false);
        Require(!window.TopMost, "Static test window unexpectedly topmost.");
        return Task.CompletedTask;
    }

    private static async Task ImageTransitionAsync()
    {
        string outgoing = MakeImage("transition-outgoing.png", Color.Red, new Size(64, 64));
        string incoming = MakeImage("transition-incoming.png", Color.Blue, new Size(64, 64));
        using var window = new ImageWindow(new Rectangle(0, 0, 320, 240), true);
        window.ShowImage(outgoing);
        using var frame = new Bitmap(320, 240);
        window.DrawToBitmap(frame, new Rectangle(Point.Empty, frame.Size));
        Require(Near(frame.GetPixel(160, 120), Color.Red), "The initial local image did not appear immediately.");

        window.ShowImage(incoming);
        Require(window.TransitionProgress < 0.1, "Image transition did not start at full outgoing-image opacity.");
        window.DrawToBitmap(frame, new Rectangle(Point.Empty, frame.Size));
        Color start = frame.GetPixel(160, 120);
        Require(start.R > start.B * 2, $"The outgoing image was not visible at transition start: {start}.");

        await Task.Delay(1000);
        window.DrawToBitmap(frame, new Rectangle(Point.Empty, frame.Size));
        Color middle = frame.GetPixel(160, 120);
        Require(window.TransitionProgress is > 0.35 and < 0.65
            && middle.R > 20 && middle.B > 20,
            $"Both images were not blended near the one-second midpoint: progress={window.TransitionProgress:F2}, pixel={middle}.");

        await Task.Delay(1150);
        window.DrawToBitmap(frame, new Rectangle(Point.Empty, frame.Size));
        Color end = frame.GetPixel(160, 120);
        Require(window.TransitionProgress >= 1 && Near(end, Color.Blue),
            $"The incoming image did not reach full opacity after two seconds: progress={window.TransitionProgress:F2}, pixel={end}.");
    }

    private static async Task CaptureAsync(Form host)
    {
        await using var server = new FixtureServer();
        using var capture = new CaptureService(log);
        Require(capture.CacheDirectory.StartsWith(artifacts, StringComparison.OrdinalIgnoreCase), "Capture cache is not isolated.");
        var window = Field<Form>(capture, "window");
        await Task.Delay(150);
        nint foreground = GetForegroundWindow();
        Require(foreground != 0 && foreground != window.Handle,
            "No valid foreground baseline is available before capture.");
        Exception? observationFailure = null;
        bool externalForegroundChange = false;
        using var observer = new System.Windows.Forms.Timer { Interval = 40 };
        observer.Tick += (_, _) =>
        {
            try { externalForegroundChange |= !VerifyOffscreen(window, foreground); }
            catch (Exception ex) { observationFailure ??= ex; }
        };
        observer.Start();
        var saved = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        async Task<string> Good(string route, Color color)
        {
            string? result = await capture.CaptureAsync(server.Uri(route), new Size(800, 600), TimeSpan.FromSeconds(20), CancellationToken.None);
            Require(result is not null && File.Exists(result), $"{route}: no screenshot.");
            Require(saved.Add(result!), $"{route}: reused an earlier screenshot path.");
            using var bitmap = new Bitmap(result!);
            Require(bitmap.Size == new Size(800, 600), $"{route}: expected 800x600, got {bitmap.Size}.");
            VerifyPixels(bitmap, color);
            VerifyCaptureWindow(window);
            Console.WriteLine($"  verified {route}: 800x600 nonblank PNG");
            return result!;
        }
        await Good("/solid", Pink);
        VerifyImageWindowOverlay(saved.First(), new Size(800, 600), new Rectangle(0, 0, 800, 600));
        int count = Directory.GetFiles(capture.CacheDirectory).Length;
        string? rejected = await capture.CaptureAsync(server.Uri("/missing"), new Size(800, 600), TimeSpan.FromSeconds(10), CancellationToken.None);
        Require(rejected is null, "Main HTTP404 with HTTP200 subresource was accepted.");
        Require(Directory.GetFiles(capture.CacheDirectory).Length == count, "Rejected HTTP404 persisted an image/partial file.");
        Require(server.Requests.Contains("/resource"), "HTTP404 fixture did not actually fetch the HTTP200 subresource.");
        foreach (string route in new[] { "/forbidden", "/server-error", "/created", "/partial", "/redirect-missing" })
        {
            rejected = await capture.CaptureAsync(server.Uri(route), new Size(800, 600), TimeSpan.FromSeconds(4), CancellationToken.None);
            Require(rejected is null, $"Non-200 main document was accepted: {route}");
            Require(Directory.GetFiles(capture.CacheDirectory).Length == count, $"{route} persisted an error screenshot.");
        }
        rejected = await capture.CaptureAsync(new Uri("http://skyworks-smoke-nonexistent.invalid/"), new Size(800, 600), TimeSpan.FromSeconds(4), CancellationToken.None);
        Require(rejected is null && Directory.GetFiles(capture.CacheDirectory).Length == count,
            "DNS failure/browser-generated error page persisted a screenshot.");
        await Good("/iframe", Cyan);
        Require(server.Requests.Contains("/frame-missing"), "Main200 fixture did not actually fetch iframe404.");
        await Good("/redirect", Yellow);
        count = Directory.GetFiles(capture.CacheDirectory).Length;
        using (var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(400)))
        {
            var watch = Stopwatch.StartNew();
            bool cancelled = false;
            try { await capture.CaptureAsync(server.Uri("/delayed"), new Size(800, 600), TimeSpan.FromSeconds(10), cancel.Token); }
            catch (OperationCanceledException) { cancelled = true; }
            Require(cancelled && watch.Elapsed < TimeSpan.FromSeconds(4), "Delayed main response did not promptly propagate caller cancellation.");
        }
        var timeoutWatch = Stopwatch.StartNew();
        rejected = await capture.CaptureAsync(server.Uri("/delayed"), new Size(800, 600), TimeSpan.FromMilliseconds(500), CancellationToken.None);
        Require(rejected is null && timeoutWatch.Elapsed < TimeSpan.FromSeconds(4), "Delayed main response did not return null within timeout.");
        Require(Directory.GetFiles(capture.CacheDirectory).Length == count, "Cancelled/timed-out requests persisted a capture or reused prior image.");
        await Good("/solid", Pink);
        observer.Stop();
        if (observationFailure is not null) throw observationFailure;
        if (externalForegroundChange)
        {
            const string message = "INCONCLUSIVE unchanged-foreground check: another desktop window changed focus. Capture-window nonactivation and offscreen/style assertions still passed.";
            Console.WriteLine("  " + message);
            log.Write(message);
        }
        else Console.WriteLine("  verified foreground remained unchanged during capture");
        Require(File.Exists(log.FilePath), "Diagnostic log missing.");
        string diagnostics = File.ReadAllText(log.FilePath);
        Require(diagnostics.Contains("404") && diagnostics.Contains("timed out"), "Failure status/timeout diagnostics missing.");
    }

    private static async Task FallbackAsync()
    {
        await using var server = new FixtureServer();
        using var capture = new CaptureService(log);
        string Bare(string route) => $"{server.Uri(route).Host}:{server.Uri(route).Port}{route}";
        LinkTarget Target(string value) => JsonLinks.Deserialize(JsonSerializer.Serialize(new[] { value })).Targets.Single();
        string bare = Bare("/policy-success");
        var target = Target(bare);
        Require(target.AllowHttpFallback && target.Attempts.Count == 2, "Scheme-less entry lost HTTPS-first fallback policy.");
        var page = await capture.CaptureTargetAsync(target, new Size(800, 600), TimeSpan.FromSeconds(4), CancellationToken.None);
        Require(page is not null && page.DocumentUri.Scheme == "http" && server.TlsAttempts > 0
            && server.Requests.Contains("/policy-success"), "An HTTP-only site was not captured after its HTTPS attempt failed.");
        using (var pixels = new Bitmap(page!.FilePath)) VerifyPixels(pixels, Pink);
        string diagnostics = File.ReadAllText(log.FilePath);
        Require(diagnostics.Contains("HTTP fallback succeeded") && !diagnostics.Contains($"No eligible screenshot for {bare}."),
            "Successful fallback was incorrectly diagnosed as complete URL failure.");
        int count = Directory.GetFiles(capture.CacheDirectory).Length;
        string explicitHttps = new UriBuilder(server.Uri("/explicit-https")) { Scheme = "https", Port = server.Uri("/explicit-https").Port }.Uri.AbsoluteUri;
        target = Target(explicitHttps);
        Require(!target.AllowHttpFallback && target.Attempts.Count == 1, "Explicit HTTPS gained automatic HTTP fallback.");
        page = await capture.CaptureTargetAsync(target, new Size(800, 600), TimeSpan.FromSeconds(4), CancellationToken.None);
        Require(page is null && !server.Requests.Contains("/explicit-https") && Directory.GetFiles(capture.CacheDirectory).Length == count,
            "Explicit HTTPS was downgraded to an HTTP request or persisted an error image.");
        string failed = Bare("/missing");
        page = await capture.CaptureTargetAsync(Target(failed), new Size(800, 600), TimeSpan.FromSeconds(4), CancellationToken.None);
        Require(page is null && Directory.GetFiles(capture.CacheDirectory).Length == count, "Both failed attempts produced a screenshot.");
        string final = File.ReadAllLines(log.FilePath).Last(line => line.Contains($"No eligible screenshot for {failed}."));
        Require(final.Contains("https://") && final.Contains("http://") && final.Contains("404") && final.Contains("All permitted attempts failed"),
            "Final scheme-less failure did not identify both attempt outcomes.");
    }

    private static async Task RefreshPresentationAsync()
    {
        await using var server = new FixtureServer();
        string directory = Path.Combine(artifacts, "refresh-assets");
        Directory.CreateDirectory(directory);
        MakeImage(Path.Combine("refresh-assets", "first.png"), Cyan);
        string links = Path.Combine(artifacts, "refresh-links.json");
        File.WriteAllText(links, JsonSerializer.Serialize(new[] { "/solid", "/forbidden", "/server-error", "/redirect" }.Select(route => server.Uri(route).AbsoluteUri)));
        using var parent = new NonActivatingHost { ClientSize = new Size(400, 300) };
        parent.Show();
        using var context = new SaverContext(new SaverSettings(), log, parent.Handle);
        var child = Field<List<ImageWindow>>(context, "windows")[0];
        var gallery = Field<ShuffledCycle<string>>(context, "webpages");
        gallery.Replace([]);
        var prior = gallery.Items.ToHashSet(StringComparer.OrdinalIgnoreCase);
        string initial = Field<Dictionary<ImageWindow, string>>(context, "displayed")[child];
        var capture = new CaptureService(log);
        typeof(SaverContext).GetField("capture", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(context, capture);
        Func<LinkLoadResult> fixtureLinks = () => JsonLinks.Deserialize(File.ReadAllText(links));
        var work = (Task)typeof(SaverContext).GetMethod("RefreshLoopAsync", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(context, [fixtureLinks])!;
        typeof(SaverContext).GetField("refresh", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(context, work);
        try
        {
            var deadline = Stopwatch.StartNew();
            while (Field<HashSet<string>>(context, "pendingScreenshots").Count == 0 && deadline.Elapsed < TimeSpan.FromSeconds(12))
                await Task.Delay(25);
            Require(Field<HashSet<string>>(context, "pendingScreenshots").Count > 0, "No successful image was staged during refresh.");
            Require(prior.SetEquals(gallery.Items), "Capture callbacks modified the active page cycle before the batch completed.");
            Require(Field<Dictionary<ImageWindow, string>>(context, "displayed")[child] == initial,
                "A captured page was shown immediately in request order.");
            while ((gallery.Count != 2 || gallery.Items.Any(prior.Contains)) && deadline.Elapsed < TimeSpan.FromSeconds(20))
                await Task.Delay(25);
            Require(gallery.Count == 2 && gallery.Items.All(path => !prior.Contains(path)),
                "The completed cycle included failed URLs, stale screenshots, or lost a successful page.");
            var colors = new HashSet<int>();
            foreach (string path in gallery.Items)
            {
                using var image = new Bitmap(path);
                Color pixel = image.GetPixel(image.Width / 2, image.Height / 2);
                Require(Near(pixel, Pink) || Near(pixel, Yellow), "A failed page's image entered the completed cycle.");
                colors.Add(Near(pixel, Pink) ? Pink.ToArgb() : Yellow.ToArgb());
            }
            Require(colors.SetEquals(new[] { Pink.ToArgb(), Yellow.ToArgb() }), "Refreshed images were not the two eligible HTTP 200 pages.");
            var shown = new HashSet<string> { Field<Dictionary<ImageWindow, string>>(context, "displayed")[child] };
            typeof(SaverContext).GetMethod("ShowNextImage", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(context, [child]);
            Require(shown.Add(Field<Dictionary<ImageWindow, string>>(context, "displayed")[child]) && shown.SetEquals(gallery.Items),
                "The first refreshed shuffled cycle repeated or lost a page.");
        }
        finally
        {
            parent.Close();
            await work.WaitAsync(TimeSpan.FromSeconds(3));
        }
    }

    private static bool VerifyOffscreen(Form window, nint foreground)
    {
        nint actualForeground = GetForegroundWindow();
        Require(actualForeground != window.Handle && GetAncestor(actualForeground, 2) != window.Handle,
            $"Capture window or its child became foreground: {DescribeWindow(actualForeground)}.");
        VerifyCaptureWindow(window);
        return actualForeground == foreground;
    }

    private static void VerifyCaptureWindow(Form window)
    {
        Require(window.Visible, "Capture window is hidden; compositor rendering not exercised.");
        Require(Screen.AllScreens.All(screen => !screen.Bounds.IntersectsWith(window.Bounds)), $"Capture window intersects a real monitor: {window.Bounds}.");
        long style = GetWindowLongPtr(window.Handle, -20).ToInt64();
        Require((style & 0x08000080) == 0x08000080, "Capture window lacks WS_EX_NOACTIVATE/TOOLWINDOW.");
    }

    private static async Task PreviewAsync()
    {
        string[] assets = EmbeddedAssets.Images.ToArray();
        Require(assets.Length > 0, "No backgrounds were compiled into the application.");
        using var parent = new Form { ClientSize = new Size(400, 300), Text = "Isolated preview parent", TopMost = false };
        parent.Show();
        nint parentHandle = parent.Handle;
        using var context = new SaverContext(new SaverSettings
        {
            RefreshSeconds = 3600
        }, log, parentHandle);
        Require(Field<System.Windows.Forms.Timer>(context, "rotationTimer").Interval == 3_600_000,
            "Image rotation timer did not use the shared refresh interval.");
        bool exited = false;
        context.ThreadExit += (_, _) => exited = true;
        var windows = Field<List<ImageWindow>>(context, "windows");
        Require(windows.Count == 1, "Preview created multiple display windows.");
        var child = windows[0];
        Require(GetParent(child.Handle) == parentHandle, "Preview HWND is not embedded in supplied parent.");
        Require((GetWindowLongPtr(child.Handle, -16).ToInt64() & 0x40000000) != 0, "Preview lacks WS_CHILD.");
        Require(!child.TopMost && (GetWindowLongPtr(child.Handle, -20).ToInt64() & 8) == 0, "Preview is topmost.");
        Require(child.ClientSize == parent.ClientSize, "Initial preview client size differs from parent.");
        Require(Field<object?>(context, "capture") is null, "Preview initialized capture/browser.");
        string first = Field<Dictionary<ImageWindow, string>>(context, "displayed")[child];
        Require(assets.Contains(first), "Preview did not immediately display an actual local asset.");
        using (var pixels = new Bitmap(child.Width, child.Height))
        {
            child.DrawToBitmap(pixels, new Rectangle(Point.Empty, pixels.Size));
            VerifyEmbeddedCenter(pixels, first);
        }
        var gallery = Field<ShuffledCycle<string>>(context, "webpages");
        gallery.Replace([]);
        string[] originalAssets = Field<ShuffledCycle<string>>(context, "images").Items.ToArray();
        Require(originalAssets.ToHashSet().SetEquals(assets), "Asset loading lost files or included the emergency fallback.");
        var show = typeof(SaverContext).GetMethod("ShowNextImage", BindingFlags.NonPublic | BindingFlags.Instance)!;
        bool randomized = false;
        for (int cycle = 0; cycle < 3; cycle++)
        {
            var order = new List<string>();
            if (cycle == 0) order.Add(first);
            while (order.Count < assets.Length)
            {
                show.Invoke(context, [child]);
                order.Add(Field<Dictionary<ImageWindow, string>>(context, "displayed")[child]);
            }
            Require(order.Distinct().Count() == assets.Length && order.ToHashSet().SetEquals(assets),
                "Local image cycle repeated an asset before displaying all loaded assets.");
            randomized |= !order.SequenceEqual(assets);
        }
        Require(randomized, "All three local image cycles retained filename order.");
        string[] pages = [MakeImage("shuffle-pink.png", Pink), MakeImage("shuffle-cyan.png", Cyan), MakeImage("shuffle-yellow.png", Yellow)];
        gallery.Replace(pages);
        for (int cycle = 0; cycle < 3; cycle++)
        {
            var shown = new HashSet<string>();
            for (int i = 0; i < pages.Length; i++)
            {
                show.Invoke(context, [child]);
                string displayed = Field<Dictionary<ImageWindow, string>>(context, "displayed")[child];
                Require(shown.Add(displayed), "A webpage image repeated before this shuffled cycle completed.");
                using var pixels = new Bitmap(child.ClientSize.Width, child.ClientSize.Height);
                child.DrawToBitmap(pixels, new Rectangle(Point.Empty, pixels.Size));
                using var stored = new Bitmap(displayed);
                Require(Near(pixels.GetPixel(pixels.Width / 2, pixels.Height / 2), stored.GetPixel(400, 300)),
                    "Shuffled presentation did not render its stored image.");
            }
            Require(shown.SetEquals(pages), "Shuffled cycle lost an eligible page.");
        }
        gallery.Replace([]);
        show.Invoke(context, [child]);
        Require(originalAssets.SequenceEqual(Field<ShuffledCycle<string>>(context, "images").Items), "Webpage cycles removed fallback assets.");
        Require(originalAssets.Contains(Field<Dictionary<ImageWindow, string>>(context, "displayed")[child]),
            "An empty successful-page batch did not fall back to Assets.");
        parent.ClientSize = new Size(640, 360);
        await Task.Delay(350);
        Require(child.ClientSize == parent.ClientSize, $"Preview resize mismatch: {child.ClientSize} vs {parent.ClientSize}.");
        PostMessage(child.Handle, 0x0200, 0, (nint)((25 << 16) | 25));
        PostMessage(child.Handle, 0x0100, 0x41, 0);
        PostMessage(child.Handle, 0x0101, 0x41, 0);
        await Task.Delay(1250);
        typeof(SaverContext).GetMethod("CheckInputOrParent", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(context, null);
        Require(!exited && !child.IsDisposed && child.Visible, "Preview exited after synthetic input/poll.");
        parent.Close();
        var deadline = Stopwatch.StartNew();
        while (!exited && deadline.Elapsed < TimeSpan.FromSeconds(3)) await Task.Delay(50);
        Require(exited && child.IsDisposed, "Closing preview parent did not end preview.");
    }

    private static async Task PublishedPreviewAsync()
    {
        string saver = Path.GetFullPath(Path.Combine("publish", "Skyworks.ScreenSaver.scr"));
        Require(File.Exists(saver), $"Published .scr missing: {saver}. Publish the app before running this test.");
        string published = Path.GetDirectoryName(saver)!;
        string isolated = Path.Combine(artifacts, "published-no-assets");
        Directory.CreateDirectory(isolated);
        File.Copy(saver, Path.Combine(isolated, "Skyworks.ScreenSaver.scr"));
        saver = Path.Combine(isolated, "Skyworks.ScreenSaver.scr");
        Require(!Directory.Exists(Path.Combine(isolated, "Assets")), "Isolated deployment unexpectedly contains external Assets.");
        using var parent = new NonActivatingHost
        {
            Text = "Published screensaver isolated preview host",
            ClientSize = new Size(420, 280),
            StartPosition = FormStartPosition.CenterScreen,
            TopMost = false
        };
        parent.Show();
        nint parentHandle = parent.Handle;
        var start = new ProcessStartInfo(saver) { UseShellExecute = false };
        start.ArgumentList.Add("/p");
        start.ArgumentList.Add(parentHandle.ToInt64().ToString(System.Globalization.CultureInfo.InvariantCulture));
        start.Environment["DOTNET_BUNDLE_EXTRACT_BASE_DIR"] = Path.Combine(artifacts, "bundle-extraction");
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Published .scr process did not launch.");
        try
        {
            nint child = 0;
            var watch = Stopwatch.StartNew();
            while (child == 0 && !process.HasExited && watch.Elapsed < TimeSpan.FromSeconds(12))
            {
                EnumChildWindows(parentHandle, (hwnd, _) =>
                {
                    GetWindowThreadProcessId(hwnd, out uint owner);
                    if (owner == process.Id && GetParent(hwnd) == parentHandle) child = hwnd;
                    return true;
                }, 0);
                if (child == 0) await Task.Delay(50);
            }
            Require(child != 0, process.HasExited
                ? $"Published .scr exited before embedding, exit={process.ExitCode}."
                : "Published .scr did not create an embedded preview child within 12 seconds.");
            Require(GetParent(child) == parentHandle && (GetWindowLongPtr(child, -16).ToInt64() & 0x40000000) != 0,
                "Published preview is not a WS_CHILD of the supplied cross-process HWND.");
            Require((GetWindowLongPtr(child, -20).ToInt64() & 8) == 0, "Published preview child is topmost.");
            uint dpi = GetDpiForWindow(parentHandle);
            Require(dpi != 0 && GetDpiForWindow(child) == dpi, "Published preview and parent have different window DPI.");
            Require(GetClientRect(child, out var initial) && initial.Size == parent.ClientSize,
                "Published preview initially differs from host client size.");
            parent.ClientSize = new Size(640, 360);
            watch.Restart();
            bool resized = false;
            while (!process.HasExited && watch.Elapsed < TimeSpan.FromSeconds(3))
            {
                resized = GetClientRect(child, out var rect) && rect.Size == parent.ClientSize;
                if (resized) break;
                await Task.Delay(50);
            }
            Require(resized, "Published preview did not follow host client resize.");
            parent.Close();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Require(process.ExitCode == 0, $"Published preview parent shutdown returned exit={process.ExitCode}.");
            Console.WriteLine($"  published .scr PID={process.Id}: embedded child, DPI={dpi}, resize and clean exit verified");
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(5));
            }
        }
    }

    private sealed class NonActivatingHost : Form
    {
        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.ExStyle |= 0x08000080;
                return parameters;
            }
        }
    }

    private static string MakeImage(string name, Color color, Size? dimensions = null)
    {
        string path = Path.Combine(artifacts, name);
        using var bitmap = new Bitmap((dimensions ?? new Size(800, 600)).Width, (dimensions ?? new Size(800, 600)).Height);
        using (var graphics = Graphics.FromImage(bitmap)) graphics.Clear(color);
        bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        return path;
    }

    private static void VerifyPixels(Bitmap bitmap, Color expected, bool checkBottomRight = true)
    {
        foreach (var point in (checkBottomRight ? new[] { new Point(10, 10), new Point(400, 300), new Point(789, 589) } : new[] { new Point(10, 10), new Point(400, 300) }))
        {
            var actual = bitmap.GetPixel(point.X, point.Y);
            Require(Near(actual, expected), $"Blank/wrong screenshot pixel {point}: expected {expected}, got {actual}.");
        }
    }

    private static bool Near(Color actual, Color expected) =>
        Math.Abs(actual.R - expected.R) <= 3 && Math.Abs(actual.G - expected.G) <= 3 && Math.Abs(actual.B - expected.B) <= 3;

    private static T Field<T>(object instance, string name) =>
        (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(instance)!;

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static string DescribeWindow(nint hwnd)
    {
        GetWindowThreadProcessId(hwnd, out uint process);
        var className = new StringBuilder(256);
        GetClassName(hwnd, className, className.Capacity);
        return $"HWND=0x{hwnd:X} PID={process} class={className}";
    }

    [DllImport("user32.dll")] private static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] private static extern nint GetAncestor(nint hwnd, uint flags);
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(nint hwnd, out uint process);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassName(nint hwnd, StringBuilder className, int count);
    [DllImport("user32.dll")] private static extern nint GetParent(nint hwnd);
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")] private static extern nint GetWindowLongPtr(nint hwnd, int index);
    [DllImport("user32.dll")] private static extern bool PostMessage(nint hwnd, uint message, nint wparam, nint lparam);
    private delegate bool EnumWindowsCallback(nint hwnd, nint parameter);
    [DllImport("user32.dll")] private static extern bool EnumChildWindows(nint parent, EnumWindowsCallback callback, nint parameter);
    [DllImport("user32.dll")] private static extern uint GetDpiForWindow(nint hwnd);
    [DllImport("user32.dll")] private static extern bool GetClientRect(nint hwnd, out ClientRectangle rectangle);
    [StructLayout(LayoutKind.Sequential)]
    private struct ClientRectangle
    {
        public int Left, Top, Right, Bottom;
        public readonly Size Size => new(Right - Left, Bottom - Top);
    }

    private sealed class FixtureServer : IAsyncDisposable
    {
        private readonly TcpListener listener = new(IPAddress.Loopback, 0);
        private readonly CancellationTokenSource stop = new();
        private readonly Task accept;
        private readonly List<Task> requests = [];
        public System.Collections.Concurrent.ConcurrentBag<string> Requests { get; } = [];
        private readonly int port;
        private int tlsAttempts;
        public int TlsAttempts => Volatile.Read(ref tlsAttempts);

        public FixtureServer()
        {
            listener.Start();
            port = ((IPEndPoint)listener.LocalEndpoint).Port;
            accept = AcceptAsync();
        }

        public Uri Uri(string route) => new($"http://127.0.0.1:{port}{route}");

        private async Task AcceptAsync()
        {
            try
            {
                while (!stop.IsCancellationRequested)
                {
                    var client = await listener.AcceptTcpClientAsync(stop.Token);
                    requests.Add(ReplyAsync(client));
                }
            }
            catch (OperationCanceledException) { }
        }

        private async Task ReplyAsync(TcpClient client)
        {
            using (client)
            {
                try
                {
                    using var stream = client.GetStream();
                    byte[] prefix = new byte[1];
                    if (await stream.ReadAsync(prefix, stop.Token) == 0) return;
                    if (prefix[0] == 0x16) { Interlocked.Increment(ref tlsAttempts); return; }
                    using var reader = new StreamReader(stream, Encoding.ASCII, false, 1024, true);
                    string? rest = await reader.ReadLineAsync(stop.Token);
                    if (rest is null) return;
                    string first = (char)prefix[0] + rest;
                    string route = first.Split(' ')[1].Split('?')[0];
                    Requests.Add(route);
                    while (!string.IsNullOrEmpty(await reader.ReadLineAsync(stop.Token))) { }
                    if (route == "/delayed") await Task.Delay(TimeSpan.FromSeconds(30), stop.Token);
                    int status = route switch
                    {
                        "/missing" or "/frame-missing" => 404,
                        "/forbidden" => 403,
                        "/server-error" => 500,
                        "/created" => 201,
                        "/partial" => 206,
                        "/redirect" or "/redirect-missing" => 302,
                        _ => 200
                    };
                    string color = route == "/iframe" ? "#14d2dc" : route == "/redirected" ? "#f0dc1e" : "#f028a0";
                    string extra = route == "/missing" ? "<img src='/resource'>" : route == "/iframe" ? "<iframe style='position:absolute;left:-2000px;width:1px;height:1px' src='/frame-missing'></iframe>" : "";
                    byte[] body = Encoding.UTF8.GetBytes($"<!doctype html><html><head><style>html,body{{margin:0;width:100%;height:100%;background:{color}}}</style></head><body>{extra}</body></html>");
                    string redirect = status == 302 ? $"Location: {(route == "/redirect-missing" ? "/missing" : "/redirected")}\r\n" : "";
                    byte[] header = Encoding.ASCII.GetBytes($"HTTP/1.1 {status} Fixture\r\n{redirect}Content-Type: text/html; charset=utf-8\r\nContent-Length: {body.Length}\r\nConnection: close\r\nCache-Control: no-store\r\n\r\n");
                    await stream.WriteAsync(header, stop.Token);
                    await stream.WriteAsync(body, stop.Token);
                }
                catch (OperationCanceledException) { }
                catch (IOException) { }
                catch (SocketException) { }
            }
        }

        public async ValueTask DisposeAsync()
        {
            stop.Cancel();
            listener.Stop();
            await accept;
            await Task.WhenAll(requests);
            stop.Dispose();
        }
    }
}
