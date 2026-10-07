using Skyworks.ScreenSaver.Core;

namespace Skyworks.ScreenSaver;

public sealed record ScreenSaverInstallation(string ScreenSaverPath, SaverSettings Settings);

public static class ScreenSaverRegistration
{
    public const string DeploymentMarker = "Skyworks.ScreenSaver.deployment";
    public const string DeploymentSignature = "Skyworks.ScreenSaver self-contained win-x64";

    public static string ResolveDeployment(string applicationDirectory, string processPath)
    {
        string directory = Path.GetFullPath(applicationDirectory);
        string executable = Path.GetFullPath(processPath);
        if (!string.Equals(Path.GetDirectoryName(executable), Path.TrimEndingDirectorySeparator(directory), StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(Path.GetFileNameWithoutExtension(executable), "Skyworks.ScreenSaver", StringComparison.OrdinalIgnoreCase) ||
            !(Path.GetExtension(executable).Equals(".exe", StringComparison.OrdinalIgnoreCase) ||
              Path.GetExtension(executable).Equals(".scr", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Run Skyworks.ScreenSaver.exe or .scr directly to install it, rather than launching its DLL with dotnet.");
        string path = Path.Combine(directory, "Skyworks.ScreenSaver.scr");
        if (!File.Exists(path)) throw new FileNotFoundException("The .scr file is missing beside the application. Rebuild or publish the project.", path);
        string marker = Path.Combine(directory, DeploymentMarker);
        bool published = File.Exists(marker) && File.ReadAllText(marker).Trim() == DeploymentSignature;
        if (!published)
        {
            string[] dependencies = ["Skyworks.ScreenSaver.dll", "Skyworks.ScreenSaver.deps.json", "Skyworks.ScreenSaver.runtimeconfig.json",
                "Skyworks.ScreenSaver.Core.dll", "Microsoft.Web.WebView2.Core.dll", "Microsoft.Web.WebView2.WinForms.dll"];
            foreach (string dependency in dependencies)
                if (!File.Exists(Path.Combine(directory, dependency)))
                    throw new FileNotFoundException($"The deployment is incomplete: {dependency} is missing. Rebuild or publish before installing.", Path.Combine(directory, dependency));
            if (!File.Exists(Path.Combine(directory, "WebView2Loader.dll")) &&
                !File.Exists(Path.Combine(directory, "runtimes", "win-x64", "native", "WebView2Loader.dll")))
                throw new FileNotFoundException("The WebView2 native loader is missing. Rebuild or publish before installing.");
        }
        return path;
    }

    public static ScreenSaverInstallation PrepareInstallation(string applicationDirectory, string processPath, SaverSettings settings, string? installationRoot = null)
    {
        settings.Validate();
        string source = Path.GetFullPath(applicationDirectory);
        string sourceSaver = ResolveDeployment(source, processPath);
        string root = Path.GetFullPath(installationRoot ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Skyworks.ScreenSaver", "Installed"));
        string deployments = Path.Combine(root, "Deployments");
        if (root.Equals(Path.TrimEndingDirectorySeparator(source), StringComparison.OrdinalIgnoreCase) ||
            root.StartsWith(Path.TrimEndingDirectorySeparator(source) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The installation directory cannot be inside the source deployment.");
        var installedSettings = new SaverSettings
        {
            RefreshSeconds = settings.RefreshSeconds,
            CaptureTimeoutSeconds = settings.CaptureTimeoutSeconds
        };
        // An already installed version needs no copying, so running executables are never overwritten.
        if (source.StartsWith(Path.TrimEndingDirectorySeparator(deployments) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return new ScreenSaverInstallation(sourceSaver, installedSettings);
        string destination = Path.Combine(deployments, Guid.NewGuid().ToString("N"));
        try
        {
            // Copy the whole deployment, not just the apphost; native and managed dependencies stay together.
            CopyDirectory(source, destination);
            string saver = ResolveDeployment(destination, Path.Combine(destination, Path.GetFileName(processPath)));
            return new ScreenSaverInstallation(saver, installedSettings);
        }
        catch (Exception installError)
        {
            try { if (Directory.Exists(destination)) Directory.Delete(destination, recursive: true); }
            catch (Exception cleanupError) when (cleanupError is IOException or UnauthorizedAccessException)
            { throw new AggregateException("Installation failed and its incomplete deployment could not be removed.", installError, cleanupError); }
            throw;
        }
    }

    public static System.Diagnostics.ProcessStartInfo InstallationCommand(string screenSaverPath)
    {
        string path = Path.GetFullPath(screenSaverPath);
        if (!Path.GetExtension(path).Equals(".scr", StringComparison.OrdinalIgnoreCase) || !File.Exists(path))
            throw new FileNotFoundException("A valid existing .scr file is required.", path);
        return new System.Diagnostics.ProcessStartInfo { FileName = path, Verb = "install", UseShellExecute = true };
    }

    public static void SelectAndOpen(string screenSaverPath)
    {
        using var process = System.Diagnostics.Process.Start(InstallationCommand(screenSaverPath))
            ?? throw new InvalidOperationException("Windows could not open the screen saver installer.");
    }

    private static void CopyDirectory(string source, string destination, bool deploymentRoot = true)
    {
        Directory.CreateDirectory(destination);
        foreach (string file in Directory.EnumerateFiles(source))
        {
            if (deploymentRoot && new[] { "Links.json", "Links.txt", "Link.txt" }
                .Contains(Path.GetFileName(file), StringComparer.OrdinalIgnoreCase)) continue;
            string target = Path.Combine(destination, Path.GetFileName(file));
            File.Copy(file, target, overwrite: false);
        }
        foreach (string directory in Directory.EnumerateDirectories(source))
        {
            if (deploymentRoot && Path.GetFileName(directory).Equals("Assets", StringComparison.OrdinalIgnoreCase)) continue;
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new IOException($"Installation does not follow linked directories: {directory}");
            CopyDirectory(directory, Path.Combine(destination, Path.GetFileName(directory)), deploymentRoot: false);
        }
    }

}
