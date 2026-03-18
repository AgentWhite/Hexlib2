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
    public override UnitClass Class 
    { 
        get => UnitClass.Elite; 
        set { /* Crews are always Elite, ignore setter */ } 
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Crew"/> class with Elite class.
    /// </summary>
    public Crew(string name, int fp, int range, int morale, Nationality nationality) 
        : base(fp, range, morale, name, UnitClass.Elite, nationality)
    {
    }
}
