using System;

namespace ASL.Infrastructure;

/// <summary>
/// Service responsible for ASL dice rolling mechanics.
/// </summary>
public class ASLDice
{
    private readonly Random _random = new Random();

    /// <summary>
    /// Occurs whenever a dice roll is performed. UI components can subscribe to this to show animations or results.
    /// </summary>
    public event Action<DiceRollResult>? OnDiceRolled;

    /// <summary>
    /// Performs a 2d6 roll (one colored, one white).
    /// </summary>
    /// <returns>The result of the roll.</returns>
    public DiceRollResult DR()
    {
        var white = _random.Next(1, 7);
        var colored = _random.Next(1, 7);
        var result = new DiceRollResult(white, colored, 2);
        
        OnDiceRolled?.Invoke(result);
        return result;
    }

    /// <summary>
    /// Performs a single 1d6 roll (result is in WhiteDie).
    /// </summary>
    /// <returns>The result of the roll.</returns>
    public DiceRollResult dr()
    {
        var val = _random.Next(1, 7);
        var result = new DiceRollResult(val, 0, 1);
        
        OnDiceRolled?.Invoke(result);
        return result;
    }
}
