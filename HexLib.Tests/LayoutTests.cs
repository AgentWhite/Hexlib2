using Xunit;
using HexLib;

namespace HexLib.Tests;

public class LayoutTests
{
    [Fact]
    public void FlatTopped_HexToPixel_RoundTrip()
    {
        var layout = new Layout(Orientation.Flat, new Point2D(10, 10), new Point2D(0, 0));
        var h = new CubeCoordinate(12, -3, -9);
        
        var pixel = layout.HexToPixel(h);
        var hexFraction = layout.PixelToHex(pixel);
        var rounded = hexFraction.Round();
        
        Assert.Equal(h, rounded);
    }

    [Fact]
    public void PointyTopped_HexToPixel_RoundTrip()
    {
        var layout = new Layout(Orientation.Pointy, new Point2D(10, 10), new Point2D(0, 0));
        var h = new CubeCoordinate(12, -3, -9);
        
        var pixel = layout.HexToPixel(h);
        var hexFraction = layout.PixelToHex(pixel);
        var rounded = hexFraction.Round();
        
        Assert.Equal(h, rounded);
    }

    [Fact]
    public void HexCornerOffset_FlatTopped_Corner0_IsEast()
    {
        var layout = new Layout(Orientation.Flat, new Point2D(10, 10), new Point2D(0, 0));
        var offset = layout.HexCornerOffset(0);
        
        // 0 degrees for Flat is (Size, 0)
        Assert.Equal(10, offset.X, 5);
        Assert.Equal(0, offset.Y, 5);
    }
}
