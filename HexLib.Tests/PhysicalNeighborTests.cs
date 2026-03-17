using HexLib;

namespace HexLib.Tests;

public class PhysicalNeighborTests
{
    private Board CreateBoard(string name, int width, int height)
    {
        var board = new Board(width, height) { Name = name };
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var hex = new Hex(HexMath.OffsetToCube(x, y)) { Id = $"{name}_{x}_{y}" };
                board.AddHex(hex);
            }
        }
        return board;
    }

    [Fact]
    public void GetPhysicalNeighbor_SameBoard_ReturnsCorrectHex()
    {
        var board = CreateBoard("A", 10, 10);
        
        // From 1,1 going East (which is +1 Q, -1 S mapped roughly to +1 Col)
        // Offset 1,1 -> East is 2,1 in odd-r
        var startHex = board.GetHexAt(HexMath.OffsetToCube(1, 1));
        Assert.NotNull(startHex);

        var neighbor = board.GetPhysicalNeighbor(startHex.Location, PhysicalDirection.East);
        
        Assert.NotNull(neighbor);
        Assert.Equal("A_2_1", neighbor.Id);
    }

    [Fact]
    public void GetPhysicalNeighbor_CrossesRightEdge_ReturnsNeighborHex()
    {
        var boardA = CreateBoard("A", 5, 5);
        boardA.HalfHexSides = BoardEdge.Right;

        var boardB = CreateBoard("B", 5, 5);
        boardB.HalfHexSides = BoardEdge.Left;

        boardA.Join(boardB, BoardEdge.Right);

        // Stand on the rightmost edge of A (col 4, row 2)
        var startHex = boardA.GetHexAt(HexMath.OffsetToCube(4, 2));
        Assert.NotNull(startHex);

        // Go East off the edge of A
        var neighbor = boardA.GetPhysicalNeighbor(startHex.Location, PhysicalDirection.East);

        // It should land on Board B, at its leftmost edge (col 0, row 2)
        Assert.NotNull(neighbor);
        Assert.Equal("B_0_2", neighbor.Id);
    }
    
    [Fact]
    public void GetPhysicalNeighbor_CrossesBottomEdge_ReturnsNeighborHex()
    {
        var boardA = CreateBoard("A", 5, 5);
        boardA.HalfHexSides = BoardEdge.Bottom;

        var boardB = CreateBoard("B", 5, 5);
        boardB.HalfHexSides = BoardEdge.Top;

        boardA.Join(boardB, BoardEdge.Bottom);

        // Stand on the bottom row of A (col 2, row 4)
        var startHex = boardA.GetHexAt(HexMath.OffsetToCube(2, 4));
        Assert.NotNull(startHex);

        // Go SouthEast off the bottom edge of A
        var neighbor = boardA.GetPhysicalNeighbor(startHex.Location, PhysicalDirection.SouthEast);

        // By offset math, looking Southeast from an even column (col 2) means Col doesn't change, Row+1.
        // It lands on Board B at (col 2, row 0)
        Assert.NotNull(neighbor);
        Assert.Equal("B_2_0", neighbor.Id);
    }
}
