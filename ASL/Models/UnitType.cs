namespace ASL.Models;

/// <summary>
/// Specifies the general type of unit.
/// </summary>
public enum UnitType
{
    /// <summary>Single-Man Counter (Leaders, Heroes).</summary>
    SMC,
    /// <summary>Multi-Man Counter (Squads, Half-Squads, Crews).</summary>
    MMC,
    /// <summary>Vehicles (Tanks, Trucks, etc.).</summary>
    Vehicle,
    /// <summary>Ordnance (Guns, Mortars, etc.).</summary>
    Ordnance,
    /// <summary>Mountable units (Horses, etc.).</summary>
    Mountable
}