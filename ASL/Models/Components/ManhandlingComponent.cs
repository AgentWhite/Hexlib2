using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component for towed/manhandlable ordnance. Carries the manhandling number and Quick Set-Up flag.
/// </summary>
public class ManhandlingComponent : IUnitComponent
{
    /// <inheritdoc />
    [JsonIgnore]
    public Unit? Owner { get; set; }

    /// <inheritdoc />
    public string ComponentName => GetType().Name;

    /// <inheritdoc />
    public void Initialize(Unit owner) => Owner = owner;

    /// <summary>Gets or sets the manhandling number (MN) used for pushing/towing checks.</summary>
    public int ManhandlingNumber { get; set; }

    /// <summary>Gets or sets a value indicating whether the piece has Quick Set-Up (QSU) capability.</summary>
    public bool HasQuickSetUp { get; set; }
}
