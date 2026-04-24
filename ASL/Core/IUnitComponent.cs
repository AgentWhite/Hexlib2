using System.Text.Json.Serialization;
using ASL.Models.Units;
using ASL.Models.Components;

namespace ASL.Core;

/// <summary>
/// Interface for all unit components. Components define specific capabilities and statistics for a Unit.
/// </summary>
[JsonDerivedType(typeof(InfantryComponent), typeDiscriminator: "Infantry")]
[JsonDerivedType(typeof(LeadershipComponent), typeDiscriminator: "Leadership")]
[JsonDerivedType(typeof(HeroComponent), typeDiscriminator: "Hero")]
[JsonDerivedType(typeof(FirePowerComponent), typeDiscriminator: "FirePower")]
[JsonDerivedType(typeof(BPVComponent), typeDiscriminator: "BPV")]
[JsonDerivedType(typeof(SmokeProviderComponent), typeDiscriminator: "Smoke")]
[JsonDerivedType(typeof(PortageComponent), typeDiscriminator: "Portage")]
[JsonDerivedType(typeof(BreakdownComponent), typeDiscriminator: "Breakdown")]
[JsonDerivedType(typeof(MachineGunComponent), typeDiscriminator: "MachineGun")]
[JsonDerivedType(typeof(RadioComponent), typeDiscriminator: "Radio")]
[JsonDerivedType(typeof(OrdnanceComponent), typeDiscriminator: "Ordnance")]
[JsonDerivedType(typeof(LightAntiTankWeaponComponent), typeDiscriminator: "LightAntiTankWeapon")]
[JsonDerivedType(typeof(GunToHitComponent), typeDiscriminator: "GunToHit")]
[JsonDerivedType(typeof(ManhandlingComponent), typeDiscriminator: "Manhandling")]
[JsonDerivedType(typeof(LimberedComponent), typeDiscriminator: "Limbered")]
[JsonDerivedType(typeof(InherentFirepowerComponent), typeDiscriminator: "InherentFirepower")]
[JsonDerivedType(typeof(AntiAircraftComponent), typeDiscriminator: "AntiAircraft")]
public interface IUnitComponent
{
    /// <summary>
    /// Gets or sets the Unit that owns this component.
    /// </summary>
    [JsonIgnore]
    Unit? Owner { get; set; }

    /// <summary>
    /// Gets the unique name or type identifier for the component.
    /// </summary>
    string ComponentName { get; }

    /// <summary>
    /// Initializes the component with its owner unit.
    /// </summary>
    /// <param name="owner">The unit that owns this component.</param>
    void Initialize(Unit owner);
}