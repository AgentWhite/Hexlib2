using HexLib;

namespace HexLib.Tests;

public class HexTests
{
    private class TestCounter : ICounter
    {
        public string Name { get; }
        public TestCounter(string name) => Name = name;
    }

    [Fact]
    public void Hex_AddingCounter_IncreasesCount()
    {
        var hex = new Hex<object>(new CubeCoordinate(0, 0, 0));
        var counter = new TestCounter("Infantry");
        hex.AddCounter(counter);
        Assert.Single(hex.Counters);
        Assert.Contains(counter, hex.Counters);
    }

    [Fact]
    public void Hex_RemovingCounter_DecreasesCount()
    {
        var hex = new Hex<object>(new CubeCoordinate(0, 0, 0));
        var counter = new TestCounter("Tank");
        hex.AddCounter(counter);
        
        var removed = hex.RemoveCounter(counter);
        Assert.True(removed);
        Assert.Empty(hex.Counters);
    }
}
