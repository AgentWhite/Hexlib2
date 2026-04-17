using System.Collections.Generic;
using ASL.Models.Units;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Board;

namespace ASL.Models;

/// <summary>
/// Bundles ASL data (Counters, Scenarios) together for project-level persistence.
/// </summary>
public class ASLProject
{
    /// <summary>
    /// Gets or sets the list of counters in the project.
    /// </summary>
    public List<Unit> Counters { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of scenarios in the project.
    /// </summary>
    public List<Scenario> Scenarios { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of game modules in the project.
    /// </summary>
    public List<AslModule> Modules { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of map boards in the project.
    /// </summary>
    public List<AslBoard> Boards { get; set; } = new();
}
