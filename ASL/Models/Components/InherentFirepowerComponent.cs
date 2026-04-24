using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component carrying the Inherent Firepower (IFE) value printed on an ordnance counter —
/// the area-fire strength used when the piece fires on the IFT rather than via a To-Hit chart.
/// </summary>
public class InherentFirepowerComponent : IUnitComponent
{
    /// <inheritdoc />
    [JsonIgnore]
    public Unit? Owner { get; set; }

    /// <inheritdoc />
    public string ComponentName => GetType().Name;

    /// <inheritdoc />
    public void Initialize(Unit owner) => Owner = owner;

    /// <summary>Gets or sets the Inherent Firepower (IFE) value printed on the counter.</summary>
    public int InherentFirepower { get; set; }
}
