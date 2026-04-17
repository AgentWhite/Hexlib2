using ASL.Models.Equipment;
using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that represents a Radio with a specific contact number.
/// </summary>
public class RadioComponent : IUnitComponent
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
    /// Gets or sets the contact number required for the radio.
    /// </summary>
    public int ContactNumber { get; set; }

    /// <summary>
    /// Gets or sets the battery ID to the OBA (Off Board Artillery).
    /// </summary>
    public string? BatteryId { get; set; }
    /// <summary>
    /// If the contact roll has succeded and the unit has contact with the OBA (Off Board Artillery).
    /// </summary>
    public bool HasContact {get; set; }
}
