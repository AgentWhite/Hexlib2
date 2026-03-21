using ASL.Models;
using ASL.Models.Components;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ASLInputTool.Converters;

public class EquipmentTypeConverter : IValueConverter
{
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

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
