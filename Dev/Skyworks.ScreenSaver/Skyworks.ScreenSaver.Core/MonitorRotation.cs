namespace Skyworks.ScreenSaver.Core;

public sealed class MonitorRotation
{
    private readonly int monitorCount;
    private int nextIndex;

    public MonitorRotation(int monitorCount)
    {
        if (monitorCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(monitorCount), "At least one monitor is required.");
        this.monitorCount = monitorCount;
    }

    public int Next()
    {
        int result = nextIndex;
        nextIndex = nextIndex == monitorCount - 1 ? 0 : nextIndex + 1;
        return result;
    }
}
