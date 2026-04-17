using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;
using HexLib;
using Xunit;

namespace ASL.Tests;

public class InfantryCounterTests
{
    [Fact]
    public void Leader_PropertyAssignment()
    {
        var leader = new Unit 
        { 
            Name = "Sgt. Steiner", 
            Nationality = Nationality.German, 
            UnitType = UnitType.SMC
        };
        leader.AddComponent(new InfantryComponent { Morale = 9, AslClass = UnitClass.Elite, Scale = InfantryScale.SMC });
        leader.AddComponent(new LeadershipComponent { Leadership = 1 });

        Assert.Equal("Sgt. Steiner", leader.Name);
        Assert.Equal(9, leader.Infantry?.Morale);
        Assert.Equal(1, leader.Leadership?.Leadership);
        Assert.Equal(Nationality.German, leader.Nationality);
        Assert.True(leader.IsLeader);
    }

    [Fact]
    public void Hero_PropertyAssignment()
    {
        var hero = new Unit 
        { 
            Name = "Cpl. Miller", 
            Nationality = Nationality.Russian, 
            UnitType = UnitType.SMC
        };
        hero.AddComponent(new InfantryComponent { Morale = 9, AslClass = UnitClass.Elite, Scale = InfantryScale.SMC });
        hero.AddComponent(new HeroComponent());
        hero.AddComponent(new FirePowerComponent { Firepower = 1, Range = 4 });

        Assert.Equal("Cpl. Miller", hero.Name);
        Assert.Equal(1, hero.FirePower?.Firepower);
        Assert.Equal(4, hero.FirePower?.Range);
        Assert.Equal(9, hero.Infantry?.Morale);
        Assert.Equal(Nationality.Russian, hero.Nationality);
        Assert.True(hero.IsHero);
    }

    [Fact]
    public void Squad_PropertyAssignment()
    {
        var squad = new Unit 
        { 
            Name = "1st Squad", 
            Nationality = Nationality.Finnish,
            UnitType = UnitType.MMC
        };
        var infantry = new InfantryComponent 
        { 
            Scale = InfantryScale.Squad, 
            HasAssaultFire = true,
            Morale = 7, 
            BrokenMorale = 8,
            AslClass = UnitClass.FirstLine
        };
        squad.AddComponent(infantry);
        squad.AddComponent(new FirePowerComponent { Firepower = 4, Range = 6 });

        Assert.Equal(4, squad.FirePower?.Firepower);
        Assert.Equal(6, squad.FirePower?.Range);
        Assert.Equal(7, squad.Infantry?.Morale);
        Assert.Equal(UnitClass.FirstLine, squad.Infantry?.AslClass);
        Assert.Equal(8, squad.Infantry?.BrokenMorale);
        Assert.True(squad.Infantry?.HasAssaultFire);
        Assert.Equal(Nationality.Finnish, squad.Nationality);
        Assert.True(squad.IsSquad);
    }

    [Fact]
    public void JapaneseLeader_NoBrokenMorale()
    {
        var leader = new Unit 
        { 
            Name = "Lt. Sakai", 
            Nationality = Nationality.Japanese, 
            UnitType = UnitType.SMC 
        };
        leader.AddComponent(new InfantryComponent { Morale = 10, BrokenMorale = null, Scale = InfantryScale.SMC });
        leader.AddComponent(new LeadershipComponent { Leadership = 1 });

        Assert.Equal(10, leader.Infantry?.Morale);
        Assert.Null(leader.Infantry?.BrokenMorale);
        Assert.True(leader.IsLeader);
    }

    [Fact]
    public void Crew_CanBeSetToAnyClass()
    {
        var crew = new Unit 
        { 
            Name = "Gun Crew", 
            Nationality = Nationality.German,
            UnitType = UnitType.MMC
        };
        var infantry = new InfantryComponent { Scale = InfantryScale.Crew, Morale = 8, AslClass = UnitClass.Elite };
        crew.AddComponent(infantry);
        crew.AddComponent(new FirePowerComponent { Firepower = 2, Range = 2 });

        Assert.Equal(UnitClass.Elite, crew.Infantry?.AslClass);
        Assert.Equal(Nationality.German, crew.Nationality);
        Assert.True(crew.IsCrew);

        // Attempting to change class should now work (fixing the "Always Elite" bug)
        infantry.AslClass = UnitClass.Conscript;
        Assert.Equal(UnitClass.Conscript, crew.Infantry?.AslClass);
    }

    [Fact]
    public void Counters_CanBeAddedToHex()
    {
        var hex = new Hex<ASLHexMetadata>(new CubeCoordinate(0, 0, 0));
        
        var leader = new Unit { Name = "Lt. Dan", Nationality = Nationality.Partisan, UnitType = UnitType.SMC };
        leader.AddComponent(new InfantryComponent { Morale = 8, AslClass = UnitClass.Elite, Scale = InfantryScale.SMC });
        leader.AddComponent(new LeadershipComponent { Leadership = 1 });
        
        var squad = new Unit { Name = "A Co", Nationality = Nationality.Partisan, UnitType = UnitType.MMC };
        squad.AddComponent(new InfantryComponent { Scale = InfantryScale.Squad, Morale = 7, AslClass = UnitClass.FirstLine });

        hex.AddCounter(leader);
        hex.AddCounter(squad);

        Assert.Equal(2, hex.Counters.Count);
        Assert.Contains(leader, hex.Counters);
        Assert.Contains(squad, hex.Counters);
    }

    [Fact]
    public void Squad_ELRCapability()
    {
        var squad = new Unit { Name = "ELR Squad", Nationality = Nationality.German, UnitType = UnitType.MMC };
        var infantry = new InfantryComponent { Scale = InfantryScale.Squad, Morale = 7, HasELR = true };
        squad.AddComponent(infantry);

        Assert.True(squad.Infantry?.HasELR);
        
        infantry.HasELR = false;
        Assert.False(squad.Infantry?.HasELR);
    }
}
