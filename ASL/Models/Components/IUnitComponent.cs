using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Interface for all unit components. Components define specific capabilities and statistics for a Unit.
/// </summary>
[JsonDerivedType(typeof(InfantryComponent), typeDiscriminator: "Infantry")]
[JsonDerivedType(typeof(LeadershipComponent), typeDiscriminator: "Leadership")]
[JsonDerivedType(typeof(HeroComponent), typeDiscriminator: "Hero")]
[JsonDerivedType(typeof(FirePowerComponent), typeDiscriminator: "FirePower")]
[JsonDerivedType(typeof(BPVComponent), typeDiscriminator: "BPV")]
[JsonDerivedType(typeof(SmokeProviderComponent), typeDiscriminator: "Smoke")]
[JsonDerivedType(typeof(SupportWeaponComponent), typeDiscriminator: "SupportWeapon")]
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