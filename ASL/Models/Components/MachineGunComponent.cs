using ASL.Models.Equipment;
using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that defines the specific type of machine gun.
/// </summary>
public class MachineGunComponent : IUnitComponent
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
    /// Gets or sets the type of the machine gun.
    /// </summary>
    public MachineGunType Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the machine gun has spraying fire capability.
    /// </summary>
    public bool HasSprayingFire { get; set; }
}
