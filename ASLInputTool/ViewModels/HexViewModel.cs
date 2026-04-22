
namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel representing a single hex for rendering.
/// </summary>
public class HexViewModel : ViewModelBase
{
    private readonly ASLHexMetadata _metadata;
    private bool _isSelected;
    private bool _isEdge0Selected, _isEdge1Selected, _isEdge2Selected, _isEdge3Selected, _isEdge4Selected, _isEdge5Selected;
    private bool _isHighlightedForLos;

    /// <summary>Gets or sets the SVG path data for the hex outline.</summary>
    public string Points { get; set; } = string.Empty;

    /// <summary>Gets or sets the absolute 6 vertices of the hex.</summary>
    public Point[] HexCorners { get; set; } = new Point[6];

    /// <summary>Gets the logical cube coordinate of this hex.</summary>
    public CubeCoordinate Location { get; }

    /// <summary>Gets or sets the column index.</summary>
    public int Column { get; set; }

    /// <summary>Gets or sets the row index.</summary>
    public int Row { get; set; }

    /// <summary>Gets or sets the ASL coordinate ID (e.g., "A1").</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets the X coordinate of the hex center.</summary>
    public double CenterX { get; }

    /// <summary>Gets the Y coordinate of the hex center.</summary>
    public double CenterY { get; }

    /// <summary>Gets or sets the X position for the coordinate label.</summary>
    public double LabelX { get; set; }

    /// <summary>Gets or sets the Y position for the coordinate label.</summary>
    public double LabelY { get; set; }

    /// <summary>Gets or sets the hex radius/size.</summary>
    public double HexSize { get; set; }

    /// <summary>Gets the left edge for the label to center it.</summary>
    public double LabelLeft => LabelX - (HexSize * 0.5);

    /// <summary>Gets or sets a value indicating whether this hex is currently selected.</summary>
    public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }

    /// <summary>Gets or sets a value indicating whether edge 0 is selected.</summary>
    public bool IsEdge0Selected { get => _isEdge0Selected; set => SetProperty(ref _isEdge0Selected, value); }
    /// <summary>Gets or sets a value indicating whether edge 1 is selected.</summary>
    public bool IsEdge1Selected { get => _isEdge1Selected; set => SetProperty(ref _isEdge1Selected, value); }
    /// <summary>Gets or sets a value indicating whether edge 2 is selected.</summary>
    public bool IsEdge2Selected { get => _isEdge2Selected; set => SetProperty(ref _isEdge2Selected, value); }
    /// <summary>Gets or sets a value indicating whether edge 3 is selected.</summary>
    public bool IsEdge3Selected { get => _isEdge3Selected; set => SetProperty(ref _isEdge3Selected, value); }
    /// <summary>Gets or sets a value indicating whether edge 4 is selected.</summary>
    public bool IsEdge4Selected { get => _isEdge4Selected; set => SetProperty(ref _isEdge4Selected, value); }
    /// <summary>Gets or sets a value indicating whether edge 5 is selected.</summary>
    public bool IsEdge5Selected { get => _isEdge5Selected; set => SetProperty(ref _isEdge5Selected, value); }

    /// <summary>Gets or sets a value indicating whether this hex is highlighted as part of an LOS path.</summary>
    public bool IsHighlightedForLos { get => _isHighlightedForLos; set => SetProperty(ref _isHighlightedForLos, value); }

    /// <summary>Callback invoked when the terrain type changes.</summary>
    public Action? OnTerrainChanged { get; set; }

    /// <summary>Gets or sets a value indicating whether this hex has a manhole cover.</summary>
    public bool HasManhole { get => _metadata.HasManhole; set { _metadata.HasManhole = value; OnPropertyChanged(); } }

    /// <summary>Gets or sets a value indicating whether this hex has a stairwell.</summary>
    public bool HasStairwell { get => _metadata.HasStairwell; set { _metadata.HasStairwell = value; OnPropertyChanged(); } }

    /// <summary>Gets or sets the terrain type for this hex.</summary>
    public TerrainType Terrain
    {
        get => _metadata.Terrain;
        set 
        { 
            if (_metadata.Terrain != value)
            {
                _metadata.Terrain = value; 
                if (value != TerrainType.StoneBuilding && value != TerrainType.WoodenBuilding) HasStairwell = false;
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(BuildingSquareVisibility));
                OnPropertyChanged(nameof(BuildingSquareFill));
                OnPropertyChanged(nameof(IsGraveyard));
                OnPropertyChanged(nameof(IsCrag));
                OnPropertyChanged(nameof(IsPond));
                OnPropertyChanged(nameof(IsLumberyard));
                OnTerrainChanged?.Invoke();
            }
        }
    }

    /// <summary>Gets the visibility of the building square for this hex.</summary>
    public Visibility BuildingSquareVisibility => (Terrain == TerrainType.StoneBuilding || Terrain == TerrainType.WoodenBuilding) ? Visibility.Visible : Visibility.Collapsed;
    /// <summary>Gets the brush for the building square for this hex.</summary>
    public Brush BuildingSquareFill => Terrain == TerrainType.StoneBuilding ? Brushes.DimGray : Brushes.SaddleBrown;
    /// <summary>Gets the dimension of the building square.</summary>
    public double BuildingSquareSize => HexSize * 0.5;

    /// <summary>Gets a value indicating whether this hex is a graveyard.</summary>
    public bool IsGraveyard => Terrain == TerrainType.Graveyard;
    /// <summary>Gets a value indicating whether this hex is a crag.</summary>
    public bool IsCrag => Terrain == TerrainType.Crag;
    /// <summary>Gets a value indicating whether this hex is a pond.</summary>
    public bool IsPond => Terrain == TerrainType.Pond;
    /// <summary>Gets a value indicating whether this hex is a lumberyard.</summary>
    public bool IsLumberyard => Terrain == TerrainType.Lumberyard;
    /// <summary>Gets a value indicating whether this hex is wooden rubble.</summary>
    public bool IsWoodenRubble => Rubble == RubbleType.Wooden;
    /// <summary>Gets a value indicating whether this hex is stone rubble.</summary>
    public bool IsStoneRubble => Rubble == RubbleType.Stone;

    /// <summary>Gets the X coordinate of the top-left corner of the building square.</summary>
    public double BuildingSquareX => CenterX - (BuildingSquareSize / 2.0);
    /// <summary>Gets the Y coordinate of the top-left corner of the building square.</summary>
    public double BuildingSquareY => CenterY - (BuildingSquareSize / 2.0);

    /// <summary>Gets or sets the SVG path data for the inner hex (for elevation framing).</summary>
    public string InnerPoints { get; set; } = string.Empty;

    /// <summary>Gets a value indicating whether this hex is above ground level.</summary>
    public bool IsElevated => Elevation > 0;

    /// <summary>Gets or sets the elevation level for this hex.</summary>
    public int Elevation
    {
        get => _metadata.Elevation;
        set 
        { 
            if (_metadata.Elevation != value)
            {
                _metadata.Elevation = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(IsElevated));
            }
        }
    }

    /// <summary>Command invoked to select an edge of this hex.</summary>
    public ICommand SelectEdgeCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HexViewModel"/> class.
    /// </summary>
    public HexViewModel(int col, int row, CubeCoordinate location, string points, Point[] corners, string id, double cx, double cy, double ly, double size, ASLHexMetadata metadata, Action<HexViewModel, int> onSelectEdge)
    {
        Column = col;
        Row = row;
        Location = location;
        Points = points;
        HexCorners = corners;
        Id = id;
        CenterX = cx;
        CenterY = cy;
        LabelX = cx;
        LabelY = ly;
        HexSize = size;
        _metadata = metadata;

        // Optimized geometry generation
        InnerPoints = HexGeometryProvider.GenerateInnerHexPath(cx, cy, size);

        SelectEdgeCommand = new RelayCommand<string>(param => {
            if (int.TryParse(param, out int edgeIndex)) onSelectEdge?.Invoke(this, edgeIndex);
        });
    }

    /// <summary>Gets or sets a value indicating whether this hex contains shellholes.</summary>
    public bool HasShellholes { get => _metadata.HasShellholes; set { if (_metadata.HasShellholes != value) { _metadata.HasShellholes = value; OnPropertyChanged(); } } }

    /// <summary>Gets or sets the type of rubble in this hex.</summary>
    public RubbleType Rubble
    {
        get => _metadata.Rubble;
        set 
        { 
            if (_metadata.Rubble != value) 
            { 
                 _metadata.Rubble = value; 
                 OnPropertyChanged(); 
                 OnPropertyChanged(nameof(IsWoodenRubble));
                 OnPropertyChanged(nameof(IsStoneRubble));
                 OnTerrainChanged?.Invoke();
            } 
        }
    }
}
