using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a road segment to be rendered on the board.
/// </summary>
public class RoadVisualViewModel : ViewModelBase
{
    public double X1 { get; set; }
    public double Y1 { get; set; }
    public double X2 { get; set; }
    public double Y2 { get; set; }
    public Brush Stroke { get; set; } = Brushes.Gray;
    public double Thickness { get; set; } = 4.0;
}
