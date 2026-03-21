using ASL;
using ASL.Models;
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
