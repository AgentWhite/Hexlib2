using ASL.Models.Equipment;
using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;
/// <summary>
/// Component for Ordnance weapons (Guns, Mortars, etc.).
/// </summary>
public class OrdnanceComponent : IUnitComponent
{
    /// <summary>
    /// Gets or sets the unit that owns this component.
    /// </summary>
    [JsonIgnore]
    public Unit? Owner { get; set; }

    /// <inheritdoc />
    public string ComponentName => GetType().Name;

    /// <inheritdoc />
    public void Initialize(Unit owner) { }

    /// <summary>Gets or sets the caliber of the ordnance.</summary>
    public int Caliber { get; set; }

    /// <summary>Gets or sets the muzzle type (e.g., Short, Long, Mortar).</summary>
    public MuzzleType MuzzleType { get; set; }

    /// <summary>Gets or sets the targeting type (e.g., Infantry, Area).</summary>
    public TargettingType TargettingType { get; set; }

    /// <summary>Gets or sets the minimum range of the weapon.</summary>
    public int? MinRange { get; set; }

    /// <summary>Gets or sets the maximum range of the weapon.</summary>
    public int MaxRange { get; set; }
}
