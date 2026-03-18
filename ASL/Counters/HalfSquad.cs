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
    [System.Text.Json.Serialization.JsonConstructor]
    public HalfSquad(string name, int firepower, int range, int morale, UnitClass aslClass, Nationality nationality) 
        : base(firepower, range, morale, name, aslClass, nationality)
    {
    }
}
