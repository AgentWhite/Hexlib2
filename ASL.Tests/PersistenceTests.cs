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
            {
                HasAssaultFire = true
            },
            new HalfSquad("1st Squad HS", 2, 6, 7, UnitClass.FirstLine, Nationality.German),
            new Crew("Gun Crew", 2, 2, 8, Nationality.German)
        };

        // Act
        await manager.SaveCountersAsync("TestCounters", counters);
        var loaded = await manager.LoadCountersAsync("TestCounters");

        // Assert
        Assert.Equal(4, loaded.Count);
        
        var leader = Assert.IsType<Leader>(loaded[0]);
        Assert.Equal("Sgt. Steiner", leader.Name);

        var squad = Assert.IsType<Squad>(loaded[1]);
        Assert.Equal("1st Squad", squad.Name);
        
        var hs = Assert.IsType<HalfSquad>(loaded[2]);
        Assert.Equal("1st Squad HS", hs.Name);
        Assert.Equal(2, hs.Firepower);

        var crew = Assert.IsType<Crew>(loaded[3]);
        Assert.Equal("Gun Crew", crew.Name);
        Assert.Equal(UnitClass.Elite, crew.AslClass);
        
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
    public void CanSerializeAndDeserializeProject()
    {
        // Setup
        var manager = new ASLSaveManager(new FileStorageAdapter(Path.GetTempPath()));
        var project = new ASLProject
        {
            Counters = new List<BaseASLCounter>
            {
                new Leader("Sgt. Steiner", 9, 2, Nationality.German),
                new Squad("1st Squad", 4, 6, 7, UnitClass.FirstLine, Nationality.German) { HasAssaultFire = true }
            },
            Scenarios = new List<Scenario>
            {
                new Scenario { Name = "S1", Reference = "REF1" }
            }
        };

        // Act
        string json = manager.SerializeProject(project);
        var loaded = manager.DeserializeProject(json);

        // Assert
        Assert.NotNull(loaded);
        Assert.Equal(2, loaded!.Counters.Count);
        
        var squad = Assert.IsType<Squad>(loaded.Counters[1]);
        Assert.Equal("1st Squad", squad.Name);
        Assert.Equal(4, squad.Firepower);
        Assert.True(squad.HasAssaultFire);
        Assert.Equal(UnitClass.FirstLine, squad.AslClass);
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
        hex.AddCounter(new Squad("1st Squad", 4, 6, 7, UnitClass.FirstLine, Nationality.German) { HasAssaultFire = true });
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
        var squad = Assert.IsType<Squad>(loadedHex.Counters[0]);
        Assert.Equal("1st Squad", squad.Name);
        Assert.True(squad.HasAssaultFire);

        // Cleanup
        Directory.Delete(_testDataDir, true);
    }
}
