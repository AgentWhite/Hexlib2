using System.Windows;
using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a boundary visual element (Wall, Hedge, Bocage).
/// </summary>
public class HexsideTerrainVisualViewModel : ViewModelBase
{
    public Point P1 { get; set; }
    public Point P2 { get; set; }
    public Brush Stroke { get; set; } = Brushes.Gray;
    public double Thickness { get; set; } = 4.0;
    public DoubleCollection? DashArray { get; set; }

    public HexsideTerrainVisualViewModel(Point p1, Point p2, Brush stroke, double thickness, DoubleCollection? dashArray = null)
    {
        P1 = p1;
        P2 = p2;
        Stroke = stroke;
        Thickness = thickness;
        DashArray = dashArray;
    }
}
