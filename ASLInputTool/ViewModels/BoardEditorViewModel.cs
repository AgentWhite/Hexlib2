using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Text;
using System.Globalization;
using System.Windows;
using ASL;
using HexLib;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the board editor canvas and toolbar.
/// </summary>
public class BoardEditorViewModel : ViewModelBase
{
    private readonly AslBoard _board;
    private double _hexSize = 40.0;
    private HexViewModel? _selectedHex;
    private HexEdgeSelection? _selectedEdge;
    private ToolMode _currentTool = ToolMode.Select;
    private TerrainType _activeTerrain = TerrainType.OpenGround;
    private double _zoomLevel = 1.0;
    private ObservableCollection<HexViewModel> _hexes = new();

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

    /// <summary>
    /// Gets the absolute path of the background image to render behind the canvas.
    /// </summary>
    public string BackgroundImagePath { get; }

    /// <summary>
    /// Gets or sets the current editing tool.
    /// </summary>
    public ToolMode CurrentTool
    {
        get => _currentTool;
        set => SetProperty(ref _currentTool, value);
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

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardEditorViewModel"/> class.
    /// </summary>
    /// <param name="board">The board to edit.</param>
    /// <param name="imageFullPath">The background image path.</param>
    public BoardEditorViewModel(AslBoard board, string imageFullPath = "")
    {
        BackgroundImagePath = imageFullPath;
        _board = board;
        DisplayName = "Board Editor";
        SelectHexCommand = new RelayCommand<HexViewModel>(OnSelectHex);
        PaintHexCommand = new RelayCommand<HexViewModel>(OnPaintHex);
        SaveCommand = new RelayCommand(OnSave);
        ResetZoomCommand = new RelayCommand(_ => ZoomLevel = 1.0);
        
        RecalculateHexSize();
        GenerateHexGrid();
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

        SelectedEdge = new HexEdgeSelection(hex, edgeIndex, new HexsideViewModel(data));
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
    }

    private void OnSave(object? parameter)
    {
        // For now, we'll assume the Board Repository handles the actual file IO.
        // We'll just confirm the Board is updated.
        // In a real app, we'd call _repository.Save(_board);
        MessageBox.Show($"Board '{_board.Name}' changes are kept in memory. Save to disk not yet implemented via Repository.", "Save");
    }

    private void RecalculateHexSize()
    {
        var halfSides = _board.Board.HalfHexSides;
        
        // Horizontal span calculation:
        // center-to-center steps are 1.5S each.
        // Plus left overhang (S if full, 0 if halved) and right overhang (S if full, 0 if halved).
        double multiplierW = 1.5 * (_board.Width - 1);
        if (!halfSides.HasFlag(BoardEdge.Left)) multiplierW += 1.0;
        if (!halfSides.HasFlag(BoardEdge.Right)) multiplierW += 1.0;
        
        // Vertical span calculation:
        // Distance between rows is sqrt(3)*S.
        // For a board with N row positions, the total vertical units is exactly N * sqrt(3).
        // This accounts for the staggering where high columns are halved at Top/Bottom
        // and low columns are full height between those boundaries.
        double h_unit = Math.Sqrt(3);
        double multiplierH = h_unit * _board.Height; 
        
        // No extra padding needed for multiplierH if it's based on N * h_unit,
        // as y=0 starts at the center of row 0 high columns.

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

    public double ActualGridWidth { get; private set; }
    public double ActualGridHeight { get; private set; }

    private void GenerateHexGrid()
    {
        _hexes.Clear();
        var orientation = HexTopOrientation.FlatTopped;
        var halfSides = _board.Board.HalfHexSides;
        
        foreach (var hex in _board.Board.Hexes.Values)
        {
            var (col, row) = hex.Location.ToOffset(orientation);
            
            // Calculate pixel center for FlatTopped (Odd-Q)
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
            _hexes.Add(new HexViewModel(col, row, hex.Location, pointsString, corners, hex.Id, x, labelY, _hexSize, hex.Metadata!, OnSelectEdge));
        }
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

public class HexEdgeSelection
{
    public HexViewModel Hex { get; }
    public int EdgeIndex { get; }
    public HexsideViewModel Data { get; }

    public HexEdgeSelection(HexViewModel hex, int edgeIndex, HexsideViewModel data)
    {
        Hex = hex;
        EdgeIndex = edgeIndex;
        Data = data;
    }
}

public class HexsideViewModel : ViewModelBase
{
    private readonly ASLEdgeData _data;

    public HexsideViewModel(ASLEdgeData data)
    {
        _data = data;
    }

    public bool HasWall
    {
        get => _data.HasWall;
        set { _data.HasWall = value; OnPropertyChanged(); }
    }

    public bool HasHedge
    {
        get => _data.HasHedge;
        set { _data.HasHedge = value; OnPropertyChanged(); }
    }

    public bool HasBocage
    {
        get => _data.HasBocage;
        set { _data.HasBocage = value; OnPropertyChanged(); }
    }

    public bool HasRoad
    {
        get => _data.HasRoad;
        set { _data.HasRoad = value; OnPropertyChanged(); }
    }
}
