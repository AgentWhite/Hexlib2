using ASL.Models.Board;
using ASLInputTool.ViewModels;
using ASLInputTool.Tests.Fixtures;
using HexLib;
using System.Collections.Generic;
using System.Windows;

namespace ASLInputTool.Tests;

[Collection("SettingsManager")]
public class HexViewModelTests
{
    private static HexViewModel MakeHex(
        ASLHexMetadata? metadata = null,
        Action<HexViewModel, int>? onSelectEdge = null)
    {
        var coord = HexMath.OffsetToCube(2, 3);
        var corners = new Point[6];
        return new HexViewModel(
            col: 2, row: 3,
            location: coord,
            points: "M 0,0 Z",
            corners: corners,
            id: "C3",
            cx: 50.0, cy: 60.0, ly: 55.0,
            size: 30.0,
            metadata: metadata ?? new ASLHexMetadata(),
            onSelectEdge: onSelectEdge ?? ((_, _) => { })
        );
    }

    // ── Construction ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SetsProperties()
    {
        var vm = MakeHex();

        Assert.Equal(2, vm.Column);
        Assert.Equal(3, vm.Row);
        Assert.Equal("C3", vm.Id);
        Assert.Equal(50.0, vm.CenterX);
        Assert.Equal(60.0, vm.CenterY);
        Assert.Equal(30.0, vm.HexSize);
        Assert.False(string.IsNullOrEmpty(vm.InnerPoints));
    }

    // ── IsSelected / Edge selection ───────────────────────────────────────────

    [Fact]
    public void IsSelected_PropertyChanged_Fires()
    {
        var vm = MakeHex();
        var fired = new List<string?>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        vm.IsSelected = true;

        Assert.Contains(nameof(HexViewModel.IsSelected), fired);
    }

    [Theory]
    [InlineData(nameof(HexViewModel.IsEdge0Selected))]
    [InlineData(nameof(HexViewModel.IsEdge1Selected))]
    [InlineData(nameof(HexViewModel.IsEdge2Selected))]
    [InlineData(nameof(HexViewModel.IsEdge3Selected))]
    [InlineData(nameof(HexViewModel.IsEdge4Selected))]
    [InlineData(nameof(HexViewModel.IsEdge5Selected))]
    public void EdgeSelected_PropertyChanged_Fires(string propertyName)
    {
        var vm = MakeHex();
        var fired = new List<string?>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        typeof(HexViewModel).GetProperty(propertyName)!.SetValue(vm, true);

        Assert.Contains(propertyName, fired);
    }

    // ── SelectEdgeCommand ─────────────────────────────────────────────────────

    [Fact]
    public void SelectEdgeCommand_ValidIndex_InvokesCallback()
    {
        HexViewModel? callbackHex = null;
        int callbackEdge = -1;
        var vm = MakeHex(onSelectEdge: (h, e) => { callbackHex = h; callbackEdge = e; });

        vm.SelectEdgeCommand.Execute("3");

        Assert.Same(vm, callbackHex);
        Assert.Equal(3, callbackEdge);
    }

    [Fact]
    public void SelectEdgeCommand_InvalidParam_DoesNotThrow()
    {
        var vm = MakeHex();
        var ex = Record.Exception(() => vm.SelectEdgeCommand.Execute("not-an-int"));
        Assert.Null(ex);
    }

    // ── Terrain ───────────────────────────────────────────────────────────────

    [Fact]
    public void SetTerrain_FiresTerrainRelatedNotifications()
    {
        var vm = MakeHex();
        var fired = new List<string?>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        vm.Terrain = TerrainType.Woods;

        Assert.Contains(nameof(HexViewModel.Terrain), fired);
        Assert.Contains(nameof(HexViewModel.BuildingSquareVisibility), fired);
    }

    [Fact]
    public void SetTerrain_Building_BuildingSquareIsVisible()
    {
        var vm = MakeHex();
        vm.Terrain = TerrainType.StoneBuilding;

        Assert.Equal(Visibility.Visible, vm.BuildingSquareVisibility);
    }

    [Fact]
    public void SetTerrain_NonBuilding_BuildingSquareIsCollapsed()
    {
        var vm = MakeHex();
        vm.Terrain = TerrainType.Woods;

        Assert.Equal(Visibility.Collapsed, vm.BuildingSquareVisibility);
    }

    [Fact]
    public void SetTerrain_ToNonBuilding_ClearsStairwell()
    {
        var vm = MakeHex();
        vm.Terrain = TerrainType.StoneBuilding;
        vm.HasStairwell = true;

        vm.Terrain = TerrainType.Woods;

        Assert.False(vm.HasStairwell);
    }

    [Fact]
    public void SetTerrain_InvokesOnTerrainChangedCallback()
    {
        var vm = MakeHex();
        bool called = false;
        vm.OnTerrainChanged = () => called = true;

        vm.Terrain = TerrainType.Marsh;

        Assert.True(called);
    }

    [Fact]
    public void SetTerrain_SameValue_DoesNotFireEvent()
    {
        var vm = MakeHex(); // default is OpenGround
        var fired = new List<string?>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        vm.Terrain = TerrainType.OpenGround; // same value, should not fire

        Assert.Empty(fired);
    }

    // ── Computed terrain flags ────────────────────────────────────────────────

    [Theory]
    [InlineData(TerrainType.Graveyard, nameof(HexViewModel.IsGraveyard))]
    [InlineData(TerrainType.Crag, nameof(HexViewModel.IsCrag))]
    [InlineData(TerrainType.Pond, nameof(HexViewModel.IsPond))]
    [InlineData(TerrainType.Lumberyard, nameof(HexViewModel.IsLumberyard))]
    public void TerrainFlags_CorrectForMatchingTerrain(TerrainType terrain, string flagProperty)
    {
        var vm = MakeHex();
        vm.Terrain = terrain;

        bool flag = (bool)typeof(HexViewModel).GetProperty(flagProperty)!.GetValue(vm)!;
        Assert.True(flag);
    }

    // ── Elevation ─────────────────────────────────────────────────────────────

    [Fact]
    public void SetElevation_FiresPropertyAndIsElevated()
    {
        var vm = MakeHex();
        var fired = new List<string?>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName);

        vm.Elevation = 2;

        Assert.Contains(nameof(HexViewModel.Elevation), fired);
        Assert.Contains(nameof(HexViewModel.IsElevated), fired);
        Assert.True(vm.IsElevated);
    }

    [Fact]
    public void Elevation_Zero_IsElevatedFalse()
    {
        var vm = MakeHex();
        vm.Elevation = 1;
        vm.Elevation = 0;

        Assert.False(vm.IsElevated);
    }

    // ── Rubble ────────────────────────────────────────────────────────────────

    [Fact]
    public void SetRubble_Stone_IsStoneRubbleTrue()
    {
        var vm = MakeHex();
        vm.Rubble = RubbleType.Stone;

        Assert.True(vm.IsStoneRubble);
        Assert.False(vm.IsWoodenRubble);
    }

    [Fact]
    public void SetRubble_Wooden_IsWoodenRubbleTrue()
    {
        var vm = MakeHex();
        vm.Rubble = RubbleType.Wooden;

        Assert.True(vm.IsWoodenRubble);
        Assert.False(vm.IsStoneRubble);
    }

    // ── LabelLeft ─────────────────────────────────────────────────────────────

    [Fact]
    public void LabelLeft_IsLabelXMinusHalfSize()
    {
        var vm = MakeHex();
        Assert.Equal(vm.LabelX - vm.HexSize * 0.5, vm.LabelLeft);
    }
}

// ─────────────────────────────────────────────────────────────────────────────

[Collection("SettingsManager")]
public class HexsideViewModelTests
{
    private static HexsideViewModel MakeHexside(Action? onChanged = null, bool houseAvailable = true)
        => new HexsideViewModel(new ASLEdgeData(), onChanged, houseAvailable);

    // ── Each property delegates to ASLEdgeData ────────────────────────────────

    [Theory]
    [InlineData(nameof(HexsideViewModel.HasWall))]
    [InlineData(nameof(HexsideViewModel.HasHedge))]
    [InlineData(nameof(HexsideViewModel.HasBocage))]
    [InlineData(nameof(HexsideViewModel.HasPavedRoad))]
    [InlineData(nameof(HexsideViewModel.HasDirtRoad))]
    [InlineData(nameof(HexsideViewModel.HasPath))]
    [InlineData(nameof(HexsideViewModel.HasHouse))]
    [InlineData(nameof(HexsideViewModel.HasStream))]
    [InlineData(nameof(HexsideViewModel.HasGully))]
    [InlineData(nameof(HexsideViewModel.HasCanal))]
    [InlineData(nameof(HexsideViewModel.HasCliff))]
    public void Property_Set_ReflectsInGet(string propertyName)
    {
        var vm = MakeHexside();
        var prop = typeof(HexsideViewModel).GetProperty(propertyName)!;

        prop.SetValue(vm, true);

        Assert.True((bool)prop.GetValue(vm)!);
    }

    [Theory]
    [InlineData(nameof(HexsideViewModel.HasWall))]
    [InlineData(nameof(HexsideViewModel.HasHedge))]
    [InlineData(nameof(HexsideViewModel.HasPavedRoad))]
    [InlineData(nameof(HexsideViewModel.HasStream))]
    [InlineData(nameof(HexsideViewModel.HasCliff))]
    public void Property_Set_FiresOnChangedCallback(string propertyName)
    {
        bool callbackFired = false;
        var vm = MakeHexside(onChanged: () => callbackFired = true);
        var prop = typeof(HexsideViewModel).GetProperty(propertyName)!;

        prop.SetValue(vm, true);

        Assert.True(callbackFired);
    }

    [Theory]
    [InlineData(nameof(HexsideViewModel.HasWall))]
    [InlineData(nameof(HexsideViewModel.HasHedge))]
    [InlineData(nameof(HexsideViewModel.HasStream))]
    public void Property_Set_FiresPropertyChangedEvent(string propertyName)
    {
        var vm = MakeHexside();
        var fired = new List<string?>();
        vm.PropertyChanged += (_, e) => fired.Add(e.PropertyName);
        var prop = typeof(HexsideViewModel).GetProperty(propertyName)!;

        prop.SetValue(vm, true);

        Assert.Contains(propertyName, fired);
    }

    // ── IsRowhouse implies HasHouse ───────────────────────────────────────────

    [Fact]
    public void SetIsRowhouse_True_AlsoSetsHasHouse()
    {
        var vm = MakeHexside();

        vm.IsRowhouse = true;

        Assert.True(vm.HasHouse);
        Assert.True(vm.IsRowhouse);
    }

    [Fact]
    public void SetIsRowhouse_False_DoesNotClearHasHouse()
    {
        var vm = MakeHexside();
        vm.IsRowhouse = true;

        vm.IsRowhouse = false;

        Assert.False(vm.IsRowhouse);
        Assert.True(vm.HasHouse); // HasHouse is not automatically cleared
    }

    // ── IsHouseConnectionAvailable ────────────────────────────────────────────

    [Fact]
    public void IsHouseConnectionAvailable_ReflectsConstructorParam()
    {
        var available = MakeHexside(houseAvailable: true);
        var unavailable = MakeHexside(houseAvailable: false);

        Assert.True(available.IsHouseConnectionAvailable);
        Assert.False(unavailable.IsHouseConnectionAvailable);
    }
}
