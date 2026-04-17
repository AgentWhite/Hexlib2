namespace ASL.Models.Board;

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
    /// If true, a paved road passes through this hexside.
    /// </summary>
    public bool HasPavedRoad { get; set; }

    /// <summary>
    /// If true, a dirt road passes through this hexside.
    /// </summary>
    public bool HasDirtRoad { get; set; }

    /// <summary>
    /// If true, this hexside is part of a continuous building (house).
    /// </summary>
    public bool HasHouse { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a stream through this hexside.</summary>
    public bool HasStream { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a gully through this hexside.</summary>
    public bool HasGully { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a canal through this hexside.</summary>
    public bool HasCanal { get; set; }

    /// <summary>Gets or sets a value indicating whether this is a rowhouse hexside.</summary>
    public bool IsRowhouse { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a cliff on this hexside.</summary>
    public bool HasCliff { get; set; }

    /// <summary>Gets or sets a value indicating whether there is a path crossing this hexside.</summary>
    public bool HasPath { get; set; }
}
