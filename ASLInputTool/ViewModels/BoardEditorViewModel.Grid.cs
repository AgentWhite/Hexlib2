
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
        
        double multiplierW = 1.5 * (_board.Width - 1);
        if (!halfSides.HasFlag(BoardEdge.Left)) multiplierW += 1.0;
        if (!halfSides.HasFlag(BoardEdge.Right)) multiplierW += 1.0;
        
        double h_unit = Math.Sqrt(3);
        double multiplierH = h_unit * _board.Height; 
        
        if (multiplierW <= 0) multiplierW = 1.0;
        if (multiplierH <= 0) multiplierH = 1.0;

        double sizeW = _board.CanvasWidth / multiplierW;
        double sizeH = _board.CanvasHeight / multiplierH;
        
        _hexSize = Math.Min(sizeW, sizeH);
        UpdateLayout();
        
        if (double.IsNaN(_hexSize) || double.IsInfinity(_hexSize) || _hexSize < 0.1)
            _hexSize = 40.0;
        
        ActualGridWidth = multiplierW * _hexSize;
        ActualGridHeight = multiplierH * _hexSize;
        OnPropertyChanged(nameof(ActualGridWidth));
        OnPropertyChanged(nameof(ActualGridHeight));
    }

    private void GenerateHexGrid()
    {
        _hexes.Clear();
        
        foreach (var hex in _board.Board.Hexes.Values)
        {
            var center = _layout.HexToPixel(hex.Location);
            
            // Centralized geometry generation
            string points = HexGeometryProvider.GenerateHexPath(_layout, hex.Location, out var wpfCorners);

            var orientation = HexTopOrientation.FlatTopped;
            var (col, row) = hex.Location.ToOffset(orientation, _board.IsFirstColShiftedDown);

            double labelY = center.Y - (_hexSize * Math.Sqrt(3) / 2.0) + (_hexSize * 0.1); 
            var hexVm = new HexViewModel(col, row, hex.Location, points, wpfCorners, hex.Id, center.X, center.Y, labelY, _hexSize, hex.Metadata!, OnSelectEdge);
            hexVm.OnTerrainChanged = RefreshAllVisuals;
            _hexes.Add(hexVm);
        }
    }
}
