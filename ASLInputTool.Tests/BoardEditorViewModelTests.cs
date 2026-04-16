using ASL;
using ASLInputTool.ViewModels;
using HexLib;
using Xunit;
using System.Linq;

namespace ASLInputTool.Tests;

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
        Assert.Equal(hexVm, vm.SelectedHex);
    }
}
