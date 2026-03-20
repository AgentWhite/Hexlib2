namespace ASL.Models;


/// <summary>
/// Represents the method by which smoke is delivered.
/// </summary>
public enum SmokeDeliveryMethod
{
    /// <summary>
    /// Smoke is placed by an infantry unit.
    /// </summary>
    Placement_dr,
    /// <summary>
    /// Smoke is delivered by ordnance.
    /// </summary>
    Ordnance_DR,
    /// <summary>
    /// Smoke is delivered by a dispenser.
    /// </summary>
    Dispenser_DR
}