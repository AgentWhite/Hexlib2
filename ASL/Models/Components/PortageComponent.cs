using ASL.Models.Equipment;
using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that defines the portage (weight) and dismantle status of a unit.
/// </summary>
public class PortageComponent : IUnitComponent
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
    /// Gets or sets the portage cost (weight) of the unit when assembled.
    /// </summary>
    public int AssembledCost { get; set; }

    /// <summary>
    /// Gets or sets the portage cost (weight) of the unit when dismantled, if applicable.
    /// </summary>
    public int? DismantledCost { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the unit is currently dismantled.
    /// </summary>
    public bool IsDismantled { get; set; }

    /// <summary>
    /// Gets or sets the file path to the image representing the unit when dismantled.
    /// </summary>
    public string? DismantledImage { get; set; }

    /// <summary>
    /// Gets a value indicating whether the unit can be dismantled.
    /// </summary>
    [JsonIgnore]
    public bool CanBeDismantled => DismantledCost.HasValue;

    /// <summary>
    /// Dismantles the unit if it is capable of being dismantled.
    /// </summary>
    public void Dismantle()
    {
        if (CanBeDismantled) IsDismantled = true;
    }

    /// <summary>
    /// Assembles the unit.
    /// </summary>
    public void Assemble()
    {
        IsDismantled = false;
    }
}
