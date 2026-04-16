namespace ASL.Models.Maps;

/// <summary>
/// Specifies the geometric or logical type of an ASL map.
/// </summary>
public enum MapType
{
    /// <summary>
    /// A standard 10x33 hex board (e.g., Board 1).
    /// </summary>
    Standard,

    /// <summary>Geomorphic half-board (10x17 hexes).</summary>
    HalfBoard,

    /// <summary>Bonus Pack board (17x20 hexes).</summary>
    BonusPack,

    /// <summary>Starter Pack board (17x22 hexes).</summary>
    StarterPack,

    /// <summary>
    /// A board with non-standard dimensions or half-hex configurations.
    /// </summary>
    NonStandard,

    /// <summary>
    /// A standalone map (e.g., Red Barricades, Deluxe boards).
    /// </summary>
    StandAlone
}
