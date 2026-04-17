using ASL.Models.Modules;
using System.Collections.Generic;
using System.Linq;

namespace ASL.Models.Scenarios;

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
    /// Gets or sets the file path to the image representing this scenario.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Gets or sets the historical background and context of the scenario.
    /// </summary>
    public ScenarioDescription Description { get; set; } = new();

    /// <summary>
    /// Gets or sets the physical module this scenario belongs to.
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Gets or sets the scenario sides for the scenario. Must at least contain
    /// an attacker and a defender.
    /// </summary>
    public List<ScenarioSide> ScenarioSides { get; set; } = new();

    /// <summary>
    /// Gets the side designated as the Attacker.
    /// </summary>
    public ScenarioSide? AttackerSide => ScenarioSides.FirstOrDefault(s => s.Side == Side.Attacker);

    /// <summary>
    /// Gets the side designated as the Defender.
    /// </summary>
    public ScenarioSide? DefenderSide => ScenarioSides.FirstOrDefault(s => s.Side == Side.Defender);

    /// <summary>
    /// Gets or sets the number of turns for the scenario.
    /// </summary>
    public int Turns { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the final turn is a half-turn (only the first player acts).
    /// </summary>
    public bool HasHalfTurn { get; set; }

    /// <summary>
    /// Defines which physical direction on the global map represents 
    /// Magnetic North for ASL rules (Wind, Drift, Random Direction, etc).
    /// </summary>
    public HexLib.PhysicalDirection MagneticNorth { get; set; } = HexLib.PhysicalDirection.NorthEast;
}
