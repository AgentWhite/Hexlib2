using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a jagged cliff visual element.
/// </summary>
public class CliffVisualViewModel : ViewModelBase
{
    /// <summary>Gets or sets the geometry data for the cliff.</summary>
    public Geometry PathData { get; set; }

    /// <summary>Gets or sets the stroke brush for the cliff line.</summary>
    public Brush Stroke { get; set; } = new SolidColorBrush(Color.FromRgb(60, 40, 20)); // Dark Brown/Black

    /// <summary>Gets or sets the thickness of the cliff line.</summary>
    public double Thickness { get; set; } = 4.0;

    /// <summary>
    /// Initializes a new instance of the <see cref="CliffVisualViewModel"/> class.
    /// </summary>
    /// <param name="geometry">The geometry data.</param>
    public CliffVisualViewModel(Geometry geometry)
    {
        PathData = geometry;
    }
}
