using HexLib;

namespace HexLib.Tests;

public class HexMathTests
{
    [Fact]
    public void ToOffset_ConvertsCorrectly()
    {
        // 0, 0
        var cube1 = new CubeCoordinate(0, 0, 0);
        var offset1 = cube1.ToOffset();
        Assert.Equal((0, 0), offset1);

        // 1, 1 (odd row shifts Q)
        var cube2 = new CubeCoordinate(1, 1, -2);
        var offset2 = cube2.ToOffset();
        Assert.Equal((1, 1), offset2);

        // 1, 2 (even row doesn't shift Q)
        var cube3 = new CubeCoordinate(0, 2, -2);
        var offset3 = cube3.ToOffset();
        Assert.Equal((1, 2), offset3);
    }

    [Fact]
    public void OffsetToCube_ConvertsCorrectly()
    {
        var offset1 = (0, 0);
        var cube1 = HexMath.OffsetToCube(offset1.Item1, offset1.Item2);
        Assert.Equal(new CubeCoordinate(0, 0, 0), cube1);

        var offset2 = (1, 1);
        var cube2 = HexMath.OffsetToCube(offset2.Item1, offset2.Item2);
        Assert.Equal(new CubeCoordinate(1, 1, -2), cube2);

        var offset3 = (1, 2);
        var cube3 = HexMath.OffsetToCube(offset3.Item1, offset3.Item2);
        Assert.Equal(new CubeCoordinate(0, 2, -2), cube3);
    }
    [Fact]
    public void ToOffset_FlatTopped_ConvertsCorrectly()
    {
        // 0, 0
        var cube1 = new CubeCoordinate(0, 0, 0);
        var offset1 = cube1.ToOffset(HexTopOrientation.FlatTopped);
        Assert.Equal((0, 0), offset1);

        // 1, 1 (odd col shifts R)
        var cube2 = new CubeCoordinate(1, 1, -2);
        var offset2 = cube2.ToOffset(HexTopOrientation.FlatTopped);
        Assert.Equal((1, 1), offset2);

        // 2, 1 (even col doesn't shift R)
        var cube3 = new CubeCoordinate(2, 0, -2);
        var offset3 = cube3.ToOffset(HexTopOrientation.FlatTopped);
        Assert.Equal((2, 1), offset3);
    }

    [Fact]
    public void OffsetToCube_FlatTopped_ConvertsCorrectly()
    {
        var offset1 = (0, 0);
        var cube1 = HexMath.OffsetToCube(offset1.Item1, offset1.Item2, HexTopOrientation.FlatTopped);
        Assert.Equal(new CubeCoordinate(0, 0, 0), cube1);

        var offset2 = (1, 1);
        var cube2 = HexMath.OffsetToCube(offset2.Item1, offset2.Item2, HexTopOrientation.FlatTopped);
        Assert.Equal(new CubeCoordinate(1, 1, -2), cube2);

        var offset3 = (2, 1);
        var cube3 = HexMath.OffsetToCube(offset3.Item1, offset3.Item2, HexTopOrientation.FlatTopped);
        Assert.Equal(new CubeCoordinate(2, 0, -2), cube3);
    }
}
