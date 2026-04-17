using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;
using Xunit;

namespace ASL.Tests;

public class ComponentIntegrityTests
{
    [Fact]
    public void Unit_ComponentAddition_UpdatesHelperProperties()
    {
        var unit = new Unit();
        
        Assert.False(unit.IsLeader);
        Assert.False(unit.IsSquad);

        unit.AddComponent(new LeadershipComponent { Leadership = 1 });
        unit.AddComponent(new InfantryComponent { Scale = InfantryScale.SMC });
        unit.UnitType = UnitType.SMC;
        
        Assert.True(unit.IsLeader);
        Assert.False(unit.IsSquad);

        unit.AddComponent(new FirePowerComponent { Firepower = 4, Range = 6 });
        unit.GetComponent<InfantryComponent>()!.Scale = InfantryScale.Squad;
        unit.UnitType = UnitType.MMC;

        Assert.True(unit.IsSquad);
    }

    [Fact]
    public void Unit_GetComponent_ReturnsSpecificInstance()
    {
        var unit = new Unit();
        var smoke = new SmokeProviderComponent { CapabilityNumber = 2 };
        unit.AddComponent(smoke);

        var retrieved = unit.GetComponent<SmokeProviderComponent>();
        Assert.Same(smoke, retrieved);
        Assert.Equal(2, retrieved?.CapabilityNumber);
    }

    [Fact]
    public void Unit_IsHero_RequiresHeroComponent()
    {
        var unit = new Unit { Name = "Heroic Leader" };
        unit.AddComponent(new InfantryComponent { Scale = InfantryScale.SMC });
        unit.AddComponent(new LeadershipComponent { Leadership = 1 });
        
        Assert.False(unit.IsHero);

        unit.AddComponent(new HeroComponent());
        Assert.True(unit.IsHero);
    }
}
