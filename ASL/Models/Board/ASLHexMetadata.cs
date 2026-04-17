using HexLib;

namespace ASL.Models.Board;

/// <summary>
/// Metadata for ASL-specific hexes, including terrain and elevation.
/// </summary>
public class ASLHexMetadata
{
    /// <summary>Gets or sets the type of terrain in this hex.</summary>
    public TerrainType Terrain { get; set; } = TerrainType.OpenGround;
    /// <summary>Gets or sets the elevation level of the hex (0=ground level).</summary>
    public int Elevation { get; set; } = 0; // Level 0, 1, 2...
    
    // Additional ASL features
    /// <summary>Gets or sets a value indicating whether this hex contains a manhole cover.</summary>
    public bool HasManhole { get; set; }

    /// <summary>Gets or sets a value indicating whether this hex contains a stairwell.</summary>
    public bool HasStairwell { get; set; }

    /// <summary>Gets or sets a value indicating whether this hex contains shellholes.</summary>
    public bool HasShellholes { get; set; }

    /// <summary>Gets or sets the type of rubble in this hex.</summary>
    public RubbleType Rubble { get; set; } = RubbleType.None;
}
