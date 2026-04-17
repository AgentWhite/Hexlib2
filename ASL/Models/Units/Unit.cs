using ASL.Core;
using ASL.Models.Components;
using HexLib;
using System.Text.Json.Serialization;
using System.Linq;
using System;
using System.Collections.Generic;

namespace ASL.Models.Units;

/// <summary>
/// Represents a basic unit in the ASL system, which can be composed of multiple components.
/// Implements ICounter for hex-based positioning.
/// </summary>
public class Unit : ICounter
{
    /// <summary>
    /// Gets the unique identifier for the unit, currently defaulting to its Name.
    /// </summary>
    [JsonIgnore]
    public string Id => Name;

    /// <summary>
    /// Gets or sets the name of the unit.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path to the front image of the unit's counter.
    /// </summary>
    public string? ImagePathFront { get; set; }

    /// <summary>
    /// Gets or sets the file path to the back image of the unit's counter.
    /// </summary>
    public string? ImagePathBack { get; set; }
    
    /// <summary>
    /// Gets or sets the general unit type (SMC, MMC, etc.).
    /// </summary>
    public UnitType UnitType { get; set; }
    
    /// <summary>
    /// Gets or sets the nationality of the unit.
    /// </summary>
    public Nationality Nationality { get; set; }  
    
    /// <summary>
    /// Gets or sets the parent unit (e.g., if this is a support weapon carried by a squad).
    /// </summary>
    [JsonIgnore]
    public Unit? Parent { get; set; }

    /// <summary>
    /// Gets or sets the list of components that define the unit's capabilities.
    /// </summary>
    public List<IUnitComponent> Components { get; set; } = new();

    /// <summary>
    /// Retrieves a component of the specified type, if it exists.
    /// </summary>
    /// <typeparam name="T">The type of component to retrieve.</typeparam>
    /// <returns>The component instance, or null if not found.</returns>
    public T? GetComponent<T>() where T : class, IUnitComponent
    {
        return Components.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Checks if the unit has a component of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of component to check for.</typeparam>
    /// <returns>True if the component exists; otherwise, false.</returns>
    public bool HasComponent<T>() => Components.Any(c => c is T);
    
    /// <summary>
    /// Adds a component to the unit and initializes it.
    /// </summary>
    /// <typeparam name="T">The type of component to add.</typeparam>
    /// <param name="comp">The component instance to add.</param>
    /// <exception cref="InvalidOperationException">Thrown if a component of the same type already exists.</exception>
    public void AddComponent<T>(T comp) where T : IUnitComponent
    {
        if (HasComponent<T>()) throw new InvalidOperationException($"Component {typeof(T).Name} already exists.");
        
        if(comp is IUnitComponent component && component.Owner != null)
        {
            component.Owner.RemoveComponent(component);
            component.Owner = null;
        }

        comp.Initialize(this);
        Components.Add(comp);
    }

    /// <summary>
    /// Removes a specific component from the unit.
    /// </summary>
    /// <typeparam name="T">The type of component to remove.</typeparam>
    /// <param name="comp">The component instance to remove.</param>
    public void RemoveComponent<T>(T comp) where T : IUnitComponent
    {
        if (Components.Contains(comp))
        {
            Components.Remove(comp);
            comp.Owner = null; // Clean up the back-reference
        }
    }

    // Helper methods
    // High-Level Shortcuts (The "Identity" logic)

    /// <summary>
    /// Gets a value indicating whether this unit represents a Leader (SMC with Leadership and Infantry components).
    /// </summary>
    [JsonIgnore]
    public bool IsLeader => UnitType == UnitType.SMC && HasComponent<LeadershipComponent>() && HasComponent<InfantryComponent>();

    /// <summary>
    /// Gets a value indicating whether this unit represents a Hero (SMC with Hero and Infantry components).
    /// </summary>
    [JsonIgnore]
    public bool IsHero => UnitType == UnitType.SMC && HasComponent<HeroComponent>() && HasComponent<InfantryComponent>();

    /// <summary>
    /// Gets a value indicating whether this unit represents a Half-Squad.
    /// </summary>
    [JsonIgnore]
    public bool IsHalfSquad => UnitType == UnitType.MMC && HasComponent<InfantryComponent>() && Infantry?.Scale == InfantryScale.HalfSquad;

    /// <summary>
    /// Gets a value indicating whether this unit represents a Crew.
    /// </summary>
    [JsonIgnore]
    public bool IsCrew => UnitType == UnitType.MMC && HasComponent<InfantryComponent>() && Infantry?.Scale == InfantryScale.Crew;

    /// <summary>
    /// Gets a value indicating whether this unit represents a full Squad.
    /// </summary>
    [JsonIgnore]
    public bool IsSquad => UnitType == UnitType.MMC && HasComponent<InfantryComponent>() && Infantry?.Scale == InfantryScale.Squad;

    /// <summary>
    /// Gets a value indicating whether this unit represents a Support Weapon.
    /// </summary>
    [JsonIgnore]
    // TODO: This logic is currently incorrect as other unit types may also have a BreakdownComponent.
    public bool IsSupportWeapon => HasComponent<BreakdownComponent>();
    
    // Commonly used components for convenience

    /// <summary>
    /// Gets the InfantryComponent of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public InfantryComponent? Infantry => GetComponent<InfantryComponent>();

    /// <summary>
    /// Gets the LeadershipComponent of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public LeadershipComponent? Leadership => GetComponent<LeadershipComponent>();

    /// <summary>
    /// Gets the HeroComponent of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public HeroComponent? Hero => GetComponent<HeroComponent>();

    /// <summary>
    /// Gets the FirePowerComponent of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public FirePowerComponent? FirePower => GetComponent<FirePowerComponent>();

    /// <summary>
    /// Gets the BPVComponent of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public BPVComponent? Bpv => GetComponent<BPVComponent>();

    /// <summary>
    /// Gets the PortageComponent of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public PortageComponent? Portage => GetComponent<PortageComponent>();

    /// <summary>
    /// Gets the BreakdownComponent of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public BreakdownComponent? Breakdown => GetComponent<BreakdownComponent>();

    /// <summary>
    /// Gets the MachineGunComponent of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public MachineGunComponent? MachineGun => GetComponent<MachineGunComponent>();

    /// <summary>
    /// Gets the Rate of Fire of the unit, if present.
    /// </summary>
    [JsonIgnore]
    public int? RateOfFire => FirePower?.RateOfFire;
}
