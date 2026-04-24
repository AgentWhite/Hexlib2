using ASL.Core;
using ASL.Models.Equipment;
using ASL.Models.Units;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component for ordnance that resolves direct-fire attacks via a gun To-Hit chart.
/// Not added to Mortars (area-fire only).
/// </summary>
public class GunToHitComponent : IUnitComponent
{
    /// <inheritdoc />
    [JsonIgnore]
    public Unit? Owner { get; set; }

    /// <inheritdoc />
    public string ComponentName => GetType().Name;

    /// <inheritdoc />
    public void Initialize(Unit owner) => Owner = owner;

    /// <summary>Gets or sets the gun's classification, selecting the default To-Hit chart.</summary>
    public GunClass GunClass { get; set; }

    /// <summary>Gets or sets a value indicating whether the gun is eligible for acquisition counters.</summary>
    public bool CanAcquire { get; set; } = true;

    /// <summary>Gets or sets an optional per-unit To-Hit table override (range → base To-Hit number).</summary>
    public Dictionary<int, int>? PrivateToHitTable { get; set; }
}
