using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that defines basic infantry statistics and capabilities.
/// </summary>
public class InfantryComponent : IUnitComponent
{

    /// <summary>
    /// Gets or sets a value indicating whether the unit has Assault Fire capability.
    /// </summary>
    public bool HasAssaultFire { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the unit has Spraying Fire capability.
    /// </summary>
    public bool HasSprayingFire { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the unit can Self Rally.
    /// </summary>
    public bool CanSelfRally { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the unit has ELR (Experience Level Rating) capability.
    /// </summary>
    public bool HasELR { get; set; }

    /// <summary>
    /// Gets or sets the smoke placement exponent, or null if the unit cannot place smoke.
    /// </summary>
    public int? SmokePlacementExponent { get; set; }

    /// <summary>
    /// Gets or sets the scale of the infantry unit (Squad, Half-Squad, Crew, SMC).
    /// </summary>
    public InfantryScale Scale { get; set; }
    
    /// <summary>
    /// Gets or sets the unit's morale value.
    /// </summary>
    // Moved from Unit
    public int Morale { get; set; }

    /// <summary>
    /// Gets or sets the unit's broken morale value, or null for units that don't have one (e.g., Japanese leaders).
    /// </summary>
    public int? BrokenMorale { get; set; }

    /// <summary>
    /// Gets or sets the ASL class of the unit (Elite, First Line, etc.).
    /// </summary>
    public UnitClass AslClass { get; set; }

    /// <summary>
    /// gets the unit that owns this component.
    /// </summary>
    [JsonIgnore]
    public Unit? Owner { get; set; }

    /// <summary>
    /// Gets the name of the component.
    /// </summary>
    public string ComponentName => GetType().Name;

    /// <summary>
    /// Initializes the component with its owner unit.
    /// </summary>
    /// <param name="owner">The unit that owns this component.</param>
    public void Initialize(Unit owner) => Owner = owner;
}