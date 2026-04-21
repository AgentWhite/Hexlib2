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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

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

    /// <summary>
    /// Saves a scenario to disk as a discrete file.
    /// </summary>
    Task SaveToDiskAsync(Scenario scenario, string? originalName = null);

    /// <summary>
    /// Scans the scenarios folder and loads all scenarios.
    /// </summary>
    Task<IEnumerable<Scenario>> ScanAndLoadAsync();

    /// <summary>
    /// Deletes a scenario file from disk.
    /// </summary>
    Task DeleteFromDiskAsync(Scenario scenario);

    /// <summary>
    /// Gets all shared insignias in the project.
    /// </summary>
    ObservableCollection<Insignia> AllInsignias { get; }

    /// <summary>
    /// Loads the shared insignias from disk.
    /// </summary>
    Task LoadInsigniasAsync();

    /// <summary>
    /// Saves a new insignia or updates an existing one on disk.
    /// </summary>
    Task SaveInsigniaAsync(Insignia insignia);
}
