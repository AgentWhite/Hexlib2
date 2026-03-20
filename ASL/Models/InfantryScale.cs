namespace ASL.Models;

/// <summary>
/// Specifies the scale of an infantry unit.
/// </summary>
public enum InfantryScale
{
    /// <summary>Full squad.</summary>
    Squad,
    /// <summary>Half-squad.</summary>
    HalfSquad,
    /// <summary>Gun or weapon crew.</summary>
    Crew,
    /// <summary>Single-man counter (Leader or Hero).</summary>
    SMC
}