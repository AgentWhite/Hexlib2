using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Text;
using System.Globalization;
using ASL;
using HexLib;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel representing a single hex for rendering.
/// </summary>
public class HexViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the SVG path data for the hex outline.
    /// </summary>
    public string Points { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the absolute 6 vertices of the hex.
    /// </summary>
    public Point[] HexCorners { get; set; } = new Point[6];

    /// <summary>
    /// Gets the logical cube coordinate of this hex.
    /// </summary>
    public CubeCoordinate Location { get; }

    /// <summary>
    /// Gets or sets the column index.
    /// </summary>
    public int Column { get; set; }

    /// <summary>
    /// Gets or sets the row index.
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// Gets or sets the ASL coordinate ID (e.g., "A1").
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets the X coordinate of the hex center.
    /// </summary>
    public double CenterX { get; }

    /// <summary>
    /// Gets the Y coordinate of the hex center.
    /// </summary>
    public double CenterY { get; }

    /// <summary>
    /// Gets or sets the X position for the coordinate label.
    /// </summary>
    public double LabelX { get; set; }

    /// <summary>
    /// Gets or sets the Y position for the coordinate label.
    /// </summary>
    public double LabelY { get; set; }

    /// <summary>
    /// Gets or sets the hex radius/size.
    /// </summary>
    public double HexSize { get; set; }

    /// <summary>
    /// Gets the left edge for the label to center it.
    /// </summary>
    public double LabelLeft => LabelX - (HexSize * 0.5);

    private readonly ASLHexMetadata _metadata;
    private bool _isSelected;

    /// <summary>
    /// Gets or sets a value indicating whether this hex is currently selected.
    /// </summary>
    public bool IsSelected 
    { 
        get => _isSelected; 
        set => SetProperty(ref _isSelected, value); 
    }

    private bool _isEdge0Selected;
    public bool IsEdge0Selected { get => _isEdge0Selected; set => SetProperty(ref _isEdge0Selected, value); }

    private bool _isEdge1Selected;
    public bool IsEdge1Selected { get => _isEdge1Selected; set => SetProperty(ref _isEdge1Selected, value); }

    private bool _isEdge2Selected;
    public bool IsEdge2Selected { get => _isEdge2Selected; set => SetProperty(ref _isEdge2Selected, value); }

    private bool _isEdge3Selected;
    public bool IsEdge3Selected { get => _isEdge3Selected; set => SetProperty(ref _isEdge3Selected, value); }

    private bool _isEdge4Selected;
    public bool IsEdge4Selected { get => _isEdge4Selected; set => SetProperty(ref _isEdge4Selected, value); }

    private bool _isEdge5Selected;
    public bool IsEdge5Selected { get => _isEdge5Selected; set => SetProperty(ref _isEdge5Selected, value); }

    /// <summary>
    /// Callback invoked when the terrain type changes.
    /// </summary>
    public Action? OnTerrainChanged { get; set; }

    /// <summary>
    /// Gets or sets the terrain type for this hex.
    /// </summary>
    public TerrainType Terrain
    {
        get => _metadata.Terrain;
        set 
        { 
            if (_metadata.Terrain != value)
            {
                _metadata.Terrain = value; 
                OnPropertyChanged(); 
                OnPropertyChanged(nameof(BuildingSquareVisibility));
                OnPropertyChanged(nameof(BuildingSquareFill));
                OnTerrainChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// Gets the visibility of the building square for this hex.
    /// </summary>
    public Visibility BuildingSquareVisibility => (Terrain == TerrainType.StoneBuilding || Terrain == TerrainType.WoodenBuilding) ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Gets the brush for the building square for this hex.
    /// </summary>
    public Brush BuildingSquareFill => Terrain == TerrainType.StoneBuilding ? Brushes.DimGray : Brushes.SaddleBrown;

    /// <summary>
    /// Gets the dimension of the building square.
    /// </summary>
    public double BuildingSquareSize => HexSize * 0.5;

    /// <summary>
    /// Gets the X coordinate of the top-left corner of the building square.
    /// </summary>
    public double BuildingSquareX => CenterX - (BuildingSquareSize / 2.0);

    /// <summary>
    /// Gets the Y coordinate of the top-left corner of the building square.
    /// </summary>
    public double BuildingSquareY => CenterY - (BuildingSquareSize / 2.0);

    /// <summary>
    /// Gets or sets the SVG path data for the inner hex (for elevation framing).
    /// </summary>
    public string InnerPoints { get; set; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this hex is above ground level.
    /// </summary>
    public bool IsElevated => Elevation > 0;

    /// <summary>
    /// Gets or sets the elevation level for this hex.
    /// </summary>
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

    /// <summary>
    /// Command invoked to select an edge of this hex.
    /// </summary>
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
        LabelX = cx; // Usually labels are centered on X
        LabelY = ly;
        HexSize = size;
        _metadata = metadata;

        // Calculate inner points (80% scale) for elevation framing
        var sbInner = new StringBuilder();
        double innerScale = 0.8;
        for (int i = 0; i < 6; i++)
        {
            double angleDeg = 60 * i;
            double angleRad = Math.PI / 180 * angleDeg;
            double px = cx + (size * innerScale) * Math.Cos(angleRad);
            double py = cy + (size * innerScale) * Math.Sin(angleRad);
            
            if (i == 0) sbInner.Append("M "); else sbInner.Append("L ");
            sbInner.AppendFormat(CultureInfo.InvariantCulture, "{0:F2},{1:F2} ", px, py);
        }
        sbInner.Append("Z");
        InnerPoints = sbInner.ToString();

        SelectEdgeCommand = new RelayCommand<string>(param => {
            if (int.TryParse(param, out int edgeIndex))
            {
                onSelectEdge?.Invoke(this, edgeIndex);
            }
        });
    }
}
