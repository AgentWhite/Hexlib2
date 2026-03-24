using System.Windows.Media;
using System.Windows;
using System.Text;
using System.Globalization;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="HexViewModel"/> class.
    /// </summary>
    /// <param name="col">Column index.</param>
    /// <param name="row">Row index.</param>
    /// <param name="points">SVG path data.</param>
    public HexViewModel(int col, int row, string points, string id, double lx, double ly, double size)
    {
        Column = col;
        Row = row;
        Points = points;
        Id = id;
        LabelX = lx;
        LabelY = ly;
        HexSize = size;
    }
}
