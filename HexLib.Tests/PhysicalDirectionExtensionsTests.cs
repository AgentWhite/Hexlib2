using System;
using Xunit;
using HexLib;

namespace HexLib.Tests;

public class PhysicalDirectionExtensionsTests
{
    [Theory]
    [InlineData(PhysicalDirection.NorthWest, 1, PhysicalDirection.NorthEast)]
    [InlineData(PhysicalDirection.NorthWest, 2, PhysicalDirection.East)]
    [InlineData(PhysicalDirection.NorthWest, 6, PhysicalDirection.NorthWest)]   // Full rotation
    [InlineData(PhysicalDirection.NorthWest, 7, PhysicalDirection.NorthEast)]   // Full rotation + 1
    [InlineData(PhysicalDirection.West, -1, PhysicalDirection.SouthWest)]       // Counter-clockwise
    [InlineData(PhysicalDirection.NorthEast, -2, PhysicalDirection.West)]
    public void RotateClockwise_PointyTopped_RotatesCorrectly(PhysicalDirection start, int steps, PhysicalDirection expected)
    {
        var result = start.RotateClockwise(steps, HexTopOrientation.PointyTopped);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(PhysicalDirection.North, 1, PhysicalDirection.NorthEast)]
    [InlineData(PhysicalDirection.North, 2, PhysicalDirection.SouthEast)]
    [InlineData(PhysicalDirection.North, 6, PhysicalDirection.North)]
    [InlineData(PhysicalDirection.NorthWest, -1, PhysicalDirection.SouthWest)]
    public void RotateClockwise_FlatTopped_RotatesCorrectly(PhysicalDirection start, int steps, PhysicalDirection expected)
    {
        var result = start.RotateClockwise(steps, HexTopOrientation.FlatTopped);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RotateClockwise_InvalidDirectionForOrientation_ThrowsArgumentException()
    {
        // North is not natively a valid edge direction for PointyTopped hexes
        Assert.Throws<ArgumentException>(() => PhysicalDirection.North.RotateClockwise(1, HexTopOrientation.PointyTopped));
    }
}
