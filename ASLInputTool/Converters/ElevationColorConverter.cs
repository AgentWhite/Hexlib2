using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ASLInputTool.Converters;

/// <summary>
/// Converts an elevation level to a SolidColorBrush for terrain shading.
/// </summary>
public class ElevationColorConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int elevation)
        {
            switch (elevation)
            {
                case 0:
                    return new SolidColorBrush(Color.FromRgb(200, 230, 200)); // Light Green (Standard Ground)
                case 1:
                    return new SolidColorBrush(Color.FromRgb(210, 180, 140)); // Tan
                case 2:
                    return new SolidColorBrush(Color.FromRgb(180, 150, 100)); // Medium Brown
                case 3:
                    return new SolidColorBrush(Color.FromRgb(150, 120, 70));  // Brown
                default:
                    return new SolidColorBrush(Color.FromRgb(110, 80, 40));   // Dark Brown (Level 4+)
            }
        }
        return Brushes.Transparent;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
