using System;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Text;
using System.Globalization;
using ASL;
using HexLib;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the board editor canvas and toolbar.
/// </summary>
public class BoardEditorViewModel : ViewModelBase
{
    private readonly AslBoard _board;
    private double _hexSize = 40.0;
    private ObservableCollection<HexViewModel> _hexes = new();

    /// <summary>
    /// Gets the underlying ASL board being edited.
    /// </summary>
    public AslBoard Board => _board;

    /// <summary>
    /// Gets the collection of hexes for rendering.
    /// </summary>
    public ObservableCollection<HexViewModel> Hexes => _hexes;

    /// <summary>
    /// Gets or sets the size of the hexes (radius).
    /// </summary>
    public double HexSize
    {
        get => _hexSize;
        set { if (SetProperty(ref _hexSize, value)) GenerateHexGrid(); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardEditorViewModel"/> class.
    /// </summary>
    /// <param name="board">The board to edit.</param>
    public BoardEditorViewModel(AslBoard board)
    {
        _board = board;
        DisplayName = "Board Editor";
        RecalculateHexSize();
        GenerateHexGrid();
    }

    private void RecalculateHexSize()
    {
        var halfSides = _board.Board.HalfHexSides;
        
        // Horizontal span calculation:
        // center-to-center steps are 1.5S each.
        // Plus left overhang (S if full, 0 if halved) and right overhang (S if full, 0 if halved).
        double multiplierW = 1.5 * (_board.Width - 1);
        if (!halfSides.HasFlag(BoardEdge.Left)) multiplierW += 1.0;
        if (!halfSides.HasFlag(BoardEdge.Right)) multiplierW += 1.0;
        
        // Vertical span calculation:
        // Distance between rows is sqrt(3)*S.
        // For a standard board (Height rows), centers range from 0 to Height*H.
        // If bottom is NOT halved, we need an extra 0.5H at the end.
        // If top is NOT halved, we start at 0.5H, so we add 0.5H at the start.
        double h_unit = Math.Sqrt(3);
        double multiplierH = h_unit * _board.Height;
        if (!halfSides.HasFlag(BoardEdge.Top)) multiplierH += h_unit * 0.5;
        if (!halfSides.HasFlag(BoardEdge.Bottom)) multiplierH += h_unit * 0.5;

        // Calculate maximum possible hex size to fit within both dimensions
        double sizeW = _board.CanvasWidth / multiplierW;
        double sizeH = _board.CanvasHeight / multiplierH;
        
        _hexSize = Math.Min(sizeW, sizeH);
        
        // Update Actual Dimensions for the View (so the background white rectangle matches the grid exactly)
        ActualGridWidth = multiplierW * _hexSize;
        ActualGridHeight = multiplierH * _hexSize;
        OnPropertyChanged(nameof(ActualGridWidth));
        OnPropertyChanged(nameof(ActualGridHeight));
    }

    public double ActualGridWidth { get; private set; }
    public double ActualGridHeight { get; private set; }

    private void GenerateHexGrid()
    {
        _hexes.Clear();
        var orientation = HexTopOrientation.FlatTopped;
        var halfSides = _board.Board.HalfHexSides;
        
        foreach (var hex in _board.Board.Hexes.Values)
        {
            var (col, row) = hex.Location.ToOffset(orientation);
            
            // Calculate pixel center for FlatTopped (Odd-Q)
            // Stagger: Determine which columns are shifted down by 0.5h
            bool isShiftedDown = _board.Board.ShiftingOddColumns ? (col % 2 == 1) : (col % 2 == 0);
            double x = _hexSize * 1.5 * col;
            double y = _hexSize * Math.Sqrt(3) * (row + (isShiftedDown ? 0.5 : 0));
            
            // Adjust origin based on Half-Hex Sides
            if (!halfSides.HasFlag(BoardEdge.Left))
                x += _hexSize;

            if (!halfSides.HasFlag(BoardEdge.Top))
                y += _hexSize * Math.Sqrt(3) / 2;

            var points = GetHexPoints(x, y, _hexSize);
            double labelY = y - (_hexSize * Math.Sqrt(3) / 2.0) + (_hexSize * 0.1); 
            _hexes.Add(new HexViewModel(col, row, points, hex.Id, x, labelY, _hexSize));
        }
    }

    private string GetHexPoints(double centerX, double centerY, double size)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 6; i++)
        {
            double angleDeg = 60 * i; // Flat topped starts at 0 deg
            double angleRad = Math.PI / 180 * angleDeg;
            double px = centerX + size * Math.Cos(angleRad);
            double py = centerY + size * Math.Sin(angleRad);
            
            if (i == 0) sb.Append("M "); else sb.Append("L ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2},{1:F2} ", px, py);
        }
        sb.Append("Z");
        return sb.ToString();
    }
}
