using ASL;
using ASL.Core;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Infrastructure;
using ASL.Services;
using Xunit;

namespace ASL.Tests;

public class DiceTests
{
    [Fact]
    public void dr_ReturnsValueInWhiteDie()
    {
        var dice = new ASLDice();
        for (int i = 0; i < 100; i++)
        {
            var result = dice.dr();
            Assert.InRange(result.Total, 1, 6);
            Assert.Equal(1, result.DiceCount);
            Assert.InRange(result.WhiteDie, 1, 6);
            Assert.Equal(0, result.ColoredDie); // dr has no colored die
        }
    }

    [Fact]
    public void DR_ReturnsWhiteDieFirstAndColoredDieSecond()
    {
        var dice = new ASLDice();
        for (int i = 0; i < 100; i++)
        {
            var result = dice.DR();
            Assert.InRange(result.Total, 2, 12);
            Assert.InRange(result.WhiteDie, 1, 6);
            Assert.InRange(result.ColoredDie, 1, 6);
            Assert.Equal(2, result.DiceCount);
        }
    }

    [Fact]
    public void DiceService_FiresEventOnRoll()
    {
        var dice = new ASLDice();
        DiceRollResult? rolled = null;
        dice.OnDiceRolled += (res) => rolled = res;

        var result = dice.DR();
        
        Assert.NotNull(rolled);
        Assert.Equal(result, rolled);
        Assert.Equal(2, rolled.DiceCount);
    }
}
