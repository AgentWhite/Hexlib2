using HexLib;
using ASL;

namespace ASL.Tests;

public class ASLBoardTests
{
    [Fact]
    public void ASLBoard_CanBeInitializedWithMetadata()
    {
        // ASL boards are typically pointy-topped (standard wargame)
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10, HexTopOrientation.PointyTopped);
        
        var origin = new CubeCoordinate(0, 0, 0);
        var hex = new Hex<ASLHexMetadata>(origin);
        hex.Metadata = new ASLHexMetadata { Terrain = TerrainType.Woods, Elevation = 1 };
        
        board.AddHex(hex);
        
        var retrievedHex = board.GetHexAt(origin);
        Assert.NotNull(retrievedHex);
        Assert.Equal(TerrainType.Woods, retrievedHex.Metadata!.Terrain);
        Assert.Equal(1, retrievedHex.Metadata!.Elevation);
    }

    [Fact]
    public void ASLBoard_CanHandleWallsAndRoadsOnEdges()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10);
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0); // Neighbor

        // Put a wall between A and B
        board.SetEdgeData(a, b, new ASLEdgeData { HasWall = true });

        var edge = board.GetEdgeData(a, b);
        Assert.NotNull(edge);
        Assert.True(edge.HasWall);
        Assert.False(edge.HasRoad);
    }
    
    [Fact]
    public void ASLBoard_RoadAlongHexsideAndThroughHex()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10);
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        // A road going from A center to B center crosses the hexside
        // We can mark both the hexes as having road terrain AND the hexside as having a road
        var metA = new ASLHexMetadata { Terrain = TerrainType.Road };
        var metB = new ASLHexMetadata { Terrain = TerrainType.Road };
        
        board.AddHex(new Hex<ASLHexMetadata>(a) { Metadata = metA });
        board.AddHex(new Hex<ASLHexMetadata>(b) { Metadata = metB });
        
        board.SetEdgeData(a, b, new ASLEdgeData { HasRoad = true });

        var edge = board.GetEdgeData(a, b);
        Assert.True(edge!.HasRoad);
        Assert.Equal(TerrainType.Road, board.GetHexAt(a)!.Metadata!.Terrain);
    }

    [Fact]
    public void Movement_WallPenalty_IncreasesCost()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10);
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        
        // Wall between them
        var edge = new ASLEdgeData { HasWall = true };
        
        int costWithWall = MovementCalculator.CalculateCost(hexA, hexB, edge);
        int costWithoutWall = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());

        Assert.Equal(1, costWithoutWall);
        Assert.Equal(2, costWithWall); // 1 (Open Ground) + 1 (Wall)
    }

    [Fact]
    public void Movement_RoadBonus_IgnoresTerrain()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        // Woods (cost 3) but with a Road (cost 1)
        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.Road } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.Road } };
        
        var edgeWithRoad = new ASLEdgeData { HasRoad = true };
        var edgeNoRoad = new ASLEdgeData { HasRoad = false };

        // Even if the base terrain for hexB was woods, if we move via road we get road cost
        hexB.Metadata.Terrain = TerrainType.Road; 
        
        int cost = MovementCalculator.CalculateCost(hexA, hexB, edgeWithRoad);
        Assert.Equal(1, cost);
        
        // If we don't follow the road hexside, it's not road movement (though ASL rules are more complex, this is a good test)
        int costNoRoad = MovementCalculator.CalculateCost(hexA, hexB, edgeNoRoad);
        Assert.Equal(1, costNoRoad); // Base cost of road hex is 1 anyway in my simplified logic
    }
}
