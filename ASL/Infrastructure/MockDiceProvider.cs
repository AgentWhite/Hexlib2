namespace ASL.Infrastructure;

/// <summary>
/// A mock dice provider that returns pre-enqueued rolls for testing purposes.
/// </summary>
public class MockDiceProvider : IDiceProvider
{
    private readonly Queue<int> _singleDieQueue = new();

    /// <summary>
    /// Enqueues a series of dice rolls to be returned in order.
    /// </summary>
    /// <param name="rolls">The sequence of rolls to enqueue.</param>
    public void Enqueue(params int[] rolls) 
    {
        foreach (var r in rolls) _singleDieQueue.Enqueue(r);
    }

    /// <inheritdoc />
    public DiceRollResult DR(string? description = null) => new DiceRollResult(_singleDieQueue.Dequeue(), _singleDieQueue.Dequeue());
    
    /// <inheritdoc />
    public int dr(string? description = null) => _singleDieQueue.Dequeue();

    /// <inheritdoc />
    public IEnumerable<int> RollMultiple(int count, string? description = null)
    {
        var results = new List<int>();
        for (int i = 0; i < count; i++) results.Add(_singleDieQueue.Dequeue());
        return results;
    }
}