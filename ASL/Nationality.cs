using System.Text.Json.Serialization;

namespace ASL;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Nationality
{
    German,
    Russian,
    Finnish,
    Partisan
}
