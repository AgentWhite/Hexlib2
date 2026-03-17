namespace HexLib;

/// <summary>
/// Represents the 6 physical cardinal directions of movement on a pointy-top hex grid.
/// </summary>
public enum PhysicalDirection
{
    /// <summary>Direction pointing directly up (used primarily in FlatTopped layouts).</summary>
    North,
    
    /// <summary>Direction pointing top-left.</summary>
    NorthWest,
    
    /// <summary>Direction pointing top-right.</summary>
    NorthEast,
    
    /// <summary>Direction pointing directly right (used primarily in PointyTopped layouts).</summary>
    East,
    
    /// <summary>Direction pointing bottom-right.</summary>
    SouthEast,
    
    /// <summary>Direction pointing directly down (used primarily in FlatTopped layouts).</summary>
    South,
    
    /// <summary>Direction pointing bottom-left.</summary>
    SouthWest,
    
    /// <summary>Direction pointing directly left (used primarily in PointyTopped layouts).</summary>
    West
}
