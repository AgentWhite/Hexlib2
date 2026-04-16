using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace ASLInputTool.Converters;

/// <summary>
/// Converts enum values to display-friendly strings by splitting CamelCase words.
/// </summary>
public class EnumToStringConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;
        string text = value.ToString() ?? string.Empty;
        // Split camelCase or PascalCase strings with a space
        return Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
