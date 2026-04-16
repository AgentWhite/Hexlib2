using ASL.Models.Maps;
using HexLib;

namespace ASL;

/// <summary>
/// Represents a map board in the ASL system.
/// </summary>
public class AslBoard
{
    private Board<ASLHexMetadata, ASLEdgeData> _board;
    private string _name = string.Empty;
    private bool _isFirstColShiftedDown = true;

    /// <summary>
    /// Gets or sets the name/identifier of the map (e.g., "1", "4", "62").
    /// </summary>
    public string Name 
    { 
        get => _name; 
        set 
        { 
            _name = value; 
            if (_board != null) _board.Name = value; 
        } 
    }

    /// <summary>
    /// Gets or sets the type of this map.
    /// </summary>
    public MapType Type { get; set; } = MapType.Standard;

    /// <summary>
    /// Gets or sets a value indicating whether the first column (A) is shifted down relative to the second (B).
    /// </summary>
    public bool IsFirstColShiftedDown
    {
        get => _isFirstColShiftedDown;
        set
        {
            _isFirstColShiftedDown = value;
            if (_board != null) 
            {
                _board.ShiftingOddColumns = !value;
                RefreshIds();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the top edge contains half-hexes.
    /// </summary>
    public bool IsTopRowHalf { get => _isTopRowHalf; set { _isTopRowHalf = value; SyncHalfHexSides(); RefreshIds(); } }
    private bool _isTopRowHalf = true;

    /// <summary>
    /// Gets or sets a value indicating whether the bottom edge contains half-hexes.
    /// </summary>
    public bool IsBottomRowHalf { get => _isBottomRowHalf; set { _isBottomRowHalf = value; SyncHalfHexSides(); } }
    private bool _isBottomRowHalf = true;

    /// <summary>
    /// Gets or sets a value indicating whether the left edge contains half-hexes.
    /// </summary>
    public bool IsLeftEdgeHalf { get => _isLeftEdgeHalf; set { _isLeftEdgeHalf = value; SyncHalfHexSides(); } }
    private bool _isLeftEdgeHalf = true;

    /// <summary>
    /// Gets or sets a value indicating whether the right edge contains half-hexes.
    /// </summary>
    public bool IsRightEdgeHalf { get => _isRightEdgeHalf; set { _isRightEdgeHalf = value; SyncHalfHexSides(); } }
    private bool _isRightEdgeHalf = true;

    private void SyncHalfHexSides()
    {
        if (_board == null) return;
        _board.HalfHexSides = BoardEdge.None;
        if (IsTopRowHalf) _board.HalfHexSides |= BoardEdge.Top;
        if (IsBottomRowHalf) _board.HalfHexSides |= BoardEdge.Bottom;
        if (IsLeftEdgeHalf) _board.HalfHexSides |= BoardEdge.Left;
        if (IsRightEdgeHalf) _board.HalfHexSides |= BoardEdge.Right;
    }
    

    /// <summary>
    /// Gets or sets the hex width of the map.
    /// </summary>
    public int Width { get; set; } = 33;

    /// <summary>
    /// Gets or sets the hex height of the map.
    /// </summary>
    public int Height { get; set; } = 10;

    /// <summary>
    /// Gets or sets the width of the canvas in pixels.
    /// </summary>
    public int CanvasWidth { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the height of the canvas in pixels.
    /// </summary>
    public int CanvasHeight { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the underlying HexLib board.
    /// </summary>
    public Board<ASLHexMetadata, ASLEdgeData> Board
    {
        get => _board;
        set => _board = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AslBoard"/> class.
    /// </summary>
    public AslBoard() 
    {
        _board = new Board<ASLHexMetadata, ASLEdgeData>(Width, Height, HexTopOrientation.FlatTopped);
    }

    /// <summary>
    /// Generates an ASL coordinate string (e.g., "A1", "AA10") from column and row indices.
    /// </summary>
    /// <param name="col">0-based column index.</param>
    /// <param name="row">0-based row index.</param>
    /// <param name="shiftingOddColumns">If true, rows are offset for hexagonal coordinate systems.</param>
    /// <param name="isTopRowHalf">If true, the top row contains half-hexes (index 0 becomes 0 if high column).</param>
    /// <returns>The ASL coordinate string.</returns>
    public static string GetAslCoordinate(int col, int row, bool shiftingOddColumns, bool isTopRowHalf)
    {
        int letterIndex = col % 26;
        int repeatCount = col / 26 + 1;
        char letter = (char)('A' + letterIndex);
        string colStr = new string(letter, repeatCount);
        
        // ASL standard labeling:
        // If a column starts with a half-hex at top-center (a "High" column), that half-hex is labeled 0.
        // Even columns (0, 2...) are "High" if shiftingOddColumns is true.
        // Odd columns (1, 3...) are "High" if shiftingOddColumns is false.
        bool isHighColumn = (col % 2 == (shiftingOddColumns ? 0 : 1));
        
        int rowLabel;
        if (isTopRowHalf && isHighColumn)
        {
            rowLabel = row; // row 0 -> "0", row 1 -> "1"
        }
        else
        {
            rowLabel = row + 1; // row 0 -> "1" (Standard)
        }
        
        return $"{colStr}{rowLabel}";
    }

    /// <summary>
    /// Populates the underlying HexLib board with hexes based on current Width and Height.
    /// </summary>
    public void PopulateBoard()
    {
        // Set Presets for Standard maps only if the board is not yet initialized or dimensions are zero.
        // This avoids overwriting settings loaded from disk.
        if (_board == null || Width == 0 || Height == 0)
        {
            if (Type == MapType.Standard)
            {
                Width = 33;
                Height = 10;
                IsFirstColShiftedDown = true; // Column A is Low, Column B is High/Halved
                IsTopRowHalf = true;
                IsBottomRowHalf = true;
                IsLeftEdgeHalf = true;
                IsRightEdgeHalf = true;
            }
            else if (Type == MapType.HalfBoard || Type == MapType.BonusPack || Type == MapType.StarterPack)
            {
                Width = 17;
                if (Type == MapType.HalfBoard) Height = 10;
                else if (Type == MapType.BonusPack) Height = 20;
                else if (Type == MapType.StarterPack) Height = 22;

                IsFirstColShiftedDown = true;
                IsTopRowHalf = true;
                IsBottomRowHalf = true;
                IsLeftEdgeHalf = true;
                IsRightEdgeHalf = true;
            }
        }

        // Re-initialize board if dimensions change. 
        // Orientation is hardcoded to FlatTopped for ASL.
        if (_board == null || _board.Width != Width || _board.Height != Height)
        {
            _board = new Board<ASLHexMetadata, ASLEdgeData>(Width, Height, HexTopOrientation.FlatTopped);
        }
        
        // Sync properties that the internal board tracks
        _board.Name = Name;
        _board.ShiftingOddColumns = !IsFirstColShiftedDown;

        // Apply HalfHexSides to underlying board
        _board.HalfHexSides = BoardEdge.None;
        if (IsTopRowHalf) _board.HalfHexSides |= BoardEdge.Top;
        if (IsBottomRowHalf) _board.HalfHexSides |= BoardEdge.Bottom;
        if (IsLeftEdgeHalf) _board.HalfHexSides |= BoardEdge.Left;
        if (IsRightEdgeHalf) _board.HalfHexSides |= BoardEdge.Right;

        for (int r = 0; r < Height; r++)
        {
            for (int c = 0; c < Width; c++)
            {
                AddHexIfMissing(c, r);
            }
        }

        // Fill gaps to ensure straight board boundaries
        // Standard ASL: Even columns are high (halved at top/bottom center).
        // If ShiftingOddColumns is true: Col 0, 2... are high.
        // If ShiftingOddColumns is false: Col 1, 3... are high.
        int firstHighCol = _board.ShiftingOddColumns ? 0 : 1;

        if (_board.HalfHexSides.HasFlag(BoardEdge.Bottom))
        {
            for (int c = firstHighCol; c < Width; c += 2) AddHexIfMissing(c, Height);
        }
        
        // Note: Odd columns (B, D...) are full-hexes at both top (r=0) and bottom (r=Height-1)
        // when Height is the units of height.
    }

    private void AddHexIfMissing(int c, int r)
    {
        var cube = HexMath.OffsetToCube(c, r, HexTopOrientation.FlatTopped, IsFirstColShiftedDown);
        var hex = _board.GetHexAt(cube);
        if (hex == null)
        {
            hex = new Hex<ASLHexMetadata>(cube)
            {
                Id = GetAslCoordinate(c, r, _board.ShiftingOddColumns, IsTopRowHalf)
            };
            _board.AddHex(hex);
        }
        else
        {
            // Update or restore ID based on current structural rules
            hex.Id = GetAslCoordinate(c, r, _board.ShiftingOddColumns, IsTopRowHalf);
        }

        if (hex.Metadata == null)
        {
            hex.Metadata = new ASLHexMetadata { Terrain = TerrainType.OpenGround };
        }
    }

    /// <summary>
    /// Re-calculates and applies ASL coordinate IDs to all hexes on the board.
    /// Should be called after changing structural properties like IsTopRowHalf.
    /// </summary>
    public void RefreshIds()
    {
        if (_board == null) return;

        var orientation = HexTopOrientation.FlatTopped;
        foreach (var hex in _board.Hexes.Values)
        {
            var (col, row) = hex.Location.ToOffset(orientation, IsFirstColShiftedDown);
            hex.Id = GetAslCoordinate(col, row, !IsFirstColShiftedDown, IsTopRowHalf);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AslBoard"/> class with a name.
    /// </summary>
    public AslBoard(string name) : this()
    {
        Name = name;
    }
}
