namespace ASL.Counters;

/// <summary>
/// Represents a Single Man Counter (SMC) Leader in ASL.
/// Leaders have a leadership modifier that affects other units in the same location.
/// </summary>
public class Leader : BaseASLCounter
{
    /// <summary>
    /// Gets or sets the leadership modifier (e.g., -1, 0, 1).
    /// </summary>
    public int Leadership { get; set; }

    /// <inheritdoc />
    public override string Stats => $"Morale: {Morale}, Leadership: {Leadership}";

    /// <summary>
    /// Initializes a new instance of the <see cref="Leader"/> class.
    /// </summary>
    /// <param name="name">Leader name.</param>
    /// <param name="morale">Leader morale.</param>
    /// <param name="leadership">Leadership modifier.</param>
    /// <param name="nationality">Nationality.</param>
    [System.Text.Json.Serialization.JsonConstructor]
    public Leader(string name, int morale, int leadership, Nationality nationality) : base(name, morale, nationality)
    {
        Leadership = leadership;
    }
}
