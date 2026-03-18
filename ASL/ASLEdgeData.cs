namespace ASL;

/// <summary>
/// Represents data on a hexside in ASL (Walls, Hedges, Roads, etc.).
/// </summary>
public class ASLEdgeData
{
    /// <summary>Gets or sets a value indicating whether there is a wall on this hexside.</summary>
    public bool HasWall { get; set; }
    /// <summary>Gets or sets a value indicating whether there is a hedge on this hexside.</summary>
    public bool HasHedge { get; set; }
    /// <summary>Gets or sets a value indicating whether there is bocage on this hexside.</summary>
    public bool HasBocage { get; set; }
    
    /// <summary>
    /// If true, a road follows this hexside (connecting two hexes).
    /// </summary>
    public bool HasRoad { get; set; }
}
