using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Base class for building visual elements.
/// </summary>
public abstract class BuildingVisualBase : ViewModelBase
{
    /// <summary>Gets or sets the X position on the canvas.</summary>
    public double CanvasX { get; set; } = 0;

    /// <summary>Gets or sets the Y position on the canvas.</summary>
    public double CanvasY { get; set; } = 0;

    /// <summary>Gets or sets the fill brush for the visual.</summary>
    public Brush Fill { get; set; } = Brushes.SaddleBrown;
}

/// <summary>
/// Represents a square building centered in a hex.
/// </summary>
public class BuildingSquareViewModel : BuildingVisualBase
{
    /// <summary>Gets or sets the X coordinate.</summary>
    public double X { get; set; }

    /// <summary>Gets or sets the Y coordinate.</summary>
    public double Y { get; set; }

    /// <summary>Gets or sets the size of the square.</summary>
    public double Size { get; set; }
}

/// <summary>
/// Represents a rectangular connector between two building hexes.
/// </summary>
public class BuildingConnectorViewModel : BuildingVisualBase
{
    /// <summary>Gets or sets the X coordinate of the first point.</summary>
    public double X1 { get; set; }

    /// <summary>Gets or sets the Y coordinate of the first point.</summary>
    public double Y1 { get; set; }

    /// <summary>Gets or sets the X coordinate of the second point.</summary>
    public double X2 { get; set; }

    /// <summary>Gets or sets the Y coordinate of the second point.</summary>
    public double Y2 { get; set; }

    /// <summary>Gets or sets the thickness of the connector.</summary>
    public double Thickness { get; set; }
}

/// <summary>
/// Represents a black divider line for rowhouses.
/// </summary>
public class BuildingDividerViewModel : BuildingVisualBase
{
    /// <summary>Gets or sets the X coordinate of the first point.</summary>
    public double X1 { get; set; }

    /// <summary>Gets or sets the Y coordinate of the first point.</summary>
    public double Y1 { get; set; }

    /// <summary>Gets or sets the X coordinate of the second point.</summary>
    public double X2 { get; set; }

    /// <summary>Gets or sets the Y coordinate of the second point.</summary>
    public double Y2 { get; set; }

    /// <summary>Gets or sets the thickness of the divider line.</summary>
    public double Thickness { get; set; } = 2.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="BuildingDividerViewModel"/> class.
    /// </summary>
    public BuildingDividerViewModel()
    {
        Fill = Brushes.Black;
    }
}
