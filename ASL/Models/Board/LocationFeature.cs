using System;

namespace ASL.Models.Board;

/// <summary>
/// Bitwise flags representing various terrain features present in a location.
/// </summary>
[Flags]
public enum LocationFeatures
{
    /// <summary>A foxhole dug into the ground.</summary>
    Foxhole,
    /// <summary>A trench system.</summary>
    Trench,
    /// <summary>Craters from explosions.</summary>
    Shellhole
}