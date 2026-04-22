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

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioRepository"/> class.
    /// </summary>
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
    public ObservableCollection<Insignia> AllInsignias { get; } = new();

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
    public async Task LoadInsigniasAsync()
    {
        AllInsignias.Clear();
        string baseFolder = SettingsManager.Instance.Settings.ScenariosFolder;
        if (string.IsNullOrWhiteSpace(baseFolder)) return;

        string filePath = Path.Combine(baseFolder, "insignias.asl");
        if (!File.Exists(filePath)) return;

        try
        {
            string json = await File.ReadAllTextAsync(filePath);
            var insignias = JsonSerializer.Deserialize<List<Insignia>>(json, _jsonOptions);
            if (insignias != null)
            {
                foreach (var insignia in insignias)
                {
                    FixInsigniaImagePath(insignia, baseFolder);
                    AllInsignias.Add(insignia);
                }
            }
        }
        catch { /* Handle/log error */ }
    }

    /// <inheritdoc />
    public async Task SaveInsigniaAsync(Insignia insignia)
    {
        string baseFolder = SettingsManager.Instance.Settings.ScenariosFolder;
        if (string.IsNullOrWhiteSpace(baseFolder)) return;

        string imagesDir = Path.Combine(baseFolder, "images");
        if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

        // Process insignia image
        insignia.ImagePath = ProcessImage(insignia.ImagePath, imagesDir, new HashSet<string>());

        if (!AllInsignias.Contains(insignia))
            AllInsignias.Add(insignia);

        await SaveAllInsigniasAsync();
        
        // Re-fix paths for UI display
        FixInsigniaImagePath(insignia, baseFolder);
    }

    private async Task SaveAllInsigniasAsync()
    {
        string baseFolder = SettingsManager.Instance.Settings.ScenariosFolder;
        if (string.IsNullOrWhiteSpace(baseFolder)) return;

        // Prepare copies with relative paths for saving
        var saveCopies = AllInsignias.Select(i => new Insignia
        {
            Name = i.Name,
            ImagePath = GetRelativePath(i.ImagePath, baseFolder)
        }).ToList();

        string json = JsonSerializer.Serialize(saveCopies, _jsonOptions);
        string filePath = Path.Combine(baseFolder, "insignias.asl");
        await File.WriteAllTextAsync(filePath, json);
    }

    private string? GetRelativePath(string? fullPath, string baseFolder)
    {
        if (string.IsNullOrEmpty(fullPath)) return null;
        if (!Path.IsPathRooted(fullPath)) return fullPath;
        
        string imagesDir = Path.Combine(baseFolder, "images");
        if (fullPath.StartsWith(imagesDir, StringComparison.OrdinalIgnoreCase))
        {
            return Path.Combine("images", Path.GetFileName(fullPath));
        }
        return fullPath;
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

        // Update the live object's path to the local copy
        scenario.ImagePath = relativeImagePath;
        FixScenarioImagePaths(scenario, baseFolder);
        
        // Insignias are already handled by SaveInsigniaAsync, but let's ensure paths are correct
        foreach (var side in scenario.ScenarioSides)
        {
            if (side.Insignia != null)
            {
                // We don't save the insignia full object in the scenario ASL usually?
                // Actually, Scenario class currently includes ScenarioSide list.
                // If ScenarioSide has Insignia property, it will be serialized.
                // We should make sure the insignia inside ScenarioSide also has relative path before serialize.
            }
        }

        string json = JsonSerializer.Serialize(saveCopy, _jsonOptions);
        string filePath = Path.Combine(baseFolder, $"{scenario.Name}.asl");
        await File.WriteAllTextAsync(filePath, json);
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

        foreach (var side in scenario.ScenarioSides)
        {
            if (side.Insignia != null)
            {
                FixInsigniaImagePath(side.Insignia, baseFolder);
            }
        }
    }

    private void FixInsigniaImagePath(Insignia insignia, string baseFolder)
    {
        if (!string.IsNullOrEmpty(insignia.ImagePath) && !Path.IsPathRooted(insignia.ImagePath))
            insignia.ImagePath = Path.GetFullPath(Path.Combine(baseFolder, insignia.ImagePath));
    }
}
