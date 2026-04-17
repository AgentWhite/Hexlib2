using ASL.Models.Equipment;
using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component that provides smoke placement capabilities.
/// </summary>
public class SmokeProviderComponent : IUnitComponent
{
    /// <inheritdoc/>
    public string ComponentName => GetType().Name;
    /// <inheritdoc/>
    [JsonIgnore]
    public Unit? Owner { get; set; }
    /// <inheritdoc/>
    public void Initialize(Unit owner) => Owner = owner;
    
    /// <summary>
    /// Gets or sets the "Exponent" (for Infantry/SW) or "Depletion Number" (for Ordnance/Vehicles).
    /// </summary>
    public int CapabilityNumber { get; set; }

    /// <summary>
    /// Gets or sets the type of smoke provided.
    /// </summary>
    public SmokeType SmokeType { get; set; }
}
