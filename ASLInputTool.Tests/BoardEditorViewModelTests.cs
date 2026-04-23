using ASL;
using ASL.Core;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Infrastructure;
using ASL.Services;
using ASLInputTool.ViewModels;
using HexLib;
using Xunit;
using System.Linq;

namespace ASLInputTool.Tests;
using ASLInputTool.Tests.Fixtures;

[Collection("SettingsManager")]
public class BoardEditorViewModelTests
{
    [Fact]
    public void BoardEditorViewModel_PaintHex_UpdatesUnderlyingMetadata()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10, HexTopOrientation.FlatTopped);
        var origin = new CubeCoordinate(0, 0, 0);
        var hex = new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround } };
        board.AddHex(hex);
        
        var aslBoard = new AslBoard("Test Board") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        
        var hexVm = vm.Hexes.First(h => h.Location.Equals(origin));
        
        vm.CurrentTool = ToolMode.Select;
        vm.ActiveTerrain = TerrainType.StoneBuilding;
        
        vm.PaintHex(hexVm); // Uses ActiveTerrain internally
        
        Assert.Equal(TerrainType.StoneBuilding, hexVm.Terrain);
        Assert.Equal(TerrainType.StoneBuilding, hex.Metadata.Terrain);
    }

    [Fact]
    public void BoardEditorViewModel_RecalculateHexSize_DoesNotProduceNaN()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10, HexTopOrientation.FlatTopped);
        var aslBoard = new AslBoard("Test Board") { Board = board };
        
        // Very small canvas
        aslBoard.CanvasWidth = 10;
        aslBoard.CanvasHeight = 10;
        
        var vm = new BoardEditorViewModel(aslBoard);
        
        Assert.False(double.IsNaN(vm.HexSize));
        Assert.False(double.IsInfinity(vm.HexSize));
        Assert.True(vm.HexSize > 0);
    }

    [Fact]
    public void BoardEditorViewModel_ToolSwitching_ResetsSelections()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(10, 10, HexTopOrientation.FlatTopped);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Id = "A1", Metadata = new ASLHexMetadata() });
        
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        
        vm.CurrentTool = ToolMode.Select;
        var hexVm = vm.Hexes.First(h => h.Location.Equals(origin));
        
        // Selecting a hex via the command
        vm.SelectHexCommand.Execute(hexVm);
        
        Assert.Equal(hexVm, vm.SelectedHex);
        Assert.True(hexVm.IsSelected);
        
        // Switch tool should not necessarily clear selection, but let's see how it behaves
        vm.CurrentTool = ToolMode.Paint;
        // Selection should persist or be handled based on UI requirements
        Assert.Null(vm.SelectedHex);
    }

    [Fact]
    public void HexViewModel_CragTerrain_SetsIsCrag()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(1, 1);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        
        var hexVm = vm.Hexes.First();
        
        hexVm.Terrain = TerrainType.Crag;
        Assert.True(hexVm.IsCrag);
        
        hexVm.Terrain = TerrainType.Woods;
        Assert.False(hexVm.IsCrag);
    }

    [Fact]
    public void HexViewModel_Shellholes_SetsProperty()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(1, 1);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        
        var hexVm = vm.Hexes.First();
        
        hexVm.HasShellholes = true;
        Assert.True(hexVm.HasShellholes);
        
        hexVm.HasShellholes = false;
        Assert.False(hexVm.HasShellholes);
    }

    [Fact]
    public void HexViewModel_PondTerrain_SetsIsPond()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(1, 1);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);

        var hexVm = vm.Hexes.First();

        hexVm.Terrain = TerrainType.Pond;
        Assert.True(hexVm.IsPond);

        hexVm.Terrain = TerrainType.Woods;
        Assert.False(hexVm.IsPond);
    }

    // ── Elevation ─────────────────────────────────────────────────────────────

    [Fact]
    public void PaintHex_SetsElevation_UpdatesIsElevated()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(1, 1);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        var hexVm = vm.Hexes.First();

        hexVm.Elevation = 2;

        Assert.True(hexVm.IsElevated);
        Assert.Equal(2, hexVm.Elevation);
    }

    [Fact]
    public void Elevation_SetToZero_IsElevatedFalse()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(1, 1);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        var hexVm = vm.Hexes.First();

        hexVm.Elevation = 3;
        hexVm.Elevation = 0;

        Assert.False(hexVm.IsElevated);
    }

    // ── Edge selection ────────────────────────────────────────────────────────

    [Fact]
    public void SelectMode_SelectEdge_SetsSelectedEdge()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(1, 1);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        var hexVm = vm.Hexes.First();

        vm.CurrentTool = ToolMode.Select;
        hexVm.SelectEdgeCommand.Execute("2");

        Assert.NotNull(vm.SelectedEdge);
        Assert.Equal(2, vm.SelectedEdge!.EdgeIndex);
        Assert.Same(hexVm, vm.SelectedEdge.Hex);
    }

    [Fact]
    public void SelectMode_SelectEdge_MarksEdgeVisualSelected()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(1, 1);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        var hexVm = vm.Hexes.First();

        vm.CurrentTool = ToolMode.Select;
        hexVm.SelectEdgeCommand.Execute("1");

        Assert.True(hexVm.IsEdge1Selected);
    }

    // ── Road tool ─────────────────────────────────────────────────────────────

    [Fact]
    public void RoadTool_FirstClick_SetsRoadStartHex()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(2, 2);
        var origin = HexMath.OffsetToCube(0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        var hexVm = vm.Hexes.First();

        vm.CurrentTool = ToolMode.Road;
        vm.SelectHexCommand.Execute(hexVm);

        Assert.Same(hexVm, vm.RoadStartHex);
    }

    [Fact]
    public void RoadTool_TwoAdjacentHexes_PavedRoad_SetsEdgeData()
    {
        // OffsetToCube(0,0) and OffsetToCube(1,0) are neighbors in flat-top layout
        var board = new Board<ASLHexMetadata, ASLEdgeData>(2, 2, HexTopOrientation.FlatTopped);
        var locA = HexMath.OffsetToCube(0, 0);
        var locB = HexMath.OffsetToCube(1, 0);
        board.AddHex(new Hex<ASLHexMetadata>(locA) { Metadata = new ASLHexMetadata() });
        board.AddHex(new Hex<ASLHexMetadata>(locB) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);

        var hexA = vm.Hexes.First(h => h.Location.Equals(locA));
        var hexB = vm.Hexes.First(h => h.Location.Equals(locB));

        vm.CurrentTool = ToolMode.Road;
        vm.ActiveRoadType = RoadToolType.Paved;
        vm.SelectHexCommand.Execute(hexA);
        vm.SelectHexCommand.Execute(hexB);

        var edgeData = board.GetEdgeData(locA, locB);
        Assert.NotNull(edgeData);
        Assert.True(edgeData!.HasPavedRoad);
    }

    [Fact]
    public void RoadTool_TwoAdjacentHexes_DirtRoad_SetsEdgeData()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(2, 2, HexTopOrientation.FlatTopped);
        var locA = HexMath.OffsetToCube(0, 0);
        var locB = HexMath.OffsetToCube(1, 0);
        board.AddHex(new Hex<ASLHexMetadata>(locA) { Metadata = new ASLHexMetadata() });
        board.AddHex(new Hex<ASLHexMetadata>(locB) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);

        var hexA = vm.Hexes.First(h => h.Location.Equals(locA));
        var hexB = vm.Hexes.First(h => h.Location.Equals(locB));

        vm.CurrentTool = ToolMode.Road;
        vm.ActiveRoadType = RoadToolType.Dirt;
        vm.SelectHexCommand.Execute(hexA);
        vm.SelectHexCommand.Execute(hexB);

        var edgeData = board.GetEdgeData(locA, locB);
        Assert.NotNull(edgeData);
        Assert.True(edgeData!.HasDirtRoad);
    }

    // ── LOS tool ──────────────────────────────────────────────────────────────

    [Fact]
    public void LosTool_FirstClick_ShowsLosLine()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(1, 1);
        var origin = new CubeCoordinate(0, 0, 0);
        board.AddHex(new Hex<ASLHexMetadata>(origin) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);
        var hexVm = vm.Hexes.First();

        vm.CurrentTool = ToolMode.LosTest;
        vm.SelectHexCommand.Execute(hexVm);

        Assert.True(vm.IsLosLineVisible);
        Assert.True(hexVm.IsHighlightedForLos);
    }

    [Fact]
    public void LosTool_TwoClicks_HighlightsPath()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(2, 2, HexTopOrientation.FlatTopped);
        var locA = HexMath.OffsetToCube(0, 0);
        var locB = HexMath.OffsetToCube(1, 0);
        board.AddHex(new Hex<ASLHexMetadata>(locA) { Metadata = new ASLHexMetadata() });
        board.AddHex(new Hex<ASLHexMetadata>(locB) { Metadata = new ASLHexMetadata() });
        var aslBoard = new AslBoard("Test") { Board = board };
        var vm = new BoardEditorViewModel(aslBoard);

        var hexA = vm.Hexes.First(h => h.Location.Equals(locA));
        var hexB = vm.Hexes.First(h => h.Location.Equals(locB));

        vm.CurrentTool = ToolMode.LosTest;
        vm.SelectHexCommand.Execute(hexA);
        vm.SelectHexCommand.Execute(hexB);

        Assert.True(hexA.IsHighlightedForLos || hexB.IsHighlightedForLos);
    }
}
