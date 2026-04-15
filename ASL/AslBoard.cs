using ASL.Models.Maps;
using HexLib;

namespace ASL;

/// <summary>
/// Represents a map board in the ASL system.
/// </summary>
public class AslBoard
{
    private Board<ASLHexMetadata, ASLEdgeData> _board;

    /// <summary>
    /// Gets or sets the name/identifier of the map (e.g., "1", "4", "62").
    /// </summary>
    public string Name 
    { 
        get => Board.Name; 
        set => Board.Name = value; 
    }

    /// <summary>
    /// Gets or sets the type of this map.
    /// </summary>
    public MapType Type { get; set; } = MapType.Standard;

    public bool IsFirstColShiftedDown
    {
        get => !Board.ShiftingOddColumns;
        set => Board.ShiftingOddColumns = !value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the top edge contains half-hexes.
    /// </summary>
    public bool IsTopRowHalf { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the bottom edge contains half-hexes.
    /// </summary>
    public bool IsBottomRowHalf { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the left edge contains half-hexes.
    /// </summary>
    public bool IsLeftEdgeHalf { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the right edge contains half-hexes.
    /// </summary>
    public bool IsRightEdgeHalf { get; set; } = true;

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
    /// <returns>The ASL coordinate string.</returns>
    public static string GetAslCoordinate(int col, int row, bool shiftingOddColumns = true)
    {
        int letterIndex = col % 26;
        int repeatCount = col / 26 + 1;
        char letter = (char)('A' + letterIndex);
        string colStr = new string(letter, repeatCount);
        
        // ASL standard labeling simply uses row numbers 1..10
        // regardless of staggering offset.
        int rowLabel = row + 1;
        
        return $"{colStr}{rowLabel}";
    }

    /// <summary>
    /// Populates the underlying HexLib board with hexes based on current Width and Height.
    /// </summary>
    public void PopulateBoard()
    {
        // Re-initialize board if dimensions or properties have changed
        if (_board.Width != Width || _board.Height != Height || _board.TopOrientation != HexTopOrientation.FlatTopped)
        {
            _board = new Board<ASLHexMetadata, ASLEdgeData>(Width, Height, HexTopOrientation.FlatTopped);
            _board.Name = Name;
        }

        // Set HalfHexSides for Standard boards (8"x22", 33 columns A-GG, 10 rows 1-10)
        // Standard boards are halved at A/GG and Top/Bottom (High cols halved).
        if (Type == MapType.Standard)
        {
            IsFirstColShiftedDown = true;
            IsTopRowHalf = true;
            IsBottomRowHalf = true;
            IsLeftEdgeHalf = true;
            IsRightEdgeHalf = true;
        }

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
        var cube = HexMath.OffsetToCube(c, r, HexTopOrientation.FlatTopped);
        if (_board.GetHexAt(cube) == null)
        {
            var hex = new Hex<ASLHexMetadata>(cube);
            hex.Id = GetAslCoordinate(c, r, _board.ShiftingOddColumns);
            _board.AddHex(hex);
        }

        // Ensure metadata is ALWAYS initialized
        var finalHex = _board.GetHexAt(cube);
        if (finalHex != null)
        {
            finalHex.Metadata ??= new ASLHexMetadata();
        }
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="AslBoard"/> class with a name.
    /// </summary>
    /// <param name="name">The name of the board.</param>
    public AslBoard(string name) : this()
    {
        Name = name;
    }
}
