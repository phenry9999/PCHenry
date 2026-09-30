namespace SkySpec.ScreenSaver;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var command = ScreenSaverCommand.Parse(args);
            switch (command.Mode)
            {
                case ScreenSaverMode.Configure:
                    Application.Run(new SettingsForm());
                    break;
                case ScreenSaverMode.Preview when command.PreviewHandle != IntPtr.Zero:
                    Application.Run(new PreviewForm(command.PreviewHandle));
                    break;
                case ScreenSaverMode.FullScreen:
                    Application.Run(new ScreenSaverApplicationContext());
                    break;
                case ScreenSaverMode.Widget:
                    using (var widgetMutex = new Mutex(
                        initiallyOwned: true,
                        name: @"Local\SkySpecStatusWidget",
                        createdNew: out var createdNew))
                    {
                        if (createdNew)
                        {
                            Application.Run(new WidgetForm());
                        }
                    }
                    break;
                default:
                    Application.Run(new SettingsForm());
                    break;
            }
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(
                exception.Message,
                "SkySpec settings error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
