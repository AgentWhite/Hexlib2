using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ASLInputTool.Services;

/// <summary>
/// Provides utility methods for edge detection and contrast-based snapping.
/// </summary>
public static class EdgeDetectionService
{
    /// <summary>
    /// Processes a BitmapSource into a grayscale byte array for fast pixel access.
    /// </summary>
    public static byte[] GetGrayscalePixels(BitmapSource source, out int width, out int height)
    {
        width = source.PixelWidth;
        height = source.PixelHeight;

        // Convert the source to a standard persistent format (Gray8)
        var graySource = new FormatConvertedBitmap(source, PixelFormats.Gray8, null, 0);
        byte[] pixels = new byte[width * height];
        graySource.CopyPixels(pixels, width, 0);
        
        return pixels;
    }

    /// <summary>
    /// Finds the highest contrast (edge) point in a local radius around a point.
    /// </summary>
    public static Point FindBestEdgePoint(Point p, byte[] pixels, int imgWidth, int imgHeight, double canvasWidth, double canvasHeight, int radius = 15)
    {
        // 1. Map canvas coordinates to image coordinates
        int imgX = (int)(p.X * imgWidth / canvasWidth);
        int imgY = (int)(p.Y * imgHeight / canvasHeight);

        int bestX = imgX;
        int bestY = imgY;
        double maxGradient = -1;

        // 2. Search neighborhood
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = imgX + dx;
                int ny = imgY + dy;

                if (nx <= 0 || nx >= imgWidth - 1 || ny <= 0 || ny >= imgHeight - 1) continue;

                // Simple Sobel-like gradient approximation (difference between adjacent pixels)
                // We use the absolute differences in horizontal and vertical directions
                int gx = Math.Abs(pixels[ny * imgWidth + (nx + 1)] - pixels[ny * imgWidth + (nx - 1)]);
                int gy = Math.Abs(pixels[(ny + 1) * imgWidth + nx] - pixels[(ny - 1) * imgWidth + nx]);
                double magnitude = Math.Sqrt(gx * gx + gy * gy);

                // Weight by distance to discourage snapping too far
                double dist = Math.Sqrt(dx * dx + dy * dy);
                double score = magnitude / (1.0 + dist * 0.2); 

                if (score > maxGradient)
                {
                    maxGradient = score;
                    bestX = nx;
                    bestY = ny;
                }
            }
        }

        // 3. Map back to canvas coordinates
        if (maxGradient < 5) return p; // Don't snap if no strong edge found

        return new Point(
            (double)bestX * canvasWidth / imgWidth,
            (double)bestY * canvasHeight / imgHeight
        );
    }
}
