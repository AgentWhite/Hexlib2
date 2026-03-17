using HexLib;

namespace HexLib.Tests;

public class MetadataAndEdgeTests
{
    public enum TerrainType { Grass, Road, Wall, Water }

    public class HexMetadata
    {
        public TerrainType Terrain { get; set; }
        public int MovementCost { get; set; }
    }

    public class EdgeData
    {
        public bool IsRoad { get; set; }
        public bool IsWall { get; set; }
    }

    [Fact]
    public void Hex_Metadata_CanBeSetAndRetrieved()
    {
        var hex = new Hex<HexMetadata>(new CubeCoordinate(0, 0, 0));
        hex.Metadata = new HexMetadata { Terrain = TerrainType.Road, MovementCost = 1 };

        Assert.NotNull(hex.Metadata);
        Assert.Equal(TerrainType.Road, hex.Metadata.Terrain);
        Assert.Equal(1, hex.Metadata.MovementCost);
    }

    [Fact]
    public void Board_EdgeData_CanBeSetAndRetrieved()
    {
        var board = new Board<HexMetadata, EdgeData>(10, 10);
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(1, -1, 0); // Neighbor

        board.SetEdgeData(a, b, new EdgeData { IsRoad = true });

        var data = board.GetEdgeData(a, b);
        Assert.NotNull(data);
        Assert.True(data.IsRoad);
        Assert.False(data.IsWall);

        // Verify commutative property
        var dataReverse = board.GetEdgeData(b, a);
        Assert.NotNull(dataReverse);
        Assert.True(dataReverse.IsRoad);
    }

    [Fact]
    public void Board_EdgeData_ReturnsDefaultIfNotFound()
    {
        var board = new Board<HexMetadata, EdgeData>(10, 10);
        var a = new CubeCoordinate(0, 0, 0);
        var b = new CubeCoordinate(0, 1, -1);

        var data = board.GetEdgeData(a, b);
        Assert.Null(data);
    }
}
