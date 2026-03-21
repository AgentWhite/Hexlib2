using System.Text.Json.Serialization;

namespace ASL.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LightAntiTankWeaponType {
    Baz43, 
    Baz44,
    Baz45,
    Panzerschreck
}