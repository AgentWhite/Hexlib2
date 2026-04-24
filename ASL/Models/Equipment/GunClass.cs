namespace ASL.Models.Equipment;

/// <summary>
/// Classification of a direct-fire gun, used to select the appropriate To-Hit chart and case-modifier set.
/// </summary>
public enum GunClass
{
    /// <summary>Anti-Tank Gun (AT) — dedicated direct-fire anti-armor piece.</summary>
    AntiTank,
    /// <summary>Field Gun / Artillery — long-range direct or dual-purpose piece.</summary>
    Artillery,
    /// <summary>Infantry Gun — short-range dual HE/HEAT support piece.</summary>
    InfantryGun,
    /// <summary>Howitzer — primarily indirect fire with secondary direct capability.</summary>
    Howitzer,
    /// <summary>Recoilless Rifle — light direct-fire weapon, typically with backblast.</summary>
    RecoillessRifle,
    /// <summary>Anti-Aircraft Gun — high-ROF piece usable against air and (usually) ground targets.</summary>
    AntiAircraft,
}
