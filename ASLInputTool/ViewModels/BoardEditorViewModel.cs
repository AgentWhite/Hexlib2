using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ASL;
using HexLib;
using System.Windows.Media.Imaging;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the board editor canvas and toolbar.
/// </summary>
public partial class BoardEditorViewModel : ViewModelBase
{
    private readonly AslBoard _board;
    private readonly IBoardRepository _repository;
    private double _hexSize = 40.0;
    private HexViewModel? _selectedHex;
    private HexEdgeSelection? _selectedEdge;
    private ToolMode _currentTool = ToolMode.Select;
    private TerrainType _activeTerrain = TerrainType.OpenGround;
    private double _zoomLevel = 1.0;
    private ObservableCollection<HexViewModel> _hexes = new();
    private ObservableCollection<RoadVisualViewModel> _roadVisuals = new();
    private ObservableCollection<RoadVisualViewModel> _waterVisuals = new();
    private ObservableCollection<BuildingVisualBase> _buildingVisuals = new();
    private ObservableCollection<CliffVisualViewModel> _cliffVisuals = new();
    private ObservableCollection<HexsideTerrainVisualViewModel> _hexsideTerrainVisuals = new();
    private bool _isUpdatingVisuals = false;

    /// <summary>
    /// Gets the collection of water visuals (streams, gullies, canals).
    /// </summary>
    public ObservableCollection<RoadVisualViewModel> WaterVisuals => _waterVisuals;

    /// <summary>
    /// Gets the collection of building visual elements for rendering.
    /// </summary>
    public ObservableCollection<BuildingVisualBase> BuildingVisuals
    {
        get => _buildingVisuals;
        set => SetProperty(ref _buildingVisuals, value);
    }

    /// <summary>
    /// Gets the collection of hexside terrain visuals (walls, hedges).
    /// </summary>
    public ObservableCollection<HexsideTerrainVisualViewModel> HexsideTerrainVisuals
    {
        get => _hexsideTerrainVisuals;
        set => SetProperty(ref _hexsideTerrainVisuals, value);
    }

    /// <summary>
    /// Gets the collection of road segments for rendering.
    /// </summary>
    public ObservableCollection<RoadVisualViewModel> RoadVisuals => _roadVisuals;

    /// <summary>
    /// Gets the collection of cliff visuals for rendering.
    /// </summary>
    public ObservableCollection<CliffVisualViewModel> CliffVisuals
    {
        get => _cliffVisuals;
        set => SetProperty(ref _cliffVisuals, value);
    }

    /// <summary>
    /// Gets the underlying ASL board being edited.
    /// </summary>
    public AslBoard Board => _board;

    /// <summary>
    /// Gets the collection of hexes for rendering.
    /// </summary>
    public ObservableCollection<HexViewModel> Hexes => _hexes;

    /// <summary>
    /// Gets or sets the size of the hexes (radius).
    /// </summary>
    public double HexSize
    {
        get => _hexSize;
        set { if (SetProperty(ref _hexSize, value)) GenerateHexGrid(); }
    }

    /// <summary>
    /// Gets or sets the currently selected hex.
    /// </summary>
    public HexViewModel? SelectedHex
    {
        get => _selectedHex;
        set 
        { 
            if (SetProperty(ref _selectedHex, value)) 
            {
                if (value != null)
                {
                    _selectedEdge = null;
                    OnPropertyChanged(nameof(SelectedEdge));
                }
            } 
        }
    }

    /// <summary>
    /// Gets or sets the currently selected edge.
    /// </summary>
    public HexEdgeSelection? SelectedEdge
    {
        get => _selectedEdge;
        set 
        { 
            if (SetProperty(ref _selectedEdge, value)) 
            {
                if (value != null)
                {
                    _selectedHex = null;
                    OnPropertyChanged(nameof(SelectedHex));
                }
            }
        }
    }

    private readonly string _backgroundImagePath;

    /// <summary>
    /// Gets the background image to render behind the canvas.
    /// </summary>
    public ImageSource? BackgroundImage { get; }

    /// <summary>
    /// Gets or sets the current editing tool.
    /// </summary>
    public ToolMode CurrentTool
    {
        get => _currentTool;
        set => SetProperty(ref _currentTool, value);
    }

    /// <summary>
    /// Synchronizes the board's internal structures and re-generates the hex grid.
    /// Used when configuration settings like staggering or halving are changed.
    /// </summary>
    public void RefreshGrid()
    {
        _board.PopulateBoard();
        RecalculateHexSize();
        GenerateHexGrid();
        RefreshAllVisuals();
    }

    /// <summary>
    /// Gets or sets the active terrain for the paint tool.
    /// </summary>
    public TerrainType ActiveTerrain
    {
        get => _activeTerrain;
        set => SetProperty(ref _activeTerrain, value);
    }

    /// <summary>
    /// Gets or sets the current zoom scale.
    /// </summary>
    public double ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, Math.Max(0.1, Math.Min(10.0, value)));
    }

    /// <summary>
    /// Gets all available terrain types for selection.
    /// </summary>
    public Array AvailableTerrainTypes => Enum.GetValues(typeof(TerrainType));

    /// <summary>
    /// Gets all available rubble types.
    /// </summary>
    public Array AvailableRubbleTypes => Enum.GetValues(typeof(RubbleType));

    /// <summary>
    /// Gets the available elevation levels.
    /// </summary>
    public int[] AvailableElevations => new[] { 0, 1, 2, 3, 4, 5 };

    /// <summary>
    /// Command to select a hex.
    /// </summary>
    public ICommand SelectHexCommand { get; }

    /// <summary>
    /// Command to paint a hex.
    /// </summary>
    public ICommand PaintHexCommand { get; }

    /// <summary>
    /// Command to save the modified board.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to reset zoom to 100%.
    /// </summary>
    public ICommand ResetZoomCommand { get; }

    private readonly string? _originalName;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardEditorViewModel"/> class.
    /// </summary>
    /// <param name="board">The board to edit.</param>
    /// <param name="repository">The board repository.</param>
    /// <param name="imageFullPath">The background image path.</param>
    /// <param name="originalName">The name on disk before any renaming.</param>
    public BoardEditorViewModel(AslBoard board, IBoardRepository repository, string imageFullPath = "", string? originalName = null)
    {
        _backgroundImagePath = imageFullPath;
        if (!string.IsNullOrEmpty(imageFullPath) && System.IO.File.Exists(imageFullPath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(imageFullPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // This prevents file locking
                bitmap.EndInit();
                bitmap.Freeze(); // Optimize for cross-thread access
                BackgroundImage = bitmap;
            }
            catch { /* Fallback to no image if load fails */ }
        }

        _board = board;
        _repository = repository;
        _originalName = originalName ?? board.Name;
        DisplayName = "Board Editor";
        SelectHexCommand = new RelayCommand<HexViewModel>(OnSelectHex);
        PaintHexCommand = new RelayCommand<HexViewModel>(OnPaintHex);
        SaveCommand = new RelayCommand(OnSave);
        ResetZoomCommand = new RelayCommand(_ => ZoomLevel = 1.0);
        
        RecalculateHexSize();
        GenerateHexGrid();
        RefreshAllVisuals();
    }

    private void OnSelectHex(HexViewModel? hex)
    {
        if (hex == null) return;

        if (CurrentTool == ToolMode.Select)
        {
            if (SelectedHex != null) SelectedHex.IsSelected = false;
            
            ClearAllEdgeVisuals();

            SelectedHex = hex;
            SelectedHex.IsSelected = true;
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

        // Clear existing visual hex selection
        if (SelectedHex != null)
        {
            SelectedHex.IsSelected = false;
        }

        // Clear all existing edge selection visual properties
        ClearAllEdgeVisuals();

        // Apply visual selection to this exact edge
        SetEdgeVisualSelection(hex, edgeIndex, true);

        // Map visual edgeIndex to CubeCoordinate direction index
        // Edge index mapping for FlatTopped (based on GetHexPoints angles):
        // 0: SE (30 deg) -> Dir 0 (1, 0, -1)
        // 1: S (90 deg) -> Dir 5 (0, 1, -1)
        // 2: SW (150 deg) -> Dir 4 (-1, 1, 0)
        // 3: NW (210 deg) -> Dir 3 (-1, 0, 1)
        // 4: N (270 deg) -> Dir 2 (0, -1, 1)
        // 5: NE (330 deg) -> Dir 1 (1, -1, 0)
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

    private void RefreshAllVisuals()
    {
        UpdateRoadVisuals();
        UpdateBuildingVisuals();
        UpdateCliffVisuals();
        UpdateHexsideTerrainVisuals();
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
    public void PaintHex(HexViewModel hex)
    {
        hex.Terrain = ActiveTerrain;
        UpdateBuildingVisuals();
    }

    private async void OnSave(object? parameter)
    {
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

    private void RecalculateHexSize()
    {
        var halfSides = _board.Board.HalfHexSides;
        
        // Horizontal Span Calculation:
        // In a Flat-Topped hex grid:
        // - Each hex has a width of 2S (where S is the circumradius).
        // - The horizontal distance between adjacent columns is exactly 1.5S.
        // - The first and last hexes contribute an extra 0.5S to the total span.
        // - If a board side is NOT halved, we add a full extra S to that side to ensure the hex isn't clipped.
        double multiplierW = 1.5 * (_board.Width - 1);
        if (!halfSides.HasFlag(BoardEdge.Left)) multiplierW += 1.0;
        if (!halfSides.HasFlag(BoardEdge.Right)) multiplierW += 1.0;
        
        // Vertical Span Calculation:
        // In a Flat-Topped hex grid:
        // - Each hex has a height of sqrt(3) * S.
        // - The vertical distance between adjacent rows in the same column is sqrt(3) * S.
        // - Staggering shifts adjacent columns vertically by 0.5 * sqrt(3) * S.
        // - Total vertical units = N * sqrt(3), accounting for the full height of the staggered grid.
        double h_unit = Math.Sqrt(3);
        double multiplierH = h_unit * _board.Height; 
        
        // Final pixel dimensions = Multiplier * HexSize.
        // The View background rectangle uses these values to perfectly frame the hex grid.

        // Safety check to avoid division by zero or negative multipliers
        if (multiplierW <= 0) multiplierW = 1.0;
        if (multiplierH <= 0) multiplierH = 1.0;

        // Calculate maximum possible hex size to fit within both dimensions
        double sizeW = _board.CanvasWidth / multiplierW;
        double sizeH = _board.CanvasHeight / multiplierH;
        
        _hexSize = Math.Min(sizeW, sizeH);
        
        // Final sanity check for size
        if (double.IsNaN(_hexSize) || double.IsInfinity(_hexSize) || _hexSize < 0.1)
            _hexSize = 40.0;
        
        // Update Actual Dimensions for the View (so the background white rectangle matches the grid exactly)
        ActualGridWidth = multiplierW * _hexSize;
        ActualGridHeight = multiplierH * _hexSize;
        OnPropertyChanged(nameof(ActualGridWidth));
        OnPropertyChanged(nameof(ActualGridHeight));
    }

    /// <summary>Gets the actual width of the hex grid in pixels.</summary>
    public double ActualGridWidth { get; private set; }
    /// <summary>Gets the actual height of the hex grid in pixels.</summary>
    public double ActualGridHeight { get; private set; }

    private void GenerateHexGrid()
    {
        _hexes.Clear();
        var orientation = HexTopOrientation.FlatTopped;
        var halfSides = _board.Board.HalfHexSides;
        
        foreach (var hex in _board.Board.Hexes.Values)
        {
            var (col, row) = hex.Location.ToOffset(orientation, _board.IsFirstColShiftedDown);
            
            // Calculate pixel center for FlatTopped (Odd-Q or Even-Q)
            // Stagger: Determine which columns are shifted down by 0.5h
            bool isShiftedDown = _board.Board.ShiftingOddColumns ? (col % 2 == 1) : (col % 2 == 0);
            double x = _hexSize * 1.5 * col;
            double y = _hexSize * Math.Sqrt(3) * (row + (isShiftedDown ? 0.5 : 0));
            
            // Adjust origin based on Half-Hex Sides
            if (!halfSides.HasFlag(BoardEdge.Left))
                x += _hexSize;

            if (!halfSides.HasFlag(BoardEdge.Top))
                y += _hexSize * Math.Sqrt(3) / 2;

            var (pointsString, corners) = GetHexPoints(x, y, _hexSize);
            double labelY = y - (_hexSize * Math.Sqrt(3) / 2.0) + (_hexSize * 0.1); 
            var hexVm = new HexViewModel(col, row, hex.Location, pointsString, corners, hex.Id, x, y, labelY, _hexSize, hex.Metadata!, OnSelectEdge);
            hexVm.OnTerrainChanged = RefreshAllVisuals;
            _hexes.Add(hexVm);
        }
    }

    private void UpdateRoadVisuals()
    {
        _roadVisuals.Clear();
        _waterVisuals.Clear();
        var processedEdges = new HashSet<(CubeCoordinate, CubeCoordinate)>();

        foreach (var hexVm in _hexes)
        {
            for (int i = 0; i < 6; i++)
            {
                var neighborLoc = hexVm.Location.GetNeighbor(i);
                var edgeKey = NormalizeEdge(hexVm.Location, neighborLoc);
                
                if (processedEdges.Contains(edgeKey)) continue;
                processedEdges.Add(edgeKey);

                var edgeData = _board.Board.GetEdgeData(hexVm.Location, neighborLoc);
                if (edgeData == null) continue;

                var neighborHexVm = _hexes.FirstOrDefault(h => h.Location.Equals(neighborLoc));
                Point destPoint;

                if (neighborHexVm != null)
                {
                    destPoint = new Point(neighborHexVm.CenterX, neighborHexVm.CenterY);
                }
                else
                {
                    // Boundary connection: use hexside midpoint
                    int edgeIndex = i switch
                    {
                        0 => 0, // SE
                        5 => 1, // S
                        4 => 2, // SW
                        3 => 3, // NW
                        2 => 4, // N
                        1 => 5, // NE
                        _ => 0
                    };
                    var p1 = hexVm.HexCorners[edgeIndex];
                    var p2 = hexVm.HexCorners[(edgeIndex + 1) % 6];
                    destPoint = new Point((p1.X + p2.X) / 2.0, (p1.Y + p2.Y) / 2.0);
                }

                // 1. Water features (drawn below roads)
                if (edgeData.HasStream)
                {
                    _waterVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = Brushes.DodgerBlue, Thickness = 4.0
                    });
                }
                if (edgeData.HasGully)
                {
                    _waterVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = new SolidColorBrush(Color.FromRgb(210, 180, 140)), // Tan
                        Thickness = 6.0
                    });
                }
                if (edgeData.HasCanal)
                {
                    _waterVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = Brushes.DodgerBlue, Thickness = 20.0
                    });
                }

                // 2. Roads
                if (edgeData.HasPavedRoad)
                {
                    _roadVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = Brushes.LightGray, Thickness = 6.0
                    });
                }
                if (edgeData.HasDirtRoad)
                {
                    _roadVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = new SolidColorBrush(Color.FromRgb(160, 110, 60)), // Lighter brown
                        Thickness = 6.0
                    });
                }
                if (edgeData.HasPath)
                {
                    _roadVisuals.Add(new RoadVisualViewModel
                    {
                        X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                        X2 = destPoint.X, Y2 = destPoint.Y,
                        Stroke = new SolidColorBrush(Color.FromRgb(120, 80, 40)), // Darker brown for thin path
                        Thickness = 2.0
                    });
                }
            }
        }
    }

    private void UpdateBuildingVisuals()
    {
        if (_isUpdatingVisuals) return;
        _isUpdatingVisuals = true;

        try
        {
            var newList = new ObservableCollection<BuildingVisualBase>();
            var processedEdges = new HashSet<(CubeCoordinate, CubeCoordinate)>();
            double squareSizeFactor = 0.5; // 50% of hex size as requested
            double connectorThicknessFactor = 0.75; // 75% of square size

            foreach (var hexVm in _hexes)
            {
                for (int edgeIndex = 0; edgeIndex < 6; edgeIndex++)
                {
                    int dirIndex = edgeIndex switch
                    {
                        0 => 0, // SE
                        1 => 5, // S
                        2 => 4, // SW
                        3 => 3, // NW
                        4 => 2, // N
                        5 => 1, // NE
                        _ => 0
                    };

                    var neighborLoc = hexVm.Location.GetNeighbor(dirIndex);
                    var edgeKey = NormalizeEdge(hexVm.Location, neighborLoc);
                    if (processedEdges.Contains(edgeKey)) continue;
                    processedEdges.Add(edgeKey);

                    var neighborHexVm = _hexes.FirstOrDefault(h => h.Location.Equals(neighborLoc));
                    if (neighborHexVm == null) continue;

                    var edgeData = _board.Board.GetEdgeData(hexVm.Location, neighborLoc);
                    if (edgeData != null && (edgeData.HasHouse || edgeData.IsRowhouse))
                    {
                        // Cleanup: If either hex is not a building, remove the house/rowhouse connection
                        if (!IsBuildingTerrain(hexVm.Terrain) || !IsBuildingTerrain(neighborHexVm.Terrain))
                        {
                            edgeData.HasHouse = false;
                            edgeData.IsRowhouse = false;
                            continue;
                        }

                        newList.Add(new BuildingConnectorViewModel
                        {
                            CanvasX = 0,
                            CanvasY = 0,
                            X1 = hexVm.CenterX, Y1 = hexVm.CenterY,
                            X2 = neighborHexVm.CenterX, Y2 = neighborHexVm.CenterY,
                            Thickness = _hexSize * squareSizeFactor * connectorThicknessFactor,
                            Fill = IsBuildingTerrain(hexVm.Terrain) ? GetBuildingBrush(hexVm.Terrain) : GetBuildingBrush(neighborHexVm.Terrain)
                        });

                        if (edgeData.IsRowhouse)
                        {
                            var p1 = hexVm.HexCorners[edgeIndex];
                            var p2 = hexVm.HexCorners[(edgeIndex + 1) % 6];
                            
                            // Calculate midpoint and direction of the hexside
                            double midX = (p1.X + p2.X) / 2.0;
                            double midY = (p1.Y + p2.Y) / 2.0;
                            double dx = p2.X - p1.X;
                            double dy = p2.Y - p1.Y;
                            double sideLen = Math.Sqrt(dx * dx + dy * dy);
                            
                            // Unit vector along the hexside
                            double ux = dx / sideLen;
                            double uy = dy / sideLen;
                            
                            // The line should be the width of the building connector
                            double lineHalfLen = (_hexSize * squareSizeFactor * connectorThicknessFactor) / 2.0;

                            newList.Add(new BuildingDividerViewModel
                            {
                                CanvasX = 0,
                                CanvasY = 0,
                                X1 = midX - ux * lineHalfLen, 
                                Y1 = midY - uy * lineHalfLen,
                                X2 = midX + ux * lineHalfLen, 
                                Y2 = midY + uy * lineHalfLen,
                                Thickness = 3.0 // Black divider line
                            });
                        }
                    }
                }
            }

            BuildingVisuals = newList;
        }
        finally
        {
            _isUpdatingVisuals = false;
        }
    }

    private bool IsBuildingTerrain(TerrainType terrain)
    {
        return terrain == TerrainType.StoneBuilding || 
               terrain == TerrainType.WoodenBuilding;
    }

    private Brush GetBuildingBrush(TerrainType terrain)
    {
        return terrain switch
        {
            TerrainType.StoneBuilding => Brushes.DimGray,
            TerrainType.WoodenBuilding => Brushes.SaddleBrown,
            _ => Brushes.Transparent
        };
    }

    private (CubeCoordinate, CubeCoordinate) NormalizeEdge(CubeCoordinate a, CubeCoordinate b)
    {
        return (a.Q < b.Q || (a.Q == b.Q && a.R < b.R)) ? (a, b) : (b, a);
    }

    private (string pointsString, Point[] corners) GetHexPoints(double centerX, double centerY, double size)
    {
        var sb = new StringBuilder();
        var corners = new Point[6];
        for (int i = 0; i < 6; i++)
        {
            double angleDeg = 60 * i; // Flat topped starts at 0 deg
            double angleRad = Math.PI / 180 * angleDeg;
            double px = centerX + size * Math.Cos(angleRad);
            double py = centerY + size * Math.Sin(angleRad);
            
            corners[i] = new Point(px, py);

            if (i == 0) sb.Append("M "); else sb.Append("L ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2},{1:F2} ", px, py);
        }
        sb.Append("Z");
        return (sb.ToString(), corners);
    }
}

/// <summary>
/// Represents a selection of a specific edge of a hex.
/// </summary>
public class HexEdgeSelection
{
    /// <summary>Gets the hex associated with the selection.</summary>
    public HexViewModel Hex { get; }
    
    /// <summary>Gets the index of the selected edge (0-5).</summary>
    public int EdgeIndex { get; }
    
    /// <summary>Gets the view model for the hexside data.</summary>
    public HexsideViewModel Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HexEdgeSelection"/> class.
    /// </summary>
    /// <param name="hex">The hex.</param>
    /// <param name="edgeIndex">Index of the edge.</param>
    /// <param name="data">The hexside data.</param>
    public HexEdgeSelection(HexViewModel hex, int edgeIndex, HexsideViewModel data)
    {
        Hex = hex;
        EdgeIndex = edgeIndex;
        Data = data;
    }
}

/// <summary>
/// View model for managing properties of a specific hexside.
/// </summary>
public class HexsideViewModel : ViewModelBase
{
    private readonly ASLEdgeData _data;
    private readonly Action _onChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="HexsideViewModel"/> class.
    /// </summary>
    /// <param name="data">The edge data.</param>
    /// <param name="onChanged">Callback for when data changes.</param>
    /// <param name="isHouseAvailable">Whether house connections are possible on this edge.</param>
    public HexsideViewModel(ASLEdgeData data, Action onChanged, bool isHouseAvailable = true)
    {
        _data = data;
        _onChanged = onChanged;
        IsHouseConnectionAvailable = isHouseAvailable;
    }

    /// <summary>Gets a value indicating whether a house connection can be made at this hexside.</summary>
    public bool IsHouseConnectionAvailable { get; }

    /// <summary>Gets or sets a value indicating whether this hexside has a wall.</summary>
    public bool HasWall
    {
        get => _data.HasWall;
        set 
        { 
            _data.HasWall = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a hedge.</summary>
    public bool HasHedge
    {
        get => _data.HasHedge;
        set 
        { 
            _data.HasHedge = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has bocage.</summary>
    public bool HasBocage
    {
        get => _data.HasBocage;
        set 
        { 
            _data.HasBocage = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a paved road.</summary>
    public bool HasPavedRoad
    {
        get => _data.HasPavedRoad;
        set 
        { 
            _data.HasPavedRoad = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a dirt road.</summary>
    public bool HasDirtRoad
    {
        get => _data.HasDirtRoad;
        set 
        { 
            _data.HasDirtRoad = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a path.</summary>
    public bool HasPath
    {
        get => _data.HasPath;
        set 
        { 
            _data.HasPath = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a house connection.</summary>
    public bool HasHouse
    {
        get => _data.HasHouse;
        set 
        { 
            _data.HasHouse = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a stream.</summary>
    public bool HasStream
    {
        get => _data.HasStream;
        set 
        { 
            _data.HasStream = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a gully.</summary>
    public bool HasGully
    {
        get => _data.HasGully;
        set 
        { 
            _data.HasGully = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a canal.</summary>
    public bool HasCanal
    {
        get => _data.HasCanal;
        set 
        { 
            _data.HasCanal = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside is a rowhouse connection.</summary>
    public bool IsRowhouse
    {
        get => _data.IsRowhouse;
        set 
        { 
            _data.IsRowhouse = value; 
            if (value) HasHouse = true; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a cliff.</summary>
    public bool HasCliff
    {
        get => _data.HasCliff;
        set 
        { 
            _data.HasCliff = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }
}

// Extension to BoardEditorViewModel
public partial class BoardEditorViewModel
{
    private void UpdateCliffVisuals()
    {
        var newList = new ObservableCollection<CliffVisualViewModel>();
        var processedEdges = new HashSet<(CubeCoordinate, CubeCoordinate)>();

        foreach (var hexVm in _hexes)
        {
            for (int i = 0; i < 6; i++)
            {
                var neighborLoc = hexVm.Location.GetNeighbor(i);
                var edgeKey = NormalizeEdge(hexVm.Location, neighborLoc);
                if (processedEdges.Contains(edgeKey)) continue;
                processedEdges.Add(edgeKey);

                var edgeData = _board.Board.GetEdgeData(hexVm.Location, neighborLoc);
                if (edgeData != null && edgeData.HasCliff)
                {
                    // Map direction index to visual hex corners
                    int edgeIndex = i switch
                    {
                        0 => 0, // SE
                        5 => 1, // S
                        4 => 2, // SW
                        3 => 3, // NW
                        2 => 4, // N
                        1 => 5, // NE
                        _ => 0
                    };

                    var p1 = hexVm.HexCorners[edgeIndex];
                    var p2 = hexVm.HexCorners[(edgeIndex + 1) % 6];

                    newList.Add(new CliffVisualViewModel(CreateJaggedGeometry(p1, p2)));
                }
            }
        }
        CliffVisuals = newList;
    }

    private Geometry CreateJaggedGeometry(Point p1, Point p2)
    {
        var stream = new StreamGeometry();
        using (var context = stream.Open())
        {
            context.BeginFigure(p1, true, false);

            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            
            // Perpendicular vector
            double nx = -dy / len;
            double ny = dx / len;

            int segments = 8;
            double toothSize = 4.0;

            for (int j = 1; j <= segments; j++)
            {
                double t = (double)j / segments;
                var currentP = new Point(p1.X + dx * t, p1.Y + dy * t);
                
                // Add a "tooth" at each segment
                var toothPoint = new Point(currentP.X + nx * toothSize, currentP.Y + ny * toothSize);
                context.LineTo(toothPoint, true, false);
                context.LineTo(currentP, true, false);
            }
        }
        stream.Freeze();
        return stream;
    }

    private void UpdateHexsideTerrainVisuals()
    {
        var newList = new ObservableCollection<HexsideTerrainVisualViewModel>();
        var processedEdges = new HashSet<(CubeCoordinate, CubeCoordinate)>();

        foreach (var hexVm in _hexes)
        {
            for (int i = 0; i < 6; i++)
            {
                var neighborLoc = hexVm.Location.GetNeighbor(i);
                var edgeKey = NormalizeEdge(hexVm.Location, neighborLoc);
                if (processedEdges.Contains(edgeKey)) continue;
                processedEdges.Add(edgeKey);

                var edgeData = _board.Board.GetEdgeData(hexVm.Location, neighborLoc);
                if (edgeData != null)
                {
                    // Map direction index to visual hex corners
                    int edgeIndex = i switch
                    {
                        0 => 0, // SE
                        5 => 1, // S
                        4 => 2, // SW
                        3 => 3, // NW
                        2 => 4, // N
                        1 => 5, // NE
                        _ => 0
                    };

                    var p1 = hexVm.HexCorners[edgeIndex];
                    var p2 = hexVm.HexCorners[(edgeIndex + 1) % 6];

                    if (edgeData.HasWall)
                    {
                        newList.Add(new HexsideTerrainVisualViewModel(p1, p2, Brushes.Gray, 5.0));
                    }
                    if (edgeData.HasHedge)
                    {
                        newList.Add(new HexsideTerrainVisualViewModel(p1, p2, Brushes.ForestGreen, 5.0));
                    }
                    if (edgeData.HasBocage)
                    {
                        newList.Add(new HexsideTerrainVisualViewModel(p1, p2, Brushes.DarkGreen, 8.0));
                    }
                }
            }
        }
        HexsideTerrainVisuals = newList;
    }
}
