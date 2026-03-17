using HexLib;
using System.Linq;

namespace HexLib.Tests;

public class HexAliasTests
{
    private class TestCounter : ICounter
    {
        public string Name { get; }
        public TestCounter(string name) => Name = name;
    }

    [Fact]
    public void Hex_WithPrimaryAlias_SharesCounters()
    {
        var primary = new Hex<object>(new CubeCoordinate(0, 0, 0)) { Id = "Primary" };
        var alias = new Hex<object>(new CubeCoordinate(1, -1, 0)) { Id = "Alias" };
        
        alias.PrimaryHexAlias = primary;

        var counter1 = new TestCounter("Counter 1");
        primary.AddCounter(counter1);

        var counter2 = new TestCounter("Counter 2");
        alias.AddCounter(counter2); // Should route to primary

        // Both hexes should report 2 counters
        Assert.Equal(2, primary.Counters.Count);
        Assert.Equal(2, alias.Counters.Count);

        // They are the exact same counters
        Assert.Contains(counter1, alias.Counters);
        Assert.Contains(counter2, primary.Counters);

        // Remove via alias removes from primary
        alias.RemoveCounter(counter1);
        Assert.Single(primary.Counters);
        Assert.Single(alias.Counters);
        Assert.Contains(counter2, alias.Counters);
    }

    [Fact]
    public void Hex_WithoutPrimaryAlias_IsIndependent()
    {
        var hex = new Hex<object>(new CubeCoordinate(0, 0, 0)) { Id = "Independent" };
        hex.AddCounter(new TestCounter("C"));

        Assert.Single(hex.Counters);
        
        hex.RemoveCounter(hex.Counters.First());
        Assert.Empty(hex.Counters);
    }
}
