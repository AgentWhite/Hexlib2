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
    /// Gets or sets a value indicating whether the unit has Assault Fire capability.
    /// </summary>
    public bool HasAssaultFire { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the unit has Spraying Fire capability.
    /// </summary>
    public bool HasSprayingFire { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the unit has Experience Level Rating (ELR) capability.
    /// </summary>
    public bool HasELR { get; set; }

    /// <summary>
    /// Gets or sets the morale level of the unit when it is broken.
    /// </summary>
    public int BrokenMoraleLevel { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the unit can attempt Self Rally.
    /// </summary>
    public bool CanSelfRally { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the unit has a Smoke placement exponent.
    /// </summary>
    public bool HasSmokeExponent { get; set; }

    /// <summary>
    /// Gets or sets the numeric exponent used for smoke placement attempts.
    /// </summary>
    public int SmokePlacementExponent { get; set; }

    /// <summary>
    /// Gets or sets the Basic Point Value (BPV) of the unit.
    /// </summary>
    public int BPV { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiManCounter"/> class.
    /// </summary>
    protected MultiManCounter(int firepower, int range, int morale, string name, UnitClass aslClass, Nationality nationality) 
        : base(name, morale, nationality)
    {
        Firepower = firepower;
        Range = range;
        AslClass = aslClass;
    }
}
