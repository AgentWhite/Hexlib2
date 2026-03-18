namespace ASL.Counters;

/// <summary>
/// Represents a Single Man Counter (SMC) Hero in ASL.
/// Heroes are exceptional individuals with higher firepower and range than normal units.
/// </summary>
public class Hero : BaseASLCounter
{
    /// <summary>
    /// Gets or sets the hero's firepower value.
    /// </summary>
    public int Firepower { get; set; }

    /// <summary>
    /// Gets or sets the hero's range value.
    /// </summary>
    public int Range { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Hero"/> class.
    /// </summary>
    /// <param name="name">Hero name.</param>
    /// <param name="firepower">Hero firepower.</param>
    /// <param name="range">Hero range.</param>
    /// <param name="morale">Hero morale.</param>
    /// <param name="nationality">Nationality.</param>
    [System.Text.Json.Serialization.JsonConstructor]
    public Hero(string name, int firepower, int range, int morale, Nationality nationality) : base(name, morale, nationality)
    {
        Firepower = firepower;
        Range = range;
        AslClass = UnitClass.Elite;
    }
}
