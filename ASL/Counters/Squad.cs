namespace ASL.Counters;

/// <summary>
/// Represents a full-strength Multi-Man Counter (MMC) Squad in ASL.
/// </summary>
public class Squad : MultiManCounter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Squad"/> class.
    /// </summary>
    public Squad(string name, int fp, int range, int morale, UnitClass unitClass, Nationality nationality) 
        : base(fp, range, morale, name, unitClass, nationality)
    {
    }
}
