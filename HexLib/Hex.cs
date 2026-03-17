namespace HexLib;

/// <summary>
/// Represents a single hex space on the board, holding its coordinates, its physical identity, and any counters occupying it.
/// </summary>
public class Hex
{
    /// <summary>
    /// The physical identifier for this hex (e.g., "A1" or "0103").
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// The logical cube coordinate identifying this hex's unrotated location relative to its parent board's origin.
    /// </summary>
    public CubeCoordinate Location { get; }

    private readonly List<ICounter> _localCounters = new List<ICounter>();
    
    /// <summary>
    /// Specifies the Primary hex that this hex delegates its counter storage to, used when joining half-hex boards.
    /// </summary>
    public Hex? PrimaryHexAlias { get; set; }

    /// <summary>
    /// A read-only collection of counters present in this hex.
    /// </summary>
    public IReadOnlyList<ICounter> Counters => PrimaryHexAlias != null ? PrimaryHexAlias.Counters : _localCounters;

    /// <summary>
    /// Initializes a new instance of the <see cref="Hex"/> class at the specified logical coordinates.
    /// </summary>
    /// <param name="location">The logical position relative to the board's top-left origin.</param>
    public Hex(CubeCoordinate location)
    {
        Location = location;
    }

    public void AddCounter(ICounter counter)
    {
        if (PrimaryHexAlias != null)
        {
            PrimaryHexAlias.AddCounter(counter);
            return;
        }

        if (counter == null) throw new ArgumentNullException(nameof(counter));
        _localCounters.Add(counter);
    }

    public bool RemoveCounter(ICounter counter)
    {
        if (PrimaryHexAlias != null)
        {
            return PrimaryHexAlias.RemoveCounter(counter);
        }

        if (counter == null) throw new ArgumentNullException(nameof(counter));
        return _localCounters.Remove(counter);
    }
}
