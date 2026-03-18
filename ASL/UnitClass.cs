using System.Text.Json.Serialization;

namespace ASL;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UnitClass
{
    Elite,
    FirstLine,
    SecondLine,
    Green,
    Conscript
}
