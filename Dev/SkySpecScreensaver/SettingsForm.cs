namespace SkySpec.ScreenSaver;

internal sealed class SettingsForm : Form {
    private readonly NumericUpDown _refreshInterval = new();
    private readonly CheckBox _startWidgetWithWindows = new();
    private readonly CheckBox _widgetAlwaysOnTop = new();
    private readonly ComboBox _screenSaverMonitor = new();
    private readonly TextBox[] _urlInputs = new TextBox[4];
    private readonly CheckBox[] _includeInputs = new CheckBox[4];
    private readonly StatusDashboardControl _preview = new();
    private readonly string[] _names = ["Local", "Dev", "Stage", "Prod"];
    private readonly ScreenSaverSettings _loadedSettings;

    public SettingsForm() {
        Text = $"{ApplicationBranding.Title} Settings";
        Icon = ApplicationBranding.Icon;
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 650);
        Size = new Size(1100, 760);

        _loadedSettings = ScreenSaverSettings.Load();
        var root = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var options = CreateOptionsPanel(_loadedSettings);
        _preview.ApplySettings(_loadedSettings);

        var buttons = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 0)
        };
        var cancelButton = new Button {
            Text = "&Cancel",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };
        var saveButton = new Button { Text = "&Save", AutoSize = true };
        var refreshButton = new Button { Text = "Refresh &Preview", AutoSize = true };
        cancelButton.Click += (_, _) => Close();
        saveButton.Click += (_, _) => SaveAndClose();
        refreshButton.Click += async (_, _) => await UpdatePreviewAsync();
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(refreshButton);
        CancelButton = cancelButton;

        root.Controls.Add(options, 0, 0);
        root.Controls.Add(_preview, 0, 1);
        root.Controls.Add(buttons, 0, 2);
        Controls.Add(root);

        Shown += async (_, _) => await _preview.StartAsync();
    }

    private Control CreateOptionsPanel(ScreenSaverSettings settings) {
        var options = new TableLayoutPanel {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 8,
            Padding = new Padding(0, 0, 0, 12)
        };
        options.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        options.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        options.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        for (var index = 0; index < _names.Length; index++) {
            options.Controls.Add(
                new Label {
                    Text = $"{_names[index]} URL",
                    AutoSize = true,
                    Anchor = AnchorStyles.Right,
                    TextAlign = ContentAlignment.MiddleRight,
                    Margin = new Padding(3, 7, 12, 7)
                },
                0,
                index);

            _urlInputs[index] = new TextBox {
                Text = settings.Endpoints[index].Url,
                Dock = DockStyle.Fill,
                Margin = new Padding(3, 4, 3, 4)
            };
            options.Controls.Add(_urlInputs[index], 1, index);

            _includeInputs[index] = new CheckBox {
                Text = "Include",
                Checked = settings.Endpoints[index].Include,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(8, 6, 3, 6)
            };
            options.Controls.Add(_includeInputs[index], 2, index);
        }

        options.Controls.Add(
            new Label {
                Text = "Refresh interval (m)",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(3, 7, 12, 7)
            },
            0,
            4);

        _refreshInterval.Minimum = 1;
        _refreshInterval.Maximum = 1440;
        _refreshInterval.Value = Math.Clamp(settings.RefreshIntervalMinutes, 1, 1440);
        _refreshInterval.Width = 80;
        options.Controls.Add(_refreshInterval, 1, 4);

        var screens = Screen.AllScreens;
        options.Controls.Add(
            new Label {
                Text = $"Screen saver monitor",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(3, 7, 12, 7)
            },
            0,
            5);

        _screenSaverMonitor.DropDownStyle = ComboBoxStyle.DropDownList;
        _screenSaverMonitor.Dock = DockStyle.Left;
        _screenSaverMonitor.Width = 220;
        for (var index = 0; index < screens.Length; index++) {
            var screen = screens[index];
            _screenSaverMonitor.Items.Add(
                new MonitorOption(
                    screen.DeviceName,
                    $"Monitor {index + 1}{(screen.Primary ? " (Primary)" : string.Empty)}"));
        }

        var selectedMonitorIndex = _screenSaverMonitor.Items
            .Cast<MonitorOption>()
            .Select((monitor, index) => new { monitor, index })
            .FirstOrDefault(item => string.Equals(
                item.monitor.DeviceName,
                settings.ScreenSaverMonitorDeviceName,
                StringComparison.OrdinalIgnoreCase))
            ?.index;
        _screenSaverMonitor.SelectedIndex = selectedMonitorIndex
            ?? Array.FindIndex(screens, screen => screen.Primary);
        options.Controls.Add(_screenSaverMonitor, 1, 5);
        options.SetColumnSpan(_screenSaverMonitor, 2);

        _startWidgetWithWindows.Text = "Start desktop widget when I sign in to Windows";
        _startWidgetWithWindows.Checked = settings.StartWidgetWithWindows;
        _startWidgetWithWindows.AutoSize = true;
        options.Controls.Add(_startWidgetWithWindows, 1, 6);
        options.SetColumnSpan(_startWidgetWithWindows, 2);

        _widgetAlwaysOnTop.Text = "Keep desktop widget above other windows";
        _widgetAlwaysOnTop.Checked = settings.WidgetAlwaysOnTop;
        _widgetAlwaysOnTop.AutoSize = true;
        options.Controls.Add(_widgetAlwaysOnTop, 1, 7);
        options.SetColumnSpan(_widgetAlwaysOnTop, 2);

        return options;
    }

    private ScreenSaverSettings ReadSettings() {
        var endpoints = new List<EnvironmentEndpoint>();
        for (var index = 0; index < _names.Length; index++) {
            if (!Uri.TryCreate(_urlInputs[index].Text.Trim(), UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)) {
                throw new InvalidOperationException($"{_names[index]} must have a valid HTTP or HTTPS URL.");
            }

            endpoints.Add(
                new(_names[index], uri.AbsoluteUri) {
                    Include = _includeInputs[index].Checked
                });
        }

        if (!int.TryParse(_refreshInterval.Text, out var refreshIntervalMinutes)
            || refreshIntervalMinutes < _refreshInterval.Minimum
            || refreshIntervalMinutes > _refreshInterval.Maximum) {
            throw new InvalidOperationException(
                $"Refresh interval must be between {_refreshInterval.Minimum} and {_refreshInterval.Maximum} minutes.");
        }

        return new() {
            RefreshIntervalMinutes = refreshIntervalMinutes,
            Endpoints = endpoints,
            StartWidgetWithWindows = _startWidgetWithWindows.Checked,
            WidgetAlwaysOnTop = _widgetAlwaysOnTop.Checked,
            ShowWidgetOnTaskbar = _loadedSettings.ShowWidgetOnTaskbar,
            ScreenSaverMonitorDeviceName =
                (_screenSaverMonitor.SelectedItem as MonitorOption)?.DeviceName,
            WidgetLeft = _loadedSettings.WidgetLeft,
            WidgetTop = _loadedSettings.WidgetTop,
            WidgetWidth = _loadedSettings.WidgetWidth,
            WidgetHeight = _loadedSettings.WidgetHeight
        };
    }

    private async Task UpdatePreviewAsync() {
        try {
            _preview.ApplySettings(ReadSettings());
            await _preview.RefreshStatusesAsync();
        }
        catch (InvalidOperationException exception) {
            MessageBox.Show(this, exception.Message, "Invalid setting", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void SaveAndClose() {
        try {
            var settings = ReadSettings();
            settings.Save();
            WidgetStartupRegistration.Apply(settings.StartWidgetWithWindows);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (InvalidOperationException exception) {
            MessageBox.Show(this, exception.Message, "Invalid setting", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (IOException exception) {
            MessageBox.Show(this, exception.Message, "Could not save settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (UnauthorizedAccessException exception) {
            MessageBox.Show(this, exception.Message, "Could not update Windows startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (System.Security.SecurityException exception) {
            MessageBox.Show(this, exception.Message, "Could not update Windows startup", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private sealed record MonitorOption(string DeviceName, string DisplayName) {
        public override string ToString() => DisplayName;
    }
}
