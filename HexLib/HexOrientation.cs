
/// <summary>
/// Specifies the orientation of a hexagonal grid.
/// </summary>
/// <remarks>
/// PointyTop means the hexagon has a vertex facing upward,
/// while FlatTop means the hexagon has a flat side facing upward.
/// </remarks>
public enum HexOrientation
{
    /// <summary>
    /// Hexagons are oriented with a vertex pointing upward.
    /// </summary>
    PointyTop,

    /// <summary>
    /// Hexagons are oriented with a flat side facing upward.
    /// </summary>
    FlatTop
}