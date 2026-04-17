using ASL.Models.Equipment;
using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that defines the breakdown and removal numbers of a support weapon.
/// </summary>
public class BreakdownComponent : IUnitComponent
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
    /// Gets or sets the number at which the weapon breaks down.
    /// </summary>
    public int BreakdownNumber { get; set; }

    /// <summary>
    /// Gets or sets the number at which the weapon is removed from play (permanent breakdown).
    /// </summary>
    public int RemovalNumber { get; set; }

    /// <summary>
    /// Gets or sets the number required to repair the weapon.
    /// </summary>
    public int RepairNumber { get; set; }
}
