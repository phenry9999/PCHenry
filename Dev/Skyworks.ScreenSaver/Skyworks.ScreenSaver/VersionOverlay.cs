using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace Skyworks.ScreenSaver;

internal static class VersionOverlay
{
    internal static RectangleF Draw(Graphics graphics, Rectangle imageBounds, int dpi, Image? image = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dpi);
        if (imageBounds.Width < 2 || imageBounds.Height < 2) return RectangleF.Empty;
        float scale = dpi / 96f;
        float padding = Math.Min(8f * scale, Math.Min(imageBounds.Width, imageBounds.Height) / 12f);
        float availableWidth = imageBounds.Width - 2 * padding;
        float availableHeight = imageBounds.Height - 2 * padding;
        using var format = StringFormat.GenericTypographic;
        format.FormatFlags |= StringFormatFlags.NoWrap;
        Font font = new("Segoe UI", 21f * scale, FontStyle.Regular, GraphicsUnit.Pixel);
        GraphicsState state = graphics.Save();
        try
        {
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            var size = graphics.MeasureString(AppVersion.DisplayText, font, PointF.Empty, format);
            for (int attempt = 0; attempt < 3 && (size.Width > availableWidth || size.Height > availableHeight); attempt++)
            {
                float shrink = Math.Min(availableWidth / size.Width, availableHeight / size.Height) * 0.98f;
                float fontSize = Math.Max(0.1f, font.Size * shrink);
                font.Dispose();
                font = new Font("Segoe UI", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
                size = graphics.MeasureString(AppVersion.DisplayText, font, PointF.Empty, format);
            }
            var position = new PointF(MathF.Floor(imageBounds.Right - padding - size.Width), MathF.Floor(imageBounds.Bottom - padding - size.Height));
            var textBounds = new RectangleF(position, size);
            Color foreground = SelectTextColor(image, imageBounds, textBounds);
            graphics.SetClip(imageBounds, CombineMode.Intersect);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var glyphs = new GraphicsPath();
            glyphs.AddString(AppVersion.DisplayText, font.FontFamily, (int)font.Style, font.Size, position, format);
            using var outline = new Pen(foreground == Color.White ? Color.Black : Color.White,
                Math.Max(1.5f * scale, Math.Min(3f * scale, font.Size / 7f)))
            { LineJoin = LineJoin.Round };
            using var brush = new SolidBrush(foreground);
            graphics.DrawPath(outline, glyphs);
            graphics.FillPath(brush, glyphs);
            return textBounds;
        }
        finally
        {
            font.Dispose();
            graphics.Restore(state);
        }
    }

    internal static Color SelectTextColor(Image? image, Rectangle imageBounds, RectangleF textBounds)
    {
        if (image is not Bitmap bitmap || imageBounds.Width <= 0 || imageBounds.Height <= 0) return Color.White;
        int whiteReadable = 0, blackReadable = 0;
        double whiteScore = 0, blackScore = 0;
        const int columns = 24, rows = 8;
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                float displayedX = textBounds.Left + (x + 0.5f) * textBounds.Width / columns;
                float displayedY = textBounds.Top + (y + 0.5f) * textBounds.Height / rows;
                int sourceX = Math.Clamp((int)((displayedX - imageBounds.Left) * bitmap.Width / imageBounds.Width), 0, bitmap.Width - 1);
                int sourceY = Math.Clamp((int)((displayedY - imageBounds.Top) * bitmap.Height / imageBounds.Height), 0, bitmap.Height - 1);
                Color pixel = bitmap.GetPixel(sourceX, sourceY);
                double alpha = pixel.A / 255d;
                double luminance = 0.2126 * Linear((byte)Math.Round(pixel.R * alpha))
                    + 0.7152 * Linear((byte)Math.Round(pixel.G * alpha))
                    + 0.0722 * Linear((byte)Math.Round(pixel.B * alpha));
                double whiteContrast = 1.05 / (luminance + 0.05);
                double blackContrast = (luminance + 0.05) / 0.05;
                if (whiteContrast >= 4.5) whiteReadable++;
                if (blackContrast >= 4.5) blackReadable++;
                whiteScore += Math.Log(whiteContrast);
                blackScore += Math.Log(blackContrast);
            }
        // Prefer readable coverage; a mean luminance can hide isolated low-contrast patches.
        return whiteReadable > blackReadable || (whiteReadable == blackReadable && whiteScore >= blackScore)
            ? Color.White : Color.Black;
    }

    private static double Linear(byte component)
    {
        double value = component / 255d;
        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}
