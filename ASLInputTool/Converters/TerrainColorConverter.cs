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
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TerrainType terrain)
        {
            bool isIcon = parameter?.ToString() == "Icon";

            switch (terrain)
            {
                case TerrainType.OpenGround:
                    return new SolidColorBrush(Color.FromRgb(200, 230, 200));
                case TerrainType.Woods:
                    return new SolidColorBrush(Color.FromRgb(34, 139, 34));     // Forest green
                case TerrainType.Orchard:
                    return new SolidColorBrush(Color.FromRgb(154, 205, 50));    // Yellow green
                case TerrainType.Grain:
                    return new SolidColorBrush(Color.FromRgb(244, 164, 96));    // Sandy brown / wheat
                case TerrainType.Brush:
                    return new SolidColorBrush(Color.FromRgb(143, 188, 143));   // Dark sea green
                case TerrainType.StoneBuilding:
                    return isIcon ? Brushes.DimGray : new SolidColorBrush(Color.FromRgb(200, 230, 200)); 
                case TerrainType.WoodenBuilding:
                    return isIcon ? Brushes.SaddleBrown : new SolidColorBrush(Color.FromRgb(200, 230, 200)); 
                case TerrainType.Water:
                    return new SolidColorBrush(Color.FromRgb(70, 130, 180));    // Steel blue
                case TerrainType.Marsh:
                    return new SolidColorBrush(Color.FromRgb(85, 107, 47));       // Dark olive green
                case TerrainType.Graveyard:
                    return new SolidColorBrush(Color.FromRgb(27, 58, 27));      // Dark Forest Green
                default:
                    return Brushes.LightGray;
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
