using HexLib;

namespace HexLib.Tests;

public class BoardTests
{
    [Fact]
    public void Board_Initialization_SetsProperties()
    {
        var board = new Board(10, 15);
        Assert.Equal(10, board.Width);
        Assert.Equal(15, board.Height);
        Assert.Equal(BoardOrientation.Degree0, board.Orientation);
        Assert.Equal(BoardEdge.None, board.HalfHexSides);
        Assert.Empty(board.Hexes);
    }

    [Fact]
    public void AddHex_AddsHexToBoard()
    {
        var board = new Board(10, 10);
        var hex = new Hex(new CubeCoordinate(0, 0, 0));
        board.AddHex(hex);

        Assert.Single(board.Hexes);
        Assert.Equal(hex, board.GetHexAt(new CubeCoordinate(0, 0, 0)));
    }

    [Fact]
    public void AddHex_DuplicateLocation_ThrowsException()
    {
        var board = new Board(10, 10);
        var hex = new Hex(new CubeCoordinate(0, 0, 0));
        board.AddHex(hex);

        Assert.Throws<ArgumentException>(() => board.AddHex(new Hex(new CubeCoordinate(0, 0, 0))));
    }

    [Fact]
    public void SetHalfHexSides_ModifiesProperty()
    {
        var board = new Board(10, 10);
        board.HalfHexSides = BoardEdge.Top | BoardEdge.Bottom;
        
        Assert.True(board.HalfHexSides.HasFlag(BoardEdge.Top));
        Assert.True(board.HalfHexSides.HasFlag(BoardEdge.Bottom));
        Assert.False(board.HalfHexSides.HasFlag(BoardEdge.Left));
    }

    [Fact]
    public void Orientation_Degree90_SwapsDimensions()
    {
        var board = new Board(10, 20);
        board.Orientation = BoardOrientation.Degree90;
        Assert.Equal(20, board.Width);
        Assert.Equal(10, board.Height);
    }
    
    [Fact]
    public void Orientation_Degree90_RotatesHalfHexSides()
    {
        var board = new Board(10, 10);
        board.HalfHexSides = BoardEdge.Top | BoardEdge.Right;
        board.Orientation = BoardOrientation.Degree90;
        
        Assert.True(board.HalfHexSides.HasFlag(BoardEdge.Right));
        Assert.True(board.HalfHexSides.HasFlag(BoardEdge.Bottom));
        Assert.False(board.HalfHexSides.HasFlag(BoardEdge.Top));
        Assert.False(board.HalfHexSides.HasFlag(BoardEdge.Left));
    }
}
