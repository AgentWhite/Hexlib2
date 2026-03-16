/// <summary>
/// Represents the outer edge of a hex map.
/// </summary>
/// <remarks>
/// This enumeration is typically used when connecting or "butting" multiple
/// hex maps together. By specifying which edge of a map is being referenced,
/// adjacent maps can be aligned so that the hexes along one map edge connect
/// seamlessly with the hexes of another map.
///
/// For example, the <see cref="Right"/> edge of one map may be connected to the
/// <see cref="Left"/> edge of another map to create a larger continuous world.
/// The exact alignment of hex coordinates depends on the chosen hex orientation
/// (for example pointy-top or flat-top layouts).
/// </remarks>
public enum MapEdge
{
    /// <summary>
    /// The upper boundary of the map.
    /// Used when attaching another map above this one.
    /// </summary>
    Top,

    /// <summary>
    /// The lower boundary of the map.
    /// Used when attaching another map below this one.
    /// </summary>
    Bottom,

    /// <summary>
    /// The left boundary of the map.
    /// Used when attaching another map to the left side.
    /// </summary>
    Left,

    /// <summary>
    /// The right boundary of the map.
    /// Used when attaching another map to the right side.
    /// </summary>
    Right
}