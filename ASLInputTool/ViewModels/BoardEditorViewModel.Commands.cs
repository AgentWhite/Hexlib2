using System;
using System.Linq;
using System.Windows;
using ASL;
using ASL.Core;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Infrastructure;
using ASL.Services;
using HexLib;
using ASLInputTool.Infrastructure;
using System.Windows.Input;
using System.Windows.Media;

namespace ASLInputTool.ViewModels;

public partial class BoardEditorViewModel
{
    private Brush GetBrushForTerrain(TerrainType terrain)
    {
        switch (terrain)
        {
            case TerrainType.Woods:
                return new SolidColorBrush(Color.FromRgb(34, 139, 34)); // Forest Green
            case TerrainType.StoneBuilding:
                return Brushes.DimGray;
            default:
                return Brushes.LightGray;
        }
    }

    private void HandlePenRectClick(Point p)
    {
        if (!_isPenDrawing)
        {
            // First click: Start drawing
            _penStartPoint = p;
            IsPenDrawing = true;
            PenGhostGeometry = null;
            
            // Clear selection when drawing starts
            foreach (var d in _customTerrainDrawings) d.IsSelected = false;
            return;
        }

        // Second click: Finalize
        if (_penStartPoint.HasValue)
        {
            var rect = new Rect(_penStartPoint.Value, p);
            var newGeometry = new RectangleGeometry(rect);

            var existingDrawing = _customTerrainDrawings.FirstOrDefault(d => d.TerrainType == ActivePenTerrain);
            if (existingDrawing != null)
            {
                // Union with existing
                existingDrawing.Geometry = Geometry.Combine(existingDrawing.Geometry, newGeometry, GeometryCombineMode.Union, null);
            }
            else
            {
                // Create new
                var drawing = new TerrainDrawingViewModel
                {
                    TerrainType = ActivePenTerrain,
                    Geometry = newGeometry,
                    Fill = GetBrushForTerrain(ActivePenTerrain)
                };
                _customTerrainDrawings.Add(drawing);
            }
        }

        IsPenDrawing = false;
        _penStartPoint = null;
        PenGhostGeometry = null;
    }

    private void HandlePenRectHover(Point p)
    {
        if (IsPenDrawing && _penStartPoint.HasValue)
        {
            PenGhostGeometry = new RectangleGeometry(new Rect(_penStartPoint.Value, p));
        }
    }

    private void HandlePolygonClick(Point p)
    {
        if (IsPolygonSnapped && _activePolygonPoints.Count >= 3)
        {
            ClosePolygon();
            return;
        }

        _activePolygonPoints.Add(p);
        
        // Initial ghost
        HandlePolygonHover(p);
    }

    private void HandlePolygonHover(Point p)
    {
        if (_activePolygonPoints.Count == 0)
        {
            PolygonGhostGeometry = null;
            IsPolygonSnapped = false;
            return;
        }

        Point startPoint = _activePolygonPoints[0];
        Point lastPoint = _activePolygonPoints[^1];
        
        // Check for snapping (ignore if only 1 point to avoid instant close)
        double distToStart = (p - startPoint).Length;
        const double SnapThreshold = 15;
        
        if (distToStart < SnapThreshold && _activePolygonPoints.Count >= 2)
        {
            IsPolygonSnapped = true;
            PolygonSnapPoint = startPoint;
            p = startPoint; // Snap mouse point for ghosting
        }
        else
        {
            IsPolygonSnapped = false;
        }

        // Build preview geometry
        // 1. Solid segments
        var solidGeometry = new StreamGeometry();
        using (var ctx = solidGeometry.Open())
        {
            ctx.BeginFigure(startPoint, false, false);
            if (_activePolygonPoints.Count > 1)
            {
                ctx.PolyLineTo(_activePolygonPoints.Skip(1).ToList(), true, true);
            }
        }

        // 2. Dashed current segment
        var dashedGeometry = new LineGeometry(lastPoint, p);

        // Combine for rendering (using a Group so they can have different stroke styles in XAML if needed, 
        // but here we combine them into one for the simple property)
        var group = new GeometryGroup();
        group.Children.Add(solidGeometry);
        group.Children.Add(dashedGeometry);
        
        PolygonGhostGeometry = group;
    }

    private void ClosePolygon()
    {
        if (_activePolygonPoints.Count < 3)
        {
            _activePolygonPoints.Clear();
            PolygonGhostGeometry = null;
            IsPolygonSnapped = false;
            return;
        }

        // Create finalized geometry
        var pathGeometry = new PathGeometry();
        var figure = new PathFigure { StartPoint = _activePolygonPoints[0], IsClosed = true, IsFilled = true };
        figure.Segments.Add(new PolyLineSegment(_activePolygonPoints.Skip(1), true));
        pathGeometry.Figures.Add(figure);

        // Merge with existing terrain
        var existingDrawing = _customTerrainDrawings.FirstOrDefault(d => d.TerrainType == ActivePenTerrain);
        if (existingDrawing != null)
        {
            var mode = CurrentTool == ToolMode.PenSubtract ? GeometryCombineMode.Exclude : GeometryCombineMode.Union;
            existingDrawing.Geometry = Geometry.Combine(existingDrawing.Geometry, pathGeometry, mode, null);
        }
        else
        {
            var drawing = new TerrainDrawingViewModel
            {
                TerrainType = ActivePenTerrain,
                Geometry = pathGeometry,
                Fill = GetBrushForTerrain(ActivePenTerrain)
            };
            _customTerrainDrawings.Add(drawing);
        }

        // Cleanup
        _activePolygonPoints.Clear();
        PolygonGhostGeometry = null;
        IsPolygonSnapped = false;
    }

    private void OnSelectDrawing(TerrainDrawingViewModel drawing)
    {
        if (CurrentTool != ToolMode.PenSelect) return;

        bool wasSelected = drawing.IsSelected;
        
        // Exclusive selection
        foreach (var d in _customTerrainDrawings) d.IsSelected = false;
        
        drawing.IsSelected = !wasSelected;
    }

    private void OnDeleteSelectedDrawing()
    {
        var toRemove = _customTerrainDrawings.Where(d => d.IsSelected).ToList();
        foreach (var d in toRemove)
        {
            _customTerrainDrawings.Remove(d);
        }
    }

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
        else if (CurrentTool == ToolMode.Road)
        {
            HandleRoadToolClick(hex);
        }
        else if (CurrentTool == ToolMode.LosTest)
        {
            HandleLosToolClick(hex);
        }
    }

    private void HandleLosToolClick(HexViewModel hex)
    {
        if (!_isLosPlacing)
        {
            // First click: Start placing
            ClearLos();
            _losStartHex = hex;
            _isLosPlacing = true;
            
            // Set initial line state
            LosLineX1 = hex.CenterX;
            LosLineY1 = hex.CenterY;
            LosLineX2 = hex.CenterX;
            LosLineY2 = hex.CenterY;
            IsLosLineVisible = true;
            
            // Highlight the start hex as feedback
            hex.IsHighlightedForLos = true;
            return;
        }

        // Second click: Finish placing
        _losEndHex = hex;
        _isLosPlacing = false;
        
        // Finalize LOS line
        LosLineX2 = _losEndHex.CenterX;
        LosLineY2 = _losEndHex.CenterY;

        // Calculate traversed hexes
        var path = HexMath.GetLine(_losStartHex.Location, _losEndHex.Location);
        
        // Highlight hexes
        foreach (var h in _hexes)
        {
            h.IsHighlightedForLos = path.Any(p => p.Equals(h.Location));
        }
    }

    private void OnHexHover(HexViewModel hex)
    {
        if (CurrentTool == ToolMode.LosTest && _isLosPlacing && _losStartHex != null)
        {
            // Update ghost line
            LosLineX2 = hex.CenterX;
            LosLineY2 = hex.CenterY;
            
            // Optional: Preview highlight? Might be too noisy.
            // For now just the line.
        }
    }

    private void ClearLos()
    {
        IsLosLineVisible = false;
        foreach (var h in _hexes)
        {
            h.IsHighlightedForLos = false;
        }
        _losStartHex = null;
        _losEndHex = null;
    }

    private void HandleRoadToolClick(HexViewModel hex)
    {
        if (RoadStartHex == null)
        {
            RoadStartHex = hex;
            return;
        }

        int edgeIndex = GetNeighborEdgeIndex(RoadStartHex, hex);
        if (edgeIndex != -1)
        {
            CommitRoadEdge(RoadStartHex, hex, edgeIndex);
            RoadStartHex = hex;
            RoadPreviewVisuals.Clear();
        }
        else
        {
            RoadStartHex = hex;
            RoadPreviewVisuals.Clear();
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
        if (hex == null) return;
        
        if (CurrentTool == ToolMode.Paint)
        {
            // Only paint if the left mouse button is down (for MouseEnter case)
            if (System.Windows.Input.Mouse.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                PaintHex(hex);
            }
        }
        else if (CurrentTool == ToolMode.Road)
        {
            UpdateRoadPreview(hex);
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

    private int GetNeighborEdgeIndex(HexViewModel from, HexViewModel target)
    {
        for (int i = 0; i < 6; i++)
        {
            var neighborLoc = from.Location.GetNeighbor(i);
            if (neighborLoc.Equals(target.Location))
            {
                // HexLib direction to Visual edge index mapping
                return i switch
                {
                    0 => 0, // SE
                    5 => 1, // S
                    4 => 2, // SW
                    3 => 3, // NW
                    2 => 4, // N
                    1 => 5, // NE
                    _ => -1
                };
            }
        }
        return -1;
    }

    private void CommitRoadEdge(HexViewModel from, HexViewModel to, int edgeIndex)
    {
        // Get or create edge data
        var edgeData = _board.Board.GetEdgeData(from.Location, to.Location);
        if (edgeData == null)
        {
            edgeData = new ASLEdgeData();
            _board.Board.SetEdgeData(from.Location, to.Location, edgeData);
        }

        // Apply selected road type
        switch (ActiveRoadType)
        {
            case RoadToolType.Paved:
                edgeData.HasPavedRoad = true;
                break;
            case RoadToolType.Dirt:
                edgeData.HasDirtRoad = true;
                break;
            case RoadToolType.Path:
                edgeData.HasPath = true;
                break;
        }

        RefreshAllVisuals();
    }

    private async void OnSave(object? parameter)
    {
        if (_repository == null) return;
        try
        {
            await _repository.SaveToDiskAsync(_board, _backgroundImagePath, _originalName);
            await SaveLosData();
            MessageBox.Show($"Board '{_board.Name}' saved successfully.", "Save Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save board: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async System.Threading.Tasks.Task SaveLosData()
    {
        try
        {
            string baseFolder = ASLInputTool.Infrastructure.SettingsManager.Instance.Settings.BoardsFolder;
            string boardFolder = System.IO.Path.Combine(baseFolder, _board.Name);
            string losFilePath = System.IO.Path.Combine(boardFolder, $"{_board.Name}.los.json");

            var dto = new ASL.Persistence.LosDataDto
            {
                Drawings = _customTerrainDrawings.Select(d => d.ToDto()).ToList()
            };

            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(dto, options);
            await System.IO.File.WriteAllTextAsync(losFilePath, json);
        }
        catch { /* Ignore non-critical save failures */ }
    }

    private async System.Threading.Tasks.Task LoadLosData()
    {
        try
        {
            _customTerrainDrawings.Clear();
            string baseFolder = ASLInputTool.Infrastructure.SettingsManager.Instance.Settings.BoardsFolder;
            string boardFolder = System.IO.Path.Combine(baseFolder, _board.Name);
            string losFilePath = System.IO.Path.Combine(boardFolder, $"{_board.Name}.los.json");

            if (System.IO.File.Exists(losFilePath))
            {
                string json = await System.IO.File.ReadAllTextAsync(losFilePath);
                var dto = System.Text.Json.JsonSerializer.Deserialize<ASL.Persistence.LosDataDto>(json);
                if (dto != null)
                {
                    foreach (var drawingDto in dto.Drawings)
                    {
                        var vm = TerrainDrawingViewModel.FromDto(drawingDto, GetBrushForTerrain(drawingDto.TerrainType));
                        _customTerrainDrawings.Add(vm);
                    }
                    // Notify that the collection has been populated
                    OnPropertyChanged(nameof(CustomTerrainDrawings));
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load LOS data: {ex.Message}");
        }
    }

    private ICommand? _clearLosCommand;
    public ICommand ClearLosCommand => _clearLosCommand ??= new RelayCommand(_ => ClearLos());

    private ICommand? _hoverHexCommand;
    public ICommand HoverHexCommand => _hoverHexCommand ??= new RelayCommand<HexViewModel>(OnHexHover);
}
