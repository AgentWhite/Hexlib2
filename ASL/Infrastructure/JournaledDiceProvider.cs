using System.ComponentModel;

namespace ASL.Infrastructure;

public class JournaledDiceProvider : IDiceProvider
{
    private readonly IDiceProvider _innerProvider;
    private readonly List<DiceEntry> _journal = new();

    public JournaledDiceProvider(IDiceProvider inner)
    {
        _innerProvider = inner;
    }

    // Accessor for the Replay system
    public IReadOnlyList<DiceEntry> GetHistory() => _journal;

    public DiceRollResult DR(string? description = null)
    {
        var result = _innerProvider.DR();
        _journal.Add(new DiceEntry(description ?? "DR", result.ColoredDie, result.WhiteDie));
        return result;
    }

    public int dr(string? description = null)
    {
        var result = _innerProvider.dr();
        _journal.Add(new DiceEntry(description ?? "dr", result));
        return result;
    }

    public IEnumerable<int> RollMultiple(int count, string? description = null)
    {
        var results = _innerProvider.RollMultiple(count).ToList();
        foreach(var r in results) 
            _journal.Add(new DiceEntry(description ?? "Multi", r));
        return results;
    }
}