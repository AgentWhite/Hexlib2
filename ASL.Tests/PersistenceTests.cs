using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ASL.Counters;
using ASL.Persistence;
using HexLib;
using HexLib.Persistence;
using Xunit;

namespace ASL.Tests;

public class PersistenceTests
{
    private readonly string _testDataDir = Path.Combine(Path.GetTempPath(), "ASLTestData");

    [Fact]
    public async Task CanSaveAndLoadCounters()
    {
        // Setup
        var storage = new FileStorageAdapter(_testDataDir);
        var manager = new ASLSaveManager(storage);
        var counters = new List<BaseASLCounter>
        {
            new Leader("Sgt. Steiner", 9, 2, Nationality.German),
            new Squad("1st Squad", 4, 6, 7, UnitClass.FirstLine, Nationality.German)
        };

        // Act
        await manager.SaveCountersAsync("TestCounters", counters);
        var loaded = await manager.LoadCountersAsync("TestCounters");

        // Assert
        Assert.Equal(2, loaded.Count);
        Assert.IsType<Leader>(loaded[0]);
        Assert.IsType<Squad>(loaded[1]);
        Assert.Equal("Sgt. Steiner", loaded[0].Name);
        Assert.Equal(9, loaded[0].Morale);
        
        // Cleanup
        Directory.Delete(_testDataDir, true);
    }

    [Fact]
    public async Task CanSaveAndLoadScenario()
    {
        // Setup
        var storage = new FileStorageAdapter(_testDataDir);
        var manager = new ASLSaveManager(storage);
        var scenario = new Scenario
        {
            Name = "TEST SCENARIO",
            Reference = "T1",
            Description = new ScenarioDescription("Place", "Date", "Desc")
        };

        // Act
        await manager.SaveScenarioAsync(scenario);
        var loaded = await manager.LoadScenarioAsync("TEST SCENARIO");

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(scenario.Name, loaded!.Name);
        Assert.Equal(scenario.Description.Place, loaded.Description.Place);

        // Cleanup
        Directory.Delete(_testDataDir, true);
    }

    [Fact]
    public async Task CanSaveAndLoadBoard()
    {
        // Setup
        var storage = new FileStorageAdapter(_testDataDir);
        var manager = new ASLSaveManager(storage);
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10) { Name = "Board 1" };
        
        var hex = new Hex<ASLHexMetadata>(new CubeCoordinate(0, 0, 0))
        {
            Metadata = new ASLHexMetadata { Terrain = TerrainType.Woods }
        };
        hex.AddCounter(new Squad("1st Squad", 4, 6, 7, UnitClass.FirstLine, Nationality.German));
        board.AddHex(hex);

        // Act
        await manager.SaveBoardAsync("TestMap", board);
        var loaded = await manager.LoadBoardAsync("TestMap");

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal("Board 1", loaded!.Name);
        var loadedHex = loaded.GetHexAt(new CubeCoordinate(0, 0, 0));
        Assert.NotNull(loadedHex);
        Assert.Equal(TerrainType.Woods, loadedHex!.Metadata.Terrain);
        Assert.Single(loadedHex.Counters);
        Assert.IsType<Squad>(loadedHex.Counters[0]);
        Assert.Equal("1st Squad", loadedHex.Counters[0].Name);

        // Cleanup
        Directory.Delete(_testDataDir, true);
    }
}
