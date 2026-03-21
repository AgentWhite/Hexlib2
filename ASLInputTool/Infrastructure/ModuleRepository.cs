using ASL;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Concrete implementation of IModuleRepository.
/// </summary>
public class ModuleRepository : IModuleRepository
{
    /// <inheritdoc />
    public ObservableCollection<AslModule> AllModules { get; } = new();

    /// <inheritdoc />
    public void Initialize(IEnumerable<AslModule> modules)
    {
        AllModules.Clear();
        foreach (var module in modules)
        {
            AllModules.Add(module);
        }
    }

    /// <inheritdoc />
    public void ProcessData(string projectPath)
    {
        string projectDir = Path.GetDirectoryName(projectPath) ?? string.Empty;
        if (string.IsNullOrEmpty(projectDir)) return;

        foreach (var module in AllModules)
        {
            if (!string.IsNullOrEmpty(module.FrontImage) && !Path.IsPathRooted(module.FrontImage))
            {
                module.FrontImage = Path.GetFullPath(Path.Combine(projectDir, module.FrontImage));
            }
            if (!string.IsNullOrEmpty(module.BackImage) && !Path.IsPathRooted(module.BackImage))
            {
                module.BackImage = Path.GetFullPath(Path.Combine(projectDir, module.BackImage));
            }
        }
    }

    /// <inheritdoc />
    public void Add(AslModule module)
    {
        AllModules.Add(module);
    }

    /// <inheritdoc />
    public void Remove(AslModule module)
    {
        AllModules.Remove(module);
    }

    /// <inheritdoc />
    public void Clear()
    {
        AllModules.Clear();
    }
}
