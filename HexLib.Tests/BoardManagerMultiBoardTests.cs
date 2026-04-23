namespace HexLib.Tests;

public class BoardManagerMultiBoardTests
{
    private static Board<object, object> MakeBoard(string name, int width = 10, int height = 10)
    {
        var board = new Board<object, object>(width, height) { Name = name };
        for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            board.AddHex(new Hex<object>(HexMath.OffsetToCube(x, y)) { Id = $"{name}_{x}_{y}" });
        return board;
    }

    // ── Registration ──────────────────────────────────────────────────────────

    [Fact]
    public void SetAnchorBoard_WithPreJoinedChain_RegistersAllBoards()
    {
        var a = MakeBoard("A");
        a.HalfHexSides = BoardEdge.Right;
        var b = MakeBoard("B");
        b.HalfHexSides = BoardEdge.Left | BoardEdge.Right;
        var c = MakeBoard("C");
        c.HalfHexSides = BoardEdge.Left;

        a.Join(b, BoardEdge.Right);
        b.Join(c, BoardEdge.Right);

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);

        Assert.Equal(3, manager.Boards.Count);
        Assert.Contains(manager.Boards, board => board.Name == "A");
        Assert.Contains(manager.Boards, board => board.Name == "B");
        Assert.Contains(manager.Boards, board => board.Name == "C");
    }

    [Fact]
    public void JoinBoard_ThirdBoard_RegistersAllThree()
    {
        var a = MakeBoard("A");
        a.HalfHexSides = BoardEdge.Right;
        var b = MakeBoard("B");
        b.HalfHexSides = BoardEdge.Left | BoardEdge.Right;
        var c = MakeBoard("C");
        c.HalfHexSides = BoardEdge.Left;

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);
        manager.JoinBoard(b, a, BoardEdge.Right);
        manager.JoinBoard(c, b, BoardEdge.Right);

        Assert.Equal(3, manager.Boards.Count);
    }

    [Fact]
    public void JoinBoard_ThrowsIfTargetNotManaged()
    {
        var a = MakeBoard("A");
        var b = MakeBoard("B");
        var unrelated = MakeBoard("Unrelated");

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);

        Assert.Throws<InvalidOperationException>(() => manager.JoinBoard(b, unrelated, BoardEdge.Right));
    }

    [Fact]
    public void SetAnchorBoard_Twice_Throws()
    {
        var a = MakeBoard("A");
        var b = MakeBoard("B");
        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);

        Assert.Throws<InvalidOperationException>(() => manager.SetAnchorBoard(b));
    }

    // ── Global offsets (linear chain A-B-C) ──────────────────────────────────

    [Fact]
    public void ThreeBoard_LinearChain_GlobalOffsetsAreAdditive()
    {
        var a = MakeBoard("A", 10, 10);
        a.HalfHexSides = BoardEdge.Right;
        var b = MakeBoard("B", 10, 10);
        b.HalfHexSides = BoardEdge.Left | BoardEdge.Right;
        var c = MakeBoard("C", 10, 10);
        c.HalfHexSides = BoardEdge.Left;

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);
        manager.JoinBoard(b, a, BoardEdge.Right);
        manager.JoinBoard(c, b, BoardEdge.Right);

        // A origin hex is at global (0,0)
        var hexA = a.GetHexAt(HexMath.OffsetToCube(0, 0));
        Assert.Equal(HexMath.OffsetToCube(0, 0), manager.ToGlobalCoordinate(hexA!));

        // B origin hex is at global (10,0)
        var hexB = b.GetHexAt(HexMath.OffsetToCube(0, 0));
        Assert.Equal(HexMath.OffsetToCube(10, 0), manager.ToGlobalCoordinate(hexB!));

        // C origin hex is at global (20,0)
        var hexC = c.GetHexAt(HexMath.OffsetToCube(0, 0));
        Assert.Equal(HexMath.OffsetToCube(20, 0), manager.ToGlobalCoordinate(hexC!));
    }

    [Fact]
    public void ThreeBoard_VerticalChain_GlobalOffsetsAreAdditive()
    {
        var a = MakeBoard("A", 10, 10);
        a.HalfHexSides = BoardEdge.Bottom;
        var b = MakeBoard("B", 10, 10);
        b.HalfHexSides = BoardEdge.Top | BoardEdge.Bottom;
        var c = MakeBoard("C", 10, 10);
        c.HalfHexSides = BoardEdge.Top;

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);
        manager.JoinBoard(b, a, BoardEdge.Bottom);
        manager.JoinBoard(c, b, BoardEdge.Bottom);

        var hexC = c.GetHexAt(HexMath.OffsetToCube(0, 0));
        Assert.Equal(HexMath.OffsetToCube(0, 20), manager.ToGlobalCoordinate(hexC!));
    }

    // ── GetDistance across 3 boards ───────────────────────────────────────────

    [Fact]
    public void GetDistance_AcrossThreeBoards_IsCorrect()
    {
        var a = MakeBoard("A", 10, 10);
        a.HalfHexSides = BoardEdge.Right;
        var b = MakeBoard("B", 10, 10);
        b.HalfHexSides = BoardEdge.Left | BoardEdge.Right;
        var c = MakeBoard("C", 10, 10);
        c.HalfHexSides = BoardEdge.Left;

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);
        manager.JoinBoard(b, a, BoardEdge.Right);
        manager.JoinBoard(c, b, BoardEdge.Right);

        // A(0,0) is at global (0,0). C(0,0) is at global (20,0).
        var hexA = a.GetHexAt(HexMath.OffsetToCube(0, 0))!;
        var hexC = c.GetHexAt(HexMath.OffsetToCube(0, 0))!;

        Assert.Equal(20, manager.GetDistance(hexA, hexC));
    }

    // ── RemoveBoard (middle of chain) ─────────────────────────────────────────

    [Fact]
    public void RemoveMiddleBoard_DisconnectsFromChain_OthersRemainRegistered()
    {
        var a = MakeBoard("A", 10, 10);
        a.HalfHexSides = BoardEdge.Right;
        var b = MakeBoard("B", 10, 10);
        b.HalfHexSides = BoardEdge.Left | BoardEdge.Right;
        var c = MakeBoard("C", 10, 10);
        c.HalfHexSides = BoardEdge.Left;

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);
        manager.JoinBoard(b, a, BoardEdge.Right);
        manager.JoinBoard(c, b, BoardEdge.Right);

        manager.RemoveBoard("B");

        Assert.Equal(2, manager.Boards.Count);
        Assert.DoesNotContain(manager.Boards, board => board.Name == "B");
        Assert.Contains(manager.Boards, board => board.Name == "A");
        Assert.Contains(manager.Boards, board => board.Name == "C");
    }

    [Fact]
    public void RemoveMiddleBoard_BreaksNeighborLinks()
    {
        var a = MakeBoard("A", 10, 10);
        a.HalfHexSides = BoardEdge.Right;
        var b = MakeBoard("B", 10, 10);
        b.HalfHexSides = BoardEdge.Left | BoardEdge.Right;
        var c = MakeBoard("C", 10, 10);
        c.HalfHexSides = BoardEdge.Left;

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);
        manager.JoinBoard(b, a, BoardEdge.Right);
        manager.JoinBoard(c, b, BoardEdge.Right);

        manager.RemoveBoard("B");

        Assert.False(a.Neighbors.ContainsKey(BoardEdge.Right));
        Assert.False(c.Neighbors.ContainsKey(BoardEdge.Left));
    }

    [Fact]
    public void RemoveBoard_NonExistentName_DoesNotThrow()
    {
        var a = MakeBoard("A");
        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);

        var ex = Record.Exception(() => manager.RemoveBoard("Ghost"));
        Assert.Null(ex);
    }

    // ── T-shaped / L-shaped arrangements ──────────────────────────────────────

    [Fact]
    public void TShaped_ThreeBoards_AllRegisteredWithCorrectOffsets()
    {
        // A is anchor. B is to the right of A. C is below A.
        var a = MakeBoard("A", 10, 10);
        a.HalfHexSides = BoardEdge.Right | BoardEdge.Bottom;
        var b = MakeBoard("B", 10, 10);
        b.HalfHexSides = BoardEdge.Left;
        var c = MakeBoard("C", 10, 10);
        c.HalfHexSides = BoardEdge.Top;

        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);
        manager.JoinBoard(b, a, BoardEdge.Right);
        manager.JoinBoard(c, a, BoardEdge.Bottom);

        Assert.Equal(3, manager.Boards.Count);

        var hexB = b.GetHexAt(HexMath.OffsetToCube(0, 0))!;
        var hexC = c.GetHexAt(HexMath.OffsetToCube(0, 0))!;

        Assert.Equal(HexMath.OffsetToCube(10, 0), manager.ToGlobalCoordinate(hexB));
        Assert.Equal(HexMath.OffsetToCube(0, 10), manager.ToGlobalCoordinate(hexC));
    }

    // ── RemoveBoard last board ─────────────────────────────────────────────────

    [Fact]
    public void RemoveOnlyBoard_ClearsAnchor()
    {
        var a = MakeBoard("A");
        var manager = new BoardManager<object, object>();
        manager.SetAnchorBoard(a);

        manager.RemoveBoard("A");

        Assert.Null(manager.AnchorBoard);
        Assert.Empty(manager.Boards);
    }
}
