using System.Text.Json.Serialization;

namespace ASL.Models;

/// <summary>
/// Specifies the type of machine gun.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MachineGunType
{
    /// <summary>
    /// Light machine gun.
    /// </summary>
    LMG,

    /// <summary>
    /// Medium machine gun.
    /// </summary>
    MMG,

    /// <summary>
    /// Heavy machine gun.
    /// </summary>
    HMG
}
