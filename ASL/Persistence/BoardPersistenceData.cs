using ASL.Models.Units;
using ASL.Models.Board;
using HexLib;
using System.Collections.Generic;
using System.Linq;

namespace ASL.Persistence;

/// <summary>
/// Data Transfer Object for persisting a complete board.
/// </summary>
public class BoardDto
{
    /// <summary>Gets or sets the name of the board.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Gets or sets the width of the board.</summary>
    public int Width { get; set; }
    /// <summary>Gets or sets the height of the board.</summary>
    public int Height { get; set; }
    /// <summary>Gets or sets the orientation of the board.</summary>
    public HexTopOrientation TopOrientation { get; set; }
    /// <summary>Gets or sets a value indicating whether odd columns are shifted down.</summary>
    public bool ShiftingOddColumns { get; set; }
    /// <summary>Gets or sets the bits representing which sides are halved.</summary>
    public BoardEdge HalfHexSides { get; set; }
    /// <summary>Gets or sets the collection of hexes on the board.</summary>
    public List<HexDto> Hexes { get; set; } = new();
    /// <summary>Gets or sets the collection of modified edges on the board.</summary>
    public List<EdgeDto> Edges { get; set; } = new();

    /// <summary>
    /// Creates a BoardDto from an existing Board object.
    /// </summary>
    public static BoardDto FromBoard(Board<ASLHexMetadata, ASLEdgeData> board)
    {
        var dto = new BoardDto
        {
            Name = board.Name,
            Width = board.Width,
            Height = board.Height,
            TopOrientation = board.TopOrientation,
            ShiftingOddColumns = board.ShiftingOddColumns,
            HalfHexSides = board.HalfHexSides
        };

        foreach (var kvp in board.Hexes)
        {
            dto.Hexes.Add(new HexDto
            {
                Q = kvp.Key.Q,
                R = kvp.Key.R,
                S = kvp.Key.S,
                Id = kvp.Value.Id,
                Metadata = kvp.Value.Metadata ?? new ASLHexMetadata(),
                Counters = kvp.Value.Counters.OfType<Unit>().ToList()
            });
        }

        foreach (var kvp in board.Edges)
        {
            dto.Edges.Add(new EdgeDto
            {
                Q1 = kvp.Key.Item1.Q,
                R1 = kvp.Key.Item1.R,
                S1 = kvp.Key.Item1.S,
                Q2 = kvp.Key.Item2.Q,
                R2 = kvp.Key.Item2.R,
                S2 = kvp.Key.Item2.S,
                Data = kvp.Value
            });
        }

        return dto;
    }

    /// <summary>
    /// Converts this DTO back to a functional Board object.
    /// </summary>
    public Board<ASLHexMetadata, ASLEdgeData> ToBoard()
    {
        var board = new Board<ASLHexMetadata, ASLEdgeData>(Width, Height, TopOrientation)
        {
            Name = Name,
            ShiftingOddColumns = ShiftingOddColumns,
            HalfHexSides = HalfHexSides
        };

        foreach (var hexDto in Hexes)
        {
            var coord = new CubeCoordinate(hexDto.Q, hexDto.R, hexDto.S);
            var hex = new Hex<ASLHexMetadata>(coord)
            {
                Id = hexDto.Id,
                Metadata = hexDto.Metadata
            };
            foreach (var counter in hexDto.Counters)
            {
                hex.AddCounter(counter);
            }
            board.AddHex(hex);
        }

        foreach (var edgeDto in Edges)
        {
            var c1 = new CubeCoordinate(edgeDto.Q1, edgeDto.R1, edgeDto.S1);
            var c2 = new CubeCoordinate(edgeDto.Q2, edgeDto.R2, edgeDto.S2);
            board.SetEdgeData(c1, c2, edgeDto.Data);
        }

        return board;
    }
}

/// <summary>
/// Data Transfer Object for persisting an individual hex.
/// </summary>
public class HexDto
{
    /// <summary>Gets or sets the Q coordinate.</summary>
    public int Q { get; set; }
    /// <summary>Gets or sets the R coordinate.</summary>
    public int R { get; set; }
    /// <summary>Gets or sets the S coordinate.</summary>
    public int S { get; set; }
    /// <summary>Gets or sets the physical identifier for this hex.</summary>
    public string Id { get; set; } = string.Empty;
    /// <summary>Gets or sets the metadata associated with the hex.</summary>
    public ASLHexMetadata Metadata { get; set; } = new();
    /// <summary>Gets or sets the collection of units in the hex.</summary>
    public List<Unit> Counters { get; set; } = new();
}

/// <summary>
/// Data Transfer Object for persisting an individual hex edge.
/// </summary>
public class EdgeDto
{
    /// <summary>Q coordinate of first hex.</summary>
    public int Q1 { get; set; }
    /// <summary>R coordinate of first hex.</summary>
    public int R1 { get; set; }
    /// <summary>S coordinate of first hex.</summary>
    public int S1 { get; set; }
    /// <summary>Q coordinate of second hex.</summary>
    public int Q2 { get; set; }
    /// <summary>R coordinate of second hex.</summary>
    public int R2 { get; set; }
    /// <summary>S coordinate of second hex.</summary>
    public int S2 { get; set; }
    /// <summary>Metadata associated with the edge.</summary>
    public ASLEdgeData Data { get; set; } = new();
}
