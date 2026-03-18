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
    /// Gets or sets the file path to the image representing this counter.
    /// </summary>
    public string? ImagePath { get; set; }

    /// <summary>
    /// Gets the base morale level of the unit.
    /// </summary>
    public int Morale { get; set; }

    /// <summary>
    /// Gets or sets the nationality/allegiance of the unit.
    /// </summary>
    public Nationality Nationality { get; set; }

    /// <summary>
    /// Gets a summarized string representation of the unit's key statistics for UI display.
    /// </summary>
    public abstract string Stats { get; }

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
