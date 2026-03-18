using System;
using System.Globalization;
using System.Windows.Data;
using ASL.Counters;

namespace ASLInputTool.Converters;

/// <summary>
/// Converts an ASL counter to its formatted string representation of stats for display in the UI.
/// Separation of concerns: Keep display formatting out of the core library.
/// </summary>
public class CounterStatsConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            Leader leader => $"{leader.Morale}{(leader.Leadership >= 0 ? "+" : "")}{leader.Leadership}",
            Hero hero => $"{hero.Firepower}-{hero.Range}-{hero.Morale}",
            MultiManCounter mmc => $"{mmc.Firepower}-{mmc.Range}-{mmc.Morale}{(mmc.HasSmokeExponent ? "s" + (mmc.SmokePlacementExponent > 0 ? mmc.SmokePlacementExponent.ToString() : "") : "")}",
            _ => string.Empty
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
