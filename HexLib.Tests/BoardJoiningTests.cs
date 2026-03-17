using HexLib;

namespace HexLib.Tests;

public class BoardJoiningTests
{
    [Fact]
    public void Join_ValidRightJoin_LinksBoardsMutually()
    {
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.Right };
        var boardB = new Board<object, object>(36, 16) { Name = "B", HalfHexSides = BoardEdge.Left };

        boardA.Join(boardB, BoardEdge.Right);

        Assert.Equal(boardB, boardA.Neighbors[BoardEdge.Right]);
        Assert.Equal(boardA, boardB.Neighbors[BoardEdge.Left]);
    }

    [Fact]
    public void Join_ValidBottomJoin_LinksBoardsMutually()
    {
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.Bottom };
        var boardB = new Board<object, object>(36, 16) { Name = "B", HalfHexSides = BoardEdge.Top };

        boardA.Join(boardB, BoardEdge.Bottom);

        Assert.Equal(boardB, boardA.Neighbors[BoardEdge.Bottom]);
        Assert.Equal(boardA, boardB.Neighbors[BoardEdge.Top]);
    }

    [Fact]
    public void CanJoin_MismatchingDimensions_ReturnsFalse()
    {
        // Joining Right requires same Height. Width can be different.
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.Right };
        var boardB = new Board<object, object>(36, 15) { Name = "B", HalfHexSides = BoardEdge.Left }; // Height mismatch

        Assert.False(boardA.CanJoin(boardB, BoardEdge.Right));
    }

    [Fact]
    public void CanJoin_MissingHalfHexEdges_ReturnsFalse()
    {
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.None }; // Missing Right
        var boardB = new Board<object, object>(36, 16) { Name = "B", HalfHexSides = BoardEdge.Left };

        Assert.False(boardA.CanJoin(boardB, BoardEdge.Right));
    }

    [Fact]
    public void Join_AlreadyJoinedEdge_ThrowsException()
    {
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.Right };
        var boardB = new Board<object, object>(36, 16) { Name = "B", HalfHexSides = BoardEdge.Left };
        var boardC = new Board<object, object>(36, 16) { Name = "C", HalfHexSides = BoardEdge.Left };

        boardA.Join(boardB, BoardEdge.Right);

        Assert.Throws<InvalidOperationException>(() => boardA.Join(boardC, BoardEdge.Right));
    }

    [Fact]
    public void Unlink_RemovesMutualLinks()
    {
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.Right };
        var boardB = new Board<object, object>(36, 16) { Name = "B", HalfHexSides = BoardEdge.Left };
        boardA.Join(boardB, BoardEdge.Right);

        boardA.Unlink(BoardEdge.Right);

        Assert.False(boardA.Neighbors.ContainsKey(BoardEdge.Right));
        Assert.False(boardB.Neighbors.ContainsKey(BoardEdge.Left));
    }

    [Fact]
    public void Rotate_WhileJoined_ThrowsException()
    {
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.Right };
        var boardB = new Board<object, object>(36, 16) { Name = "B", HalfHexSides = BoardEdge.Left };
        boardA.Join(boardB, BoardEdge.Right);

        Assert.Throws<InvalidOperationException>(() => boardA.Orientation = BoardOrientation.Degree90);
    }
    
    [Fact]
    public void Rotate_AfterUnlink_Succeeds()
    {
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.Right };
        var boardB = new Board<object, object>(36, 16) { Name = "B", HalfHexSides = BoardEdge.Left };
        boardA.Join(boardB, BoardEdge.Right);
        boardA.Unlink(BoardEdge.Right);

        // Should not throw
        boardA.Orientation = BoardOrientation.Degree90;
        Assert.Equal(BoardOrientation.Degree90, boardA.Orientation);
    }

    [Fact]
    public void CanJoin_RotatedBoard_MismatchedDimensions_ReturnsFalse()
    {
        var boardA = new Board<object, object>(32, 16) { Name = "A", HalfHexSides = BoardEdge.Right };
        var boardB = new Board<object, object>(32, 16) { Name = "B", HalfHexSides = BoardEdge.Left };
        
        // boardB is rotated, making it effectively 16x32, with half hex on Top
        boardB.Orientation = BoardOrientation.Degree90; 

        // Expected fail because A wants to join B on Right.
        // A wants to join on A's Right (height 16). So A checks B's Left.
        // B's effective dimensions: W=16, H=32. Left side has height 32.
        // Dimensions don't match (16 != 32).
        Assert.False(boardA.CanJoin(boardB, BoardEdge.Right));
    }

    [Fact]
    public void CanJoin_RotatedBoard_MatchingDimensions_ReturnsTrue()
    {
        var boardA = new Board<object, object>(16, 32) { Name = "A", HalfHexSides = BoardEdge.Right };
        // boardB is 32x16. Rotated 90 deg, it becomes 16x32.
        // We want it to join A on A's Right. That means B needs a Left half-hex effectively.
        // Before 90 deg rotation, Left was Bottom. So physical Bottom.
        var boardB = new Board<object, object>(32, 16) { Name = "B", HalfHexSides = BoardEdge.Bottom };
        boardB.Orientation = BoardOrientation.Degree90; 
        
        Assert.True(boardA.CanJoin(boardB, BoardEdge.Right));
    }
    
    [Fact]
    public void Join_SetsOppositeNeighbors_Correctly()
    {
        var boardA = new Board<object, object>(36, 16) { Name = "A", HalfHexSides = BoardEdge.Top };
        var boardB = new Board<object, object>(36, 16) { Name = "B", HalfHexSides = BoardEdge.Bottom };
        boardA.Join(boardB, BoardEdge.Top, (h1, h2) => h1); // Arbitrary delegate for now

        Assert.Equal(boardB, boardA.Neighbors[BoardEdge.Top]);
        Assert.Equal(boardA, boardB.Neighbors[BoardEdge.Bottom]);
    }
}
