using ASLInputTool.Converters;
using System.Windows;
using Xunit;

namespace ASLInputTool.Tests;

public class ConverterTests
{
    [Theory]
    [InlineData(true, Visibility.Collapsed)]
    [InlineData(false, Visibility.Visible)]
    public void InverseBooleanToVisibilityConverter_Convert_ReturnsExpected(bool input, Visibility expected)
    {
        var converter = new InverseBooleanToVisibilityConverter();
        var result = converter.Convert(input, typeof(Visibility), null!, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(Visibility.Collapsed, true)]
    [InlineData(Visibility.Visible, false)]
    [InlineData(Visibility.Hidden, false)]
    public void InverseBooleanToVisibilityConverter_ConvertBack_ReturnsExpected(Visibility input, bool expected)
    {
        var converter = new InverseBooleanToVisibilityConverter();
        var result = converter.ConvertBack(input, typeof(bool), null!, System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(expected, result);
    }
}
