using ASL.Models;
using ASL.Models.Components;
using Xunit;

namespace ASL.Tests;

public class SupportWeaponTests
{
    [Fact]
    public void Unit_IsSupportWeapon_ReturnsTrue_WhenBreakdownComponentPresent()
    {
        // Arrange
        var unit = new Unit();
        unit.AddComponent(new BreakdownComponent());

        // Act & Assert
        Assert.True(unit.IsSupportWeapon);
    }

    [Fact]
    public void Unit_SupportWeapon_CanHaveGranularComponents()
    {
        // Arrange
        var unit = new Unit();
        unit.AddComponent(new BreakdownComponent { BreakdownNumber = 10, RemovalNumber = 12 });
        unit.AddComponent(new FirePowerComponent { Firepower = 4, Range = 8, RateOfFire = 3 });
        unit.AddComponent(new PortageComponent { AssembledCost = 2, IsDismantled = true });

        // Act & Assert
        Assert.True(unit.IsSupportWeapon);
        
        Assert.Equal(3, unit.RateOfFire);

        Assert.NotNull(unit.Breakdown);
        Assert.Equal(10, unit.Breakdown.BreakdownNumber);
        Assert.Equal(12, unit.Breakdown.RemovalNumber);
        
        Assert.NotNull(unit.FirePower);
        Assert.Equal(4, unit.FirePower.Firepower);
        Assert.Equal(8, unit.FirePower.Range);

        Assert.NotNull(unit.Portage);
        Assert.Equal(2, unit.Portage.AssembledCost);
        Assert.True(unit.Portage.IsDismantled);
    }
}
