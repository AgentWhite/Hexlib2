using ASL.Models.Equipment;
using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that defines the firepower and range of a unit.
/// </summary>
public class FirePowerComponent : IUnitComponent
{
    /// <summary>
    /// Gets the unit that owns this component.
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

    /// <summary>
    /// Gets or sets the base firepower value.
    /// </summary>
    public int Firepower { get; set; }

    /// <summary>
    /// Gets or sets the base range value (in hexes).
    /// </summary>
    public int Range { get; set; }

    /// <summary>
    /// Gets or sets the rate of fire (ROF).
    /// </summary>
    public int? RateOfFire { get; set; }
}
