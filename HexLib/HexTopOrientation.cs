namespace HexLib;

/// <summary>
/// Represents the physical geometry layout of hexes on a wargame board.
/// </summary>
public enum HexTopOrientation
{
    /// <summary>Hexes have a pointy top (Odd-R offset layout). Most common in wargames.</summary>
    PointyTopped,
    
    /// <summary>Hexes have a flat top (Odd-Q offset layout).</summary>
    FlatTopped
}
