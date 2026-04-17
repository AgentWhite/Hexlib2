using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;
using Xunit;

namespace ASL.Tests;

public class PortageTests
{
    [Fact]
    public void PortageComponent_CanBeDismantled_ReturnsTrue_WhenDismantledCostSet()
    {
        // Arrange
        var component = new PortageComponent
        {
            AssembledCost = 5,
            DismantledCost = 2
        };

        // Act & Assert
        Assert.True(component.CanBeDismantled);
    }

    [Fact]
    public void PortageComponent_CanBeDismantled_ReturnsFalse_WhenDismantledCostNull()
    {
        // Arrange
        var component = new PortageComponent
        {
            AssembledCost = 5,
            DismantledCost = null
        };

        // Act & Assert
        Assert.False(component.CanBeDismantled);
    }

    [Fact]
    public void PortageComponent_Dismantle_SetsIsDismantled_WhenAllowed()
    {
        // Arrange
        var component = new PortageComponent
        {
            AssembledCost = 5,
            DismantledCost = 2
        };

        // Act
        component.Dismantle();

        // Assert
        Assert.True(component.IsDismantled);
    }

    [Fact]
    public void PortageComponent_Dismantle_DoesNotSetIsDismantled_WhenNotAllowed()
    {
        // Arrange
        var component = new PortageComponent
        {
            AssembledCost = 5,
            DismantledCost = null
        };

        // Act
        component.Dismantle();

        // Assert
        Assert.False(component.IsDismantled);
    }

    [Fact]
    public void PortageComponent_Assemble_ResetsIsDismantled()
    {
        // Arrange
        var component = new PortageComponent
        {
            AssembledCost = 5,
            DismantledCost = 2,
            IsDismantled = true
        };

        // Act
        component.Assemble();

        // Assert
        Assert.False(component.IsDismantled);
    }
}
