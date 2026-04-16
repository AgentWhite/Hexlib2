using System.Windows;
using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a boundary visual element (Wall, Hedge, Bocage).
/// </summary>
public class HexsideTerrainVisualViewModel : ViewModelBase
{
    /// <summary>Gets or sets the first endpoint of the boundary line.</summary>
    public Point P1 { get; set; }

    /// <summary>Gets or sets the second endpoint of the boundary line.</summary>
    public Point P2 { get; set; }

    /// <summary>Gets or sets the stroke brush for the boundary line.</summary>
    public Brush Stroke { get; set; } = Brushes.Gray;

    /// <summary>Gets or sets the thickness of the boundary line.</summary>
    public double Thickness { get; set; } = 4.0;

    /// <summary>Gets or sets the dash pattern for the boundary line.</summary>
    public DoubleCollection? DashArray { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HexsideTerrainVisualViewModel"/> class.
    /// </summary>
    /// <param name="p1">The first endpoint.</param>
    /// <param name="p2">The second endpoint.</param>
    /// <param name="stroke">The stroke brush.</param>
    /// <param name="thickness">The thickness.</param>
    /// <param name="dashArray">The dash pattern.</param>
    public HexsideTerrainVisualViewModel(Point p1, Point p2, Brush stroke, double thickness, DoubleCollection? dashArray = null)
    {
        P1 = p1;
        P2 = p2;
        Stroke = stroke;
        Thickness = thickness;
        DashArray = dashArray;
    }
}
