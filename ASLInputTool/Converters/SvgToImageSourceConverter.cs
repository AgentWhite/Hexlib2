using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using SharpVectors.Renderers.Wpf;
using SharpVectors.Converters;

namespace ASLInputTool.Converters;

/// <summary>
/// Converts an SVG string literal into a WPF DrawingImage using SharpVectors.
/// Includes isolation logic to prevent TypeInitializationExceptions from crashing the UI thread.
/// </summary>
public class SvgToImageSourceConverter : IValueConverter
{
    private const int MaxSvgLength = 2 * 1024 * 1024; // 2MB safety limit
    private static readonly object _syncLock = new();

    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string svgContent && !string.IsNullOrWhiteSpace(svgContent))
        {
            // Early exit if the string is clearly not SVG or is too large
            if (svgContent.Length > MaxSvgLength || !IsSvgFormat(svgContent))
            {
                return null;
            }

            lock (_syncLock)
            {
                try
                {
                    // Isolated call to SharpVectors
                    return SvgRenderer.Render(svgContent);
                }
                catch
                {
                    // Any error (even native ones or type load failures) should return null, not crash
                    return null;
                }
            }
        }
        return null;
    }

    private static bool IsSvgFormat(string content)
    {
        string trimmed = content.TrimStart();
        return trimmed.StartsWith("<svg", StringComparison.OrdinalIgnoreCase) || 
               trimmed.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Inner class to isolate SharpVectors assembly loading.
    /// If there is a DLL issue, the exception will occur here and can be caught by the outer block.
    /// </summary>
    private static class SvgRenderer
    {
        public static DrawingImage? Render(string svgContent)
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgContent));
            
            var settings = new WpfDrawingSettings();
            settings.IncludeRuntime = false;
            settings.TextAsGeometry = true;
            
            var reader = new FileSvgReader(settings);
            var drawing = reader.Read(stream);
            
            if (drawing != null)
            {
                var image = new DrawingImage(drawing);
                if (image.CanFreeze) image.Freeze(); 
                return image;
            }
            return null;
        }
    }
}
