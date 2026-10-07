using Skyworks.ScreenSaver.Core;

namespace Skyworks.ScreenSaver;

internal sealed class SettingsDialog : Form
{
    private readonly NumericUpDown refresh = new() { Minimum = 5, Maximum = 86400 };
    private readonly NumericUpDown timeout = new() { Minimum = 10, Maximum = 180 };
    private readonly SettingsStore store;
    private readonly DiagnosticLog log;

    public SettingsDialog(SaverSettings settings, SettingsStore store, DiagnosticLog log, string? warning)
    {
        this.store = store;
        this.log = log;
        Text = "Skyworks screensaver settings";
        StartPosition = FormStartPosition.CenterScreen;
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(650, 291);
        MinimumSize = new Size(550, 291);
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 5 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        AddRow(layout, 0, "Refresh interval (seconds)", refresh);
        AddRow(layout, 1, "Capture timeout (seconds)", timeout);
        var description = new Label
        {
            Text = "Backgrounds and the webpage list are built into the application. Images rotate and webpages refresh on the same interval. Each image change updates only one monitor.",
            Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 10, 0, 10)
        };
        layout.Controls.Add(description, 0, 2);
        layout.SetColumnSpan(description, 2);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        var save = new Button { Text = "Save", AutoSize = true };
        save.Click += (_, _) => { if (SaveSettings()) Close(); };
        var preview = new Button { Text = "Full preview", AutoSize = true };
        preview.Click += (_, _) =>
        {
            if (!SaveSettings()) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!, "/s") { UseShellExecute = false });
            }
            catch (Exception ex) { log.Write("Cannot start preview.", ex); MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        var diagnostics = new Button { Text = "Diagnostics", AutoSize = true };
        diagnostics.Click += (_, _) =>
        {
            try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("notepad.exe", $"\"{log.FilePath}\"") { UseShellExecute = false }); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        var cancel = new Button { Text = "Cancel", AutoSize = true, DialogResult = DialogResult.Cancel };
        buttons.Controls.AddRange([save, preview, diagnostics, cancel]);
        layout.Controls.Add(buttons, 0, 3);
        layout.SetColumnSpan(buttons, 2);
        var install = new Button { Text = "Set as Windows screensaver", AutoSize = true, Anchor = AnchorStyles.Left };
        install.Click += (_, _) =>
        {
            try
            {
                ScreenSaverRegistration.ResolveDeployment(AppContext.BaseDirectory, Environment.ProcessPath!);
                if (!SaveSettings()) return;
                var installation = ScreenSaverRegistration.PrepareInstallation(AppContext.BaseDirectory, Environment.ProcessPath!, store.Load());
                store.Save(installation.Settings);
                ScreenSaverRegistration.SelectAndOpen(installation.ScreenSaverPath);
                log.Write($"Opened Windows install/select for per-user deployment: {installation.ScreenSaverPath}");
            }
            catch (Exception ex)
            {
                log.Write("Cannot register the Windows screensaver.", ex);
                MessageBox.Show(this, ex.Message, "Set as Windows screensaver", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };
        layout.Controls.Add(install, 0, 4);
        layout.SetColumnSpan(install, 2);
        Controls.Add(layout);
        AcceptButton = save;
        CancelButton = cancel;
        refresh.Value = settings.RefreshSeconds;
        timeout.Value = settings.CaptureTimeoutSeconds;
        if (warning is not null) Shown += (_, _) => MessageBox.Show(this, warning, "Settings could not be loaded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static void AddRow(TableLayoutPanel layout, int row, string label, Control control)
    {
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        layout.Controls.Add(control, 1, row);
    }

    private bool SaveSettings()
    {
        try
        {
            var settings = ReadSettings();
            settings.Validate();
            var result = EmbeddedAssets.LoadLinks();
            if (result.Diagnostics.Count > 0)
                MessageBox.Show(this, string.Join(Environment.NewLine, result.Diagnostics), "Invalid URL entries will be skipped", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            store.Save(settings);
            log.Write("User settings saved.");
            return true;
        }
        catch (Exception ex)
        {
            log.Write("Cannot save settings.", ex);
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

    }

    private SaverSettings ReadSettings() => new()
    {
        RefreshSeconds = (int)refresh.Value,
        CaptureTimeoutSeconds = (int)timeout.Value
    };
}
