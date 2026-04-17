using System;
using System.Globalization;
using System.Windows.Data;
using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
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
                var leadership = unit.Leadership?.Leadership ?? 0;
                var leadershipSign = leadership >= 0 ? "+" : "";
                return $"{morale}{leadershipSign}{leadership}";
            }
            if (unit.IsHero)
            {
                var fp = unit.FirePower?.Firepower ?? 0;
                var r = unit.FirePower?.Range ?? 0;
                var morale = unit.Infantry?.Morale ?? 0;
                return $"{fp}-{r}-{morale}";
            }
            if (unit.IsSupportWeapon)
            {
                var fp = unit.FirePower?.Firepower ?? 0;
                var r = unit.FirePower?.Range ?? 0;
                var rof = unit.RateOfFire ?? 0;
                return $"{fp}-{r}-{rof}";
            }
            if (unit.IsSquad || unit.IsHalfSquad || unit.IsCrew)
            {
                var fp = unit.FirePower?.Firepower ?? 0;
                var r = unit.FirePower?.Range ?? 0;
                var morale = unit.Infantry?.Morale ?? 0;
                return $"{fp}-{r}-{morale}";
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
