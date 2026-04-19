using ASL.Models.Board;
using System.Collections.Generic;

namespace ASL.Persistence;

/// <summary>
/// Data Transfer Object for persisting custom LOS terrain drawings.
/// </summary>
public class LosDataDto
{
    /// <summary>Gets or sets the collection of terrain drawings.</summary>
    public List<TerrainDrawingDto> Drawings { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for an individual terrain polygon.
/// </summary>
public class TerrainDrawingDto
{
    /// <summary>Gets or sets the type of terrain.</summary>
    public TerrainType TerrainType { get; set; }
    
    /// <summary>
    /// Gets or sets the SVG path string representing the geometry.
    /// </summary>
    public string PathData { get; set; } = string.Empty;
}
