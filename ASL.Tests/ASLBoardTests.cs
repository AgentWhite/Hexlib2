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
        Assert.False(edge.HasPavedRoad);
    }
    
    [Fact]
    public void ASLBoard_RoadAlongHexside()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10);
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);
 
        // A road connects two hexes via the hexside
        board.AddHex(new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } });
        board.AddHex(new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } });
        
        board.SetEdgeData(a, b, new ASLEdgeData { HasPavedRoad = true });
 
        var edge = board.GetEdgeData(a, b);
        Assert.True(edge!.HasPavedRoad);
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
        
        double costWithWall = MovementCalculator.CalculateCost(hexA, hexB, edge);
        double costWithoutWall = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());
 
        Assert.Equal(1.0, costWithoutWall);
        Assert.Equal(2.0, costWithWall); // 1.0 (Open Ground) + 1 (Wall)
    }
 
    [Fact]
    public void Movement_RoadBonus_IgnoresTerrain()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);
 
        // Woods (cost 3) but with a Paved Road (cost 0.5)
        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.Woods } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.Woods } };
        
        var edgeWithPaved = new ASLEdgeData { HasPavedRoad = true };
        var edgeWithDirt = new ASLEdgeData { HasDirtRoad = true };
        
        double costPaved = MovementCalculator.CalculateCost(hexA, hexB, edgeWithPaved);
        Assert.Equal(0.5, costPaved);

        double costDirt = MovementCalculator.CalculateCost(hexA, hexB, edgeWithDirt);
        Assert.Equal(1.0, costDirt);
        
        // If we don't follow the road hexside, it's woods movement
        double costNoRoad = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());
        Assert.Equal(3.0, costNoRoad); 
    }

    [Fact]
    public void Movement_Crag_CostsTwo()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.Crag } };
        
        double cost = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());
        Assert.Equal(2.0, cost);
    }

    [Fact]
    public void Movement_Shellholes_CostsTwoOnOpenGround()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround, HasShellholes = true } };
        
        double cost = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());
        Assert.Equal(2.0, cost);
    }

    [Fact]
    public void Movement_Pond_IsImpassable()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.Pond } };
        
        double cost = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());
        Assert.Equal(99.0, cost);
    }

    [Fact]
    public void Movement_Lumberyard_CostsTwo()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.Lumberyard } };
        
        double cost = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());
        Assert.Equal(2.0, cost);
    }

    [Fact]
    public void Movement_StoneRubble_CostsThree()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround, Rubble = RubbleType.Stone } };
        
        double cost = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());
        Assert.Equal(3.0, cost);
    }

    [Fact]
    public void Movement_WoodenRubble_CostsThree()
    {
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0);

        var hexA = new Hex<ASLHexMetadata>(a) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        var hexB = new Hex<ASLHexMetadata>(b) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround, Rubble = RubbleType.Wooden } };
        
        double cost = MovementCalculator.CalculateCost(hexA, hexB, new ASLEdgeData());
        Assert.Equal(3.0, cost);
    }
}
