using ASL.Core;
using ASL.Models.Units;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;

/// <summary>
/// Component marking an ordnance piece (or vehicle main armament) as able to engage aerial targets.
/// </summary>
public class AntiAircraftComponent : IUnitComponent
{
    /// <inheritdoc />
    [JsonIgnore]
    public Unit? Owner { get; set; }

    /// <inheritdoc />
    public string ComponentName => GetType().Name;

    /// <inheritdoc />
    public void Initialize(Unit owner) => Owner = owner;

    /// <summary>Gets or sets a value indicating whether the piece is capable of anti-aircraft fire.</summary>
    public bool IsAACapable { get; set; } = true;

    /// <summary>Gets or sets a value indicating whether the piece is dedicated AA and cannot fire on ground targets.</summary>
    public bool IsAAOnly { get; set; }

    /// <summary>Gets or sets an optional DRM applied to anti-aircraft attacks.</summary>
    public int? AAModifier { get; set; }
}
