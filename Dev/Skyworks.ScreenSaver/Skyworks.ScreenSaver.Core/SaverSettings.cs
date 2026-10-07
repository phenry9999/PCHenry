namespace Skyworks.ScreenSaver.Core;

public sealed class SaverSettings {
    public int RefreshSeconds { get; set; } = 30;
    public int CaptureTimeoutSeconds { get; set; } = 45;

    public void Validate() {
        if (RefreshSeconds is < 5 or > 86400)
            throw new ArgumentOutOfRangeException(nameof(RefreshSeconds), "Refresh seconds must be between 5 and 86400.");
        if (CaptureTimeoutSeconds is < 10 or > 180)
            throw new ArgumentOutOfRangeException(nameof(CaptureTimeoutSeconds), "Capture timeout seconds must be between 10 and 180.");
    }
}
