using HexLib;

namespace HexLib.Tests;

public class BoardManagerTests
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
    public void SetAnchorBoard_WithUnjoinedBoard_AddsCorrectly()
    {
        var manager = new BoardManager<object, object>();
        var board = CreateBoard("A", 10, 10);

        manager.SetAnchorBoard(board);

        Assert.Equal(board, manager.AnchorBoard);
        Assert.Single(manager.Boards);
    }

    [Fact]
    public void AddBoard_ThrowsIfAlreadyAdded()
    {
        var manager = new BoardManager<object, object>();
        var board = CreateBoard("A", 10, 10);

        manager.SetAnchorBoard(board);

        Assert.Throws<ArgumentException>(() => manager.AddBoard(board, 10, 0));
    }

    [Fact]
    public void BoardManager_LockedRotation_ThrowsIfManagedBoardRotated()
    {
        var manager = new BoardManager<object, object>();
        var board = CreateBoard("A", 10, 10);
        manager.SetAnchorBoard(board);

        Assert.Throws<InvalidOperationException>(() => board.Orientation = BoardOrientation.Degree90);
    }
    
    [Fact]
    public void RemoveAnchor_ShiftsMapCorrectly()
    {
        var manager = new BoardManager<object, object>();
        var anchor = CreateBoard("Anchor", 10, 10);
        anchor.HalfHexSides = BoardEdge.Right;

        var boardB = CreateBoard("B", 10, 10);
        boardB.HalfHexSides = BoardEdge.Left;

        // B is 10 hexes to the right of Anchor
        manager.SetAnchorBoard(anchor);
        manager.JoinBoard(boardB, anchor, BoardEdge.Right);

        // Before removing anchor, B's top-left hex (0,0) is at global offset (10,0)
        var hexB = boardB.GetHexAt(HexMath.OffsetToCube(0, 0));
        var globalBefore = manager.ToGlobalCoordinate(hexB!);
        Assert.Equal(HexMath.OffsetToCube(10, 0), globalBefore);

        // Remove the anchor
        manager.RemoveBoard("Anchor");

        // B is now the new Anchor. It should be shifted to (0,0)
        Assert.Equal(boardB, manager.AnchorBoard);
        
        var globalAfter = manager.ToGlobalCoordinate(hexB!);
        Assert.Equal(HexMath.OffsetToCube(0, 0), globalAfter);
    }

    [Fact]
    public void GetDistance_AcrossJoinedBoards_ReturnsCorrectMath()
    {
        var manager = new BoardManager<object, object>();
        var boardA = CreateBoard("A", 10, 10);
        boardA.HalfHexSides = BoardEdge.Right;

        var boardB = CreateBoard("B", 10, 10);
        boardB.HalfHexSides = BoardEdge.Left;

        manager.SetAnchorBoard(boardA);
        manager.JoinBoard(boardB, boardA, BoardEdge.Right);

        // Hex A at offset (9, 0)
        var hexA = boardA.GetHexAt(HexMath.OffsetToCube(9, 0));
        
        // Hex B at offset (0, 0) (technically joined, so physical distance is very small)
        var hexB = boardB.GetHexAt(HexMath.OffsetToCube(0, 0));

        // Global A offset should be (9, 0). Global B offset should be (10, 0).
        // The distance between (9,0) and (10,0) in odd-r Pointy Top is exactly 1.
        int distance = manager.GetDistance(hexA!, hexB!);

        Assert.Equal(1, distance);
    }
    
    [Fact]
    public void DoubleJoin_ThrowsException()
    {
        var boardA = CreateBoard("A", 10, 10);
        boardA.HalfHexSides = BoardEdge.Right | BoardEdge.Bottom;
        
        var boardB = CreateBoard("B", 10, 10);
        boardB.HalfHexSides = BoardEdge.Left | BoardEdge.Top;

        boardA.Join(boardB, BoardEdge.Right);
        
        // Attempting to join the exact same board B onto the bottom of A
        Assert.Throws<InvalidOperationException>(() => boardA.Join(boardB, BoardEdge.Bottom));
    }
}
