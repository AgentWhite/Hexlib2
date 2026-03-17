using HexLib;

namespace HexLib.Tests;

public class HexOrientationTests
{
    [Fact]
    public void PointyTopped_HasSixSpecificDirections()
    {
        var board = new Board<object, object>(10, 10, HexTopOrientation.PointyTopped);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<object>(origin));

        // Pointy topped valid directions: NW, NE, E, SE, SW, W
        var directions = new[] { 
            PhysicalDirection.NorthWest, PhysicalDirection.NorthEast, PhysicalDirection.East, 
            PhysicalDirection.SouthEast, PhysicalDirection.SouthWest, PhysicalDirection.West 
        };

        foreach (var dir in directions)
        {
            var offset = board.GetPhysicalOffset_Internal(dir); // I might need to make this internal or just check coordinates
            var target = origin + offset;
            board.AddHex(new Hex<object>(target));
            Assert.NotNull(board.GetPhysicalNeighbor(origin, dir));
        }
        
        // Invalid for pointy
        Assert.Throws<ArgumentException>(() => board.GetPhysicalNeighbor(origin, PhysicalDirection.North));
        Assert.Throws<ArgumentException>(() => board.GetPhysicalNeighbor(origin, PhysicalDirection.South));
    }

    [Fact]
    public void FlatTopped_HasSixSpecificDirections()
    {
        var board = new Board<object, object>(10, 10, HexTopOrientation.FlatTopped);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<object>(origin));
        
        // Flat topped valid directions: N, NE, SE, S, SW, NW
        var directions = new[] { 
            PhysicalDirection.North, PhysicalDirection.NorthEast, PhysicalDirection.SouthEast, 
            PhysicalDirection.South, PhysicalDirection.SouthWest, PhysicalDirection.NorthWest 
        };

        foreach (var dir in directions)
        {
            var offset = board.GetPhysicalOffset_Internal(dir);
            var target = origin + offset;
            board.AddHex(new Hex<object>(target));
            Assert.NotNull(board.GetPhysicalNeighbor(origin, dir));
        }
        
        // Invalid for flat
        Assert.Throws<ArgumentException>(() => board.GetPhysicalNeighbor(origin, PhysicalDirection.East));
        Assert.Throws<ArgumentException>(() => board.GetPhysicalNeighbor(origin, PhysicalDirection.West));
    }
}
