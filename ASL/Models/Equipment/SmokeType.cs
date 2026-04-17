using System.Text.Json.Serialization;

namespace ASL.Models.Equipment;

/// <summary>
/// Represents the type of smoke.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SmokeType
{
    /// <summary>
    /// White smoke.
    /// </summary>
    White,
    /// <summary>
    /// White Phosphorus smoke.
    /// </summary>
    WP,
    /// <summary>
    /// Candle smoke.
    /// </summary>
    Candles, 
    /// <summary>
    /// Smoke Discharger smoke.
    /// </summary>
    SmokeDischarger
}
