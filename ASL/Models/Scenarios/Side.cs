using System.Text.Json.Serialization;

namespace ASL.Models.Scenarios;

/// <summary>
/// Represents the side of a unit in a scenario and it determines
/// who sets up first and who moves first.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Side
{
    /// <summary>
    /// The attacker is the side that sets up second and moves first.
    /// </summary>
    Attacker,
    /// <summary>
    /// The defender is the side that sets up first and moves second.
    /// </summary>
    Defender
}