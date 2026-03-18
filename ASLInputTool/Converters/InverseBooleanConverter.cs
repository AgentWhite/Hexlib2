using System;
using System.Globalization;
using System.Windows.Data;

namespace ASLInputTool.Converters;

/// <summary>
/// Inverts a boolean value (True to False, False to True).
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return false;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b) return !b;
        return false;
    }
}
