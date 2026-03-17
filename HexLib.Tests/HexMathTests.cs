using HexLib;

namespace HexLib.Tests;

public class HexMathTests
{
    [Fact]
    public void CubeToOffset_ConvertsCorrectly()
    {
        // 0, 0
        var cube1 = new CubeCoordinate(0, 0, 0);
        var offset1 = HexMath.CubeToOffset(cube1);
        Assert.Equal((0, 0), offset1);

        // 1, 1 (odd row shifts Q)
        var cube2 = new CubeCoordinate(1, 1, -2);
        var offset2 = HexMath.CubeToOffset(cube2);
        Assert.Equal((1, 1), offset2);

        // 1, 2 (even row doesn't shift Q)
        var cube3 = new CubeCoordinate(0, 2, -2);
        var offset3 = HexMath.CubeToOffset(cube3);
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
}
