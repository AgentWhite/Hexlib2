using ASL.Models;
using ASL.Models.Components;
using ASL.Models.Equipment;
using ASL.Models.Units;
using ASL.Persistence;
using HexLib.Persistence;
using System.IO;
using Xunit;

namespace ASL.Tests;

public class OrdnanceComponentTests : IDisposable
{
    private readonly string _tempDir;
    private readonly ASLSaveManager _manager;

    public OrdnanceComponentTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
        _manager = new ASLSaveManager(new FileStorageAdapter(_tempDir));
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    [Fact]
    public void Unit_ExposesOrdnanceConvenienceProperties()
    {
        var unit = new Unit { UnitType = UnitType.Ordnance };

        Assert.Null(unit.Ordnance);
        Assert.Null(unit.GunToHit);
        Assert.Null(unit.Manhandling);
        Assert.Null(unit.Limbered);
        Assert.Null(unit.InherentFirepower);
        Assert.Null(unit.AntiAircraft);

        unit.AddComponent(new OrdnanceComponent { Caliber = 75, MaxRange = 16 });
        unit.AddComponent(new GunToHitComponent { GunClass = GunClass.AntiTank });
        unit.AddComponent(new ManhandlingComponent { ManhandlingNumber = 8, HasQuickSetUp = true });
        unit.AddComponent(new LimberedComponent { LimberedPortageCost = 30 });
        unit.AddComponent(new InherentFirepowerComponent { InherentFirepower = 6 });
        unit.AddComponent(new AntiAircraftComponent { IsAACapable = true, AAModifier = -1 });

        Assert.Equal(75, unit.Ordnance?.Caliber);
        Assert.Equal(GunClass.AntiTank, unit.GunToHit?.GunClass);
        Assert.Equal(8, unit.Manhandling?.ManhandlingNumber);
        Assert.True(unit.Manhandling?.HasQuickSetUp);
        Assert.Equal(30, unit.Limbered?.LimberedPortageCost);
        Assert.Equal(6, unit.InherentFirepower?.InherentFirepower);
        Assert.Equal(-1, unit.AntiAircraft?.AAModifier);
    }

    [Fact]
    public void AddComponent_DuplicateGunToHit_Throws()
    {
        var unit = new Unit { UnitType = UnitType.Ordnance };
        unit.AddComponent(new GunToHitComponent { GunClass = GunClass.AntiTank });

        Assert.Throws<InvalidOperationException>(() =>
            unit.AddComponent(new GunToHitComponent { GunClass = GunClass.Howitzer }));
    }

    [Fact]
    public void SerializeDeserialize_PreservesAtGunComposition()
    {
        var unit = new Unit { Name = "57L AT", UnitType = UnitType.Ordnance };
        unit.AddComponent(new OrdnanceComponent
        {
            Caliber = 57,
            MuzzleType = MuzzleType.LongBarrel,
            TargettingType = TargettingType.DirectFire,
            MaxRange = 24,
        });
        unit.AddComponent(new FirePowerComponent { Firepower = 12, RateOfFire = 2 });
        unit.AddComponent(new BreakdownComponent { BreakdownNumber = 12, RemovalNumber = 11, RepairNumber = 3 });
        unit.AddComponent(new BPVComponent { BPV = 22 });
        unit.AddComponent(new GunToHitComponent { GunClass = GunClass.AntiTank });
        unit.AddComponent(new ManhandlingComponent { ManhandlingNumber = 10 });
        unit.AddComponent(new LimberedComponent { LimberedPortageCost = 28 });

        var project = new ASLProject { Counters = { unit } };
        var loaded = _manager.DeserializeProject(_manager.SerializeProject(project));

        Assert.NotNull(loaded);
        var reloaded = loaded!.Counters[0];
        Assert.Equal(UnitType.Ordnance, reloaded.UnitType);
        Assert.Equal(57, reloaded.Ordnance?.Caliber);
        Assert.Equal(MuzzleType.LongBarrel, reloaded.Ordnance?.MuzzleType);
        Assert.Equal(GunClass.AntiTank, reloaded.GunToHit?.GunClass);
        Assert.Equal(10, reloaded.Manhandling?.ManhandlingNumber);
        Assert.Equal(28, reloaded.Limbered?.LimberedPortageCost);
        Assert.Null(reloaded.InherentFirepower);
        Assert.Null(reloaded.AntiAircraft);
    }

    [Fact]
    public void SerializeDeserialize_PreservesMortarComposition()
    {
        var unit = new Unit { Name = "81mm MTR", UnitType = UnitType.Ordnance };
        unit.AddComponent(new OrdnanceComponent
        {
            Caliber = 81,
            TargettingType = TargettingType.IndirectFire,
            MinRange = 2,
            MaxRange = 23,
        });
        unit.AddComponent(new FirePowerComponent { Firepower = 20 });
        unit.AddComponent(new BreakdownComponent { BreakdownNumber = 11, RemovalNumber = 12, RepairNumber = 3 });
        unit.AddComponent(new BPVComponent { BPV = 12 });
        unit.AddComponent(new PortageComponent { AssembledCost = 15, DismantledCost = 8 });
        unit.AddComponent(new InherentFirepowerComponent { InherentFirepower = 2 });

        var project = new ASLProject { Counters = { unit } };
        var loaded = _manager.DeserializeProject(_manager.SerializeProject(project));

        Assert.NotNull(loaded);
        var reloaded = loaded!.Counters[0];
        Assert.Equal(2, reloaded.Ordnance?.MinRange);
        Assert.Equal(23, reloaded.Ordnance?.MaxRange);
        Assert.Equal(2, reloaded.InherentFirepower?.InherentFirepower);
        Assert.Null(reloaded.GunToHit);
        Assert.Null(reloaded.Manhandling);
        Assert.Null(reloaded.Limbered);
    }

    [Fact]
    public void SerializeDeserialize_PreservesAaGunComposition()
    {
        var unit = new Unit { Name = "Flak 36", UnitType = UnitType.Ordnance };
        unit.AddComponent(new OrdnanceComponent
        {
            Caliber = 88,
            MuzzleType = MuzzleType.LongBarrel,
            TargettingType = TargettingType.DirectFire,
            MaxRange = 32,
        });
        unit.AddComponent(new FirePowerComponent { Firepower = 16, RateOfFire = 2 });
        unit.AddComponent(new BreakdownComponent { BreakdownNumber = 12, RemovalNumber = 11, RepairNumber = 3 });
        unit.AddComponent(new BPVComponent { BPV = 50 });
        unit.AddComponent(new GunToHitComponent { GunClass = GunClass.AntiAircraft });
        unit.AddComponent(new ManhandlingComponent { ManhandlingNumber = 14 });
        unit.AddComponent(new LimberedComponent { LimberedPortageCost = 60 });
        unit.AddComponent(new AntiAircraftComponent { IsAACapable = true, IsAAOnly = false, AAModifier = -2 });

        var project = new ASLProject { Counters = { unit } };
        var loaded = _manager.DeserializeProject(_manager.SerializeProject(project));

        Assert.NotNull(loaded);
        var reloaded = loaded!.Counters[0];
        Assert.Equal(GunClass.AntiAircraft, reloaded.GunToHit?.GunClass);
        Assert.True(reloaded.AntiAircraft?.IsAACapable);
        Assert.False(reloaded.AntiAircraft?.IsAAOnly);
        Assert.Equal(-2, reloaded.AntiAircraft?.AAModifier);
    }

    [Fact]
    public void GunToHitComponent_PrivateTableRoundTrips()
    {
        var unit = new Unit { UnitType = UnitType.Ordnance };
        unit.AddComponent(new GunToHitComponent
        {
            GunClass = GunClass.InfantryGun,
            CanAcquire = false,
            PrivateToHitTable = new Dictionary<int, int> { { 1, 10 }, { 2, 9 }, { 3, 8 } },
        });
        var project = new ASLProject { Counters = { unit } };

        var loaded = _manager.DeserializeProject(_manager.SerializeProject(project));

        var th = loaded!.Counters[0].GunToHit;
        Assert.NotNull(th);
        Assert.False(th!.CanAcquire);
        Assert.Equal(10, th.PrivateToHitTable![1]);
        Assert.Equal(9, th.PrivateToHitTable[2]);
        Assert.Equal(8, th.PrivateToHitTable[3]);
    }
}
