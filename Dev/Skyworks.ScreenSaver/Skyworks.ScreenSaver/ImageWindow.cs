using System.Diagnostics;
using System.Drawing.Imaging;

namespace Skyworks.ScreenSaver;

internal sealed class ImageWindow : Form
{
    private Image? image;
    private Image? outgoingImage;
    private bool outgoingIsSurface;
    private readonly System.Windows.Forms.Timer transitionTimer = new() { Interval = 33 };
    private readonly ImageAttributes outgoingAttributes = new();
    private readonly ImageAttributes incomingAttributes = new();
    private long transitionStarted;
    private const double TransitionDurationSeconds = 2;

    public ImageWindow(Rectangle bounds, bool preview)
    {
        AutoScaleMode = AutoScaleMode.None;
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = bounds;
        BackColor = Color.Black;
        ShowInTaskbar = false;
        TopMost = !preview;
        DoubleBuffered = true;
        transitionTimer.Tick += (_, _) =>
        {
            if (TransitionProgress >= 1)
            {
                transitionTimer.Stop();
                outgoingImage?.Dispose();
                outgoingImage = null;
                outgoingIsSurface = false;
            }
            Invalidate();
        };
    }

    internal double TransitionProgress => Math.Clamp(
        (double)(Stopwatch.GetTimestamp() - transitionStarted) / Stopwatch.Frequency / TransitionDurationSeconds, 0, 1);

    public void ShowImage(string path)
    {
        using var stream = EmbeddedAssets.OpenImage(path);
        using var source = Image.FromStream(stream);
        var next = new Bitmap(source);
        if (image is null)
        {
            image = next;
            Invalidate();
            return;
        }

        if (outgoingImage is not null)
        {
            var currentFrame = new Bitmap(Math.Max(1, ClientSize.Width), Math.Max(1, ClientSize.Height));
            using (var graphics = Graphics.FromImage(currentFrame))
            {
                graphics.Clear(BackColor);
                DrawImageComposite(graphics, TransitionProgress);
            }
            outgoingImage.Dispose();
            outgoingImage = currentFrame;
            outgoingIsSurface = true;
        }
        else
        {
            outgoingImage = image;
            outgoingIsSurface = false;
        }
        image = next;
        transitionStarted = Stopwatch.GetTimestamp();
        transitionTimer.Start();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (image is null) return;
        double progress = outgoingImage is null ? 1 : TransitionProgress;
        DrawImageComposite(e.Graphics, progress);
        VersionOverlay.Draw(e.Graphics, GetImageBounds(image), DeviceDpi, image);
    }

    private void DrawImageComposite(Graphics graphics, double progress)
    {
        if (outgoingImage is not null)
        {
            var oldBounds = outgoingIsSurface ? ClientRectangle : GetImageBounds(outgoingImage);
            DrawImageLayer(graphics, outgoingImage, oldBounds, (float)(1 - progress), outgoingAttributes);
        }
        DrawImageLayer(graphics, image!, GetImageBounds(image!), (float)progress, incomingAttributes);
    }

    private Rectangle GetImageBounds(Image target)
    {
        double scale = Math.Min((double)ClientSize.Width / target.Width, (double)ClientSize.Height / target.Height);
        int width = Math.Max(1, (int)(target.Width * scale));
        int height = Math.Max(1, (int)(target.Height * scale));
        return new Rectangle((ClientSize.Width - width) / 2, (ClientSize.Height - height) / 2, width, height);
    }

    private static void DrawImageLayer(Graphics graphics, Image target, Rectangle bounds, float opacity, ImageAttributes attributes)
    {
        var matrix = new ColorMatrix { Matrix33 = opacity };
        attributes.SetColorMatrix(matrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
        graphics.DrawImage(target, bounds, 0, 0, target.Width, target.Height, GraphicsUnit.Pixel, attributes);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            transitionTimer.Stop();
            transitionTimer.Dispose();
            outgoingAttributes.Dispose();
            incomingAttributes.Dispose();
            outgoingImage?.Dispose();
            outgoingImage = null;
            image?.Dispose();
            image = null;
        }
        base.Dispose(disposing);
    }
}
