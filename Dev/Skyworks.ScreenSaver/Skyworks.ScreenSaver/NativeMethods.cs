using System.Runtime.InteropServices;

namespace Skyworks.ScreenSaver;

internal static class NativeMethods
{
    internal const int GwlStyle = -16;
    internal const int WsChild = 0x40000000;
    internal const int WsPopup = unchecked((int)0x80000000);
    [DllImport("user32.dll")] internal static extern nint SetParent(nint child, nint parent);
    [DllImport("user32.dll")] internal static extern bool IsWindow(nint window);
    [DllImport("user32.dll")] internal static extern int GetWindowLong(nint window, int index);
    [DllImport("user32.dll")] internal static extern int SetWindowLong(nint window, int index, int value);
    [DllImport("user32.dll")] internal static extern bool GetClientRect(nint window, out Rect rect);
    [DllImport("user32.dll")] internal static extern nint GetForegroundWindow();
    [DllImport("user32.dll")] internal static extern short GetAsyncKeyState(int key);
    [StructLayout(LayoutKind.Sequential)]
    internal struct Rect { public int Left, Top, Right, Bottom; }
}
