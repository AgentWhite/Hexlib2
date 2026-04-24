using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ASLInputTool.Services;

/// <summary>
/// Precomputed edge-detection data for a background image — grayscale pixels plus a Sobel
/// gradient-magnitude map and an adaptive snap threshold.
/// </summary>
public sealed class GradientMap
{
    /// <summary>Image pixel width.</summary>
    public int Width { get; init; }
    /// <summary>Image pixel height.</summary>
    public int Height { get; init; }
    /// <summary>Per-pixel Sobel gradient magnitude, clamped to byte range.</summary>
    public required byte[] Magnitude { get; init; }
    /// <summary>Minimum magnitude required to consider a pixel a valid snap target.</summary>
    public byte SnapThreshold { get; init; }
}

/// <summary>
/// Provides utility methods for edge detection and contrast-based snapping.
/// </summary>
public static class EdgeDetectionService
{
    /// <summary>
    /// Processes a <see cref="BitmapSource"/> into a grayscale byte array for fast pixel access.
    /// </summary>
    public static byte[] GetGrayscalePixels(BitmapSource source, out int width, out int height)
    {
        width = source.PixelWidth;
        height = source.PixelHeight;

        var graySource = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
        byte[] pixels = new byte[width * height];
        graySource.CopyPixels(pixels, width, 0);

        return pixels;
    }

    /// <summary>
    /// Builds a <see cref="GradientMap"/> for the given image: grayscale → light Gaussian blur → Sobel
    /// magnitude → percentile-based snap threshold.
    /// </summary>
    public static GradientMap BuildGradientMap(BitmapSource source, double thresholdPercentile = 0.70)
    {
        byte[] gray = GetGrayscalePixels(source, out int w, out int h);
        byte[] blurred = Blur3x3(gray, w, h);
        byte[] magnitude = Sobel(blurred, w, h);
        byte threshold = Percentile(magnitude, thresholdPercentile);

        return new GradientMap
        {
            Width = w,
            Height = h,
            Magnitude = magnitude,
            SnapThreshold = threshold
        };
    }

    /// <summary>
    /// Finds the highest-magnitude edge pixel within a zoom-aware radius of the given canvas point.
    /// Returns the original point if no candidate exceeds the map's adaptive snap threshold.
    /// </summary>
    /// <param name="canvasPoint">Point in canvas (pre-zoom-transform) coordinates.</param>
    /// <param name="map">Precomputed gradient map for the current image.</param>
    /// <param name="canvasWidth">Width of the image's canvas host in canvas units.</param>
    /// <param name="canvasHeight">Height of the image's canvas host in canvas units.</param>
    /// <param name="zoomLevel">Current view zoom level. Higher zoom → smaller image-space search radius.</param>
    /// <param name="viewPixelRadius">Desired search radius in view pixels (visually constant).</param>
    public static Point FindBestEdgePoint(
        Point canvasPoint,
        GradientMap map,
        double canvasWidth,
        double canvasHeight,
        double zoomLevel,
        int viewPixelRadius = 12)
    {
        if (canvasWidth <= 0 || canvasHeight <= 0) return canvasPoint;

        // canvas → image scale (image pixels per canvas unit)
        double sx = map.Width / canvasWidth;
        double sy = map.Height / canvasHeight;

        int imgX = (int)(canvasPoint.X * sx);
        int imgY = (int)(canvasPoint.Y * sy);

        // viewPixelRadius is in screen pixels → convert to canvas units (÷ zoom) → then to image pixels
        double canvasRadius = viewPixelRadius / Math.Max(zoomLevel, 0.01);
        int imgRadius = (int)Math.Ceiling(canvasRadius * Math.Max(sx, sy));
        imgRadius = Math.Clamp(imgRadius, 2, 64);

        int bestX = imgX;
        int bestY = imgY;
        double bestScore = -1;
        byte bestMag = 0;

        for (int dy = -imgRadius; dy <= imgRadius; dy++)
        {
            int ny = imgY + dy;
            if (ny < 0 || ny >= map.Height) continue;
            int rowBase = ny * map.Width;

            for (int dx = -imgRadius; dx <= imgRadius; dx++)
            {
                int nx = imgX + dx;
                if (nx < 0 || nx >= map.Width) continue;

                byte m = map.Magnitude[rowBase + nx];
                if (m < map.SnapThreshold) continue;

                // Prefer strong edges but penalize distance (squared, normalized to the radius).
                double distSq = dx * dx + dy * dy;
                double distPenalty = 1.0 + 2.0 * distSq / (imgRadius * imgRadius);
                double score = m / distPenalty;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMag = m;
                    bestX = nx;
                    bestY = ny;
                }
            }
        }

        if (bestMag < map.SnapThreshold) return canvasPoint;

        return new Point(bestX / sx, bestY / sy);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private static byte[] Blur3x3(byte[] src, int w, int h)
    {
        // Approximate Gaussian with a separable [1 2 1] / 4 kernel.
        byte[] tmp = new byte[w * h];
        byte[] dst = new byte[w * h];

        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                int xm = x > 0 ? x - 1 : 0;
                int xp = x < w - 1 ? x + 1 : w - 1;
                tmp[row + x] = (byte)((src[row + xm] + 2 * src[row + x] + src[row + xp] + 2) >> 2);
            }
        }

        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            int rowM = (y > 0 ? y - 1 : 0) * w;
            int rowP = (y < h - 1 ? y + 1 : h - 1) * w;
            for (int x = 0; x < w; x++)
            {
                dst[row + x] = (byte)((tmp[rowM + x] + 2 * tmp[row + x] + tmp[rowP + x] + 2) >> 2);
            }
        }

        return dst;
    }

    private static byte[] Sobel(byte[] src, int w, int h)
    {
        byte[] mag = new byte[w * h];

        for (int y = 1; y < h - 1; y++)
        {
            int row = y * w;
            int rowM = row - w;
            int rowP = row + w;
            for (int x = 1; x < w - 1; x++)
            {
                int a = src[rowM + x - 1], b = src[rowM + x], c = src[rowM + x + 1];
                int d = src[row  + x - 1],              f = src[row  + x + 1];
                int g = src[rowP + x - 1], hh = src[rowP + x], i = src[rowP + x + 1];

                int gx = (c + 2 * f + i) - (a + 2 * d + g);
                int gy = (g + 2 * hh + i) - (a + 2 * b + c);

                int m = (int)Math.Sqrt(gx * gx + gy * gy);
                if (m > 255) m = 255;
                mag[row + x] = (byte)m;
            }
        }

        return mag;
    }

    private static byte Percentile(byte[] values, double percentile)
    {
        // Histogram-based percentile — O(n), no allocation beyond 256 bins.
        var hist = new int[256];
        for (int i = 0; i < values.Length; i++) hist[values[i]]++;

        long target = (long)(values.LongLength * Math.Clamp(percentile, 0.0, 1.0));
        long running = 0;
        for (int i = 0; i < 256; i++)
        {
            running += hist[i];
            if (running >= target) return (byte)Math.Max(i, 8); // floor at 8 so completely flat images still reject noise
        }
        return 255;
    }
}
