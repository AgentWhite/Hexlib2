namespace ASL.Models.Components;

public class SupportWeaponComponent : IUnitComponent
{
    public string ComponentName => GetType().Name;
    public Unit Owner { get; set; } = null!;
    public void Initialize(Unit owner) => Owner = owner;

    public int FirePower { get; set; }
    public int Range { get; set; }
    public int RateOfFire { get; set; }

    public int BreakdownNumber { get; set; }
    public int PortageCost { get; set; }
}