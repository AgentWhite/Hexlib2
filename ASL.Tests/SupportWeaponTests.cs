using ASL.Models.Units;
using ASL.Models.Components;

namespace ASL.Tests;

public class SupportWeaponTests
{
    // ── IsSupportWeapon: requires PortageComponent, excludes RadioComponent ───

    [Fact]
    public void IsSupportWeapon_WithPortageOnly_IsTrue()
    {
        var unit = new Unit { Name = "MG42" };
        unit.AddComponent(new PortageComponent { AssembledCost = 2 });

        Assert.True(unit.IsSupportWeapon);
    }

    [Fact]
    public void IsSupportWeapon_WithPortageAndBreakdown_IsTrue()
    {
        var unit = new Unit { Name = "MMG" };
        unit.AddComponent(new PortageComponent { AssembledCost = 2 });
        unit.AddComponent(new BreakdownComponent { BreakdownNumber = 10 });

        Assert.True(unit.IsSupportWeapon);
    }

    [Fact]
    public void IsSupportWeapon_WithRadioAndPortage_IsFalse()
    {
        var unit = new Unit { Name = "Radio" };
        unit.AddComponent(new PortageComponent { AssembledCost = 1 });
        unit.AddComponent(new RadioComponent());

        Assert.False(unit.IsSupportWeapon);
    }

    [Fact]
    public void IsSupportWeapon_NoPortage_IsFalse()
    {
        var unit = new Unit { Name = "Infantry" };
        unit.AddComponent(new InfantryComponent { Morale = 8, Scale = InfantryScale.Squad });

        Assert.False(unit.IsSupportWeapon);
    }

    [Fact]
    public void IsSupportWeapon_BreakdownAloneWithoutPortage_IsFalse()
    {
        var unit = new Unit { Name = "No portage" };
        unit.AddComponent(new BreakdownComponent { BreakdownNumber = 10 });

        Assert.False(unit.IsSupportWeapon);
    }

    // ── Granular SW components are accessible via convenience properties ───────

    [Fact]
    public void SupportWeapon_CanHaveGranularComponents()
    {
        var unit = new Unit { Name = "MMG" };
        unit.AddComponent(new BreakdownComponent { BreakdownNumber = 10, RemovalNumber = 12 });
        unit.AddComponent(new FirePowerComponent { Firepower = 4, Range = 8, RateOfFire = 3 });
        unit.AddComponent(new PortageComponent { AssembledCost = 2, IsDismantled = true });

        Assert.True(unit.IsSupportWeapon);

        Assert.Equal(3, unit.RateOfFire);

        Assert.NotNull(unit.Breakdown);
        Assert.Equal(10, unit.Breakdown!.BreakdownNumber);
        Assert.Equal(12, unit.Breakdown!.RemovalNumber);

        Assert.NotNull(unit.FirePower);
        Assert.Equal(4, unit.FirePower!.Firepower);
        Assert.Equal(8, unit.FirePower!.Range);

        Assert.NotNull(unit.Portage);
        Assert.Equal(2, unit.Portage!.AssembledCost);
        Assert.True(unit.Portage!.IsDismantled);
    }
}
