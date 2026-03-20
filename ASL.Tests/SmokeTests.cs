using ASL.Models;
using ASL.Models.Components;
using Xunit;

namespace ASL.Tests;

public class SmokeTests
{
    [Fact]
    public void Unit_CanAddSmokeProviderComponent()
    {
        var unit = new Unit { Name = "Smoke Unit" };
        var smoke = new SmokeProviderComponent { CapabilityNumber = 2, SmokeType = SmokeType.White };
        unit.AddComponent(smoke);

        Assert.True(unit.HasComponent<SmokeProviderComponent>());
        Assert.Equal(2, unit.GetComponent<SmokeProviderComponent>()?.CapabilityNumber);
        Assert.Equal(SmokeType.White, unit.GetComponent<SmokeProviderComponent>()?.SmokeType);
    }
}
