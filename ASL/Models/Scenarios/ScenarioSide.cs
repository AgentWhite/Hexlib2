using ASL.Models.Units;

namespace ASL.Models.Scenarios;

/// <summary>
/// Represents a side in an ASL scenario, including nationality and role.
/// </summary>
public class ScenarioSide
{
    /// <summary>
    /// Gets or sets the nationality for this side.
    /// </summary>
    public Nationality Nationality { get; set; }

    /// <summary>
    /// Gets or sets the tactical side (Attacker or Defender).
    /// </summary>
    public Side Side { get; set; }

    /// <summary>
    /// Gets or sets the name or designation of the forces (e.g., "The 10th Guards").
    /// </summary>
    public string Name { get; set; } = string.Empty;
}