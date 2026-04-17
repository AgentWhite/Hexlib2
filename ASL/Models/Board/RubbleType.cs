namespace ASL.Models.Board;

/// <summary>
/// Defines the types of rubble that can occupy a hex.
/// </summary>
public enum RubbleType
{
    /// <summary>No rubble is present.</summary>
    None,
    /// <summary>Rubble consisting primarily of wood (from wooden buildings).</summary>
    Wooden,
    /// <summary>Rubble consisting primarily of stone or brick (from stone buildings).</summary>
    Stone
}
