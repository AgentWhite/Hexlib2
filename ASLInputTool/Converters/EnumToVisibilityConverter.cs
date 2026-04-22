using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ASLInputTool.Converters;

/// <summary>
/// Converts an enum value to Visibility. Returns Visible if the value matches the parameter, 
/// otherwise returns Collapsed.
/// </summary>
public class EnumToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return Visibility.Collapsed;

        string checkValue = value.ToString();
        string targetValue = parameter.ToString();

        var targets = targetValue.Split('|');
        bool match = targets.Any(t => checkValue.Equals(t.Trim(), StringComparison.OrdinalIgnoreCase));

        return match ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
