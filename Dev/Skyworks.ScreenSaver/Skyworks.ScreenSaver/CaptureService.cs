using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Skyworks.ScreenSaver.Core;

namespace Skyworks.ScreenSaver;

public sealed record CapturedPage(string FilePath, Uri DocumentUri);

public sealed class CaptureService : IDisposable
{
    private sealed record AttemptResult(CapturedPage? Page, string Outcome);
    private sealed class CaptureWindow : Form
    {
        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams
        {
            get
            {
                var parameters = base.CreateParams;
                parameters.ExStyle |= 0x08000000 | 0x00000080; // NOACTIVATE | TOOLWINDOW
                return parameters;
            }
        }
        public CaptureWindow()
        {
            AutoScaleMode = AutoScaleMode.None;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(SystemInformation.VirtualScreen.Right + 100, SystemInformation.VirtualScreen.Bottom + 100);
        }
    }

    private readonly CaptureWindow window = new();
    private WebView2 browser = new() { Dock = DockStyle.Fill };
    private readonly DiagnosticLog log;
    private readonly string profilePath;
    private readonly FileStream profileLease;
    private readonly SemaphoreSlim serial = new(1, 1);
    private Task<CoreWebView2Environment>? environmentTask;
    private bool disposed;
    public string CacheDirectory { get; }

    public CaptureService(DiagnosticLog log)
    {
        this.log = log;
        CacheDirectory = Path.Combine(Path.GetTempPath(), "Skyworks.ScreenSaver", "Screenshots");
        profilePath = Path.Combine(Path.GetTempPath(), "Skyworks.ScreenSaver", "Profiles", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(CacheDirectory);
        Directory.CreateDirectory(profilePath);
        profileLease = new FileStream(Path.Combine(profilePath, ".lease"), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
        CleanupOldProfiles();
        window.Controls.Add(browser);
    }

    public async Task<string?> CaptureAsync(Uri url, Size viewport, TimeSpan timeout, CancellationToken cancellation) =>
        (await CaptureAttemptAsync(url, viewport, timeout, cancellation)).Page?.FilePath;

    public async Task<CapturedPage?> CaptureTargetAsync(LinkTarget target, Size viewport, TimeSpan timeout, CancellationToken cancellation)
    {
        var outcomes = new List<string>();
        for (int i = 0; i < target.Attempts.Count; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            Uri url = target.Attempts[i];
            if (i > 0) log.Write($"HTTPS was not eligible for {target.Source}; trying unencrypted HTTP as permitted for this scheme-less entry.");
            var result = await CaptureAttemptAsync(url, viewport, timeout, cancellation);
            outcomes.Add($"{url}: {result.Outcome}");
            if (result.Page is not null)
            {
                if (i > 0) log.Write($"HTTP fallback succeeded for scheme-less entry {target.Source}; final main document HTTP 200.");
                return result.Page;
            }
        }
        log.Write($"No eligible screenshot for {target.Source}. All permitted attempts failed: {string.Join(" | ", outcomes)}");
        return null;
    }

    private async Task<AttemptResult> CaptureAttemptAsync(Uri url, Size viewport, TimeSpan timeout, CancellationToken cancellation)
    {
        await serial.WaitAsync(cancellation);
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            deadline.CancelAfter(timeout);
            var token = deadline.Token;
            window.ClientSize = new Size(Math.Clamp(viewport.Width, 320, 3840), Math.Clamp(viewport.Height, 240, 2160));
            if (!window.Visible) window.Show();
            if (browser.CoreWebView2 is null)
            {
                if (environmentTask is null || environmentTask.IsFaulted)
                    environmentTask = CoreWebView2Environment.CreateAsync(null, profilePath);
                var environment = await environmentTask.WaitAsync(token);
                await browser.EnsureCoreWebView2Async(environment).WaitAsync(token);
                var initialized = browser.CoreWebView2 ?? throw new InvalidOperationException("WebView2 initialization did not create a browser.");
                initialized.Settings.AreDefaultContextMenusEnabled = false;
                initialized.Settings.AreDevToolsEnabled = false;
                initialized.Settings.AreDefaultScriptDialogsEnabled = false;
                initialized.Settings.IsStatusBarEnabled = false;
                initialized.IsMuted = true;
                initialized.NewWindowRequested += (_, e) => e.Handled = true;
                initialized.PermissionRequested += (_, e) => e.State = CoreWebView2PermissionState.Deny;
                initialized.DownloadStarting += (_, e) => e.Cancel = true;
            }

            var core = browser.CoreWebView2 ?? throw new InvalidOperationException("WebView2 is not initialized.");
            var completed = new TaskCompletionSource<CoreWebView2NavigationCompletedEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
            ulong? navigationId = null;
            EventHandler<CoreWebView2NavigationStartingEventArgs> starting = (_, e) =>
            {
                navigationId = e.NavigationId;
            };
            EventHandler<CoreWebView2NavigationCompletedEventArgs> finished = (_, e) =>
            {
                if (e.NavigationId == navigationId) completed.TrySetResult(e);
            };
            core.NavigationStarting += starting;
            core.NavigationCompleted += finished;
            try
            {
                core.Navigate(url.AbsoluteUri);
                var result = await completed.Task.WaitAsync(token);
                // NavigationCompleted's status is authoritative for the main frame, never iframe/subresource responses.
                int status = result.HttpStatusCode;
                if (!result.IsSuccess || status != 200)
                    throw new InvalidOperationException($"Main document returned HTTP {status} ({result.WebErrorStatus}).");
                await core.ExecuteScriptAsync("""
                    (() => {
                      window.__skyworksReady = false;
                      Promise.all([
                        document.fonts ? document.fonts.ready : Promise.resolve(),
                        ...Array.from(document.images).filter(i => !i.complete).map(i => new Promise(r => {
                          i.addEventListener('load', r, {once:true}); i.addEventListener('error', r, {once:true});
                        }))
                      ]).then(() => requestAnimationFrame(() => requestAnimationFrame(() => window.__skyworksReady = true)));
                    })();
                    """).WaitAsync(token);
                var renderDeadline = DateTime.UtcNow.AddSeconds(5);
                bool ready = false;
                while (DateTime.UtcNow < renderDeadline)
                {
                    if (await core.ExecuteScriptAsync("window.__skyworksReady === true && document.readyState === 'complete'").WaitAsync(token) == "true")
                    {
                        ready = true;
                        break;
                    }
                    await Task.Delay(100, token);
                }
                if (!ready) log.Write($"Render readiness wait reached its five-second limit for {url.Host}; capturing the completed document's current state.");
                await Task.Delay(750, token);
                // If the page navigated itself while rendering, do not capture its unverified replacement.
                if (navigationId != result.NavigationId)
                    throw new InvalidOperationException("Main document navigated again before capture.");
                Uri document = new(core.Source);
                if (document.Scheme != Uri.UriSchemeHttp && document.Scheme != Uri.UriSchemeHttps)
                    throw new InvalidOperationException("The rendered main document is not an HTTP/HTTPS webpage.");
                string destination = Path.Combine(CacheDirectory, $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.png");
                string partial = destination + ".partial";
                // The asynchronous operation owns the stream even when its caller times out.
                var save = SaveCaptureAsync(core, partial, destination, () => navigationId == result.NavigationId, token);
                try { return new AttemptResult(new CapturedPage(await save.WaitAsync(token), document), "final main HTTP 200; rendered screenshot stored"); }
                catch (OperationCanceledException)
                {
                    browser.Dispose();
                    browser = new WebView2 { Dock = DockStyle.Fill };
                    window.Controls.Add(browser);
                    _ = ObserveAbandonedCaptureAsync(save);
                    throw;
                }
            }
            finally
            {
                if (!disposed)
                {
                    try
                    {
                        core.Stop();
                        core.NavigationStarting -= starting;
                        core.NavigationCompleted -= finished;
                    }
                    catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)
                    { log.Write("Browser was disposed during cancellation.", ex); }
                }
            }
        }
        catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
        {
            string outcome = $"timed out after {timeout.TotalSeconds} seconds";
            log.Write($"Capture attempt for {url} {outcome}.");
            return new AttemptResult(null, outcome);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            log.Write($"Capture attempt failed for {url}; retaining existing images.", ex);
            return new AttemptResult(null, ex.Message);
        }
        finally { serial.Release(); }
    }

    private async Task<string> SaveCaptureAsync(CoreWebView2 core, string partial, string destination, Func<bool> sameDocument, CancellationToken token)
    {
        try
        {
            await using (var output = new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await core.CapturePreviewAsync(CoreWebView2CapturePreviewImageFormat.Png, output);
                token.ThrowIfCancellationRequested();
                await output.FlushAsync(token);
            }
            if (!sameDocument()) throw new InvalidOperationException("Main document changed during screenshot capture.");
            token.ThrowIfCancellationRequested();
            File.Move(partial, destination);
            log.Write($"Stored verified HTTP 200 screenshot: {destination}");
            return destination;
        }
        finally
        {
            try { if (File.Exists(partial)) File.Delete(partial); }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write("Incomplete capture cleanup failed.", ex); }
        }
    }

    private async Task ObserveAbandonedCaptureAsync(Task<string> save)
    {
        try { await save; }
        catch (Exception ex) { log.Write("Cancelled native capture finished.", ex); }
    }

    public void CleanupCache(IReadOnlySet<string> retained)
    {
        try
        {
            var files = new DirectoryInfo(CacheDirectory).GetFiles("*.png").OrderByDescending(f => f.LastWriteTimeUtc).ToArray();
            foreach (var file in files.Select((file, index) => (file, index)))
                if (!retained.Contains(file.file.FullName) && (file.index >= 200 || file.file.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-7)))
                {
                    try { file.file.Delete(); }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write($"Cached image still in use or inaccessible: {file.file.FullName}", ex); }
                }
            foreach (var partial in new DirectoryInfo(CacheDirectory).GetFiles("*.png.partial").Where(f => f.LastWriteTimeUtc < DateTime.UtcNow.AddDays(-1)))
            {
                try
                {
                    using (var lease = new FileStream(partial.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) { }
                    partial.Delete();
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write($"Incomplete screenshot still locked or inaccessible: {partial.FullName}", ex); }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write("Cache cleanup failed.", ex); }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        browser.Dispose();
        window.Dispose();
        serial.Dispose();
        profileLease.Dispose();
        // The runtime can hold profile files briefly after disposal. Only this session's exact profile is removed.
        try { if (Directory.Exists(profilePath)) Directory.Delete(profilePath, true); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write($"Profile cleanup deferred: {profilePath}", ex); }
    }

    private void CleanupOldProfiles()
    {
        try
        {
            string parent = Path.GetDirectoryName(profilePath)!;
            foreach (var directory in new DirectoryInfo(parent).EnumerateDirectories())
            {
                if (!Guid.TryParseExact(directory.Name, "N", out _) || directory.CreationTimeUtc > DateTime.UtcNow.AddDays(-7)) continue;
                try
                {
                    // An active session retains an exclusive lease, even when it runs for more than a week.
                    using (var lease = new FileStream(Path.Combine(directory.FullName, ".lease"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) { }
                    directory.Delete(true);
                }
                catch (IOException ex) { log.Write($"Old profile still in use or locked: {directory.FullName}", ex); }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { log.Write("Old profile cleanup failed.", ex); }
    }
}
