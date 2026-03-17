using ASL;
using ASL.Counters;
using HexLib;
using Xunit;

namespace ASL.Tests;

public class InfantryCounterTests
{
    [Fact]
    public void Leader_PropertyAssignment()
    {
        var leader = new Leader("Sgt. Steiner", 9, 1, Nationality.German);
        Assert.Equal("Sgt. Steiner", leader.Name);
        Assert.Equal(9, leader.Morale);
        Assert.Equal(1, leader.Leadership);
        Assert.Equal(Nationality.German, leader.Nationality);
    }

    [Fact]
    public void Hero_PropertyAssignment()
    {
        var hero = new Hero("Cpl. Miller", 1, 4, 9, Nationality.Russian);
        Assert.Equal("Cpl. Miller", hero.Name);
        Assert.Equal(1, hero.Firepower);
        Assert.Equal(4, hero.Range);
        Assert.Equal(9, hero.Morale);
        Assert.Equal(Nationality.Russian, hero.Nationality);
    }

    [Fact]
    public void Squad_PropertyAssignment()
    {
        var squad = new Squad("1st Squad", 4, 6, 7, Nationality.Finnish);
        squad.Class = UnitClass.FirstLine;
        squad.BrokenMoraleLevel = 8;
        squad.HasAssaultFire = true;

        Assert.Equal(4, squad.Firepower);
        Assert.Equal(6, squad.Range);
        Assert.Equal(7, squad.Morale);
        Assert.Equal(UnitClass.FirstLine, squad.Class);
        Assert.Equal(8, squad.BrokenMoraleLevel);
        Assert.True(squad.HasAssaultFire);
        Assert.Equal(Nationality.Finnish, squad.Nationality);
    }

    [Fact]
    public void Crew_IsAlwaysElite()
    {
        var crew = new Crew("Gun Crew", 2, 2, 8, Nationality.German);
        Assert.Equal(UnitClass.Elite, crew.Class);
        Assert.Equal(Nationality.German, crew.Nationality);

        // Attempting to change class should be ignored
        crew.Class = UnitClass.Conscript;
        Assert.Equal(UnitClass.Elite, crew.Class);
    }

    [Fact]
    public void Counters_CanBeAddedToHex()
    {
        var hex = new Hex<ASLHexMetadata>(new CubeCoordinate(0, 0, 0));
        var leader = new Leader("Lt. Dan", 8, 1, Nationality.Partisan);
        var squad = new Squad("A Co", 4, 4, 7, Nationality.Partisan);

        hex.AddCounter(leader);
        hex.AddCounter(squad);

        Assert.Equal(2, hex.Counters.Count);
        Assert.Contains(leader, hex.Counters);
        Assert.Contains(squad, hex.Counters);
    }
}
