namespace HexLib;

/// <summary>
/// Bitwise flags representing the physical edges of a rectangular map board that contain half-hexes,
/// intended to be joined directly to half-hexes on adjacent map boards.
/// </summary>
[Flags]
public enum BoardEdge
{
    /// <summary>No board edges.</summary>
    None = 0,
    
    /// <summary>The top edge of the coordinate space.</summary>
    Top = 1,
    
    /// <summary>The right edge of the coordinate space.</summary>
    Right = 2,
    
    /// <summary>The bottom edge of the coordinate space.</summary>
    Bottom = 4,
    
    /// <summary>The left edge of the coordinate space.</summary>
    Left = 8
}
