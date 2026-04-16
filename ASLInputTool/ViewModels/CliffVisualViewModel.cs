using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a jagged cliff visual element.
/// </summary>
public class CliffVisualViewModel : ViewModelBase
{
    public Geometry Data { get; set; }
    public Brush Stroke { get; set; } = new SolidColorBrush(Color.FromRgb(60, 40, 20)); // Dark Brown/Black
    public double Thickness { get; set; } = 4.0;

    public CliffVisualViewModel(Geometry geometry)
    {
        Data = geometry;
    }
}
