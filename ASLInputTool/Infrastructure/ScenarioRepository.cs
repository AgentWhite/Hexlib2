using ASL;
using ASL.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Concrete implementation of IScenarioRepository.
/// </summary>
public class ScenarioRepository : IScenarioRepository
{
    /// <inheritdoc />
    public ObservableCollection<Scenario> AllScenarios { get; } = new();

    /// <inheritdoc />
    public void Initialize(IEnumerable<Scenario> scenarios)
    {
        AllScenarios.Clear();
        foreach (var scenario in scenarios)
        {
            AllScenarios.Add(scenario);
        }
    }

    /// <inheritdoc />
    public void ProcessData(string projectPath)
    {
        string projectDir = Path.GetDirectoryName(projectPath) ?? string.Empty;
        if (string.IsNullOrEmpty(projectDir)) return;

        foreach (var scenario in AllScenarios)
        {
            if (!string.IsNullOrEmpty(scenario.ImagePath) && !Path.IsPathRooted(scenario.ImagePath))
            {
                scenario.ImagePath = Path.GetFullPath(Path.Combine(projectDir, scenario.ImagePath));
            }
        }
    }

    /// <inheritdoc />
    public void Add(Scenario scenario)
    {
        AllScenarios.Add(scenario);
    }

    /// <inheritdoc />
    public void Remove(Scenario scenario)
    {
        AllScenarios.Remove(scenario);
    }

    /// <inheritdoc />
    public void Clear()
    {
        AllScenarios.Clear();
    }
}
