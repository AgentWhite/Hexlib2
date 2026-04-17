using System.Text.Json.Serialization;

namespace ASL.Models.Board;

/// <summary>
/// Specifies the type of terrain in a hex.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TerrainType
{
    /// <summary>Clear terrain with no modifiers.</summary>
    OpenGround,
    /// <summary>Densely wooded area.</summary>
    Woods,
    /// <summary>Area with scattered trees.</summary>
    Orchard,
    /// <summary>Agricultural crops.</summary>
    Grain,
    /// <summary>Low, scrubby vegetation.</summary>
    Brush,
    /// <summary>A sturdy stone structure.</summary>
    StoneBuilding,
    /// <summary>A lighter wooden structure.</summary>
    WoodenBuilding,
    /// <summary>A body of water.</summary>
    Water,
    /// <summary>A swampy or marshy area.</summary>
    Marsh,
    /// <summary>A place of burial.</summary>
    Graveyard,
    /// <summary>Rugged, rocky terrain with boulders.</summary>
    Crag,
    /// <summary>Small body of water within a hex.</summary>
    Pond,
    /// <summary>Area with stacks of processed timber.</summary>
    Lumberyard
}
