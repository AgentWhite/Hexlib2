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
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Concrete implementation of IScenarioRepository.
/// </summary>
public class ScenarioRepository : IScenarioRepository
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ScenarioRepository()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

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
    public void ProcessData(string projectPath) { }

    /// <inheritdoc />
    public void Add(Scenario scenario)
    {
        if (!AllScenarios.Contains(scenario))
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

    /// <inheritdoc />
    public async Task<IEnumerable<Scenario>> ScanAndLoadAsync()
    {
        var scenarios = new List<Scenario>();
        string baseFolder = SettingsManager.Instance.Settings.ScenariosFolder;
        
        if (string.IsNullOrWhiteSpace(baseFolder) || !Directory.Exists(baseFolder))
            return scenarios;

        foreach (var file in Directory.GetFiles(baseFolder, "*.asl"))
        {
            try
            {
                string json = await File.ReadAllTextAsync(file);
                var scenario = JsonSerializer.Deserialize<Scenario>(json, _jsonOptions);
                if (scenario != null)
                {
                    FixScenarioImagePaths(scenario, baseFolder);
                    scenarios.Add(scenario);
                }
            }
            catch { /* Skip invalid scenarios */ }
        }
        return scenarios;
    }

    /// <inheritdoc />
    public async Task SaveToDiskAsync(Scenario scenario, string? originalName = null)
    {
        string baseFolder = SettingsManager.Instance.Settings.ScenariosFolder;
        if (string.IsNullOrWhiteSpace(baseFolder)) return;

        if (!Directory.Exists(baseFolder)) Directory.CreateDirectory(baseFolder);

        // Handle renaming by deleting the old file
        if (!string.IsNullOrEmpty(originalName) && originalName != scenario.Name)
        {
            string oldFile = Path.Combine(baseFolder, $"{originalName}.asl");
            if (File.Exists(oldFile)) File.Delete(oldFile);
        }

        string imagesDir = Path.Combine(baseFolder, "images");
        if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

        // Process image to GUID-based relative path
        HashSet<string> usedImages = new(StringComparer.OrdinalIgnoreCase);
        string? relativeImagePath = ProcessImage(scenario.ImagePath, imagesDir, usedImages);

        // Prepare copy for saving
        var saveCopy = new Scenario
        {
            Name = scenario.Name,
            Reference = scenario.Reference,
            Turns = scenario.Turns,
            HasHalfTurn = scenario.HasHalfTurn,
            Description = scenario.Description,
            ScenarioSides = scenario.ScenarioSides,
            ImagePath = relativeImagePath
        };

        string json = JsonSerializer.Serialize(saveCopy, _jsonOptions);
        string filePath = Path.Combine(baseFolder, $"{scenario.Name}.asl");
        await File.WriteAllTextAsync(filePath, json);

        // Update the live object's path to the local copy
        scenario.ImagePath = relativeImagePath;
        FixScenarioImagePaths(scenario, baseFolder);
    }

    /// <inheritdoc />
    public async Task DeleteFromDiskAsync(Scenario scenario)
    {
        string baseFolder = SettingsManager.Instance.Settings.ScenariosFolder;
        if (string.IsNullOrWhiteSpace(baseFolder)) return;

        string filePath = Path.Combine(baseFolder, $"{scenario.Name}.asl");
        if (File.Exists(filePath)) File.Delete(filePath);
    }

    private string? ProcessImage(string? sourcePath, string targetDir, HashSet<string> usedImages)
    {
        if (string.IsNullOrEmpty(sourcePath)) return null;

        if (!Path.IsPathRooted(sourcePath) && sourcePath.StartsWith("images"))
            return sourcePath;

        if (!File.Exists(sourcePath)) return sourcePath;

        string extension = Path.GetExtension(sourcePath);
        string fullSourcePath = Path.GetFullPath(sourcePath);
        string fullTargetDir = Path.GetFullPath(targetDir);

        if (fullSourcePath.StartsWith(fullTargetDir, StringComparison.OrdinalIgnoreCase) 
            && Guid.TryParse(Path.GetFileNameWithoutExtension(sourcePath), out _))
        {
            return Path.Combine("images", Path.GetFileName(sourcePath));
        }

        string newFileName = $"{Guid.NewGuid()}{extension}";
        string destPath = Path.Combine(targetDir, newFileName);

        try
        {
            File.Copy(sourcePath, destPath, true);
            return Path.Combine("images", newFileName);
        }
        catch { return sourcePath; }
    }

    private void FixScenarioImagePaths(Scenario scenario, string baseFolder)
    {
        if (!string.IsNullOrEmpty(scenario.ImagePath) && !Path.IsPathRooted(scenario.ImagePath))
            scenario.ImagePath = Path.GetFullPath(Path.Combine(baseFolder, scenario.ImagePath));
    }
}
