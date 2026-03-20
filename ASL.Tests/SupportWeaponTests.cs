using ASL.Models;
using ASL.Models.Components;
using Xunit;

namespace ASL.Tests;

public class SupportWeaponTests
{
    [Fact]
    public void Unit_IsSupportWeapon_ReturnsTrue_WhenComponentPresent()
    {
        // Arrange
        var unit = new Unit();
        unit.AddComponent(new SupportWeaponComponent());

        // Act & Assert
        Assert.True(unit.IsSupportWeapon);
        Assert.NotNull(unit.SupportWeapon);
    }

    [Fact]
    public void Unit_IsSupportWeapon_ReturnsFalse_WhenComponentAbsent()
    {
        // Arrange
        var unit = new Unit();

        // Act & Assert
        Assert.False(unit.IsSupportWeapon);
        Assert.Null(unit.SupportWeapon);
    }
}
