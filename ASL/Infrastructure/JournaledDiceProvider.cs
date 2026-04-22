using System.ComponentModel;

namespace ASL.Infrastructure;

/// <summary>
/// A wrapper for an <see cref="IDiceProvider"/> that records all rolls into a history journal.
/// </summary>
public class JournaledDiceProvider : IDiceProvider
{
    private readonly IDiceProvider _innerProvider;
    private readonly List<DiceEntry> _journal = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="JournaledDiceProvider"/> class.
    /// </summary>
    /// <param name="inner">The underlying dice provider to wrap.</param>
    public JournaledDiceProvider(IDiceProvider inner)
    {
        _innerProvider = inner;
    }

    /// <summary>
    /// Gets the history of all rolls performed through this provider.
    /// </summary>
    /// <returns>A read-only list of <see cref="DiceEntry"/> objects.</returns>
    public IReadOnlyList<DiceEntry> GetHistory() => _journal;

    /// <inheritdoc />
    public DiceRollResult DR(string? description = null)
    {
        var result = _innerProvider.DR();
        _journal.Add(new DiceEntry(description ?? "DR", result.ColoredDie, result.WhiteDie));
        return result;
    }

    /// <inheritdoc />
    public int dr(string? description = null)
    {
        var result = _innerProvider.dr();
        _journal.Add(new DiceEntry(description ?? "dr", result));
        return result;
    }

    /// <inheritdoc />
    public IEnumerable<int> RollMultiple(int count, string? description = null)
    {
        var results = _innerProvider.RollMultiple(count).ToList();
        foreach(var r in results) 
            _journal.Add(new DiceEntry(description ?? "Multi", r));
        return results;
    }
}