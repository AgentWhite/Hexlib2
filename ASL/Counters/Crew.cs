namespace ASL.Counters;

/// <summary>
/// Represents an MMC Crew in ASL.
/// Crews are specialized units (e.g., for operating SW or Guns) and are always considered Elite class.
/// </summary>
public class Crew : MultiManCounter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Crew"/> class with Elite class.
    /// </summary>
    public Crew(string name, int fp, int range, int morale, Nationality nationality) 
        : base(fp, range, morale, name, UnitClass.Elite, nationality)
    {
    }
}
