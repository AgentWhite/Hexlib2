namespace ASL;

/// <summary>
/// Represents data on a hexside in ASL (Walls, Hedges, Roads, etc.).
/// </summary>
public class ASLEdgeData
{
    public bool HasWall { get; set; }
    public bool HasHedge { get; set; }
    public bool HasBocage { get; set; }
    
    /// <summary>
    /// If true, a road follows this hexside (connecting two hexes).
    /// </summary>
    public bool HasRoad { get; set; }
}
