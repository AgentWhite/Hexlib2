using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ASL.Models.Components;
public class LightAntiTankWeaponComponent : IUnitComponent
{
    public string ComponentName => GetType().Name;
    public void Initialize(Unit owner) => Owner = owner;
    
    [JsonIgnore]
    public Unit? Owner { get; set; }

    public LightAntiTankWeaponType WeaponType { get; set; }
    public Dictionary<int, int>? PrivateToHitTable { get; set; }
    public bool HasBackBlast { get; set; }
    public bool IsShapedCharge { get; set; }
}