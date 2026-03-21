using ASLInputTool.Converters;
using ASL.Models;
using ASL.Models.Components;
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

    [Fact]
    public void CounterStatsConverter_FormatsSmokeCorrectly()
    {
        var converter = new CounterStatsConverter();
        var unit = new Unit { Name = "Smoke Squad", UnitType = UnitType.MMC };
        unit.AddComponent(new InfantryComponent { Morale = 7, Scale = InfantryScale.Squad });
        unit.AddComponent(new FirePowerComponent { Firepower = 4, Range = 6 });
        
        // No smoke
        var statsNoSmoke = converter.Convert(unit, typeof(string), null!, System.Globalization.CultureInfo.InvariantCulture) as string;
        Assert.Equal("4-6-7", statsNoSmoke);

        // With smoke exponent 0 (should still show 4-6-7 in the stats column as requested)
        var smoke0 = new SmokeProviderComponent { CapabilityNumber = 0, SmokeType = SmokeType.White };
        unit.AddComponent(smoke0);
        var statsSmoke0 = converter.Convert(unit, typeof(string), null!, System.Globalization.CultureInfo.InvariantCulture) as string;
        Assert.Equal("4-6-7", statsSmoke0);

        // With smoke exponent 2 (should still show 4-6-7 in the stats column as requested)
        unit.RemoveComponent(smoke0);
        var smoke2 = new SmokeProviderComponent { CapabilityNumber = 2, SmokeType = SmokeType.White };
        unit.AddComponent(smoke2);
        var statsSmoke2 = converter.Convert(unit, typeof(string), null!, System.Globalization.CultureInfo.InvariantCulture) as string;
        Assert.Equal("4-6-7", statsSmoke2);
    }
}
