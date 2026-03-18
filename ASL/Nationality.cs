using System.Text.Json.Serialization;

namespace ASL;

/// <summary>
/// Specifies the nationality of an ASL unit or scenario side.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Nationality
{
    /// <summary>German nationality.</summary>
    German,
    /// <summary>Russian / Soviet nationality.</summary>
    Russian,
    /// <summary>Finnish nationality.</summary>
    Finnish,
    /// <summary>Partisan / Resistance forces.</summary>
    Partisan,
    /// <summary>American nationality.</summary>
    American
}
