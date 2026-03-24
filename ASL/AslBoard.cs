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

    /// <summary>
    /// Gets or sets the hex width of the map.
    /// </summary>
    public int Width { get; set; } = 33;

    /// <summary>
    /// Gets or sets the hex height of the map.
    /// </summary>
    public int Height { get; set; } = 11;

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

        // Set HalfHexSides for Standard boards
        if (Type == MapType.Standard)
        {
            _board.HalfHexSides = BoardEdge.Top | BoardEdge.Bottom | BoardEdge.Left | BoardEdge.Right;
        }
        else
        {
            _board.HalfHexSides = BoardEdge.None;
        }

        for (int r = 0; r < Height; r++)
        {
            for (int c = 0; c < Width; c++)
            {
                AddHexIfMissing(c, r);
            }
        }

        // Fill gaps for Top/Bottom HalfHexSides
        if (_board.HalfHexSides.HasFlag(BoardEdge.Top))
        {
            // For FlatTopped Odd-Q, Odd columns (1, 3...) have a gap at the top (Row -1)
            for (int c = 1; c < Width; c += 2) AddHexIfMissing(c, -1);
        }
        if (_board.HalfHexSides.HasFlag(BoardEdge.Bottom))
        {
            // Even columns (0, 2...) have a gap at the bottom (Row Height)
            for (int c = 0; c < Width; c += 2) AddHexIfMissing(c, Height);
        }
    }

    private void AddHexIfMissing(int c, int r)
    {
        var cube = HexMath.OffsetToCube(c, r, HexTopOrientation.FlatTopped);
        if (_board.GetHexAt(cube) == null)
        {
            _board.AddHex(new Hex<ASLHexMetadata>(cube));
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
