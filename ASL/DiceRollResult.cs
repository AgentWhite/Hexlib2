namespace ASL;

/// <summary>
/// Holds the result of an ASL dice roll, distinguishing between 1d6 (dr) and 2d6 (DR).
/// In a DR, the colored die and white die are tracked separately for mechanics like Snakes, Boxcars, and Sniper Activation.
/// </summary>
public class DiceRollResult
{
    /// <summary>
    /// The combined total of the dice rolled.
    /// </summary>
    public int Total => ColoredDie + WhiteDie;

    /// <summary>
    /// The value of the white die (the only die in a dr, or the first die in a DR).
    /// </summary>
    public int WhiteDie { get; }

    /// <summary>
    /// The value of the colored die (the second die in a DR, or 0 in a dr).
    /// </summary>
    public int ColoredDie { get; }

    /// <summary>
    /// How many dice were rolled (1 or 2).
    /// </summary>
    public int DiceCount { get; }

    public DiceRollResult(int white, int colored, int count)
    {
        WhiteDie = white;
        ColoredDie = colored;
        DiceCount = count;
    }

    public override string ToString() => DiceCount == 2 ? $"DR: {Total} ({WhiteDie},{ColoredDie})" : $"dr: {WhiteDie}";
}
