namespace Skyworks.ScreenSaver.Core;

public sealed class SaverSettings {
    public int RefreshMinutes { get; set; } = 2;
    public int RotationSeconds { get; set; } = 30;
    public int CaptureTimeoutSeconds { get; set; } = 45;

    public void Validate() {
        if (RefreshMinutes is < 1 or > 1440)
            throw new ArgumentOutOfRangeException(nameof(RefreshMinutes), "Refresh minutes must be between 1 and 1440.");
        if (RotationSeconds is < 5 or > 3600)
            throw new ArgumentOutOfRangeException(nameof(RotationSeconds), "Rotation seconds must be between 5 and 3600.");
        if (CaptureTimeoutSeconds is < 10 or > 180)
            throw new ArgumentOutOfRangeException(nameof(CaptureTimeoutSeconds), "Capture timeout seconds must be between 10 and 180.");
    }
}
