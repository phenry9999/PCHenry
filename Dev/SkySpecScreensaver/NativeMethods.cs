using System.Runtime.InteropServices;

namespace SkySpec.ScreenSaver;

internal static class NativeMethods
{
    internal const int WmNcLButtonDown = 0x00A1;
    internal const int HtCaption = 2;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool ReleaseCapture();

    [DllImport("user32.dll", EntryPoint = "SendMessageW")]
    internal static extern IntPtr SendMessage(
        IntPtr window,
        int message,
        IntPtr wParam,
        IntPtr lParam);
}
