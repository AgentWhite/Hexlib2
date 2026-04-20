namespace ASL.Infrastructure;

public class RandomDiceProvider : IDiceProvider
{
    private readonly Random _rng = new();

    public DiceRollResult DR(string? description = null) => new DiceRollResult(_rng.Next(1, 7), _rng.Next(1, 7));

    public int dr(string? description = null) => _rng.Next(1, 7);

    public IEnumerable<int> RollMultiple(int count, string? description = null)
    {
        for (int i = 0; i < count; i++)
        {
            yield return _rng.Next(1, 7);
        }
    }
}