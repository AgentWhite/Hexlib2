using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ASL;

namespace ASLInputTool.Converters;

/// <summary>
/// Converts a TerrainType to a SolidColorBrush for UI display.
/// </summary>
public class TerrainColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.OpenGround => new SolidColorBrush(Color.FromRgb(200, 230, 200)), // Light green
                TerrainType.Woods => new SolidColorBrush(Color.FromRgb(34, 139, 34)),       // Forest green
                TerrainType.Orchard => new SolidColorBrush(Color.FromRgb(154, 205, 50)),    // Yellow green
                TerrainType.Grain => new SolidColorBrush(Color.FromRgb(244, 164, 96)),      // Sandy brown / wheat
                TerrainType.Brush => new SolidColorBrush(Color.FromRgb(143, 188, 143)),     // Dark sea green
                TerrainType.Building => new SolidColorBrush(Color.FromRgb(139, 69, 19)),    // Saddle brown
                TerrainType.StoneBuilding => new SolidColorBrush(Color.FromRgb(105, 105, 105)), // Dim gray
                TerrainType.WoodenBuilding => new SolidColorBrush(Color.FromRgb(205, 133, 63)), // Peru
                TerrainType.Road => new SolidColorBrush(Color.FromRgb(211, 211, 211)),      // Light gray
                TerrainType.Water => new SolidColorBrush(Color.FromRgb(70, 130, 180)),      // Steel blue
                TerrainType.Marsh => new SolidColorBrush(Color.FromRgb(85, 107, 47)),       // Dark olive green
                _ => Brushes.Transparent,
            };
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
