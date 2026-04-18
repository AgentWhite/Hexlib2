using Xunit;
using HexLib;
using System.Collections.Generic;
using System.Linq;

namespace HexLib.Tests;

public class LosTests
{
    [Fact]
    public void GetLine_StraightHorizontal_ReturnsPath()
    {
        var start = new CubeCoordinate(0, 0, 0);
        var end = new CubeCoordinate(5, -5, 0); // Distance 5
        
        var path = HexMath.GetLine(start, end);
        
        Assert.Equal(6, path.Count); // Start + 5 hexes
        Assert.Contains(start, path);
        Assert.Contains(end, path);
        
        // Check intermediate (1, -1, 0)
        Assert.Contains(new CubeCoordinate(1, -1, 0), path);
    }

    [Fact]
    public void GetLine_SingleHex_ReturnsStart()
    {
        var start = new CubeCoordinate(2, 2, -4);
        var path = HexMath.GetLine(start, start);
        
        Assert.Single(path);
        Assert.Equal(start, path[0]);
    }

    [Fact]
    public void GetLine_Diagonal_ReturnsAllTraversed()
    {
        // Diagonal line that might cross edges
        var start = new CubeCoordinate(0, 0, 0);
        var end = new CubeCoordinate(2, 1, -3);
        
        var path = HexMath.GetLine(start, end);
        
        Assert.NotEmpty(path);
        Assert.Contains(start, path);
        Assert.Contains(end, path);
    }
}
