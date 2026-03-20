namespace ASL.Models.Components;

public class SmokeProviderComponent : IUnitComponent
{
    public string ComponentName => GetType().Name;
    public Unit Owner { get; set; } = null!;
    public void Initialize(Unit owner) => Owner = owner;

    public int SmokePlacmentExponent { get; set; }
    public SmokeType SmokeType { get; set; }
}