namespace ASL.Counters;

/// <summary>
/// Base class for Multi-Man Counters (MMC) such as Squads, Half-Squads, and Crews.
/// MMCs have more complex characteristics like fire types, ELR, and smoke capabilities.
/// </summary>
public abstract class MultiManCounter : BaseASLCounter
{
    /// <summary>
    /// Gets or sets the base firepower of the unit.
    /// </summary>
    public int Firepower { get; set; }

    /// <summary>
    /// Gets or sets the normal range of the unit.
    /// </summary>
    public int Range { get; set; }

    /// <summary>
    /// Gets or sets the unit's specific identity (e.g., "1st Squad", "2nd Section").
    /// </summary>
    public string Identity { get; set; } = string.Empty;
    
    public bool HasAssaultFire { get; set; }
    public bool HasSprayingFire { get; set; }
    public bool HasELR { get; set; }
    public int BrokenMoraleLevel { get; set; }
    public bool CanSelfRally { get; set; }
    public bool HasSmokeExponent { get; set; }
    public int SmokePlacementExponent { get; set; }
    public int BPV { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiManCounter"/> class.
    /// </summary>
    protected MultiManCounter(int firepower, int range, int morale, string identity, UnitClass aslClass, Nationality nationality) 
        : base(identity, morale, nationality)
    {
        Firepower = firepower;
        Range = range;
        Identity = identity;
        AslClass = aslClass;
    }
}
