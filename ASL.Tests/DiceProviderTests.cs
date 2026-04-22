using ASL.Infrastructure;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace ASL.Tests;

public class DiceProviderTests
{
    #region RandomDiceProvider Tests

    [Fact]
    public void RandomDiceProvider_dr_ReturnsValueInRange()
    {
        var provider = new RandomDiceProvider();
        for (int i = 0; i < 100; i++)
        {
            int result = provider.dr();
            Assert.InRange(result, 1, 6);
        }
    }

    [Fact]
    public void RandomDiceProvider_DR_ReturnsValuesInRange()
    {
        var provider = new RandomDiceProvider();
        for (int i = 0; i < 100; i++)
        {
            var result = provider.DR();
            Assert.InRange(result.ColoredDie, 1, 6);
            Assert.InRange(result.WhiteDie, 1, 6);
            Assert.InRange(result.Total, 2, 12);
        }
    }

    [Fact]
    public void RandomDiceProvider_RollMultiple_ReturnsCorrectCount()
    {
        var provider = new RandomDiceProvider();
        var results = provider.RollMultiple(5).ToList();
        
        Assert.Equal(5, results.Count);
        foreach (var r in results)
        {
            Assert.InRange(r, 1, 6);
        }
    }

    #endregion

    #region MockDiceProvider Tests

    [Fact]
    public void MockDiceProvider_ReturnsEnqueuedValues()
    {
        var provider = new MockDiceProvider();
        provider.Enqueue(1, 2, 3, 4, 5, 6);

        var dr1 = provider.dr();
        var DR1 = provider.DR();
        var multi = provider.RollMultiple(3).ToList();

        Assert.Equal(1, dr1);
        Assert.Equal(2, DR1.WhiteDie);
        Assert.Equal(3, DR1.ColoredDie);
        Assert.Equal(new List<int> { 4, 5, 6 }, multi);
    }

    #endregion

    #region JournaledDiceProvider Tests

    [Fact]
    public void JournaledDiceProvider_RecordsHistoryWithDescriptions()
    {
        var mock = new MockDiceProvider();
        mock.Enqueue(1, 2, 3, 4, 5, 6);
        var journaled = new JournaledDiceProvider(mock);

        journaled.dr("Sniper Check");
        journaled.DR("Heavy Artillery");
        journaled.RollMultiple(3, "Random Selection");

        var history = journaled.GetHistory();

        Assert.Equal(5, history.Count); // 1 (dr) + 1 (DR white) + 1 (DR colored) + 3 (multi)? 
        // Wait, JournaledDiceProvider implementation details:
        // dr adds 1 entry.
        // DR adds 1 entry with ColoredDie and WhiteDie.
        // RollMultiple adds 1 entry per die.
        // Total: 1 + 1 + 3 = 5 entries.
    }

    [Fact]
    public void JournaledDiceProvider_HistoryMatchesRollValues()
    {
        var mock = new MockDiceProvider();
        mock.Enqueue(2, 4, 6);
        var journaled = new JournaledDiceProvider(mock);

        var resultDR = journaled.DR("Test DR");
        var resultdr = journaled.dr("Test dr");

        var history = journaled.GetHistory();

        // Entry 0: DR (2, 4)
        Assert.Equal("Test DR", history[0].Type);
        Assert.Equal(4, history[0].ColoredDie);
        Assert.Equal(2, history[0].WhiteDie);

        // Entry 1: dr (6)
        Assert.Equal("Test dr", history[1].Type);
        Assert.Equal(6, history[1].ColoredDie);
        Assert.Null(history[1].WhiteDie);
    }

    #endregion
}
