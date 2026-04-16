namespace ASL.Models;

/// <summary>
/// Defines how a weapon targets its objective (Direct, Indirect, or Shaped Charge).
/// </summary>
public enum TargettingType
{
    /// <summary>Direct fire using standard to-hit procedures.</summary>
    DirectFire,
    /// <summary>Fire using shaped charge effects.</summary>
    ShapedCharge,
    /// <summary>Indirect fire (e.g., mortars, OBA).</summary>
    IndirectFire
}