using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that defines the Basic Point Value (BPV) of a unit.
/// </summary>
public class BPVComponent : IUnitComponent
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
    /// Gets or sets the Basic Point Value.
    /// </summary>
    public int BPV { get; set; }
}