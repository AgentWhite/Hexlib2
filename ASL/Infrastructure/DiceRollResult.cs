namespace ASL.Infrastructure;

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
    /// The sum of the dice rolled.
    /// </summary>
    public int Sum => ColoredDie + WhiteDie;

    /// <summary>
    /// Do the die show equal number of dots. 
    /// </summary>
    public bool IsDouble => ColoredDie == WhiteDie;

    /// <summary>
    /// Convenience method to determine if snakeyes where rolled.
    /// </summary>
    public bool IsSnakeEyes => WhiteDie == 1 && ColoredDie == 1;


    /// <summary>
    /// Initializes a new instance of the <see cref="DiceRollResult"/> class.
    /// </summary>
    /// <param name="white">The result of the white die.</param>
    /// <param name="colored">The result of the colored die (if 2d6).</param>
    public DiceRollResult(int white, int colored)
    {
        WhiteDie = white;
        ColoredDie = colored;
    }
}
