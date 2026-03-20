using ASL.Models;
using ASL.Models.Components;
using ASL.Persistence;
using HexLib;
using HexLib.Persistence;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        
        var leader = new Unit { Name = "Sgt. Steiner", Nationality = Nationality.German, UnitType = UnitType.SMC };
        leader.AddComponent(new InfantryComponent { Morale = 9, AslClass = UnitClass.Elite, Scale = InfantryScale.SMC });
        leader.AddComponent(new LeadershipComponent { Leadership = 2 });
        
        var squad = new Unit { Name = "1st Squad", Nationality = Nationality.German, UnitType = UnitType.MMC };
        squad.AddComponent(new InfantryComponent { Scale = InfantryScale.Squad, HasAssaultFire = true, Morale = 7, BrokenMorale = 8, AslClass = UnitClass.FirstLine });
        squad.AddComponent(new FirePowerComponent { Firepower = 4, Range = 6 });

        var counters = new List<Unit> { leader, squad };

        // Act
        await manager.SaveCountersAsync("TestCounters", counters);
        var loaded = await manager.LoadCountersAsync("TestCounters");

        // Assert
        Assert.Equal(2, loaded.Count);
        Assert.Equal("Sgt. Steiner", loaded[0].Name);
        Assert.True(loaded[0].IsLeader);
        Assert.Equal(2, loaded[0].Leadership?.Leadership);
        
        Assert.Equal("1st Squad", loaded[1].Name);
        Assert.True(loaded[1].IsSquad);
        Assert.True(loaded[1].Infantry?.HasAssaultFire);
        
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
        
        var leader = new Unit { Name = "Sgt. Steiner", Nationality = Nationality.German, UnitType = UnitType.SMC };
        leader.AddComponent(new InfantryComponent { Morale = 9, AslClass = UnitClass.Elite, Scale = InfantryScale.SMC });
        leader.AddComponent(new LeadershipComponent { Leadership = 2 });
        
        var squad = new Unit { Name = "1st Squad", Nationality = Nationality.German, UnitType = UnitType.MMC };
        squad.AddComponent(new InfantryComponent { Scale = InfantryScale.Squad, HasAssaultFire = true, Morale = 7, AslClass = UnitClass.FirstLine });
        squad.AddComponent(new FirePowerComponent { Firepower = 4, Range = 6 });

        var project = new ASLProject
        {
            Counters = new List<Unit> { leader, squad },
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
        
        var loadedSquad = loaded.Counters[1];
        Assert.Equal("1st Squad", loadedSquad.Name);
        Assert.True(loadedSquad.IsSquad);
        Assert.Equal(4, loadedSquad.FirePower?.Firepower);
        Assert.True(loadedSquad.Infantry?.HasAssaultFire);
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
        
        var squad = new Unit { Name = "1st Squad", Nationality = Nationality.German, UnitType = UnitType.MMC };
        squad.AddComponent(new InfantryComponent { Scale = InfantryScale.Squad, HasAssaultFire = true, Morale = 7, AslClass = UnitClass.FirstLine });
        
        hex.AddCounter(squad);
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
        
        var loadedSquad = Assert.IsType<Unit>(loadedHex.Counters[0]);
        Assert.Equal("1st Squad", loadedSquad.Name);
        Assert.True(loadedSquad.IsSquad);
        Assert.True(loadedSquad.Infantry?.HasAssaultFire);

        // Cleanup
        Directory.Delete(_testDataDir, true);
    }
}
