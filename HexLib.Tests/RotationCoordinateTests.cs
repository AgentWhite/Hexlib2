using HexLib;
using Xunit;

namespace HexLib.Tests;

public class RotationCoordinateTests
{
    private Board<object, object> CreateBoard(string name, int width, int height, BoardOrientation orientation = BoardOrientation.Degree0)
    {
        var board = new Board<object, object>(width, height) { Name = name, Orientation = orientation };
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var hex = new Hex<object>(HexMath.OffsetToCube(x, y)) { Id = $"{name}_{x}_{y}" };
                board.AddHex(hex);
            }
        }
        return board;
    }

    [Fact]
    public void ToGlobalCoordinate_WithDegree180Rotation_MapsCorrectly()
    {
        var manager = new BoardManager<object, object>();
        var anchor = CreateBoard("Anchor", 10, 10);
        
        // Board B is rotated 180 degrees and joined to the RIGHT of Anchor
        // Touching physical Left of B. In 180 deg, logical Right -> physical Left.
        var boardB = CreateBoard("B", 10, 10, BoardOrientation.Degree180);
        anchor.HalfHexSides = BoardEdge.Right;
        boardB.HalfHexSides = BoardEdge.Right; // logical Right is physical Left

        manager.SetAnchorBoard(anchor);
        manager.JoinBoard(boardB, anchor, BoardEdge.Right);

        // Logical (0,0) on rotated Board B:
        // Physical: In Degree180, logical (q, r, s) -> physical (-q, -r, -s).
        // Logical (0,0) is physical (0,0), which remains (0,0) in offset coordinates.
        // So Board B's logical (0,0) should be at global offset (10, 0).
        var hexB = boardB.GetHexAt(HexMath.OffsetToCube(0, 0));
        var globalCoord = manager.ToGlobalCoordinate(hexB!);

        // Anchor is at (0,0). B is at (10,0).
        // Global expected for hex at B(0,0) is Offset(10, 0) -> Cube(10, 0, -10)
        Assert.Equal(HexMath.OffsetToCube(10, 0), globalCoord);

        // Logical (9,0) on rotated Board B:
        // Physical: (9,0) offset -> Cube(9, 0, -9). Rotate180 -> Cube(-9, 0, 9).
        // Cube (-9, 0, 9) -> Offset (-9, 0) if we were at origin.
        // But with Board B global offset (10, 0): Result is (-9+10, 0+0) = (1, 0).
        // So logical (9,0) on a 180-rotated board joined to the right should effectively be back near the Anchor.
        var hexB_Edge = boardB.GetHexAt(HexMath.OffsetToCube(9, 0));
        var globalEdgeCoord = manager.ToGlobalCoordinate(hexB_Edge!);
        Assert.Equal(HexMath.OffsetToCube(1, 0), globalEdgeCoord);
    }

    [Fact]
    public void ToGlobalCoordinate_WithDegree90Rotation_MapsCorrectly()
    {
        var manager = new BoardManager<object, object>();
        var anchor = CreateBoard("Anchor", 10, 10, HexLib.BoardOrientation.Degree0);
        
        // Board B is rotated 90 degrees (logical up is physical right)
        // Note: HexLib Degree90 rotation maps 90 deg square rotation to -60 hex rotation
        var boardB = CreateBoard("B", 10, 33, BoardOrientation.Degree90);
        anchor.HalfHexSides = BoardEdge.Right;
        // Touching physical Left of B. In 90 deg, logical Top -> physical Right, 
        // logical Right -> physical Bottom, logical Bottom -> physical Left.
        boardB.HalfHexSides = BoardEdge.Bottom; 

        manager.SetAnchorBoard(anchor);
        manager.JoinBoard(boardB, anchor, BoardEdge.Right);

        // Board B logical (0,0) 
        var hexB = boardB.GetHexAt(new CubeCoordinate(0, 0, 0));
        var globalCoord = manager.ToGlobalCoordinate(hexB!);

        // Anchor Width = 10. Board B Offset = (10, 0).
        // Logical (0,0) rotated 90 (Rotate60) is (0,0). Offset (0,0).
        // Global = (10+0, 0+0) = (10, 0).
        Assert.Equal(HexMath.OffsetToCube(10, 0), globalCoord);
    }
}
