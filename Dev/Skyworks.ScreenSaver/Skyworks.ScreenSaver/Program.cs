using Skyworks.ScreenSaver.Core;

namespace Skyworks.ScreenSaver;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        var store = new SettingsStore();
        var log = new DiagnosticLog(store.DirectoryPath);
        log.Write("Screensaver process starting.");
        Application.ThreadException += (_, e) => log.Write("UI thread error.", e.Exception);
        try
        {
            var options = LaunchOptions.Parse(args);
            SaverSettings settings;
            string? warning = null;
            try { settings = store.Load(); }
            catch (Exception ex)
            {
                log.Write("Invalid saved settings; using documented defaults without overwriting the settings file.", ex);
                warning = ex.Message;
                settings = new SaverSettings();
            }
            if (options.Mode == SaverMode.Settings)
            {
                using var dialog = new SettingsDialog(settings, store, log, warning);
                if (options.ParentHandle != 0 && NativeMethods.IsWindow(options.ParentHandle))
                    dialog.ShowDialog(new WindowOwner(options.ParentHandle));
                else dialog.ShowDialog();
            }
            else
            {
                using var context = new SaverContext(settings, log, options.Mode == SaverMode.Preview ? options.ParentHandle : 0);
                Application.Run(context);
            }
        }
        catch (Exception ex)
        {
            log.Write("Screensaver could not start.", ex);
            if (!args.Any(a => a.StartsWith("/s", StringComparison.OrdinalIgnoreCase) || a.StartsWith("/p", StringComparison.OrdinalIgnoreCase)))
                MessageBox.Show(ex.Message, "Skyworks screensaver", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.ExitCode = 1;
        }
    }

    private sealed record WindowOwner(nint Handle) : IWin32Window;
}
