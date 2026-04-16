using ASL.Models.Maps;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Contains tool-specific metadata about a saved board.
/// </summary>
public class BoardMetadata
{
    /// <summary>Gets or sets the name/identifier of the board.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the map type (Standard, etc.).</summary>
    public MapType Type { get; set; } = MapType.Standard;
    /// <summary>Gets or sets the horizontal dimension in hexes.</summary>
    public int Width { get; set; }
    /// <summary>Gets or sets the vertical dimension in hexes.</summary>
    public int Height { get; set; }
    /// <summary>Gets or sets the filename of the background image.</summary>
    public string? ImageFileName { get; set; }
    /// <summary>Gets or sets the canvas width in pixels.</summary>
    public int CanvasWidth { get; set; }
    /// <summary>Gets or sets the canvas height in pixels.</summary>
    public int CanvasHeight { get; set; }

    /// <summary>Gets or sets a value indicating whether the first column is shifted down.</summary>
    public bool IsFirstColShiftedDown { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the top row contains half-hexes.</summary>
    public bool IsTopRowHalf { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether the bottom row contains half-hexes.</summary>
    public bool IsBottomRowHalf { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether the left edge contains half-hexes.</summary>
    public bool IsLeftEdgeHalf { get; set; } = true;
    /// <summary>Gets or sets a value indicating whether the right edge contains half-hexes.</summary>
    public bool IsRightEdgeHalf { get; set; } = true;
}
