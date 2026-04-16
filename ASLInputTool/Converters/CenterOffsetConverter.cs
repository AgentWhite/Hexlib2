using System;
using System.Globalization;
using System.Windows.Data;

namespace ASLInputTool.Converters
{
    /// <summary>
    /// Offsets a coordinate by half the hex size for hotspot positioning.
    /// </summary>
    public class CenterOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double coordinate)
            {
                double offset = -20.0;
                if (parameter != null && double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double p))
                {
                    offset = p;
                }
                return coordinate + offset;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
