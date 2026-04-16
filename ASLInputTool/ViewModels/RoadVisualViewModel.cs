using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a road segment to be rendered on the board.
/// </summary>
public class RoadVisualViewModel : ViewModelBase
{
    /// <summary>Gets or sets the X coordinate of the first endpoint.</summary>
    public double X1 { get; set; }

    /// <summary>Gets or sets the Y coordinate of the first endpoint.</summary>
    public double Y1 { get; set; }

    /// <summary>Gets or sets the X coordinate of the second endpoint.</summary>
    public double X2 { get; set; }

    /// <summary>Gets or sets the Y coordinate of the second endpoint.</summary>
    public double Y2 { get; set; }

    /// <summary>Gets or sets the stroke brush for the road line.</summary>
    public Brush Stroke { get; set; } = Brushes.Black;

    /// <summary>Gets or sets the thickness of the road line.</summary>
    public double Thickness { get; set; } = 4.0;
}
