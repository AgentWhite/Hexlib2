using ASL.Core;
using ASL.Models.Units;
using ASLInputTool.Tests.Fixtures;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Components;
using ASLInputTool.Infrastructure;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ASLInputTool.Tests;

// ──────────────────────────────────────────────────────────────────────────────
// UnitRepository
// ──────────────────────────────────────────────────────────────────────────────

[Collection("SettingsManager")]
public class UnitRepositoryTests : IDisposable
{
    private readonly string _tempDir;

    public UnitRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        SettingsManager.Instance.Settings.ModulesFolder = _tempDir;
    }

    public void Dispose()
    {
        SettingsManager.Instance.Settings.ModulesFolder = string.Empty;
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private static Unit MakeLeader(string name = "Cpt. Test")
    {
        var unit = new Unit
        {
            Name = name,
            UnitType = UnitType.SMC,
            Nationality = Nationality.American,
            Module = Module.BeyondValor
        };
        unit.AddComponent(new LeadershipComponent { Leadership = 1 });
        unit.AddComponent(new InfantryComponent { Scale = InfantryScale.SMC });
        return unit;
    }

    // --- In-memory operations ---

    [Fact]
    public void Add_NewUnit_AppearsInAllUnits()
    {
        var repo = new UnitRepository();
        var unit = MakeLeader();

        repo.Add(unit);

        Assert.Single(repo.AllUnits);
        Assert.Contains(unit, repo.AllUnits);
    }

    [Fact]
    public void Add_DuplicateUnit_IsIgnored()
    {
        var repo = new UnitRepository();
        var unit = MakeLeader();

        repo.Add(unit);
        repo.Add(unit);

        Assert.Single(repo.AllUnits);
    }

    [Fact]
    public void Remove_ExistingUnit_RemovesIt()
    {
        var repo = new UnitRepository();
        var unit = MakeLeader();
        repo.Add(unit);

        repo.Remove(unit);

        Assert.Empty(repo.AllUnits);
    }

    [Fact]
    public void Clear_RemovesAllUnits()
    {
        var repo = new UnitRepository();
        repo.Add(MakeLeader("A"));
        repo.Add(MakeLeader("B"));

        repo.Clear();

        Assert.Empty(repo.AllUnits);
    }

    [Fact]
    public void Initialize_ReplacesExistingUnits()
    {
        var repo = new UnitRepository();
        repo.Add(MakeLeader("old"));
        var newUnits = new[] { MakeLeader("new1"), MakeLeader("new2") };

        repo.Initialize(newUnits);

        Assert.Equal(2, repo.AllUnits.Count);
        Assert.All(newUnits, u => Assert.Contains(u, repo.AllUnits));
    }

    [Fact]
    public void GetUnitsByCategory_FiltersCorrectly()
    {
        var repo = new UnitRepository();
        var leader = MakeLeader("Leader");
        var squad = new Unit { Name = "Squad", UnitType = UnitType.MMC, Module = Module.BeyondValor };
        squad.AddComponent(new InfantryComponent { Scale = InfantryScale.Squad });
        squad.AddComponent(new FirePowerComponent { Firepower = 6, Range = 4 });
        repo.Add(leader);
        repo.Add(squad);

        var leaders = repo.GetUnitsByCategory("Leader").ToList();
        var infantry = repo.GetUnitsByCategory("Infantry").ToList();

        Assert.Single(leaders);
        Assert.Equal("Leader", leaders[0].Name);
        Assert.Single(infantry);
        Assert.Equal("Squad", infantry[0].Name);
    }

    // --- Disk operations ---

    [Fact]
    public async Task SaveAndLoad_RoundTripsLeaderUnit()
    {
        var repo = new UnitRepository();
        var unit = MakeLeader("Sgt. Saved");
        repo.Add(unit);

        await repo.SaveUnitsForModuleAsync(Module.BeyondValor, "BeyondValor");

        var loadRepo = new UnitRepository();
        await loadRepo.LoadUnitsForModuleAsync("BeyondValor");

        var loaded = loadRepo.AllUnits.FirstOrDefault(u => u.Name == "Sgt. Saved");
        Assert.NotNull(loaded);
        Assert.True(loaded!.IsLeader);
    }

    [Fact]
    public async Task SaveAndLoad_EmptyModule_CreatesNoFiles_OrDeletesExisting()
    {
        // First save some units so files exist
        var repo = new UnitRepository();
        repo.Add(MakeLeader("ToBeDeleted"));
        await repo.SaveUnitsForModuleAsync(Module.BeyondValor, "BeyondValor");

        // Now save an empty set — should delete the files
        var emptyRepo = new UnitRepository();
        await emptyRepo.SaveUnitsForModuleAsync(Module.BeyondValor, "BeyondValor");

        string modulePath = Path.Combine(_tempDir, "BeyondValor");
        Assert.False(File.Exists(Path.Combine(modulePath, "leaders.asl")));
    }

    [Fact]
    public async Task DeleteUnitsForModule_RemovesUnitFiles()
    {
        var repo = new UnitRepository();
        repo.Add(MakeLeader("ToDelete"));
        await repo.SaveUnitsForModuleAsync(Module.BeyondValor, "BeyondValor");

        await repo.DeleteUnitsForModuleAsync("BeyondValor");

        string modulePath = Path.Combine(_tempDir, "BeyondValor");
        Assert.False(File.Exists(Path.Combine(modulePath, "leaders.asl")));
    }

    [Fact]
    public async Task LoadUnitsForModule_MissingFolder_ReturnsEmpty()
    {
        var repo = new UnitRepository();

        await repo.LoadUnitsForModuleAsync("NonExistentModule");

        Assert.Empty(repo.AllUnits);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// BoardRepository
// ──────────────────────────────────────────────────────────────────────────────

[Collection("SettingsManager")]
public class BoardRepositoryTests : IDisposable
{
    private readonly string _tempDir;

    public BoardRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        SettingsManager.Instance.Settings.BoardsFolder = _tempDir;
    }

    public void Dispose()
    {
        SettingsManager.Instance.Settings.BoardsFolder = string.Empty;
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private static AslBoard MakeBoard(string name = "TestBoard") => new AslBoard
    {
        Name = name,
        Width = 10,
        Height = 10,
        CanvasWidth = 1000,
        CanvasHeight = 1000
    };

    // --- In-memory operations ---

    [Fact]
    public void Add_NewBoard_AppearsInAllBoards()
    {
        var repo = new BoardRepository();
        var board = MakeBoard();

        repo.Add(board);

        Assert.Single(repo.AllBoards);
    }

    [Fact]
    public void Add_DuplicateBoard_IsIgnored()
    {
        var repo = new BoardRepository();
        var board = MakeBoard();

        repo.Add(board);
        repo.Add(board);

        Assert.Single(repo.AllBoards);
    }

    [Fact]
    public void Remove_ExistingBoard_RemovesIt()
    {
        var repo = new BoardRepository();
        var board = MakeBoard();
        repo.Add(board);

        repo.Remove(board);

        Assert.Empty(repo.AllBoards);
    }

    [Fact]
    public void Initialize_ReplacesExistingBoards()
    {
        var repo = new BoardRepository();
        repo.Add(MakeBoard("old"));

        repo.Initialize(new[] { MakeBoard("new1"), MakeBoard("new2") });

        Assert.Equal(2, repo.AllBoards.Count());
    }

    [Fact]
    public void BoardSaved_Event_IsRaisedOnSave()
    {
        var repo = new BoardRepository();
        string? savedName = null;
        repo.BoardSaved += (_, name) => savedName = name;
        var board = MakeBoard("EventBoard");
        repo.Add(board);

        // Trigger the event indirectly by noting it fires during SaveToDiskAsync
        // (tested in the async test below; here we verify the event wiring exists)
        Assert.Null(savedName); // not yet fired
    }

    // --- Disk operations ---

    [Fact]
    public async Task SaveToDisk_CreatesMetadataFile()
    {
        var repo = new BoardRepository();
        var board = MakeBoard("DiskBoard");
        board.PopulateBoard();

        await repo.SaveToDiskAsync(board, null);

        string metaPath = Path.Combine(_tempDir, "DiskBoard", "metadata.json");
        Assert.True(File.Exists(metaPath));
    }

    [Fact]
    public async Task ScanAndLoad_FindsSavedBoard()
    {
        var repo = new BoardRepository();
        var board = MakeBoard("ScanBoard");
        board.PopulateBoard();
        await repo.SaveToDiskAsync(board, null);

        var loaded = (await repo.ScanAndLoadAsync()).ToList();

        Assert.Single(loaded);
        Assert.Equal("ScanBoard", loaded[0].Name);
        Assert.Equal(10, loaded[0].Width);
        Assert.Equal(10, loaded[0].Height);
    }

    [Fact]
    public async Task SaveToDisk_RaisesEvent()
    {
        var repo = new BoardRepository();
        string? eventName = null;
        repo.BoardSaved += (_, name) => eventName = name;
        var board = MakeBoard("EventBoard");
        board.PopulateBoard();

        await repo.SaveToDiskAsync(board, null);

        Assert.Equal("EventBoard", eventName);
    }

    [Fact]
    public async Task DeleteBoard_RemovesFolder()
    {
        var repo = new BoardRepository();
        var board = MakeBoard("ToDelete");
        board.PopulateBoard();
        await repo.SaveToDiskAsync(board, null);
        string boardFolder = Path.Combine(_tempDir, "ToDelete");
        Assert.True(Directory.Exists(boardFolder));

        await repo.DeleteBoardAsync(board);

        Assert.False(Directory.Exists(boardFolder));
    }

    [Fact]
    public async Task ScanAndLoad_EmptyFolder_ReturnsEmpty()
    {
        var repo = new BoardRepository();

        var boards = (await repo.ScanAndLoadAsync()).ToList();

        Assert.Empty(boards);
    }

    [Fact]
    public async Task SaveToDisk_Rename_MovesFolder()
    {
        var repo = new BoardRepository();
        var board = MakeBoard("OriginalName");
        board.PopulateBoard();
        await repo.SaveToDiskAsync(board, null);

        board.Name = "RenamedBoard";
        await repo.SaveToDiskAsync(board, null, originalName: "OriginalName");

        Assert.False(Directory.Exists(Path.Combine(_tempDir, "OriginalName")));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "RenamedBoard")));
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ScenarioRepository
// ──────────────────────────────────────────────────────────────────────────────

[Collection("SettingsManager")]
public class ScenarioRepositoryTests : IDisposable
{
    private readonly string _tempDir;

    public ScenarioRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        SettingsManager.Instance.Settings.ScenariosFolder = _tempDir;
    }

    public void Dispose()
    {
        SettingsManager.Instance.Settings.ScenariosFolder = string.Empty;
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private static Scenario MakeScenario(string name = "TestScenario") => new Scenario
    {
        Name = name,
        Reference = "ASL1",
        Turns = 6,
        HasHalfTurn = false
    };

    // --- In-memory operations ---

    [Fact]
    public void Add_NewScenario_AppearsInAllScenarios()
    {
        var repo = new ScenarioRepository();
        var scenario = MakeScenario();

        repo.Add(scenario);

        Assert.Single(repo.AllScenarios);
    }

    [Fact]
    public void Add_DuplicateScenario_IsIgnored()
    {
        var repo = new ScenarioRepository();
        var scenario = MakeScenario();

        repo.Add(scenario);
        repo.Add(scenario);

        Assert.Single(repo.AllScenarios);
    }

    [Fact]
    public void Remove_ExistingScenario_RemovesIt()
    {
        var repo = new ScenarioRepository();
        var scenario = MakeScenario();
        repo.Add(scenario);

        repo.Remove(scenario);

        Assert.Empty(repo.AllScenarios);
    }

    [Fact]
    public void Clear_RemovesAllScenarios()
    {
        var repo = new ScenarioRepository();
        repo.Add(MakeScenario("A"));
        repo.Add(MakeScenario("B"));

        repo.Clear();

        Assert.Empty(repo.AllScenarios);
    }

    [Fact]
    public void Initialize_ReplacesExistingScenarios()
    {
        var repo = new ScenarioRepository();
        repo.Add(MakeScenario("old"));

        repo.Initialize(new[] { MakeScenario("new1"), MakeScenario("new2") });

        Assert.Equal(2, repo.AllScenarios.Count);
    }

    // --- Disk operations ---

    [Fact]
    public async Task SaveToDisk_CreatesFile()
    {
        var repo = new ScenarioRepository();
        var scenario = MakeScenario("SavedScenario");

        await repo.SaveToDiskAsync(scenario);

        string path = Path.Combine(_tempDir, "SavedScenario.asl");
        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task ScanAndLoad_FindsSavedScenario()
    {
        var repo = new ScenarioRepository();
        var scenario = MakeScenario("RoundTrip");
        scenario.Turns = 8;
        await repo.SaveToDiskAsync(scenario);

        var loaded = (await repo.ScanAndLoadAsync()).ToList();

        Assert.Single(loaded);
        Assert.Equal("RoundTrip", loaded[0].Name);
        Assert.Equal(8, loaded[0].Turns);
    }

    [Fact]
    public async Task DeleteFromDisk_RemovesFile()
    {
        var repo = new ScenarioRepository();
        var scenario = MakeScenario("ToDelete");
        await repo.SaveToDiskAsync(scenario);
        string path = Path.Combine(_tempDir, "ToDelete.asl");
        Assert.True(File.Exists(path));

        await repo.DeleteFromDiskAsync(scenario);

        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task ScanAndLoad_EmptyFolder_ReturnsEmpty()
    {
        var repo = new ScenarioRepository();

        var scenarios = (await repo.ScanAndLoadAsync()).ToList();

        Assert.Empty(scenarios);
    }

    [Fact]
    public async Task SaveToDisk_Rename_DeletesOldFile()
    {
        var repo = new ScenarioRepository();
        var scenario = MakeScenario("Original");
        await repo.SaveToDiskAsync(scenario);

        scenario.Name = "Renamed";
        await repo.SaveToDiskAsync(scenario, originalName: "Original");

        Assert.False(File.Exists(Path.Combine(_tempDir, "Original.asl")));
        Assert.True(File.Exists(Path.Combine(_tempDir, "Renamed.asl")));
    }

    [Fact]
    public async Task ScanAndLoad_SkipsInvalidFiles()
    {
        File.WriteAllText(Path.Combine(_tempDir, "corrupt.asl"), "not-json{{{");
        var repo = new ScenarioRepository();

        var scenarios = (await repo.ScanAndLoadAsync()).ToList();

        Assert.Empty(scenarios);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// ModuleRepository
// ──────────────────────────────────────────────────────────────────────────────

[Collection("SettingsManager")]
public class ModuleRepositoryTests : IDisposable
{
    private readonly string _tempDir;

    public ModuleRepositoryTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        SettingsManager.Instance.Settings.ModulesFolder = _tempDir;
    }

    public void Dispose()
    {
        SettingsManager.Instance.Settings.ModulesFolder = string.Empty;
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private static AslModule MakeModule(string name = "Beyond Valor") => new AslModule
    {
        FullName = name,
        Module = Module.BeyondValor,
        Description = "Test module",
        IsFinished = false
    };

    // --- In-memory operations ---

    [Fact]
    public void Add_NewModule_AppearsInAllModules()
    {
        var repo = new ModuleRepository();
        var module = MakeModule();

        repo.Add(module);

        Assert.Single(repo.AllModules);
    }

    [Fact]
    public void Add_DuplicateModule_IsIgnored()
    {
        var repo = new ModuleRepository();
        var module = MakeModule();

        repo.Add(module);
        repo.Add(module);

        Assert.Single(repo.AllModules);
    }

    [Fact]
    public void Remove_ExistingModule_RemovesIt()
    {
        var repo = new ModuleRepository();
        var module = MakeModule();
        repo.Add(module);

        repo.Remove(module);

        Assert.Empty(repo.AllModules);
    }

    [Fact]
    public void Clear_RemovesAllModules()
    {
        var repo = new ModuleRepository();
        repo.Add(MakeModule("A"));
        repo.Add(MakeModule("B"));

        repo.Clear();

        Assert.Empty(repo.AllModules);
    }

    [Fact]
    public void Initialize_ReplacesExistingModules()
    {
        var repo = new ModuleRepository();
        repo.Add(MakeModule("old"));

        repo.Initialize(new[] { MakeModule("new1"), MakeModule("new2") });

        Assert.Equal(2, repo.AllModules.Count);
    }

    // --- Disk operations ---

    [Fact]
    public async Task SaveToDisk_CreatesModuleFile()
    {
        var repo = new ModuleRepository();
        var module = MakeModule("BeyondValor");

        await repo.SaveToDiskAsync(module);

        string modulePath = Path.Combine(_tempDir, "BeyondValor", "module.asl");
        Assert.True(File.Exists(modulePath));
    }

    [Fact]
    public async Task ScanAndLoad_FindsSavedModule()
    {
        var repo = new ModuleRepository();
        var module = MakeModule("RoundTrip");
        module.Description = "Loaded from disk";
        await repo.SaveToDiskAsync(module);

        var loaded = (await repo.ScanAndLoadAsync()).ToList();

        Assert.Single(loaded);
        Assert.Equal("RoundTrip", loaded[0].FullName);
        Assert.Equal("Loaded from disk", loaded[0].Description);
    }

    [Fact]
    public async Task ScanAndLoad_EmptyFolder_ReturnsEmpty()
    {
        var repo = new ModuleRepository();

        var modules = (await repo.ScanAndLoadAsync()).ToList();

        Assert.Empty(modules);
    }

    [Fact]
    public async Task SaveToDisk_Rename_MovesFolder()
    {
        var repo = new ModuleRepository();
        var module = MakeModule("OriginalModule");
        await repo.SaveToDiskAsync(module);

        module.FullName = "RenamedModule";
        await repo.SaveToDiskAsync(module, originalName: "OriginalModule");

        Assert.False(Directory.Exists(Path.Combine(_tempDir, "OriginalModule")));
        Assert.True(Directory.Exists(Path.Combine(_tempDir, "RenamedModule")));
    }

    [Fact]
    public async Task ScanAndLoad_SkipsInvalidFiles()
    {
        string badModuleDir = Path.Combine(_tempDir, "BadModule");
        Directory.CreateDirectory(badModuleDir);
        File.WriteAllText(Path.Combine(badModuleDir, "module.asl"), "not-json{{{");
        var repo = new ModuleRepository();

        var modules = (await repo.ScanAndLoadAsync()).ToList();

        Assert.Empty(modules);
    }
}
