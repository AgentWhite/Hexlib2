using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ASLInputTool.Converters
{
    /// <summary>
    /// Returns Visible if value is null or whitespace string, Collapsed otherwise.
    /// Useful for things that should only show if no data is present.
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasValue = value != null;
            if (value is string s) hasValue = !string.IsNullOrWhiteSpace(s);

            bool inverse = parameter != null && parameter.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) == true;
            
            if (inverse)
            {
                return hasValue ? Visibility.Visible : Visibility.Collapsed;
            }

            return hasValue ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
