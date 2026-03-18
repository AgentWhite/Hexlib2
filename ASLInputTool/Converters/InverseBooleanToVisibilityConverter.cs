using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ASLInputTool.Converters;

/// <summary>
/// Converts a boolean value to a <see cref="Visibility"/>.
/// True maps to <see cref="Visibility.Collapsed"/> and False maps to <see cref="Visibility.Visible"/>.
/// This is used to toggle between mutually exclusive elements like a "List" and an "Entry Form".
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Standard WPF BooleanToVisibility does not support inversion natively.
        // This custom converter fills that gap.
        bool boolValue = value is bool b && b;
        return boolValue ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Collapsed;
    }
}
