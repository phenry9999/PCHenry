namespace Skyworks.ScreenSaver;

public sealed class DiagnosticLog
{
    private readonly object gate = new();
    public string FilePath { get; }

    public DiagnosticLog(string directory)
    {
        Directory.CreateDirectory(directory);
        FilePath = Path.Combine(directory, "diagnostics.log");
    }

    public void Write(string message, Exception? exception = null)
    {
        var entry = $"{DateTimeOffset.Now:O} {message}{(exception is null ? "" : " " + exception)}";
        System.Diagnostics.Trace.WriteLine(entry);
        lock (gate)
        {
            try
            {
                if (File.Exists(FilePath) && new FileInfo(FilePath).Length > 2_000_000)
                    File.Move(FilePath, FilePath + ".previous", true);
                File.AppendAllText(FilePath, entry + Environment.NewLine);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                System.Diagnostics.Trace.TraceError($"Cannot write diagnostics: {ex}");
            }
        }
    }
}
