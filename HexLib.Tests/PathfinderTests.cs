using Xunit;
using System.Collections.Generic;
using HexLib;

namespace HexLib.Tests;

public class PathfinderTests
{
    [Fact]
    public void SimplePath_NoObstacles_IsMinDistance()
    {
        var board = new Board<string, string>(10, 10);
        board.InitializeGrid();
        
        var start = new CubeCoordinate(0, 0, 0);
        var goal = new CubeCoordinate(2, 0, -2); // Distance 2, within bounds
        
        var path = Pathfinder.FindPath(board, start, goal, (a, b) => 1.0f);
        
        Assert.NotNull(path);
        Assert.Equal(3, path.Count); // Start, Middle, Goal
        Assert.Equal(start, path[0]);
        Assert.Equal(goal, path[path.Count - 1]);
    }

    [Fact]
    public void Path_WithObstacle_GoesAround()
    {
        var board = new Board<string, string>(5, 5);
        board.InitializeGrid();
        
        // Start at 0,0, Goal at 2,0. Block 1,0.
        var start = new CubeCoordinate(0, 0, 0);
        var goal = new CubeCoordinate(2, 0, -2);
        var obstacle = new CubeCoordinate(1, 0, -1);
        
        var path = Pathfinder.FindPath(board, start, goal, (a, b) => 
            b == obstacle ? float.PositiveInfinity : 1.0f);
            
        Assert.NotNull(path);
        Assert.DoesNotContain(obstacle, path);
        Assert.True(path.Count > 3); // Must take a detour
    }

    [Fact]
    public void NoPath_ReturnsNull()
    {
        var board = new Board<string, string>(3, 3);
        board.InitializeGrid();
        
        var start = new CubeCoordinate(0, 0, 0);
        var goal = new CubeCoordinate(2, 2, -4); // Far away off-board
        
        var path = Pathfinder.FindPath(board, start, goal, (a, b) => 1.0f);
        
        Assert.Null(path);
    }
}
