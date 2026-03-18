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
    /// <summary>
    /// Initializes a new instance of the <see cref="HalfSquad"/> class.
    /// </summary>
    /// <param name="name">Unit name.</param>
    /// <param name="firepower">Base firepower.</param>
    /// <param name="range">Base range.</param>
    /// <param name="morale">Base morale.</param>
    /// <param name="aslClass">Unit class/quality.</param>
    /// <param name="nationality">Nationality.</param>
    [System.Text.Json.Serialization.JsonConstructor]
    public HalfSquad(string name, int firepower, int range, int morale, UnitClass aslClass, Nationality nationality) 
        : base(firepower, range, morale, name, aslClass, nationality)
    {
    }
}
