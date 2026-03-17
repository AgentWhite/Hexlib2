namespace HexLib;

/// <summary>
/// Manages a localized collection of Hexes representing a physical tile map.
/// Connects to other Board instances to form dynamic wargame geometries.
/// </summary>
public class Board
{
    /// <summary>
    /// The unique identifier or printed name of this board (e.g., "Board A").
    /// Used natively to resolve naming collisions for overlapping joined hexes.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    private readonly int _physicalWidth;
    private readonly int _physicalHeight;

    /// <summary>
    /// The effective width of the board, swapped dynamically if the board is rotated 90/270 degrees.
    /// </summary>
    public int Width => (_orientation == BoardOrientation.Degree90 || _orientation == BoardOrientation.Degree270) ? _physicalHeight : _physicalWidth;
    
    /// <summary>
    /// The effective height of the board, swapped dynamically if the board is rotated 90/270 degrees.
    /// </summary>
    public int Height => (_orientation == BoardOrientation.Degree90 || _orientation == BoardOrientation.Degree270) ? _physicalWidth : _physicalHeight;

    private BoardOrientation _orientation = BoardOrientation.Degree0;
    
    /// <summary>
    /// The manager controlling the global map scope this board belongs to, if any.
    /// </summary>
    public BoardManager? Manager { get; internal set; }

    /// <summary>
    /// The visual reading orientation of the board map grid.
    /// Cannot be altered while the board is joined to neighbors or managed globally.
    /// </summary>
    public BoardOrientation Orientation
    {
        get => _orientation;
        set
        {
            if (_neighbors.Count > 0)
            {
                throw new InvalidOperationException("Cannot rotate a board while it is joined to other boards.");
            }
            if (Manager != null)
            {
                throw new InvalidOperationException("Cannot rotate a board while it is tracked by a BoardManager.");
            }
            _orientation = value;
        }
    }
    
    private BoardEdge _physicalHalfHexSides;
    public BoardEdge HalfHexSides
    {
        get => GetEffectiveHalfHexSides(_physicalHalfHexSides, _orientation);
        set => _physicalHalfHexSides = value;
    }

    private BoardEdge GetEffectiveHalfHexSides(BoardEdge original, BoardOrientation orientation)
    {
        int rotationSteps = orientation switch
        {
            BoardOrientation.Degree90 => 1,
            BoardOrientation.Degree180 => 2,
            BoardOrientation.Degree270 => 3,
            _ => 0
        };

        if (rotationSteps == 0) return original;

        BoardEdge result = BoardEdge.None;
        if (original.HasFlag(BoardEdge.Top)) result |= RotateSingleEdge(BoardEdge.Top, rotationSteps);
        if (original.HasFlag(BoardEdge.Right)) result |= RotateSingleEdge(BoardEdge.Right, rotationSteps);
        if (original.HasFlag(BoardEdge.Bottom)) result |= RotateSingleEdge(BoardEdge.Bottom, rotationSteps);
        if (original.HasFlag(BoardEdge.Left)) result |= RotateSingleEdge(BoardEdge.Left, rotationSteps);
        return result;
    }

    private BoardEdge RotateSingleEdge(BoardEdge edge, int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            edge = edge switch
            {
                BoardEdge.Top => BoardEdge.Right,
                BoardEdge.Right => BoardEdge.Bottom,
                BoardEdge.Bottom => BoardEdge.Left,
                BoardEdge.Left => BoardEdge.Top,
                _ => edge
            };
        }
        return edge;
    }

    private readonly Dictionary<CubeCoordinate, Hex> _hexes = new Dictionary<CubeCoordinate, Hex>();
    private readonly Dictionary<BoardEdge, Board> _neighbors = new Dictionary<BoardEdge, Board>();

    public IReadOnlyDictionary<CubeCoordinate, Hex> Hexes => _hexes;
    public IReadOnlyDictionary<BoardEdge, Board> Neighbors => _neighbors;

    public Board(int width, int height)
    {
        _physicalWidth = width;
        _physicalHeight = height;
    }

    public BoardEdge GetOppositeEdge(BoardEdge edge)
    {
        return edge switch
        {
            BoardEdge.Top => BoardEdge.Bottom,
            BoardEdge.Right => BoardEdge.Left,
            BoardEdge.Bottom => BoardEdge.Top,
            BoardEdge.Left => BoardEdge.Right,
            _ => throw new ArgumentException("Invalid board edge for joining", nameof(edge))
        };
    }

    public bool CanJoin(Board other, BoardEdge direction)
    {
        if (other == null) return false;
        
        // Ensure direction is exactly one side
        if (direction != BoardEdge.Top && direction != BoardEdge.Right && 
            direction != BoardEdge.Bottom && direction != BoardEdge.Left)
        {
            return false;
        }

        // Prevent joining the identical board recursively or doubly
        if (ReferenceEquals(this, other)) return false;
        if (_neighbors.ContainsValue(other)) return false; // Already joined somewhere

        if (_neighbors.ContainsKey(direction)) return false;

        var opposite = GetOppositeEdge(direction);
        if (other._neighbors.ContainsKey(opposite)) return false;

        // Check if both boards have half hexes on the joining edges
        if (!HalfHexSides.HasFlag(direction) || !other.HalfHexSides.HasFlag(opposite))
        {
            return false;
        }

        // Validate dimension matching
        if (direction == BoardEdge.Top || direction == BoardEdge.Bottom)
        {
            if (Width != other.Width) return false;
        }
        else // Left or Right
        {
            if (Height != other.Height) return false;
        }

        return true;
    }

    public Hex? GetPhysicalNeighbor(CubeCoordinate physicalCoordinate, PhysicalDirection physicalDirection)
    {
        // 1. Calculate the target physical coordinate assuming it's on the same infinite grid
        var offset = GetPhysicalOffset(physicalDirection);
        var targetPhysical = physicalCoordinate + offset;

        // 2. Check if the targetPhysical coordinate exists natively on THIS board
        var logicalTarget = PhysicalToLogical(targetPhysical, _orientation);
        if (_hexes.TryGetValue(logicalTarget, out var hex))
        {
            return hex; // It's on our board
        }

        // 3. Coordinate is off the board. We need to determine WHICH edge we crossed.
        // This requires determining the bounds. In a hex grid, width/height form a rectangle in offset coordinates.
        // We'll delegate edge detection to a helper to see if a joined board holds the answer.
        var crossedEdge = DetectCrossedEdge(physicalCoordinate, targetPhysical);
        if (crossedEdge != BoardEdge.None && _neighbors.TryGetValue(crossedEdge, out var neighborBoard))
        {
            // We crossed to a neighbor. We need to ask the neighbor what hex is on their side of the join line.
            // Since they are mirrored on the opposite edge, we query the neighbor.
            return neighborBoard.GetMirrorHexOnEdge(GetOppositeEdge(crossedEdge), targetPhysical, this);
        }

        return null;
    }

    private Hex? GetMirrorHexOnEdge(BoardEdge incomingEdge, CubeCoordinate originalPhysicalTarget, Board requestingBoard)
    {
        // Convert the incoming physical target to an offset coordinate relative to the requesting board
        var offsetSource = HexMath.CubeToOffset(originalPhysicalTarget);

        // Calculate what the local relative offset coordinate should be on exactly this joined edge
        int localCol = incomingEdge switch
        {
            BoardEdge.Left => 0,
            BoardEdge.Right => Width - 1,
            _ => offsetSource.col
        };

        int localRow = incomingEdge switch
        {
            BoardEdge.Top => 0,
            BoardEdge.Bottom => Height - 1,
            _ => offsetSource.row
        };

        // For Left/Right joins, the rows align perfectly assuming they are joined at row 0. 
        // For Top/Bottom joins, columns align perfectly.
        // We ensure bounding is safe.
        if (localCol >= 0 && localCol < Width && localRow >= 0 && localRow < Height)
        {
            var myPhysicalTarget = HexMath.OffsetToCube(localCol, localRow);
            var myLogicalTarget = PhysicalToLogical(myPhysicalTarget, _orientation);
            
            if (_hexes.TryGetValue(myLogicalTarget, out var hex))
            {
                return hex;
            }
        }

        return null;
    }

    private BoardEdge DetectCrossedEdge(CubeCoordinate fromCube, CubeCoordinate toCube)
    {
        var fromOffset = HexMath.CubeToOffset(fromCube);
        var toOffset = HexMath.CubeToOffset(toCube);

        if (toOffset.col < 0) return BoardEdge.Left;
        if (toOffset.col >= Width) return BoardEdge.Right;
        if (toOffset.row < 0) return BoardEdge.Top;
        if (toOffset.row >= Height) return BoardEdge.Bottom;

        return BoardEdge.None;
    }

    private CubeCoordinate GetPhysicalOffset(PhysicalDirection dir)
    {
        // Standard pointy-top physical directions mapped to CubeCoordinate offsets
        return dir switch
        {
            PhysicalDirection.NorthWest => new CubeCoordinate(0, -1, 1),
            PhysicalDirection.NorthEast => new CubeCoordinate(1, -1, 0),
            PhysicalDirection.East => new CubeCoordinate(1, 0, -1),
            PhysicalDirection.SouthEast => new CubeCoordinate(0, 1, -1),
            PhysicalDirection.SouthWest => new CubeCoordinate(-1, 1, 0),
            PhysicalDirection.West => new CubeCoordinate(-1, 0, 1),
            _ => throw new ArgumentException()
        };
    }

    // Rotates a purely physical coordinate into the board's logical orientation
    private CubeCoordinate PhysicalToLogical(CubeCoordinate physical, BoardOrientation orientation)
    {
        return orientation switch
        {
            BoardOrientation.Degree90 => physical.Rotate300(),  // 90 deg square rotation approx mapped to -60 hex rotation
            BoardOrientation.Degree180 => physical.Rotate180(),
            BoardOrientation.Degree270 => physical.Rotate120(), // -90 mapped to +120
            _ => physical
        };
    }

    public void Join(Board other, BoardEdge direction, Func<Hex, Hex, Hex>? determinePrimaryHex = null)
    {
        if (!CanJoin(other, direction))
        {
            throw new InvalidOperationException($"Cannot join board '{other.Name}' to '{Name}' on edge {direction}.");
        }

        var opposite = GetOppositeEdge(direction);
        _neighbors[direction] = other;
        other._neighbors[opposite] = this;

        LinkHalfHexes(other, direction, determinePrimaryHex);
    }

    private void LinkHalfHexes(Board other, BoardEdge direction, Func<Hex, Hex, Hex>? determinePrimaryHex)
    {
        // For now, as a placeholder until physical coordinate mapping is implemented, 
        // we need to find the overlapping hexes. This requires identifying the border hexes on both ends.
        // We will implement the actual overlap pairing logic along with GetPhysicalNeighbor.
        // The core requirement is identifying the alias vs primary.
        
        Func<Hex, Hex, Hex> prioritySelector = determinePrimaryHex ?? ((h1, h2) => 
        {
            var b1Name = this.Name ?? string.Empty;
            var b2Name = other.Name ?? string.Empty;
            return string.CompareOrdinal(b1Name, b2Name) <= 0 ? h1 : h2;
        });

        // Loop over pairs of overlapping hexes (Placeholder loop to be filled with physical edge traversal)
        // foreach(var (myHex, otherHex) in GetOverlappingHexPairs(direction, other))
        // {
        //     var primary = prioritySelector(myHex, otherHex);
        //     var alias = primary == myHex ? otherHex : myHex;
        //     alias.PrimaryHexAlias = primary;
        // }
    }

    public void Unlink(BoardEdge direction)
    {
        if (_neighbors.TryGetValue(direction, out var other))
        {
            UnlinkHalfHexes(other, direction);

            var opposite = GetOppositeEdge(direction);
            _neighbors.Remove(direction);
            other._neighbors.Remove(opposite);
        }
    }

    private void UnlinkHalfHexes(Board other, BoardEdge direction)
    {
        // Placeholder for breaking the alias bonds on the overlapping edge
        // foreach(var (myHex, otherHex) in GetOverlappingHexPairs(direction, other))
        // {
        //     myHex.PrimaryHexAlias = null;
        //     otherHex.PrimaryHexAlias = null;
        // }
    }

    public void AddHex(Hex hex)
    {
        if (hex == null) throw new ArgumentNullException(nameof(hex));
        if (_hexes.ContainsKey(hex.Location))
        {
            throw new ArgumentException($"A hex already exists at location {hex.Location.Q}, {hex.Location.R}, {hex.Location.S}", nameof(hex));
        }
        _hexes[hex.Location] = hex;
    }

    public Hex? GetHexAt(CubeCoordinate location)
    {
        _hexes.TryGetValue(location, out var hex);
        return hex;
    }

    public bool RemoveHexAt(CubeCoordinate location)
    {
        return _hexes.Remove(location);
    }
}
