using System.Text.Json.Serialization;

namespace ASL;

/// <summary>
/// Specifies the quality or experience class of an ASL MMC or SMC.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitClass
{
    /// <summary>High-quality elite units.</summary>
    Elite,
    /// <summary>Standard front-line combat units.</summary>
    FirstLine,
    /// <summary>Intermediate or reserve units.</summary>
    SecondLine,
    /// <summary>Inexperienced or new units.</summary>
    Green,
    /// <summary>Low-quality draft units.</summary>
    Conscript
}
