using System.Drawing.Drawing2D;

namespace SkySpec.ScreenSaver;

internal sealed class WidgetForm : Form {
    internal const int WidgetCornerRadius = 10;
    private readonly StatusDashboardControl _dashboard = new(compact: true);
    private readonly NotifyIcon _trayIcon;
    private readonly ToolStripMenuItem _alwaysOnTopMenuItem;
    private readonly ToolStripMenuItem _showOnTaskbarMenuItem;
    private readonly System.Windows.Forms.Timer _saveBoundsTimer = new() { Interval = 500 };
    private ScreenSaverSettings _settings = new();
    private bool _initialized;

    public WidgetForm() {
        _settings = ScreenSaverSettings.Load();
        Text = ApplicationBranding.Title;
        Icon = ApplicationBranding.Icon;
        ShowInTaskbar = true;
        KeyPreview = true;
        StartPosition = FormStartPosition.Manual;
        MinimumSize = new Size(600, 220);
        BackColor = Color.FromArgb(10, 18, 32);

        _alwaysOnTopMenuItem = new ToolStripMenuItem("Always on top");
        _alwaysOnTopMenuItem.Click += (_, _) => ToggleAlwaysOnTop();
        _showOnTaskbarMenuItem = new ToolStripMenuItem("Show on taskbar");
        _showOnTaskbarMenuItem.Click += (_, _) => ToggleShowOnTaskbar();
        var menu = new ContextMenuStrip();
        var refreshMenuItem = new ToolStripMenuItem("Refresh now") {
            ShortcutKeyDisplayString = "F5"
        };
        refreshMenuItem.Click += async (_, _) => await _dashboard.RefreshStatusesAsync();
        menu.Items.Add(refreshMenuItem);
        var settingsMenuItem = new ToolStripMenuItem("Settings") {
            ShortcutKeyDisplayString = "Alt+S"
        };
        settingsMenuItem.Click += async (_, _) => await OpenSettingsAsync();
        menu.Items.Add(settingsMenuItem);
        var screenSaverMenuItem = new ToolStripMenuItem("Set as screen saver");
        screenSaverMenuItem.Click += (_, _) => InstallScreenSaver();
        var desktopMenuItem = new ToolStripMenuItem("Put On Desktop");
        desktopMenuItem.Click += (_, _) => PutOnDesktop();
        menu.Items.Add(screenSaverMenuItem);
        menu.Items.Add(desktopMenuItem);
        menu.Items.Add(_showOnTaskbarMenuItem);
        menu.Items.Add(_alwaysOnTopMenuItem);
        menu.Items.Add(new ToolStripSeparator());
        var exitMenuItem = new ToolStripMenuItem("Exit") {
            ShortcutKeyDisplayString = "Alt+F4"
        };
        exitMenuItem.Click += (_, _) => Close();
        menu.Items.Add(exitMenuItem);
        menu.Opening += (_, _) => {
            screenSaverMenuItem.Checked = ScreenSaverInstaller.IsSelected();
            desktopMenuItem.Checked = DesktopShortcut.ExistsForCurrentExecutable();
            _showOnTaskbarMenuItem.Checked = _settings.ShowWidgetOnTaskbar;
        };

        _trayIcon = new NotifyIcon {
            Icon = ApplicationBranding.Icon,
            Text = ApplicationBranding.Title,
            ContextMenuStrip = menu,
            Visible = true
        };
        _trayIcon.DoubleClick += async (_, _) => await OpenSettingsAsync();

        ContextMenuStrip = menu;
        KeyDown += async (_, eventArgs) => {
            if (eventArgs.KeyCode == Keys.F5) {
                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                await _dashboard.RefreshStatusesAsync();
            }
            else if (eventArgs.Alt && eventArgs.KeyCode == Keys.S) {
                eventArgs.Handled = true;
                eventArgs.SuppressKeyPress = true;
                await OpenSettingsAsync();
            }
        };
        Controls.Add(_dashboard);
        _dashboard.CloseRequested += (_, _) => Close();
        _dashboard.EnvironmentSelected += (_, endpoint) => OpenEnvironment(endpoint);
        _dashboard.BackgroundMouseDown += (_, eventArgs) => {
            if (eventArgs.Button != MouseButtons.Left) {
                return;
            }

            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(
                Handle,
                NativeMethods.WmNcLButtonDown,
                new IntPtr(NativeMethods.HtCaption),
                IntPtr.Zero);
        };
        _saveBoundsTimer.Tick += (_, _) => {
            _saveBoundsTimer.Stop();
            SaveBounds();
        };
        LocationChanged += (_, _) => ScheduleBoundsSave();
        ApplySettings(_settings, restoreBounds: true);

        Shown += async (_, _) => {
            _initialized = true;
            await _dashboard.StartAsync();
        };
        FormClosing += (_, _) => SaveBounds();
    }

    protected override void OnSizeChanged(EventArgs eventArgs) {
        base.OnSizeChanged(eventArgs);
        UpdateRoundedRegion();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _saveBoundsTimer.Dispose();
        }

        base.Dispose(disposing);
    }

    private void ApplySettings(ScreenSaverSettings settings, bool restoreBounds) {
        _settings = settings;
        _dashboard.ApplySettings(settings);
        TopMost = settings.WidgetAlwaysOnTop;
        ShowInTaskbar = settings.ShowWidgetOnTaskbar;
        Opacity = 1d;
        FormBorderStyle = FormBorderStyle.None;

        _alwaysOnTopMenuItem.Checked = settings.WidgetAlwaysOnTop;
        _showOnTaskbarMenuItem.Checked = settings.ShowWidgetOnTaskbar;

        if (restoreBounds) {
            RestoreBoundsFromSettings();
        }

        if (IsHandleCreated) {
            RecreateHandle();
        }

        UpdateRoundedRegion();
    }

    private void UpdateRoundedRegion() {
        var diameter = WidgetCornerRadius * 2;
        if (Width < diameter || Height < diameter) {
            return;
        }

        using var path = new GraphicsPath();
        path.AddArc(0, 0, diameter, diameter, 180, 90);
        path.AddArc(Width - diameter, 0, diameter, diameter, 270, 90);
        path.AddArc(Width - diameter, Height - diameter, diameter, diameter, 0, 90);
        path.AddArc(0, Height - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        var previousRegion = Region;
        Region = new Region(path);
        previousRegion?.Dispose();
    }

    private void RestoreBoundsFromSettings() {
        var width = Math.Max(MinimumSize.Width, _settings.WidgetWidth);
        var height = Math.Max(MinimumSize.Height, _settings.WidgetHeight);
        var requestedBounds = new Rectangle(
            _settings.WidgetLeft ?? int.MinValue,
            _settings.WidgetTop ?? int.MinValue,
            width,
            height);

        if (_settings.WidgetLeft is not null
            && _settings.WidgetTop is not null
            && Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(requestedBounds))) {
            Bounds = requestedBounds;
            return;
        }

        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? Screen.GetWorkingArea(Point.Empty);
        Bounds = new Rectangle(
            workingArea.Right - width - 24,
            workingArea.Bottom - height - 24,
            width,
            height);
    }

    private async Task OpenSettingsAsync() {
        using var settingsForm = new SettingsForm();
        if (settingsForm.ShowDialog(this) == DialogResult.OK) {
            ApplySettings(ScreenSaverSettings.Load(), restoreBounds: false);
            await _dashboard.RefreshStatusesAsync();
        }
    }

    private void InstallScreenSaver() {
        try {
            ScreenSaverInstaller.InstallAndSelect();
        }
        catch (Exception exception) when (
            exception is System.ComponentModel.Win32Exception
                or FileNotFoundException
                or InvalidOperationException) {
            MessageBox.Show(
                this,
                exception.Message,
                "Could not set screen saver",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void PutOnDesktop() {
        try {
            var shortcutPath = DesktopShortcut.Create();
            MessageBox.Show(
                this,
                $"A shortcut was created at:{Environment.NewLine}{shortcutPath}",
                "Put On Desktop",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception exception) when (
            exception is System.Runtime.InteropServices.COMException
                or IOException
                or UnauthorizedAccessException) {
            MessageBox.Show(
                this,
                exception.Message,
                "Could not create desktop shortcut",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void OpenEnvironment(EnvironmentEndpoint endpoint) {
        try {
            BrowserLauncher.OpenNewWindow(endpoint.Url);
        }
        catch (Exception exception) when (
            exception is System.ComponentModel.Win32Exception or InvalidOperationException) {
            MessageBox.Show(
                this,
                exception.Message,
                "Could not open URL",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void ToggleAlwaysOnTop() {
        _settings.WidgetAlwaysOnTop = !_settings.WidgetAlwaysOnTop;
        _settings.Save();
        ApplySettings(_settings, restoreBounds: false);
    }

    private void ToggleShowOnTaskbar() {
        _settings.ShowWidgetOnTaskbar = !_settings.ShowWidgetOnTaskbar;
        _settings.Save();
        ShowInTaskbar = _settings.ShowWidgetOnTaskbar;
        _showOnTaskbarMenuItem.Checked = _settings.ShowWidgetOnTaskbar;
    }

    private void SaveBounds() {
        if (!_initialized || WindowState != FormWindowState.Normal) {
            return;
        }

        _settings.WidgetLeft = Left;
        _settings.WidgetTop = Top;
        _settings.WidgetWidth = Width;
        _settings.WidgetHeight = Height;
        _settings.Save();
    }

    private void ScheduleBoundsSave() {
        if (!_initialized || WindowState != FormWindowState.Normal) {
            return;
        }

        _saveBoundsTimer.Stop();
        _saveBoundsTimer.Start();
    }
}
