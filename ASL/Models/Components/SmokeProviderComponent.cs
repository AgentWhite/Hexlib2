namespace ASL.Models.Components;

public class SmokeProviderComponent : IUnitComponent
{
    public string ComponentName => GetType().Name;
    public Unit Owner { get; set; } = null!;
    public void Initialize(Unit owner) => Owner = owner;
    
    // The "Exponent" (for Infantry/SW) or "Depletion Number" (for Ordnance/Vehicles)
    public int CapabilityNumber { get; set; }

    public SmokeType SmokeType { get; set; }
}