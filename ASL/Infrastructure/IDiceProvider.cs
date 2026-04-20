namespace ASL.Infrastructure;

/// <summary>
/// Defines a contract for providing dice roll results, allowing for 
/// interchangeable implementations such as random, seeded, or manual input.
/// </summary>
public interface IDiceProvider
{
    /// <summary>
    /// Performs a 2d6 roll (one colored, one white).
    /// </summary>
    /// <returns>The result of the roll.</returns>
    DiceRollResult DR(string? description = null);

    /// <summary>
    /// Performs a single 1d6 roll.
    /// </summary>
    /// <returns>The result of the roll.</returns>
    int dr(string? description = null);

    /// <summary>
    /// Roll a number of dice, mostly used for random selection.
    /// </summary>
    /// <param name="count">The number of dice to be rolled.</param>
    /// <returns></returns>
    IEnumerable<int> RollMultiple(int count, string? description = null);
}
