using ASL.Models.Equipment;
using ASL.Core;
using ASL.Models.Units;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;
/// <summary>
/// Component for Light Anti-Tank Weapons (LATW).
/// </summary>
public class LightAntiTankWeaponComponent : IUnitComponent
{
    /// <inheritdoc />
    public string ComponentName => GetType().Name;

    /// <inheritdoc />
    public void Initialize(Unit owner) => Owner = owner;
    
    /// <summary>Gets the owner unit of this component.</summary>
    [JsonIgnore]
    public Unit? Owner { get; set; }

    /// <summary>Gets or sets the type of LATW.</summary>
    public LightAntiTankWeaponType WeaponType { get; set; }

    /// <summary>Gets or sets the private to-hit table for this weapon.</summary>
    public Dictionary<int, int>? PrivateToHitTable { get; set; }

    /// <summary>Gets or sets a value indicating whether the weapon has a backblast.</summary>
    public bool HasBackBlast { get; set; }

    /// <summary>Gets or sets a value indicating whether the weapon uses a shaped charge.</summary>
    public bool IsShapedCharge { get; set; }
}
