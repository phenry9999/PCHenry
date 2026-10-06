using Skyworks.ScreenSaver.Core;

namespace Skyworks.ScreenSaver;

internal sealed class SaverContext : ApplicationContext
{
    private readonly List<ImageWindow> windows = [];
    private readonly ShuffledCycle<string> images = new(comparer: StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ImageWindow, string> displayed = [];
    private readonly ShuffledCycle<string> webpages = new(comparer: StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> pendingScreenshots = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FileStream> screenshotLeases = new(StringComparer.OrdinalIgnoreCase);
    private readonly SaverSettings settings;
    private readonly DiagnosticLog log;
    private readonly nint previewParent;
    private readonly CancellationTokenSource cancellation = new();
    private readonly System.Windows.Forms.Timer rotationTimer = new();
    private readonly System.Windows.Forms.Timer inputTimer = new() { Interval = 100 };
    private readonly bool[] previousKeys = new bool[256];
    private readonly DateTime started = DateTime.UtcNow;
    private Point mouseOrigin = Cursor.Position;
    private readonly MonitorRotation monitors;
    private CaptureService? capture;
    private Task refresh = Task.CompletedTask;
    private readonly Size captureViewport;
    private bool exiting;
    private bool closingAllowed;
    private string? fallbackPath;

    public SaverContext(SaverSettings settings, DiagnosticLog log, nint previewParent = 0)
    {
        this.settings = settings;
        this.log = log;
        this.previewParent = previewParent;
        if (previewParent != 0)
        {
            if (!NativeMethods.IsWindow(previewParent) || !NativeMethods.GetClientRect(previewParent, out var rect))
                throw new ArgumentException("The preview parent is not a valid window.");
            var window = new ImageWindow(new Rectangle(0, 0, Math.Max(1, rect.Right), Math.Max(1, rect.Bottom)), true);
            _ = window.Handle;
            NativeMethods.SetWindowLong(window.Handle, NativeMethods.GwlStyle,
                (NativeMethods.GetWindowLong(window.Handle, NativeMethods.GwlStyle) & ~NativeMethods.WsPopup) | NativeMethods.WsChild);
            NativeMethods.SetParent(window.Handle, previewParent);
            windows.Add(window);
        }
        else
        {
            foreach (var screen in Screen.AllScreens)
                windows.Add(new ImageWindow(screen.Bounds, false));
        }
        monitors = new MonitorRotation(windows.Count);
        captureViewport = new Size(windows.Max(w => w.ClientSize.Width), windows.Max(w => w.ClientSize.Height));
        LoadAssets();
        foreach (var window in windows)
        {
            window.FormClosing += (_, e) =>
            {
                if (!closingAllowed) { e.Cancel = true; BeginExit(); }
            };
            window.Show();
            ShowNextImage(window);
        }
        // Assets always appear first, before cached screenshots or any browser initialization.
        string cache = Path.Combine(Path.GetTempPath(), "Skyworks.ScreenSaver", "Screenshots");
        try
        {
            var cached = new List<string>();
            if (Directory.Exists(cache))
                foreach (string path in Directory.GetFiles(cache, "*.png").OrderByDescending(File.GetLastWriteTimeUtc).Take(100).Reverse())
                    if (RetainScreenshot(path)) cached.Add(path);
            webpages.Replace(cached);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write("Cannot load previous screenshots.", ex); }
        rotationTimer.Interval = checked(settings.RotationSeconds * 1000);
        rotationTimer.Tick += (_, _) =>
        {
            int monitor = monitors.Next();
            ShowNextImage(windows[monitor]);
            ReleaseUnusedScreenshots();
        };
        rotationTimer.Start();
        inputTimer.Tick += (_, _) => CheckInputOrParent();
        inputTimer.Start();
        for (int key = 1; key < previousKeys.Length; key++) previousKeys[key] = (NativeMethods.GetAsyncKeyState(key) & 0x8000) != 0;
        if (previewParent == 0)
        {
            try
            {
                capture = new CaptureService(log);
                capture.CleanupCache(RetainedImages());
                // Post until the message loop starts and the asset windows have painted.
                windows[0].BeginInvoke(() => { if (!exiting) refresh = RefreshLoopAsync(); });
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                log.Write("Capture cache could not be initialized; local image rotation remains available.", ex);
            }
        }
    }

    private void LoadAssets()
    {
        var loaded = EmbeddedAssets.Images;
        if (loaded.Count == 0) log.Write("No background images are embedded in this build; using the generated fallback.");
        // An original generated bitmap remains a local-file fallback even if all user images are invalid.
        string fallback = Path.Combine(Path.GetTempPath(), "Skyworks.ScreenSaver", $"fallback-{Guid.NewGuid():N}.png");
        fallbackPath = fallback;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fallback)!);
            using var bitmap = new Bitmap(1280, 720);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.FromArgb(9, 22, 42));
                using var brush = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, 1280, 720),
                    Color.FromArgb(16, 55, 91), Color.FromArgb(7, 13, 28), 45f);
                graphics.FillRectangle(brush, 0, 0, 1280, 720);
                using var pen = new Pen(Color.FromArgb(45, 110, 150), 2);
                for (int i = 0; i < 12; i++) graphics.DrawEllipse(pen, 400 - i * 50, 160 - i * 20, 480 + i * 100, 400 + i * 40);
            }
            bitmap.Save(fallback, System.Drawing.Imaging.ImageFormat.Png);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.ExternalException)
        { log.Write("Cannot store generated fallback image; existing assets will still be used.", ex); }
        images.Replace(loaded);
    }

    private void ShowNextImage(ImageWindow window)
    {
        int webpageAttempts = webpages.Count;
        while (webpageAttempts-- > 0 && webpages.TryNext(out string? screenshot))
        {
            if (TryDisplay(window, screenshot)) return;
            webpages.Remove(screenshot);
        }
        int attempts = images.Count;
        while (attempts-- > 0 && images.TryNext(out string? path))
        {
            if (TryDisplay(window, path)) return;
            images.Remove(path);
        }
        if (fallbackPath is not null && File.Exists(fallbackPath)) TryDisplay(window, fallbackPath);
    }

    private bool TryDisplay(ImageWindow window, string path)
    {
        try
        {
            window.ShowImage(path);
            displayed[window] = path;
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or OutOfMemoryException)
        {
            log.Write($"Cannot display local image: {path}", ex);
            return false;
        }
    }

    private async Task RefreshLoopAsync(Func<LinkLoadResult>? loadLinks = null)
    {
        var token = cancellation.Token;
        try
        {
            using var cadence = new PeriodicTimer(TimeSpan.FromMinutes(settings.RefreshMinutes));
            do
            {
                try
                {
                    const string links = "embedded Assets\\Links.json";
                    var parsed = (loadLinks ?? EmbeddedAssets.LoadLinks)();
                    foreach (var message in parsed.Diagnostics) log.Write($"{links}: {message}");
                    if (parsed.Urls.Count == 0) log.Write($"No valid webpage URLs in {links}; retaining local images.");
                    var successful = new List<string>();
                    var successfulDocuments = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var target in parsed.Targets)
                    {
                        token.ThrowIfCancellationRequested();
                        // A common viewport avoids reserving a monitor while the independent image timer rotates.
                        var page = await capture!.CaptureTargetAsync(target, captureViewport, TimeSpan.FromSeconds(settings.CaptureTimeoutSeconds), token);
                        if (page is null || token.IsCancellationRequested) continue;
                        string path = page.FilePath;
                        if (!successfulDocuments.Add(page.DocumentUri.AbsoluteUri))
                        {
                            try { File.Delete(path); }
                            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write($"Duplicate successful screenshot cleanup failed: {path}", ex); }
                            continue;
                        }
                        if (!RetainScreenshot(path)) continue;
                        pendingScreenshots.Add(path);
                        successful.Add(path);
                    }
                    token.ThrowIfCancellationRequested();
                    // Publish a completed batch atomically on the UI thread, then present its shuffled order.
                    webpages.Replace(successful);
                    pendingScreenshots.Clear();
                    if (webpages.Count > 0) ShowNextImage(windows[monitors.Next()]);
                    else log.Write("No HTTP 200 screenshots in this refresh; retaining displayed images and rotating Assets.");
                    ReleaseUnusedScreenshots();
                    capture!.CleanupCache(RetainedImages());
                }

                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    log.Write("Webpage refresh failed for embedded Assets\\Links.json; existing images remain available.", ex);
                }
                finally
                {
                    pendingScreenshots.Clear();
                    ReleaseUnusedScreenshots();
                }
            } while (await cadence.WaitForNextTickAsync(token));
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
    }

    private bool RetainScreenshot(string path)
    {
        try
        {
            // Other saver instances may share this cache, but cannot delete this instance's retained images.
            screenshotLeases.Add(path, new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write($"Cannot retain cached screenshot: {path}", ex); return false; }
    }

    private HashSet<string> RetainedImages() =>
        new(webpages.Items.Concat(displayed.Values).Concat(pendingScreenshots), StringComparer.OrdinalIgnoreCase);

    private void ReleaseUnusedScreenshots()
    {
        var retained = RetainedImages();
        foreach (string old in screenshotLeases.Keys.Where(p => !retained.Contains(p)).ToArray())
        {
            screenshotLeases[old].Dispose();
            screenshotLeases.Remove(old);
        }
    }

    private void CheckInputOrParent()
    {
        if (previewParent != 0)
        {
            if (!NativeMethods.IsWindow(previewParent)) { BeginExit(); return; }
            if (NativeMethods.GetClientRect(previewParent, out var rect))
                windows[0].Bounds = new Rectangle(0, 0, Math.Max(1, rect.Right), Math.Max(1, rect.Bottom));
            return;
        }
        if (DateTime.UtcNow - started < TimeSpan.FromSeconds(1))
        {
            mouseOrigin = Cursor.Position;
            for (int key = 1; key < previousKeys.Length; key++) previousKeys[key] = (NativeMethods.GetAsyncKeyState(key) & 0x8000) != 0;
            return;
        }
        Point mouse = Cursor.Position;
        if (Math.Abs(mouse.X - mouseOrigin.X) > 10 || Math.Abs(mouse.Y - mouseOrigin.Y) > 10) { BeginExit(); return; }
        for (int key = 1; key < previousKeys.Length; key++)
        {
            bool down = (NativeMethods.GetAsyncKeyState(key) & 0x8000) != 0;
            if (down && !previousKeys[key]) { BeginExit(); return; }
            previousKeys[key] = down;
        }
    }

    private async void BeginExit()
    {
        if (exiting) return;
        exiting = true;
        rotationTimer.Stop();
        inputTimer.Stop();
        cancellation.Cancel();
        foreach (var window in windows) window.Hide();
        try { await refresh; }
        catch (Exception ex) { log.Write("Shutdown error.", ex); }
        capture?.Dispose();
        closingAllowed = true;
        foreach (var window in windows) window.Dispose();
        ExitThread();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            rotationTimer.Dispose();
            inputTimer.Dispose();
            cancellation.Dispose();
            foreach (var lease in screenshotLeases.Values) lease.Dispose();
            if (fallbackPath is not null)
                try { File.Delete(fallbackPath); } catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write("Fallback cleanup failed.", ex); }
        }
        base.Dispose(disposing);
    }
}
