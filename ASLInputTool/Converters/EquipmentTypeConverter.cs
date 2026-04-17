using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ASLInputTool.Converters;

/// <summary>
/// Converts a unit to a string descriptive of its specific equipment type (MG type, Radio vs Telephone, etc.).
/// </summary>
public class EquipmentTypeConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Unit unit)
        {
            var mg = unit.GetComponent<MachineGunComponent>();
            if (mg != null)
                return mg.Type.ToString();

            var radio = unit.GetComponent<RadioComponent>();
            if (radio != null)
                return unit.Portage != null ? "Radio" : "Telephone";

            var latw = unit.GetComponent<LightAntiTankWeaponComponent>();
            if (latw != null)
                return latw.WeaponType.ToString();
        }
        return string.Empty;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
