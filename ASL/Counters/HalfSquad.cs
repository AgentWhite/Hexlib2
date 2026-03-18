namespace ASL.Counters;

/// <summary>
/// Represents a Half-Squad MMC in ASL. 
/// Typically result from splitting a squad or taking casualties.
/// </summary>
public class HalfSquad : MultiManCounter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HalfSquad"/> class.
    /// </summary>
    public HalfSquad(string name, int fp, int range, int morale, UnitClass unitClass, Nationality nationality) 
        : base(fp, range, morale, name, unitClass, nationality)
    {
    }
}
