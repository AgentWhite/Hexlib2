using System.Text.Json.Serialization;

namespace ASL.Models.Equipment;

/// <summary>
/// Defines the types of Light Anti-Tank Weapons (LATW).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LightAntiTankWeaponType {
    /// <summary>Bazooka M9/M9A1 (1943).</summary>
    Baz43, 
    /// <summary>Bazooka M18 (1944).</summary>
    Baz44,
    /// <summary>Super Bazooka (1945).</summary>
    Baz45,
    /// <summary>German Panzerschreck.</summary>
    Panzerschreck
}