namespace ASL.Counters;

/// <summary>
/// Represents a full-strength Multi-Man Counter (MMC) Squad in ASL.
/// </summary>
public class Squad : MultiManCounter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Squad"/> class.
    /// </summary>
    /// <summary>
    /// Initializes a new instance of the <see cref="Squad"/> class.
    /// </summary>
    /// <param name="name">Unit name.</param>
    /// <param name="firepower">Base firepower.</param>
    /// <param name="range">Base range.</param>
    /// <param name="morale">Base morale.</param>
    /// <param name="aslClass">Unit class/quality.</param>
    /// <param name="nationality">Nationality.</param>
    [System.Text.Json.Serialization.JsonConstructor]
    public Squad(string name, int firepower, int range, int morale, UnitClass aslClass, Nationality nationality) 
        : base(firepower, range, morale, name, aslClass, nationality)
    {
    }
}
