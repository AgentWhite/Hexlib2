using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ASLInputTool.Converters;

/// <summary>
/// Returns Collapsed if value is NOT null (or not empty string), Visible otherwise.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool hasValue = value != null;
        if (value is string s) hasValue = !string.IsNullOrWhiteSpace(s);
        
        return hasValue ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
