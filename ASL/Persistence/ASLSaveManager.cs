using ASL.Models;
using ASL.Models.Components;
using HexLib;
using HexLib.Persistence;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System;

namespace ASL.Persistence;

/// <summary>
/// Orchestrates saving and loading of ASL-specific data using a storage adapter.
/// </summary>
public class ASLSaveManager
{
    private readonly IStorageAdapter _storage;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ASLSaveManager"/> class.
    /// </summary>
    /// <param name="storage">The storage adapter to use for persistence.</param>
    public ASLSaveManager(IStorageAdapter storage)
    {
        _storage = storage;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    // --- Counters ---

    /// <summary>
    /// Saves a collection of counters to storage.
    /// </summary>
    /// <param name="name">The name/key for the counter collection.</param>
    /// <param name="counters">The counters to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveCountersAsync(string name, IEnumerable<Unit> counters)
    {
        string json = JsonSerializer.Serialize(counters, _jsonOptions);
        await _storage.SaveAsync($"Counters/{name}", json);
    }

    /// <summary>
    /// Loads a collection of counters from storage.
    /// </summary>
    /// <param name="name">The name/key of the counter collection to load.</param>
    /// <returns>A list of loaded units.</returns>
    public async Task<List<Unit>> LoadCountersAsync(string name)
    {
        string? json = await _storage.LoadAsync($"Counters/{name}");
        if (string.IsNullOrEmpty(json)) return new List<Unit>();
        var units = JsonSerializer.Deserialize<List<Unit>>(json, _jsonOptions) ?? new List<Unit>();
        RestoreBackReferences(units);
        return units;
    }

    private void RestoreBackReferences(IEnumerable<Unit> units)
    {
        foreach (var unit in units)
        {
            foreach (var comp in unit.Components)
            {
                comp.Initialize(unit);
            }
        }
    }

    /// <summary>
    /// Lists all available counter collection keys in storage.
    /// </summary>
    /// <returns>A list of storage keys.</returns>
    public Task<IEnumerable<string>> ListCountersAsync() => _storage.ListKeysAsync("Counters");

    // --- Project Level (Combined) ---

    /// <summary>
    /// Prepares a project for saving by copying all referenced images to a local "images" directory
    /// and updating the project's image paths to be relative.
    /// </summary>
    /// <param name="sourceProject">The project containing absolute file paths.</param>
    /// <param name="targetProjectFile">The absolute path to the .asl project file.</param>
    /// <returns>A copy of the project with relative image paths.</returns>
    public ASLProject PrepareProjectForSaving(ASLProject sourceProject, string targetProjectFile)
    {
        string? projectDir = Path.GetDirectoryName(targetProjectFile);
        if (projectDir == null) return sourceProject;

        string imagesDir = Path.Combine(projectDir, "images");
        if (!Directory.Exists(imagesDir)) Directory.CreateDirectory(imagesDir);

        string tempJson = SerializeProject(sourceProject);
        var project = DeserializeProject(tempJson) ?? new ASLProject();

        HashSet<string> usedImages = new(StringComparer.OrdinalIgnoreCase);

        foreach (var counter in project.Counters)
        {
            if (!string.IsNullOrEmpty(counter.ImagePathFront))
            {
                counter.ImagePathFront = ProcessImage(counter.ImagePathFront!, imagesDir);
                usedImages.Add(Path.GetFileName(counter.ImagePathFront));
            }
            
            if (!string.IsNullOrEmpty(counter.ImagePathBack))
            {
                counter.ImagePathBack = ProcessImage(counter.ImagePathBack!, imagesDir);
                usedImages.Add(Path.GetFileName(counter.ImagePathBack));
            }

            var portage = counter.GetComponent<PortageComponent>();
            if (portage != null && !string.IsNullOrEmpty(portage.DismantledImage))
            {
                portage.DismantledImage = ProcessImage(portage.DismantledImage, imagesDir);
                usedImages.Add(Path.GetFileName(portage.DismantledImage));
            }
        }

        foreach (var scenario in project.Scenarios.Where(s => !string.IsNullOrEmpty(s.ImagePath)))
        {
            scenario.ImagePath = ProcessImage(scenario.ImagePath!, imagesDir);
            usedImages.Add(Path.GetFileName(scenario.ImagePath));
        }

        foreach (var module in project.Modules.Where(m => m != null))
        {
            if (!string.IsNullOrEmpty(module.FrontImage))
            {
                module.FrontImage = ProcessImage(module.FrontImage!, imagesDir);
                usedImages.Add(Path.GetFileName(module.FrontImage));
            }
            
            if (!string.IsNullOrEmpty(module.BackImage))
            {
                module.BackImage = ProcessImage(module.BackImage!, imagesDir);
                usedImages.Add(Path.GetFileName(module.BackImage));
            }
        }

        // Cleanup unused images in the local images directory
        if (Directory.Exists(imagesDir))
        {
            var files = Directory.GetFiles(imagesDir);
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                if (!usedImages.Contains(fileName))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        // Ignore deletion errors (e.g., file in use)
                    }
                }
            }
        }

        return project;
    }

    private string ProcessImage(string sourcePath, string targetDir)
    {
        if (!Path.IsPathRooted(sourcePath) && sourcePath.StartsWith("images"))
        {
            return sourcePath;
        }

        if (!File.Exists(sourcePath)) return sourcePath;

        string fileName = Path.GetFileName(sourcePath);
        string extension = Path.GetExtension(sourcePath);
        
        if (Path.GetDirectoryName(Path.GetFullPath(sourcePath))?.Equals(Path.GetFullPath(targetDir), StringComparison.OrdinalIgnoreCase) == true
            && Guid.TryParse(Path.GetFileNameWithoutExtension(fileName), out _))
        {
            return Path.Combine("images", fileName);
        }

        string newFileName = $"{Guid.NewGuid()}{extension}";
        string destPath = Path.Combine(targetDir, newFileName);

        try
        {
            File.Copy(sourcePath, destPath, true);
            return Path.Combine("images", newFileName);
        }
        catch
        {
            return sourcePath; 
        }
    }

    /// <summary>
    /// Serializes an ASL project to a JSON string.
    /// </summary>
    /// <param name="project">The project to serialize.</param>
    /// <returns>A JSON string.</returns>
    public string SerializeProject(ASLProject project)
    {
        return JsonSerializer.Serialize(project, _jsonOptions);
    }

    /// <summary>
    /// Deserializes an ASL project from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string.</param>
    /// <returns>The deserialized ASL project, or null if deserialization failed.</returns>
    public ASLProject? DeserializeProject(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        var project = JsonSerializer.Deserialize<ASLProject>(json, _jsonOptions);
        if (project != null) RestoreBackReferences(project.Counters);
        return project;
    }

    // --- Scenarios ---

    /// <summary>
    /// Saves a single scenario to storage.
    /// </summary>
    /// <param name="scenario">The scenario to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveScenarioAsync(Scenario scenario)
    {
        string json = JsonSerializer.Serialize(scenario, _jsonOptions);
        await _storage.SaveAsync($"Scenarios/{scenario.Name}", json);
    }

    /// <summary>
    /// Loads a single scenario from storage.
    /// </summary>
    /// <param name="name">The name/key of the scenario.</param>
    /// <returns>The loaded scenario, or null if not found.</returns>
    public async Task<Scenario?> LoadScenarioAsync(string name)
    {
        string? json = await _storage.LoadAsync($"Scenarios/{name}");
        if (string.IsNullOrEmpty(json)) return null;
        return JsonSerializer.Deserialize<Scenario>(json, _jsonOptions);
    }

    /// <summary>
    /// Lists all available scenario keys in storage.
    /// </summary>
    /// <returns>A list of scenario names.</returns>
    public Task<IEnumerable<string>> ListScenariosAsync() => _storage.ListKeysAsync("Scenarios");

    // --- Boards (Maps) ---

    /// <summary>
    /// Saves an ASL board/map to storage.
    /// </summary>
    /// <param name="name">The name/key for the board.</param>
    /// <param name="board">The board object to save.</param>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveBoardAsync(string name, Board<ASLHexMetadata, ASLEdgeData> board)
    {
        var dto = BoardDto.FromBoard(board);
        string json = JsonSerializer.Serialize(dto, _jsonOptions);
        await _storage.SaveAsync($"Maps/{name}", json);
    }

    /// <summary>
    /// Loads an ASL board/map from storage.
    /// </summary>
    /// <param name="name">The name/key of the board.</param>
    /// <returns>The loaded board, or null if not found.</returns>
    public async Task<Board<ASLHexMetadata, ASLEdgeData>?> LoadBoardAsync(string name)
    {
        string? json = await _storage.LoadAsync($"Maps/{name}");
        if (string.IsNullOrEmpty(json)) return null;
        var dto = JsonSerializer.Deserialize<BoardDto>(json, _jsonOptions);
        var board = dto?.ToBoard();
        if (board != null)
        {
            foreach (var hex in board.Hexes.Values)
            {
                RestoreBackReferences(hex.Counters.OfType<Unit>());
            }
        }
        return board;
    }

    /// <summary>
    /// Lists all available map/board keys in storage.
    /// </summary>
    /// <returns>A list of map names.</returns>
    public Task<IEnumerable<string>> ListMapsAsync() => _storage.ListKeysAsync("Maps");

    private class BoardDto
    {
        public string Name { get; set; } = string.Empty;
        public int Width { get; set; }
        public int Height { get; set; }
        public HexTopOrientation TopOrientation { get; set; }
        public List<HexDto> Hexes { get; set; } = new();

        public static BoardDto FromBoard(Board<ASLHexMetadata, ASLEdgeData> board)
        {
            var dto = new BoardDto
            {
                Name = board.Name,
                Width = board.Width,
                Height = board.Height,
                TopOrientation = board.TopOrientation
            };

            foreach (var kvp in board.Hexes)
            {
                dto.Hexes.Add(new HexDto
                {
                    Q = kvp.Key.Q,
                    R = kvp.Key.R,
                    S = kvp.Key.S,
                    Metadata = kvp.Value.Metadata ?? new ASLHexMetadata(),
                    Counters = kvp.Value.Counters.OfType<Unit>().ToList()
                });
            }
            return dto;
        }

        public Board<ASLHexMetadata, ASLEdgeData> ToBoard()
        {
            var board = new Board<ASLHexMetadata, ASLEdgeData>(Width, Height, TopOrientation)
            {
                Name = Name
            };

            foreach (var hexDto in Hexes)
            {
                var coord = new CubeCoordinate(hexDto.Q, hexDto.R, hexDto.S);
                var hex = new Hex<ASLHexMetadata>(coord)
                {
                    Metadata = hexDto.Metadata
                };
                foreach (var counter in hexDto.Counters)
                {
                    hex.AddCounter(counter);
                }
                board.AddHex(hex);
            }
            return board;
        }
    }

    private class HexDto
    {
        public int Q { get; set; }
        public int R { get; set; }
        public int S { get; set; }
        public ASLHexMetadata Metadata { get; set; } = new();
        public List<Unit> Counters { get; set; } = new();
    }
}
