using System;
using System.Globalization;
using System.Windows.Data;

namespace ASLInputTool.Converters
{
    /// <summary>
    /// Converts an enum value to a boolean by comparing it to a parameter string.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter != null)
            {
                return Enum.Parse(targetType, parameter.ToString()!);
            }
            return Binding.DoNothing;
        }
    }
}
