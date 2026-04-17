using ASL;
using ASL.Core;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Infrastructure;
using ASL.Services;
using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Repository for managing Scenario data within the project.
/// </summary>
public interface IScenarioRepository
{
    /// <summary>
    /// Gets all scenarios in the project.
    /// </summary>
    ObservableCollection<Scenario> AllScenarios { get; }

    /// <summary>
    /// Initializes the repository with a collection of scenarios.
    /// </summary>
    void Initialize(IEnumerable<Scenario> scenarios);

    /// <summary>
    /// Processes scenario data, e.g., fixing relative image paths.
    /// </summary>
    void ProcessData(string projectPath);

    /// <summary>
    /// Adds a new scenario to the project.
    /// </summary>
    void Add(Scenario scenario);

    /// <summary>
    /// Removes a scenario from the project.
    /// </summary>
    void Remove(Scenario scenario);

    /// <summary>
    /// Clears all scenarios from the project.
    /// </summary>
    void Clear();
}
