using HexLib;

namespace HexLib.Tests;

public class CubeCoordinateTests
{
    [Fact]
    public void Constructor_ValidCoordinates_CreatesInstance()
    {
        var coord = new CubeCoordinate(1, -1, 0);
        Assert.Equal(1, coord.Q);
        Assert.Equal(-1, coord.R);
        Assert.Equal(0, coord.S);
    }

    [Fact]
    public void Constructor_InvalidCoordinates_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new CubeCoordinate(1, 1, 1));
    }

    [Fact]
    public void DistanceTo_CalculatesCorrectDistance()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(2, -1, -1);
        Assert.Equal(2, a.DistanceTo(b));
    }

    [Fact]
    public void Addition_ProducesCorrectCoordinate()
    {
        var a = new CubeCoordinate(1, -1, 0);
        var b = new CubeCoordinate(1, 0, -1);
        var result = a + b;
        Assert.Equal(new CubeCoordinate(2, -1, -1), result);
    }

    [Fact]
    public void Subtraction_ProducesCorrectCoordinate()
    {
        var a = new CubeCoordinate(2, -1, -1);
        var b = new CubeCoordinate(1, -1, 0);
        var result = a - b;
        Assert.Equal(new CubeCoordinate(1, 0, -1), result);
    }

    [Fact]
    public void Multiplication_ProducesCorrectCoordinate()
    {
        var a = new CubeCoordinate(1, -1, 0);
        var result = a * 2;
        Assert.Equal(new CubeCoordinate(2, -2, 0), result);
    }

    [Fact]
    public void GetNeighbor_ReturnsCorrectNeighbor()
    {
        var start = new CubeCoordinate(0, 0, 0);
        var neighbor0 = start.GetNeighbor(0);
        Assert.Equal(new CubeCoordinate(1, 0, -1), neighbor0);
    }
}
