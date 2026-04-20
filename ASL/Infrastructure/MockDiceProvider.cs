namespace ASL.Infrastructure;

public class MockDiceProvider : IDiceProvider
{
    private readonly Queue<int> _singleDieQueue = new();

    public void Enqueue(params int[] rolls) 
    {
        foreach (var r in rolls) _singleDieQueue.Enqueue(r);
    }

    public DiceRollResult DR(string? description = null) => new DiceRollResult(_singleDieQueue.Dequeue(), _singleDieQueue.Dequeue());
    
    public int dr(string? description = null) => _singleDieQueue.Dequeue();

    public IEnumerable<int> RollMultiple(int count, string? description = null)
    {
        var results = new List<int>();
        for (int i = 0; i < count; i++) results.Add(_singleDieQueue.Dequeue());
        return results;
    }
}