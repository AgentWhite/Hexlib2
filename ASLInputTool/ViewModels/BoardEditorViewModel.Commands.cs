using System;
using System.Linq;
using System.Windows;
using ASL;
using HexLib;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

public partial class BoardEditorViewModel
{
    private void OnSelectHex(HexViewModel? hex)
    {
        if (hex == null) return;

        if (CurrentTool == ToolMode.Select)
        {
            SelectedHex = hex;
        }
        else if (CurrentTool == ToolMode.Paint)
        {
            PaintHex(hex);
        }
    }

    private void ClearAllEdgeVisuals()
    {
        foreach (var h in _hexes)
        {
            h.IsEdge0Selected = false;
            h.IsEdge1Selected = false;
            h.IsEdge2Selected = false;
            h.IsEdge3Selected = false;
            h.IsEdge4Selected = false;
            h.IsEdge5Selected = false;
        }
    }

    private void OnSelectEdge(HexViewModel hex, int edgeIndex)
    {
        if (CurrentTool != ToolMode.Select) return;

        // Apply visual selection to this exact edge
        SetEdgeVisualSelection(hex, edgeIndex, true);

        // Map visual edgeIndex to CubeCoordinate direction index
        int directionIndex = edgeIndex switch
        {
            0 => 0, // SE
            1 => 5, // S
            2 => 4, // SW
            3 => 3, // NW
            4 => 2, // N
            5 => 1, // NE
            _ => 0
        };

        var neighborLoc = hex.Location.GetNeighbor(directionIndex);
        var data = _board.Board.GetEdgeData(hex.Location, neighborLoc);
        
        if (data == null)
        {
            data = new ASLEdgeData();
            _board.Board.SetEdgeData(hex.Location, neighborLoc, data);
        }

        var neighborHexVm = _hexes.FirstOrDefault(h => h.Location.Equals(neighborLoc));
        var isHouseAvailable = IsBuildingTerrain(hex.Terrain) && 
                              neighborHexVm != null && 
                              IsBuildingTerrain(neighborHexVm.Terrain);

        SelectedEdge = new HexEdgeSelection(hex, edgeIndex, new HexsideViewModel(data, RefreshAllVisuals, isHouseAvailable));
    }

    private void SetEdgeVisualSelection(HexViewModel hex, int edgeIndex, bool isSelected)
    {
        switch (edgeIndex)
        {
            case 0: hex.IsEdge0Selected = isSelected; break;
            case 1: hex.IsEdge1Selected = isSelected; break;
            case 2: hex.IsEdge2Selected = isSelected; break;
            case 3: hex.IsEdge3Selected = isSelected; break;
            case 4: hex.IsEdge4Selected = isSelected; break;
            case 5: hex.IsEdge5Selected = isSelected; break;
        }
    }

    private void OnPaintHex(HexViewModel? hex)
    {
        if (hex == null || CurrentTool != ToolMode.Paint) return;
        
        // Only paint if the left mouse button is down (for MouseEnter case)
        if (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
        {
            PaintHex(hex);
        }
    }

    /// <summary>
    /// Paints the specified hex with the active terrain.
    /// </summary>
    /// <param name="hex">The hex to paint.</param>
    public void PaintHex(HexViewModel hex)
    {
        hex.Terrain = ActiveTerrain;
        UpdateBuildingVisuals();
    }

    private async void OnSave(object? parameter)
    {
        if (_repository == null) return;
        try
        {
            await _repository.SaveToDiskAsync(_board, _backgroundImagePath, _originalName);
            MessageBox.Show($"Board '{_board.Name}' saved successfully.", "Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save board: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
