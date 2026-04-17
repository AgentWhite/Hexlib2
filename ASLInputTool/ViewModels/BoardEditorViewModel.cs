using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Media;
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
using System.Windows.Media.Imaging;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the board editor canvas and toolbar.
/// </summary>
public partial class BoardEditorViewModel : ViewModelBase
{
    private readonly AslBoard _board;
    private readonly IBoardRepository? _repository;
    private Layout _layout;
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
    private HexViewModel? _roadStartHex;
    private RoadToolType _activeRoadType = RoadToolType.Paved;
    private ObservableCollection<RoadVisualViewModel> _roadPreviewVisuals = new();
    private bool _dimBackground = true;

    /// <summary>Gets the collection of water visuals (streams, gullies, canals).</summary>
    public ObservableCollection<RoadVisualViewModel> WaterVisuals => _waterVisuals;

    /// <summary>Gets the collection of building visual elements for rendering.</summary>
    public ObservableCollection<BuildingVisualBase> BuildingVisuals
    {
        get => _buildingVisuals;
        set => SetProperty(ref _buildingVisuals, value);
    }

    /// <summary>Gets the collection of hexside terrain visuals (walls, hedges).</summary>
    public ObservableCollection<HexsideTerrainVisualViewModel> HexsideTerrainVisuals
    {
        get => _hexsideTerrainVisuals;
        set => SetProperty(ref _hexsideTerrainVisuals, value);
    }

    /// <summary>Gets the collection of road segments for rendering.</summary>
    public ObservableCollection<RoadVisualViewModel> RoadVisuals => _roadVisuals;

    /// <summary>Gets the collection of cliff visuals for rendering.</summary>
    public ObservableCollection<CliffVisualViewModel> CliffVisuals
    {
        get => _cliffVisuals;
        set => SetProperty(ref _cliffVisuals, value);
    }

    /// <summary>Gets the underlying ASL board being edited.</summary>
    public AslBoard Board => _board;

    /// <summary>Gets the collection of hexes for rendering.</summary>
    public ObservableCollection<HexViewModel> Hexes => _hexes;

    public double HexSize
    {
        get => _hexSize;
        set 
        { 
            if (SetProperty(ref _hexSize, value)) 
            {
                UpdateLayout();
                GenerateHexGrid(); 
            }
        }
    }

    private void UpdateLayout()
    {
        // Flat-topped hexes use the standard Flat orientation.
        // Size is the circumradius (center-to-corner distance).
        // Origin is adjusted in GenerateHexGrid based on board edges.
        
        var orientation = Orientation.Flat;
        var halfSides = _board.Board.HalfHexSides;
        
        // Base origin starts at 0,0
        double originX = 0;
        double originY = 0;

        // Adjust origin based on Half-Hex Sides to ensure hexes are not clipped
        if (!halfSides.HasFlag(BoardEdge.Left))
            originX += _hexSize;

        if (!halfSides.HasFlag(BoardEdge.Top))
            originY += _hexSize * Math.Sqrt(3) / 2;
        if (_board.IsFirstColShiftedDown) { originY += _hexSize * Math.Sqrt(3) / 2; }

        _layout = new Layout(orientation, new Point2D(_hexSize, _hexSize), new Point2D(originX, originY));
    }

    /// <summary>Gets or sets the currently selected hex.</summary>
    public HexViewModel? SelectedHex
    {
        get => _selectedHex;
        set 
        { 
            var old = _selectedHex;
            if (SetProperty(ref _selectedHex, value)) 
            {
                if (old != null) old.IsSelected = false;
                if (value != null)
                {
                    value.IsSelected = true;
                    SelectedEdge = null;
                }
            } 
        }
    }

    /// <summary>Gets or sets the currently selected edge.</summary>
    public HexEdgeSelection? SelectedEdge
    {
        get => _selectedEdge;
        set 
        { 
            if (SetProperty(ref _selectedEdge, value)) 
            {
                if (value != null)
                {
                    SelectedHex = null;
                }
                else
                {
                    ClearAllEdgeVisuals();
                }
            }
        }
    }

    private readonly string _backgroundImagePath;

    /// <summary>Gets the background image to render behind the canvas.</summary>
    public ImageSource? BackgroundImage { get; }

    /// <summary>Gets or sets the current editing tool.</summary>
    public ToolMode CurrentTool
    {
        get => _currentTool;
        set 
        {
            if (SetProperty(ref _currentTool, value))
            {
                if (value == ToolMode.Paint || value == ToolMode.Road)
                {
                    SelectedHex = null;
                    SelectedEdge = null;
                }
                
                if (value != ToolMode.Road)
                {
                    EndRoad();
                }
            }
        }
    }

    /// <summary>
    /// Synchronizes the board's internal structures and re-generates the hex grid.
    /// Used when configuration settings like staggering or halving are changed.
    /// </summary>
    public void RefreshGrid()
    {
        _board.PopulateBoard();
        UpdateLayout();
        RecalculateHexSize();
        GenerateHexGrid();
        RefreshAllVisuals();
    }

    /// <summary>Gets or sets the active terrain for the paint tool.</summary>
    public TerrainType ActiveTerrain
    {
        get => _activeTerrain;
        set => SetProperty(ref _activeTerrain, value);
    }

    /// <summary>Gets or sets the current zoom scale.</summary>
    public double ZoomLevel
    {
        get => _zoomLevel;
        set => SetProperty(ref _zoomLevel, Math.Max(0.1, Math.Min(10.0, value)));
    }

    /// <summary>Gets the starting hex for the current road string tool.</summary>
    public HexViewModel? RoadStartHex
    {
        get => _roadStartHex;
        set => SetProperty(ref _roadStartHex, value);
    }

    /// <summary>Gets or sets the active road type to paint.</summary>
    public RoadToolType ActiveRoadType
    {
        get => _activeRoadType;
        set => SetProperty(ref _activeRoadType, value);
    }

    /// <summary>Gets or sets a value indicating whether the background image should be dimmed.</summary>
    public bool DimBackground
    {
        get => _dimBackground;
        set => SetProperty(ref _dimBackground, value);
    }

    /// <summary>Gets the collection of temporary road previews.</summary>
    public ObservableCollection<RoadVisualViewModel> RoadPreviewVisuals => _roadPreviewVisuals;

    /// <summary>Gets all available terrain types for selection.</summary>
    public Array AvailableTerrainTypes => Enum.GetValues(typeof(TerrainType));

    /// <summary>Gets all available rubble types.</summary>
    public Array AvailableRubbleTypes => Enum.GetValues(typeof(RubbleType));

    /// <summary>Gets the available elevation levels.</summary>
    public int[] AvailableElevations => new[] { 0, 1, 2, 3, 4, 5 };

    /// <summary>Command to select a hex.</summary>
    public ICommand SelectHexCommand { get; }

    /// <summary>Command to paint a hex.</summary>
    public ICommand PaintHexCommand { get; }

    /// <summary>Command to terminate the current road stringing.</summary>
    public ICommand EndRoadCommand { get; }

    /// <summary>Command to save the modified board.</summary>
    public ICommand SaveCommand { get; }

    /// <summary>Command to reset zoom to 100%.</summary>
    public ICommand ResetZoomCommand { get; }

    private readonly string? _originalName;

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardEditorViewModel"/> class.
    /// </summary>
    /// <param name="board">The board to edit.</param>
    /// <param name="repository">The board repository.</param>
    /// <param name="imageFullPath">The background image path.</param>
    /// <param name="originalName">The name on disk before any renaming.</param>
    public BoardEditorViewModel(AslBoard board, IBoardRepository? repository = null, string imageFullPath = "", string? originalName = null)
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
        EndRoadCommand = new RelayCommand(_ => EndRoad());
        
        UpdateLayout();
        RecalculateHexSize();
        GenerateHexGrid();
        RefreshAllVisuals();
    }

    private void EndRoad()
    {
        RoadStartHex = null;
        RoadPreviewVisuals.Clear();
    }
}
