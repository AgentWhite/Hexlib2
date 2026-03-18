using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ASL.Counters;
using HexLib;
using HexLib.Persistence;

namespace ASL.Persistence;

/// <summary>
/// Orchestrates saving and loading of ASL-specific data using a storage adapter.
/// </summary>
public class ASLSaveManager
{
    private readonly IStorageAdapter _storage;
    private readonly JsonSerializerOptions _jsonOptions;

    public ASLSaveManager(IStorageAdapter storage)
    {
        _storage = storage;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    // --- Counters ---

    public async Task SaveCountersAsync(string name, IEnumerable<BaseASLCounter> counters)
    {
        string json = JsonSerializer.Serialize(counters, _jsonOptions);
        await _storage.SaveAsync($"Counters/{name}", json);
    }

    public async Task<List<BaseASLCounter>> LoadCountersAsync(string name)
    {
        string? json = await _storage.LoadAsync($"Counters/{name}");
        if (string.IsNullOrEmpty(json)) return new List<BaseASLCounter>();
        return JsonSerializer.Deserialize<List<BaseASLCounter>>(json, _jsonOptions) ?? new List<BaseASLCounter>();
    }

    public Task<IEnumerable<string>> ListCountersAsync() => _storage.ListKeysAsync("Counters");

    // --- Project Level (Combined) ---

    public string SerializeProject(ASLProject project)
    {
        return JsonSerializer.Serialize(project, _jsonOptions);
    }

    public ASLProject? DeserializeProject(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        return JsonSerializer.Deserialize<ASLProject>(json, _jsonOptions);
    }

    // --- Scenarios ---

    public async Task SaveScenarioAsync(Scenario scenario)
    {
        string json = JsonSerializer.Serialize(scenario, _jsonOptions);
        await _storage.SaveAsync($"Scenarios/{scenario.Name}", json);
    }

    public async Task<Scenario?> LoadScenarioAsync(string name)
    {
        string? json = await _storage.LoadAsync($"Scenarios/{name}");
        if (string.IsNullOrEmpty(json)) return null;
        return JsonSerializer.Deserialize<Scenario>(json, _jsonOptions);
    }

    public Task<IEnumerable<string>> ListScenariosAsync() => _storage.ListKeysAsync("Scenarios");

    // --- Boards (Maps) ---

    public async Task SaveBoardAsync(string name, Board<ASLHexMetadata, ASLEdgeData> board)
    {
        // For Board, we need to handle the fact that it's generic and contains Hexes.
        // We'll use a simplified DTO for serialization to avoid circular refs and complex dictionaries.
        var dto = BoardDto.FromBoard(board);
        string json = JsonSerializer.Serialize(dto, _jsonOptions);
        await _storage.SaveAsync($"Maps/{name}", json);
    }

    public async Task<Board<ASLHexMetadata, ASLEdgeData>?> LoadBoardAsync(string name)
    {
        string? json = await _storage.LoadAsync($"Maps/{name}");
        if (string.IsNullOrEmpty(json)) return null;
        var dto = JsonSerializer.Deserialize<BoardDto>(json, _jsonOptions);
        return dto?.ToBoard();
    }

    public Task<IEnumerable<string>> ListMapsAsync() => _storage.ListKeysAsync("Maps");

    // Internal DTOs for Board serialization
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
                    Counters = kvp.Value.Counters.OfType<BaseASLCounter>().ToList()
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
        public List<BaseASLCounter> Counters { get; set; } = new();
    }
}
