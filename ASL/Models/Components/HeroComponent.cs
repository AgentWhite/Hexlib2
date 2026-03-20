using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that marks a unit as a Hero.
/// </summary>
public class HeroComponent : IUnitComponent
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

    // TODO: These properties will be used in the future to implement hero-specific bonuses
    // public int FirepowerBonus { get; set; } = 1;
    // public int MoraleBonus { get; set; } = 1;
    // public bool CannotCower { get; set; } = true;
}