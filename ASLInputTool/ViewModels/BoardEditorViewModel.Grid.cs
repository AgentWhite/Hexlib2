using System;
using System.Globalization;
using System.Text;
using System.Windows;
using ASL;
using HexLib;

namespace ASLInputTool.ViewModels;

public partial class BoardEditorViewModel
{
    /// <summary>Gets the actual width of the hex grid in pixels.</summary>
    public double ActualGridWidth { get; private set; }
    /// <summary>Gets the actual height of the hex grid in pixels.</summary>
    public double ActualGridHeight { get; private set; }

    private void RecalculateHexSize()
    {
        var halfSides = _board.Board.HalfHexSides;
        
        // Horizontal Span Calculation:
        // In a Flat-Topped hex grid:
        // - Each hex has a width of 2S (where S is the circumradius).
        // - The horizontal distance between adjacent columns is exactly 1.5S.
        // - The first and last hexes contribute an extra 0.5S to the total span.
        // - If a board side is NOT halved, we add a full extra S to that side to ensure the hex isn't clipped.
        double multiplierW = 1.5 * (_board.Width - 1);
        if (!halfSides.HasFlag(BoardEdge.Left)) multiplierW += 1.0;
        if (!halfSides.HasFlag(BoardEdge.Right)) multiplierW += 1.0;
        
        // Vertical Span Calculation:
        // In a Flat-Topped hex grid:
        // - Each hex has a height of sqrt(3) * S.
        // - The vertical distance between adjacent rows in the same column is sqrt(3) * S.
        // - Staggering shifts adjacent columns vertically by 0.5 * sqrt(3) * S.
        // - Total vertical units = N * sqrt(3), accounting for the full height of the staggered grid.
        double h_unit = Math.Sqrt(3);
        double multiplierH = h_unit * _board.Height; 
        
        // Final pixel dimensions = Multiplier * HexSize.
        // The View background rectangle uses these values to perfectly frame the hex grid.

        // Safety check to avoid division by zero or negative multipliers
        if (multiplierW <= 0) multiplierW = 1.0;
        if (multiplierH <= 0) multiplierH = 1.0;

        // Calculate maximum possible hex size to fit within both dimensions
        double sizeW = _board.CanvasWidth / multiplierW;
        double sizeH = _board.CanvasHeight / multiplierH;
        
        _hexSize = Math.Min(sizeW, sizeH);
        
        // Final sanity check for size
        if (double.IsNaN(_hexSize) || double.IsInfinity(_hexSize) || _hexSize < 0.1)
            _hexSize = 40.0;
        
        // Update Actual Dimensions for the View (so the background white rectangle matches the grid exactly)
        ActualGridWidth = multiplierW * _hexSize;
        ActualGridHeight = multiplierH * _hexSize;
        OnPropertyChanged(nameof(ActualGridWidth));
        OnPropertyChanged(nameof(ActualGridHeight));
    }

    private void GenerateHexGrid()
    {
        _hexes.Clear();
        var orientation = HexTopOrientation.FlatTopped;
        var halfSides = _board.Board.HalfHexSides;
        
        foreach (var hex in _board.Board.Hexes.Values)
        {
            var (col, row) = hex.Location.ToOffset(orientation, _board.IsFirstColShiftedDown);
            
            // Calculate pixel center for FlatTopped (Odd-Q or Even-Q)
            // Stagger: Determine which columns are shifted down by 0.5h
            bool isShiftedDown = _board.Board.ShiftingOddColumns ? (col % 2 == 1) : (col % 2 == 0);
            double x = _hexSize * 1.5 * col;
            double y = _hexSize * Math.Sqrt(3) * (row + (isShiftedDown ? 0.5 : 0));
            
            // Adjust origin based on Half-Hex Sides
            if (!halfSides.HasFlag(BoardEdge.Left))
                x += _hexSize;

            if (!halfSides.HasFlag(BoardEdge.Top))
                y += _hexSize * Math.Sqrt(3) / 2;

            var (pointsString, corners) = GetHexPoints(x, y, _hexSize);
            double labelY = y - (_hexSize * Math.Sqrt(3) / 2.0) + (_hexSize * 0.1); 
            var hexVm = new HexViewModel(col, row, hex.Location, pointsString, corners, hex.Id, x, y, labelY, _hexSize, hex.Metadata!, OnSelectEdge);
            hexVm.OnTerrainChanged = RefreshAllVisuals;
            _hexes.Add(hexVm);
        }
    }

    private (string pointsString, Point[] corners) GetHexPoints(double centerX, double centerY, double size)
    {
        var sb = new StringBuilder();
        var corners = new Point[6];
        for (int i = 0; i < 6; i++)
        {
            double angleDeg = 60 * i; // Flat topped starts at 0 deg
            double angleRad = Math.PI / 180 * angleDeg;
            double px = centerX + size * Math.Cos(angleRad);
            double py = centerY + size * Math.Sin(angleRad);
            
            corners[i] = new Point(px, py);

            if (i == 0) sb.Append("M "); else sb.Append("L ");
            sb.AppendFormat(CultureInfo.InvariantCulture, "{0:F2},{1:F2} ", px, py);
        }
        sb.Append("Z");
        return (sb.ToString(), corners);
    }
}
