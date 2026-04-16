using HexLib;
using Xunit;

namespace HexLib.Tests;

public class ComplexJoinTests
{
    private Board<object, object> CreateBoard(string name, int width, int height)
    {
        var board = new Board<object, object>(width, height) { Name = name };
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
    public void Join_MismatchedDimensions_ThrowsException()
    {
        var boardA = CreateBoard("A", 10, 10);
        boardA.HalfHexSides = BoardEdge.Right;
        
        var boardB = CreateBoard("B", 10, 5); // Height is 5, not 10
        boardB.HalfHexSides = BoardEdge.Left;

        Assert.Throws<InvalidOperationException>(() => boardA.Join(boardB, BoardEdge.Right));
    }

    [Fact]
    public void GetPhysicalNeighbor_AcrossRotatedJoin_FindsCorrectHex()
    {
        // 1x1 Boards for simplicity
        var boardA = CreateBoard("A", 1, 1);
        boardA.HalfHexSides = BoardEdge.Right;
        var hexA = boardA.GetHexAt(new CubeCoordinate(0, 0, 0));

        var boardB = CreateBoard("B", 1, 1);
        boardB.HalfHexSides = BoardEdge.Right; // physical Left in 180
        boardB.Orientation = BoardOrientation.Degree180;
        var hexB = boardB.GetHexAt(new CubeCoordinate(0, 0, 0));

        boardA.Join(boardB, BoardEdge.Right);

        // From Hex A, going East should find Hex B
        // On PointyTopped Odd-R: East is (1, 0, -1)
        var neighbor = boardA.GetPhysicalNeighbor(hexA!.Location, PhysicalDirection.East);

        Assert.NotNull(neighbor);
        Assert.Equal(hexB, neighbor);
    }
}
