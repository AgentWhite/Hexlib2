using System.Text.Json.Serialization;

namespace ASL.Models.Components;
public class OrdnanceComponent : IUnitComponent
{
    /// <summary>
    /// Gets the unit that owns this component.
    /// </summary>
    [JsonIgnore]
    public Unit? Owner { get; set; }
    public string ComponentName => GetType().Name;
    public void Initialize(Unit owner) { }

    public int Caliber { get; set; }
    public MuzzleType MuzzleType { get; set; }
    public TargettingType TargettingType { get; set; }

    public int? MinRange { get; set; }
    public int MaxRange { get; set; }
}