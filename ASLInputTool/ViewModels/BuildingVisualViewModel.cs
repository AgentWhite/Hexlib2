using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Base class for building visual elements.
/// </summary>
public abstract class BuildingVisualBase : ViewModelBase
{
    public double CanvasX { get; set; } = 0;
    public double CanvasY { get; set; } = 0;
    public Brush Fill { get; set; } = Brushes.SaddleBrown;
}

/// <summary>
/// Represents a square building centered in a hex.
/// </summary>
public class BuildingSquareViewModel : BuildingVisualBase
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Size { get; set; }
}

/// <summary>
/// Represents a rectangular connector between two building hexes.
/// </summary>
public class BuildingConnectorViewModel : BuildingVisualBase
{
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
    public double Thickness { get; set; }
}

/// <summary>
/// Represents a black divider line for rowhouses.
/// </summary>
public class BuildingDividerViewModel : BuildingVisualBase
{
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
    public double Thickness { get; set; } = 2.0;

    public BuildingDividerViewModel()
    {
        Fill = Brushes.Black;
    }
}
