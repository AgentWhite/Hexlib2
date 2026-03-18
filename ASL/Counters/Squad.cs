namespace ASL.Counters;

/// <summary>
/// Represents a full-strength Multi-Man Counter (MMC) Squad in ASL.
/// </summary>
public class Squad : MultiManCounter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Squad"/> class.
    /// </summary>
    [System.Text.Json.Serialization.JsonConstructor]
    public Squad(string name, int firepower, int range, int morale, UnitClass aslClass, Nationality nationality) 
        : base(firepower, range, morale, name, aslClass, nationality)
    {
    }
}
