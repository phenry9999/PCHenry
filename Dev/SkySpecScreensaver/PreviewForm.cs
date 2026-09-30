using System.Runtime.InteropServices;

namespace SkySpec.ScreenSaver;

internal sealed class PreviewForm : Form
{
    private readonly StatusDashboardControl _dashboard = new();

    public PreviewForm(IntPtr previewHandle)
    {
        Icon = ApplicationBranding.Icon;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopLevel = true;

        NativeMethods.SetParent(Handle, previewHandle);
        var style = NativeMethods.GetWindowLongPtr(Handle, NativeMethods.GwlStyle).ToInt64();
        NativeMethods.SetWindowLongPtr(
            Handle,
            NativeMethods.GwlStyle,
            new IntPtr(style | NativeMethods.WsChild));

        if (NativeMethods.GetClientRect(previewHandle, out var rectangle))
        {
            Bounds = new Rectangle(0, 0, rectangle.Right, rectangle.Bottom);
        }

        _dashboard.ApplySettings(ScreenSaverSettings.Load());
        Controls.Add(_dashboard);
        Shown += async (_, _) => await _dashboard.StartAsync();
    }
}

internal static partial class NativeMethods
{
    internal const int GwlStyle = -16;
    internal const long WsChild = 0x40000000L;
    internal const int WmNcLButtonDown = 0x00A1;
    internal const int HtCaption = 2;

    [LibraryImport("user32.dll", SetLastError = true)]
    internal static partial IntPtr SetParent(IntPtr childWindow, IntPtr newParentWindow);

    [LibraryImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool GetClientRect(IntPtr window, out NativeRectangle rectangle);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReleaseCapture();

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    internal static extern IntPtr SendMessage(
        IntPtr window,
        int message,
        IntPtr wParam,
        IntPtr lParam);

    internal static IntPtr GetWindowLongPtr(IntPtr window, int index) =>
        IntPtr.Size == 8 ? GetWindowLongPtr64(window, index) : new IntPtr(GetWindowLong32(window, index));

    internal static IntPtr SetWindowLongPtr(IntPtr window, int index, IntPtr newValue) =>
        IntPtr.Size == 8
            ? SetWindowLongPtr64(window, index, newValue)
            : new IntPtr(SetWindowLong32(window, index, newValue.ToInt32()));

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLong")]
    private static partial int GetWindowLong32(IntPtr window, int index);

    [LibraryImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
    private static partial IntPtr GetWindowLongPtr64(IntPtr window, int index);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLong")]
    private static partial int SetWindowLong32(IntPtr window, int index, int newValue);

    [LibraryImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    private static partial IntPtr SetWindowLongPtr64(IntPtr window, int index, IntPtr newValue);
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeRectangle
{
    public int Left;
    public int Top;
    public int Right;
    public int Bottom;
}
