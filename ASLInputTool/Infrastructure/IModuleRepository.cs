using ASL;
using ASL.Core;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Infrastructure;
using ASL.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Repository for managing Module data within the project.
/// </summary>
public interface IModuleRepository
{
    /// <summary>
    /// Gets all modules in the project.
    /// </summary>
    ObservableCollection<AslModule> AllModules { get; }

    /// <summary>
    /// Initializes the repository with a collection of modules.
    /// </summary>
    void Initialize(IEnumerable<AslModule> modules);

    /// <summary>
    /// Processes module data, e.g., fixing relative image paths for box art.
    /// </summary>
    void ProcessData(string projectPath);

    /// <summary>
    /// Adds a new module to the project.
    /// </summary>
    void Add(AslModule module);

    /// <summary>
    /// Removes a module from the project.
    /// </summary>
    void Remove(AslModule module);

    /// <summary>
    /// Clears all modules from the project.
    /// </summary>
    void Clear();
}
