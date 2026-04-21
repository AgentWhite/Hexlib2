using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Service for advanced image manipulation, such as clipping and masking.
/// </summary>
public static class ImageProcessingService
{
    /// <summary>
    /// Clips a portion of a BitmapSource using the specified Geometry and returns the result as a new BitmapSource.
    /// The geometry is assumed to be in the coordinate space of the 'view' (e.g. an Image control).
    /// </summary>
    public static BitmapSource? ClipImage(BitmapSource source, Geometry clipGeometry, double viewWidth, double viewHeight, Stretch stretch)
    {
        try
        {
            Rect bounds = clipGeometry.Bounds;
            if (bounds.IsEmpty || bounds.Width <= 0 || bounds.Height <= 0) return null;

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.PushClip(clipGeometry);
                
                // Calculate the rectangle to draw the image into based on the stretch
                Rect destRect = CalculateDestRect(source, viewWidth, viewHeight, stretch);
                dc.DrawImage(source, destRect);
                dc.Pop();
            }

            // Render to a bitmap that fits the clipped geometry exactly
            int pxWidth = (int)Math.Max(1, Math.Ceiling(bounds.Width));
            int pxHeight = (int)Math.Max(1, Math.Ceiling(bounds.Height));

            var rtb = new RenderTargetBitmap(pxWidth, pxHeight, 96, 96, PixelFormats.Pbgra32);
            visual.Transform = new TranslateTransform(-bounds.X, -bounds.Y);
            
            rtb.Render(visual);
            rtb.Freeze();
            
            return rtb;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Image clipping failed: {ex.Message}");
            return null;
        }
    }

    private static Rect CalculateDestRect(BitmapSource source, double viewWidth, double viewHeight, Stretch stretch)
    {
        if (stretch == Stretch.None) return new Rect(0, 0, source.Width, source.Height);
        if (stretch == Stretch.Fill) return new Rect(0, 0, viewWidth, viewHeight);

        double imageAspect = source.Width / source.Height;
        double viewAspect = viewWidth / viewHeight;

        double drawWidth, drawHeight;

        if (stretch == Stretch.Uniform)
        {
            if (imageAspect > viewAspect)
            {
                drawWidth = viewWidth;
                drawHeight = viewWidth / imageAspect;
            }
            else
            {
                drawHeight = viewHeight;
                drawWidth = viewHeight * imageAspect;
            }
        }
        else // UniformToFill
        {
            if (imageAspect > viewAspect)
            {
                drawHeight = viewHeight;
                drawWidth = viewHeight * imageAspect;
            }
            else
            {
                drawWidth = viewWidth;
                drawHeight = viewWidth / imageAspect;
            }
        }

        return new Rect((viewWidth - drawWidth) / 2, (viewHeight - drawHeight) / 2, drawWidth, drawHeight);
    }
}
