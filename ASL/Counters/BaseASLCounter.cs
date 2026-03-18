using System.Text.Json.Serialization;
using HexLib;

namespace ASL.Counters;

/// <summary>
/// Base class for all Advanced Squad Leader (ASL) counters.
/// Provides core properties shared by all unit types.
/// </summary>
[JsonDerivedType(typeof(Leader), typeDiscriminator: "Leader")]
[JsonDerivedType(typeof(Squad), typeDiscriminator: "Squad")]
[JsonDerivedType(typeof(HalfSquad), typeDiscriminator: "HalfSquad")]
[JsonDerivedType(typeof(Crew), typeDiscriminator: "Crew")]
[JsonDerivedType(typeof(Hero), typeDiscriminator: "Hero")]
public abstract class BaseASLCounter : ICounter
{
    /// <summary>
    /// Gets the name or specific identity of the counter.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path to the front image of the counter.
    /// </summary>
    public string? ImagePathFront { get; set; }

    /// <summary>
    /// Gets or sets the file path to the back image of the counter.
    /// </summary>
    public string? ImagePathBack { get; set; }

    /// <summary>
    /// Gets or sets the class of the unit (Elite, 1st Line, etc.).
    /// </summary>
    public virtual UnitClass AslClass { get; set; }

    /// <summary>
    /// Gets the base morale level of the unit.
    /// </summary>
    public int Morale { get; set; }

    /// <summary>
    /// Gets or sets the nationality/allegiance of the unit.
    /// </summary>
    public Nationality Nationality { get; set; }

    /// <summary>
    /// Gets or sets the physical module/box this counter belongs to.
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseASLCounter"/> class.
    /// </summary>
    /// <param name="name">The name or identity of the counter.</param>
    /// <param name="morale">The base morale level.</param>
    /// <param name="nationality">The unit's nationality.</param>
    protected BaseASLCounter(string name, int morale, Nationality nationality)
    {
        Name = name;
        Morale = morale;
        Nationality = nationality;
    }
}
