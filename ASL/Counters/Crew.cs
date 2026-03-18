namespace ASL.Counters;

/// <summary>
/// Represents an MMC Crew in ASL.
/// Crews are specialized units (e.g., for operating SW or Guns) and are always considered Elite class.
/// </summary>
public class Crew : MultiManCounter
{
    /// <summary>
    /// Gets or sets the class of the unit. Crews are always Elite.
    /// </summary>
    public override UnitClass AslClass 
    { 
        get => UnitClass.Elite; 
        set { /* Crews are always Elite, ignore setter */ } 
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Crew"/> class with Elite class.
    /// </summary>
    [System.Text.Json.Serialization.JsonConstructor]
    public Crew(string identity, int firepower, int range, int morale, Nationality nationality) 
        : base(firepower, range, morale, identity, UnitClass.Elite, nationality)
    {
    }
}
