using System.Drawing.Drawing2D;
using System.Reflection;

namespace SkySpec.ScreenSaver;

internal sealed class StatusDashboardControl : UserControl {
    private const string CheckingText = "Checking...";
    private readonly EndpointHealthChecker _healthChecker = new();
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly System.Windows.Forms.Timer _countdownTimer = new() { Interval = 1000 };
    private readonly TableLayoutPanel _statusGrid = new();
    private readonly Label _lastCheckedLabel = new();
    private readonly CountdownClockControl _countdownClock = new();
    private readonly List<StatusColumn> _columns = [];
    private readonly bool _compact;
    private CancellationTokenSource? _refreshCancellation;
    private ScreenSaverSettings _settings = new();
    private DateTime? _lastCheckedAt;
    private DateTime? _nextRefreshAt;

    public event EventHandler<EnvironmentEndpoint>? EnvironmentSelected;

    public event MouseEventHandler? BackgroundMouseDown;

    public event EventHandler? CloseRequested;

    public StatusDashboardControl(bool compact = false) {
        _compact = compact;
        BackColor = Color.FromArgb(10, 18, 32);
        ForeColor = Color.White;
        Dock = DockStyle.Fill;

        var layout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            BackColor = BackColor,
            ColumnCount = 1,
            RowCount = 3,
            Padding = compact ? new Padding(10) : new Padding(24)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 18));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 72));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 10));

        var title = new Label {
            Text = ApplicationBranding.Title,
            Dock = DockStyle.Fill,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", compact ? 16 : 28, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };
        var titleContainer = new Panel {
            Dock = DockStyle.Fill,
            BackColor = BackColor
        };
        titleContainer.Controls.Add(title);
        if (compact) {
            var closeButton = new Button {
                Text = "\u00D7",
                AccessibleName = "Close widget",
                Dock = DockStyle.Right,
                Width = 34,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(185, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TabStop = false,
                Cursor = Cursors.Hand,
                Margin = Padding.Empty
            };
            closeButton.FlatAppearance.BorderSize = 0;
            closeButton.SizeChanged += (_, _) =>
                ApplyRoundedRegion(closeButton, WidgetForm.WidgetCornerRadius / 2);
            closeButton.Click += (_, _) => CloseRequested?.Invoke(this, EventArgs.Empty);
            titleContainer.Controls.Add(closeButton);
            closeButton.BringToFront();
        }

        _statusGrid.Dock = DockStyle.Fill;
        _statusGrid.BackColor = BackColor;
        _statusGrid.ColumnCount = 1;
        _statusGrid.RowCount = 1;

        _lastCheckedLabel.AutoSize = true;
        _lastCheckedLabel.Anchor = AnchorStyles.None;
        _lastCheckedLabel.ForeColor = Color.Silver;
        _lastCheckedLabel.Font = new Font("Segoe UI", compact ? 8 : 11);
        _lastCheckedLabel.TextAlign = ContentAlignment.MiddleCenter;
        _lastCheckedLabel.Text = CheckingText;

        var footerLayout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            BackColor = BackColor,
            ColumnCount = 3,
            RowCount = 1,
            Margin = Padding.Empty
        };
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footerLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var countdownLayout = new FlowLayoutPanel {
            AutoSize = true,
            Anchor = AnchorStyles.None,
            BackColor = BackColor,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = Padding.Empty
        };
        _countdownClock.Anchor = AnchorStyles.None;
        _countdownClock.Size = compact ? new Size(24, 24) : new Size(34, 34);
        _countdownClock.Margin = new Padding(4, 0, 4, 0);
        _countdownClock.Visible = false;
        countdownLayout.Controls.Add(_lastCheckedLabel);
        countdownLayout.Controls.Add(_countdownClock);
        footerLayout.Controls.Add(countdownLayout, 1, 0);
        var versionLabel = new Label {
            Text = $"v{Assembly.GetEntryAssembly()?.GetName().Version}",
            AutoSize = true,
            Anchor = AnchorStyles.Right,
            ForeColor = Color.FromArgb(115, 130, 150),
            Font = new Font("Segoe UI", compact ? 7 : 9),
            Margin = new Padding(0, 0, 4, 0)
        };
        footerLayout.Controls.Add(versionLabel, 2, 0);

        layout.Controls.Add(titleContainer, 0, 0);
        layout.Controls.Add(_statusGrid, 0, 1);
        layout.Controls.Add(footerLayout, 0, 2);
        Controls.Add(layout);

        foreach (var control in new Control[]
                 {
                     this,
                     layout,
                     titleContainer,
                     title,
                     _statusGrid,
                     _lastCheckedLabel,
                     versionLabel,
                     countdownLayout,
                     footerLayout,
                     _countdownClock
                 }) {
            control.MouseDown += (_, eventArgs) => BackgroundMouseDown?.Invoke(this, eventArgs);
        }

        _timer.Tick += async (_, _) => await RefreshStatusesAsync();
        _countdownTimer.Tick += (_, _) => UpdateFooter();
    }

    private static void ApplyRoundedRegion(Control control, int cornerRadius) {
        var diameter = cornerRadius * 2;
        if (control.Width < diameter || control.Height < diameter) {
            return;
        }

        using var path = new GraphicsPath();
        path.AddArc(0, 0, diameter, diameter, 180, 90);
        path.AddArc(control.Width - diameter, 0, diameter, diameter, 270, 90);
        path.AddArc(
            control.Width - diameter,
            control.Height - diameter,
            diameter,
            diameter,
            0,
            90);
        path.AddArc(0, control.Height - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        var previousRegion = control.Region;
        control.Region = new Region(path);
        previousRegion?.Dispose();
    }

    public void ApplySettings(ScreenSaverSettings settings) {
        var wasRunning = _lastCheckedAt is not null;
        _timer.Stop();
        _settings = settings;
        _timer.Interval = Math.Max(1, settings.RefreshIntervalMinutes) * 60 * 1000;

        _statusGrid.Controls.Clear();
        _statusGrid.ColumnStyles.Clear();
        _columns.Clear();

        var includedEndpoints = settings.Endpoints
            .Where(endpoint => endpoint.Include)
            .ToList();
        _statusGrid.ColumnCount = Math.Max(1, includedEndpoints.Count);
        var columnWidth = 100f / Math.Max(1, includedEndpoints.Count);
        for (var index = 0; index < Math.Max(1, includedEndpoints.Count); index++) {
            _statusGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, columnWidth));
        }

        foreach (var endpoint in includedEndpoints) {
            var column = new StatusColumn(endpoint, _compact);
            column.Selected += (_, selectedEndpoint) =>
                EnvironmentSelected?.Invoke(this, selectedEndpoint);
            _columns.Add(column);
            _statusGrid.Controls.Add(column.Container);
        }

        if (wasRunning) {
            _nextRefreshAt = DateTime.Now.AddMinutes(settings.RefreshIntervalMinutes);
            UpdateFooter();
            _timer.Start();
        }
    }

    public async Task StartAsync() {
        await RefreshStatusesAsync();
        _countdownTimer.Start();
    }

    public async Task RefreshStatusesAsync() {
        _timer.Stop();
        _lastCheckedLabel.Text = CheckingText;
        _countdownClock.Visible = false;
        _refreshCancellation?.Cancel();
        _refreshCancellation?.Dispose();
        _refreshCancellation = new CancellationTokenSource();

        for (var index = 0; index < _columns.Count; index++) {
            _columns[index].SetChecking();
        }

        var checks = _settings.Endpoints
            .Where(endpoint => endpoint.Include)
            .Select(
            endpoint => _healthChecker.CheckAsync(endpoint, _refreshCancellation.Token));

        EndpointHealth[] results;
        try {
            results = await Task.WhenAll(checks);
        }
        catch (OperationCanceledException) {
            return;
        }

        for (var index = 0; index < results.Length && index < _columns.Count; index++) {
            _columns[index].SetResult(results[index]);
        }

        _lastCheckedAt = DateTime.Now;
        _nextRefreshAt = _lastCheckedAt.Value.AddMinutes(_settings.RefreshIntervalMinutes);
        _lastCheckedLabel.Text =
            $"Last checked {_lastCheckedAt.Value:G}  |  Refresh: {_settings.RefreshIntervalMinutes}m";
        _countdownClock.FractionRemaining = 1;
        _countdownClock.Visible = true;
        _timer.Start();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _timer.Dispose();
            _countdownTimer.Dispose();
            _refreshCancellation?.Cancel();
            _refreshCancellation?.Dispose();
            _healthChecker.Dispose();
        }

        base.Dispose(disposing);
    }

    private void UpdateFooter() {
        if (_lastCheckedAt is null || _nextRefreshAt is null) {
            _lastCheckedLabel.Text = CheckingText;
            _countdownClock.FractionRemaining = 0;
            _countdownClock.Visible = false;
            return;
        }

        _countdownClock.Visible = true;
        var remaining = _nextRefreshAt.Value - DateTime.Now;
        if (remaining < TimeSpan.Zero) {
            remaining = TimeSpan.Zero;
        }

        _lastCheckedLabel.Text =
            $"Last checked {_lastCheckedAt.Value:G}  |  Refresh: {_settings.RefreshIntervalMinutes}m";

        var countdownDuration = _nextRefreshAt.Value - _lastCheckedAt.Value;
        var stagedSeconds = Math.Ceiling(remaining.TotalSeconds / 15d) * 15d;
        _countdownClock.FractionRemaining =
            stagedSeconds / countdownDuration.TotalSeconds;
    }

    private sealed class StatusColumn {
        private readonly Label _indicator;
        private readonly Label _detail;
        private readonly ToolTip _detailToolTip = new();
        private readonly Control[] _toolTipTargets;
        private string? _hoverDetailText;
        private bool _showDetailOnlyOnHover;

        public StatusColumn(EnvironmentEndpoint endpoint, bool compact) {
            Container = new TableLayoutPanel {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(19, 31, 50),
                Margin = compact ? new Padding(4) : new Padding(10),
                Padding = compact ? new Padding(3) : new Padding(8),
                RowCount = 3,
                ColumnCount = 1
            };
            Container.SizeChanged += (_, _) =>
                ApplyRoundedRegion(Container, WidgetForm.WidgetCornerRadius / 2);
            Container.Cursor = Cursors.Hand;
            Container.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
            Container.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            Container.RowStyles.Add(new RowStyle(SizeType.Percent, 20));

            var nameLabel = new Label {
                Text = endpoint.Name,
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", compact ? 13 : 22, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _indicator = new Label {
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ForeColor = Color.Goldenrod,
                Font = new Font("Segoe UI Symbol", compact ? 32 : 60, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _detail = new Label {
                Dock = DockStyle.Fill,
                Cursor = Cursors.Hand,
                ForeColor = Color.Silver,
                Font = new Font("Segoe UI", compact ? 8 : 11),
                TextAlign = ContentAlignment.TopCenter
            };

            Container.Controls.Add(nameLabel, 0, 0);
            Container.Controls.Add(_indicator, 0, 1);
            Container.Controls.Add(_detail, 0, 2);

            _toolTipTargets = [Container, nameLabel, _indicator, _detail];
            foreach (var control in _toolTipTargets) {
                control.Click += (_, _) => Selected?.Invoke(this, endpoint);
                control.MouseEnter += (_, _) => ShowHoverDetail();
                control.MouseLeave += (_, _) => HideHoverDetailWhenOutsideCard();
            }

            Container.Disposed += (_, _) => _detailToolTip.Dispose();
        }

        public event EventHandler<EnvironmentEndpoint>? Selected;

        public TableLayoutPanel Container { get; }

        public void SetChecking() {
            _indicator.Text = "...";
            _indicator.ForeColor = Color.Goldenrod;
            _detail.Text = CheckingText;
            _hoverDetailText = null;
            _showDetailOnlyOnHover = false;
            SetToolTip(null);
        }

        public void SetResult(EndpointHealth health) {
            var isLocalWarning = !health.IsHealthy
                && string.Equals(
                    health.Endpoint.Name,
                    "Local",
                    StringComparison.OrdinalIgnoreCase);
            _indicator.Text = health.IsHealthy
                ? "\u2713"
                : isLocalWarning ? "\u26A0" : "\u2717";
            _indicator.ForeColor = health.IsHealthy
                ? Color.FromArgb(55, 205, 120)
                : isLocalWarning
                    ? Color.FromArgb(245, 190, 55)
                    : Color.FromArgb(245, 80, 80);
            if (health.StatusCode is not null) {
                var statusText = $"HTTP {health.StatusCode} {health.Detail}";
                _hoverDetailText = health.IsHealthy ? statusText : null;
                _showDetailOnlyOnHover = health.IsHealthy;
                _detail.Text = health.IsHealthy ? string.Empty : statusText;
                SetToolTip(null);
                return;
            }

            _hoverDetailText = null;
            _showDetailOnlyOnHover = false;
            _detail.Text = "Unreachable";
            SetToolTip(health.Detail ?? "Unavailable");
        }

        private void ShowHoverDetail() {
            if (_showDetailOnlyOnHover) {
                _detail.Text = _hoverDetailText;
            }
        }

        private void HideHoverDetailWhenOutsideCard() {
            if (_showDetailOnlyOnHover
                && !Container.RectangleToScreen(Container.ClientRectangle).Contains(Cursor.Position)) {
                _detail.Text = string.Empty;
            }
        }

        private void SetToolTip(string? text) {
            foreach (var control in _toolTipTargets) {
                _detailToolTip.SetToolTip(control, text);
            }
        }
    }

    internal sealed class CountdownClockControl : Control {
        private double _fractionRemaining;

        public CountdownClockControl() {
            DoubleBuffered = true;
            BackColor = Color.FromArgb(10, 18, 32);
        }

        public double FractionRemaining {
            get => _fractionRemaining;
            set {
                _fractionRemaining = Math.Clamp(value, 0d, 1d);
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs eventArgs) {
            base.OnPaint(eventArgs);

            eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var strokeWidth = Math.Max(1f, Math.Min(ClientSize.Width, ClientSize.Height) / 16f);
            var inset = strokeWidth / 2f + 1f;
            var diameter = Math.Max(
                0f,
                Math.Min(ClientSize.Width, ClientSize.Height) - (inset * 2f));
            var clockBounds = new RectangleF(inset, inset, diameter, diameter);

            using var backgroundBrush = new SolidBrush(Color.FromArgb(32, 48, 68));
            eventArgs.Graphics.FillEllipse(backgroundBrush, clockBounds);

            if (_fractionRemaining > 0) {
                using var countdownBrush = new SolidBrush(Color.FromArgb(55, 205, 120));
                eventArgs.Graphics.FillPie(
                    countdownBrush,
                    clockBounds,
                    -90f,
                    (float)(360d * _fractionRemaining));
            }

            using var borderPen = new Pen(Color.FromArgb(80, 100, 125), strokeWidth);
            eventArgs.Graphics.DrawEllipse(borderPen, clockBounds);
        }
    }
}
