namespace ASL.Models.Equipment;

/// <summary>
/// Defines the type of gun muzzle, affecting range and penetration.
/// </summary>
public enum MuzzleType
{
    /// <summary>Short barrel (e.g., * weapons).</summary>
    ShortBarrel,
    /// <summary>Standard barrel.</summary>
    StandardBarrel,
    /// <summary>Long barrel (e.g., L weapons).</summary>
    LongBarrel,
    /// <summary>Extra long barrel (e.g., LL weapons).</summary>
    ExtraLongBarrel,
}