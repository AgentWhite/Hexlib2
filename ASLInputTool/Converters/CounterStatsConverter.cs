using System;
using System.Globalization;
using System.Windows.Data;
using ASL.Models;
using ASL.Models.Components;

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
        if (value is Unit unit)
        {
            if (unit.IsLeader)
            {
                var morale = unit.Infantry?.Morale ?? 0;
                var brokenMorale = unit.Infantry?.BrokenMorale;
                var leadership = unit.Leadership?.Leadership ?? 0;
                var leadershipSign = leadership >= 0 ? "+" : "";
                var bmPart = brokenMorale.HasValue ? $"-{brokenMorale.Value}" : "";
                return $"{morale}{bmPart}{leadershipSign}{leadership}";
            }
            if (unit.IsHero)
            {
                var fp = unit.FirePower?.Firepower ?? 0;
                var r = unit.FirePower?.Range ?? 0;
                var morale = unit.Infantry?.Morale ?? 0;
                return $"{fp}-{r}-{morale}";
            }
            if (unit.IsSquad || unit.IsHalfSquad || unit.IsCrew)
            {
                var fp = unit.FirePower?.Firepower ?? 0;
                var r = unit.FirePower?.Range ?? 0;
                var smokeComp = unit.GetComponent<SmokeProviderComponent>();
                var smoke = smokeComp != null ? "s" + (smokeComp.CapabilityNumber > 0 ? smokeComp.CapabilityNumber.ToString() : "") : "";
                var morale = unit.Infantry?.Morale ?? 0;
                return $"{fp}-{r}-{morale}{smoke}";
            }
        }
        return string.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
