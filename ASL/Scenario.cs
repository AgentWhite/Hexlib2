namespace ASL;

/// <summary>
/// Represents an ASL Scenario, defining the starting state and context of a game.
/// </summary>
public class Scenario
{
    /// <summary>
    /// Gets or sets the formal name of the scenario.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the scenario reference code (e.g., "ASL SCENARIO 1").
    /// </summary>
    public string Reference { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the historical background and context of the scenario.
    /// </summary>
    public ScenarioDescription Description { get; set; } = new();
}
