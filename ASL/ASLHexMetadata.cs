using HexLib;

namespace ASL;

/// <summary>
/// Metadata for ASL-specific hexes, including terrain and elevation.
/// </summary>
public class ASLHexMetadata
{
    public TerrainType Terrain { get; set; } = TerrainType.OpenGround;
    public int Elevation { get; set; } = 0; // Level 0, 1, 2...
    
    // Additional ASL features
    public bool HasSewer { get; set; }
}
