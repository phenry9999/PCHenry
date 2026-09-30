namespace SkySpec.ScreenSaver;

internal sealed class FullScreenForm : Form
{
    private readonly StatusDashboardControl? _dashboard;
    private readonly Action _requestClose;
    private Point? _initialMousePosition;

    public FullScreenForm(Screen screen, bool displayDashboard, Action requestClose)
    {
        _requestClose = requestClose;
        Icon = ApplicationBranding.Icon;
        Bounds = screen.Bounds;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        TopMost = true;
        ShowInTaskbar = false;
        BackColor = Color.FromArgb(10, 18, 32);
        KeyPreview = true;

        if (displayDashboard)
        {
            _dashboard = new StatusDashboardControl();
            _dashboard.ApplySettings(ScreenSaverSettings.Load());
            Controls.Add(_dashboard);
            _dashboard.MouseDown += (_, _) => _requestClose();
            _dashboard.MouseMove += OnMouseMove;
        }

        KeyDown += (_, _) => _requestClose();
        MouseDown += (_, _) => _requestClose();
        MouseMove += OnMouseMove;
        Shown += async (_, _) =>
        {
            if (_dashboard is not null)
            {
                await _dashboard.StartAsync();
            }
        };
    }

    private void OnMouseMove(object? sender, MouseEventArgs eventArgs)
    {
        _initialMousePosition ??= Cursor.Position;
        var movement = Math.Abs(Cursor.Position.X - _initialMousePosition.Value.X)
            + Math.Abs(Cursor.Position.Y - _initialMousePosition.Value.Y);
        if (movement > 8)
        {
            _requestClose();
        }
    }
}

internal sealed class ScreenSaverApplicationContext : ApplicationContext
{
    private readonly List<FullScreenForm> _forms = [];
    private readonly ScreenSaverInputFilter _inputFilter;
    private bool _closing;

    public ScreenSaverApplicationContext()
    {
        _inputFilter = new ScreenSaverInputFilter(CloseAll);
        Application.AddMessageFilter(_inputFilter);
        Cursor.Hide();

        foreach (var screen in Screen.AllScreens)
        {
            var form = new FullScreenForm(screen, screen.Primary, CloseAll);
            form.FormClosed += (_, _) => CloseAll();
            _forms.Add(form);
            form.Show();
        }
    }

    private void CloseAll()
    {
        if (_closing)
        {
            return;
        }

        _closing = true;
        Application.RemoveMessageFilter(_inputFilter);
        foreach (var form in _forms.ToArray())
        {
            form.TopMost = false;
            form.Hide();
            if (!form.IsDisposed)
            {
                form.Close();
            }
        }

        ExitThread();
    }

    protected override void ExitThreadCore()
    {
        Application.RemoveMessageFilter(_inputFilter);
        Cursor.Show();
        base.ExitThreadCore();
    }
}

internal sealed class ScreenSaverInputFilter(Action requestClose) : IMessageFilter
{
    private const int WmMouseMove = 0x0200;
    private const int WmKeyDown = 0x0100;
    private const int WmSysKeyDown = 0x0104;
    private const int WmLButtonDown = 0x0201;
    private const int WmRButtonDown = 0x0204;
    private const int WmMButtonDown = 0x0207;
    private const int WmMouseWheel = 0x020A;
    private const int WmXButtonDown = 0x020B;
    private Point? _initialMousePosition;

    public bool PreFilterMessage(ref Message message)
    {
        switch (message.Msg)
        {
            case WmKeyDown:
            case WmSysKeyDown:
            case WmLButtonDown:
            case WmRButtonDown:
            case WmMButtonDown:
            case WmMouseWheel:
            case WmXButtonDown:
                requestClose();
                break;
            case WmMouseMove:
                _initialMousePosition ??= Cursor.Position;
                var movement = Math.Abs(Cursor.Position.X - _initialMousePosition.Value.X)
                    + Math.Abs(Cursor.Position.Y - _initialMousePosition.Value.Y);
                if (movement > 8)
                {
                    requestClose();
                }
                break;
        }

        return false;
    }
}
