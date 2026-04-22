namespace ASL.Infrastructure;

/// <summary>
/// Provides randomized dice rolls using System.Random.
/// </summary>
public class RandomDiceProvider : IDiceProvider
{
    private readonly Random _rng = new();

    /// <inheritdoc />
    public DiceRollResult DR(string? description = null) => new DiceRollResult(_rng.Next(1, 7), _rng.Next(1, 7));

    /// <inheritdoc />
    public int dr(string? description = null) => _rng.Next(1, 7);

    /// <inheritdoc />
    public IEnumerable<int> RollMultiple(int count, string? description = null)
    {
        for (int i = 0; i < count; i++)
        {
            yield return _rng.Next(1, 7);
        }
    }
}