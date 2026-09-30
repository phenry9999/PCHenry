using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace SkySpec.ScreenSaver;

internal static class DesktopShortcut
{
    private static string ShortcutName => $"{ApplicationBranding.Title}.lnk";

    public static bool ExistsForCurrentExecutable()
    {
        var shortcutPath = GetShortcutPath();
        if (!File.Exists(shortcutPath))
        {
            return false;
        }

        var shellLink = CreateShellLink();
        try
        {
            ((IPersistFile)shellLink).Load(shortcutPath, 0);
            var targetPath = new StringBuilder(260);
            shellLink.GetPath(targetPath, targetPath.Capacity, IntPtr.Zero, 0);
            var savedTarget = targetPath.ToString();
            if (string.IsNullOrWhiteSpace(savedTarget))
            {
                return false;
            }

            return string.Equals(
                Path.GetFullPath(savedTarget),
                Path.GetFullPath(Application.ExecutablePath),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (
            exception is COMException
                or IOException
                or ArgumentException
                or NotSupportedException)
        {
            return false;
        }
        finally
        {
            Marshal.FinalReleaseComObject(shellLink);
        }
    }

    public static string Create()
    {
        var executablePath = Application.ExecutablePath;
        var shortcutPath = GetShortcutPath();

        var shellLink = CreateShellLink();
        try
        {
            shellLink.SetPath(executablePath);
            shellLink.SetWorkingDirectory(AppContext.BaseDirectory);
            shellLink.SetDescription(ApplicationBranding.Title);
            shellLink.SetIconLocation(executablePath, 0);
            ((IPersistFile)shellLink).Save(shortcutPath, true);
        }
        finally
        {
            Marshal.FinalReleaseComObject(shellLink);
        }

        return shortcutPath;
    }

    private static string GetShortcutPath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            ShortcutName);

    private static IShellLinkW CreateShellLink()
    {
        var shellLinkType = Type.GetTypeFromCLSID(
            new Guid("00021401-0000-0000-C000-000000000046"),
            throwOnError: true)!;
        return (IShellLinkW)Activator.CreateInstance(shellLinkType)!;
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder file,
            int maximumPath,
            IntPtr findData,
            uint flags);

        void GetIDList(out IntPtr itemIdList);

        void SetIDList(IntPtr itemIdList);

        void GetDescription(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder name,
            int maximumName);

        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string name);

        void GetWorkingDirectory(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder directory,
            int maximumPath);

        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string directory);

        void GetArguments(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder arguments,
            int maximumPath);

        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string arguments);

        void GetHotkey(out short hotkey);

        void SetHotkey(short hotkey);

        void GetShowCmd(out int showCommand);

        void SetShowCmd(int showCommand);

        void GetIconLocation(
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder iconPath,
            int iconPathLength,
            out int iconIndex);

        void SetIconLocation(
            [MarshalAs(UnmanagedType.LPWStr)] string iconPath,
            int iconIndex);

        void SetRelativePath(
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            uint reserved);

        void Resolve(IntPtr windowHandle, uint flags);

        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string path);
    }
}
