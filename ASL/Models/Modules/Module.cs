using System.Text.Json.Serialization;

namespace ASL.Models.Modules;

/// <summary>
/// Represents the physical modules/boxes in the ASL system.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Module
{
    /// <summary>The "Paratrooper" module.</summary>
    Paratrooper,
    /// <summary>The "Beyond Valor" module (German/Russian).</summary>
    BeyondValor,
    /// <summary>The "Yanks" module (American).</summary>
    Yanks
}
