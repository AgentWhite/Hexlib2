namespace HexLib;

/// <summary>
/// Manages a localized collection of Hexes representing a physical tile map.
/// Connects to other Board instances to form dynamic wargame geometries.
/// </summary>
/// <typeparam name="THexMetadata">User-defined type for hex terrain or data.</typeparam>
/// <typeparam name="TEdgeData">User-defined type for hexside data (roads, walls, etc.).</typeparam>
public class Board<THexMetadata, TEdgeData>
{
    /// <summary>
    /// The unique identifier or printed name of this board (e.g., "Board A").
    /// Used natively to resolve naming collisions for overlapping joined hexes.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    private readonly int _physicalWidth;
    private readonly int _physicalHeight;

    /// <summary>
    /// The physical geometric layout of the hexes on this board.
    /// </summary>
    public HexTopOrientation TopOrientation { get; }

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
    public BoardManager<THexMetadata, TEdgeData>? Manager { get; internal set; }

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

    private readonly Dictionary<CubeCoordinate, Hex<THexMetadata>> _hexes = new Dictionary<CubeCoordinate, Hex<THexMetadata>>();
    private readonly Dictionary<BoardEdge, Board<THexMetadata, TEdgeData>> _neighbors = new Dictionary<BoardEdge, Board<THexMetadata, TEdgeData>>();
    
    // Store edge data using a normalized key of (CubeCoordinate, CubeCoordinate).
    // Using (CoordMin, CoordMax) ensures Hex A and Hex B refer to the same logical edge.
    private readonly Dictionary<(CubeCoordinate, CubeCoordinate), TEdgeData> _edges = new Dictionary<(CubeCoordinate, CubeCoordinate), TEdgeData>();

    /// <summary>
    /// A read-only collection of all hexes strictly owned by this board, keyed by their logical coordinates.
    /// </summary>
    public IReadOnlyDictionary<CubeCoordinate, Hex<THexMetadata>> Hexes => _hexes;

    /// <summary>
    /// A read-only collection of other boards physically joined to this board, keyed by the edge they attach to.
    /// </summary>
    public IReadOnlyDictionary<BoardEdge, Board<THexMetadata, TEdgeData>> Neighbors => _neighbors;

    /// <summary>
    /// Initializes a new, unlinked <see cref="Board"/> with the specified physical width and height in hexes and physical orientation.
    /// </summary>
    /// <param name="width">The physical width of the board.</param>
    /// <param name="height">The physical height of the board.</param>
    /// <param name="topOrientation">The hex geometric layout (defaults to PointyTopped).</param>
    public Board(int width, int height, HexTopOrientation topOrientation = HexTopOrientation.PointyTopped)
    {
        _physicalWidth = width;
        _physicalHeight = height;
        TopOrientation = topOrientation;
    }

    /// <summary>
    /// Returns the opposite physical edge for a given edge alignment.
    /// </summary>
    /// <param name="edge">The edge to evaluate.</param>
    /// <returns>The polar opposite edge.</returns>
    /// <exception cref="ArgumentException">Thrown if an invalid or compound edge is passed.</exception>
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

    /// <summary>
    /// Evaluates if another given board can be physically attached to this board on the specified edge.
    /// Checks for valid half-hex edge configurations and dimension matching.
    /// </summary>
    /// <param name="other">The board attempting to connect.</param>
    /// <param name="direction">The single physical edge direction on this board to join.</param>
    /// <returns><c>true</c> if the join is valid; otherwise <c>false</c>.</returns>
    public bool CanJoin(Board<THexMetadata, TEdgeData> other, BoardEdge direction)
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

    /// <summary>
    /// Attempts to locate the adjacent hex in the specified physical direction, seamlessly crossing board boundaries 
    /// if joined to a neighbor.
    /// </summary>
    /// <param name="physicalCoordinate">The starting physical coordinate.</param>
    /// <param name="physicalDirection">The physical direction to travel.</param>
    /// <returns>The neighboring hex if it exists, either natively or on a joined board, or <c>null</c>.</returns>
    public Hex<THexMetadata>? GetPhysicalNeighbor(CubeCoordinate physicalCoordinate, PhysicalDirection physicalDirection)
    {
        // 1. Calculate the target physical coordinate assuming it's on the same infinite grid
        var offset = GetPhysicalOffset_Internal(physicalDirection);
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

    private Hex<THexMetadata>? GetMirrorHexOnEdge(BoardEdge incomingEdge, CubeCoordinate originalPhysicalTarget, Board<THexMetadata, TEdgeData> requestingBoard)
    {
        // Convert the incoming physical target to an offset coordinate relative to the requesting board
        var offsetSource = originalPhysicalTarget.ToOffset(requestingBoard.TopOrientation);

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
            var myPhysicalTarget = HexMath.OffsetToCube(localCol, localRow, TopOrientation);
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
        var fromOffset = fromCube.ToOffset(TopOrientation);
        var toOffset = toCube.ToOffset(TopOrientation);

        if (toOffset.col < 0) return BoardEdge.Left;
        if (toOffset.col >= Width) return BoardEdge.Right;
        if (toOffset.row < 0) return BoardEdge.Top;
        if (toOffset.row >= Height) return BoardEdge.Bottom;

        return BoardEdge.None;
    }

    internal CubeCoordinate GetPhysicalOffset_Internal(PhysicalDirection dir)
    {
        if (TopOrientation == HexTopOrientation.PointyTopped)
        {
            return dir switch
            {
                PhysicalDirection.NorthWest => new CubeCoordinate(0, -1, 1),
                PhysicalDirection.NorthEast => new CubeCoordinate(1, -1, 0),
                PhysicalDirection.East => new CubeCoordinate(1, 0, -1),
                PhysicalDirection.SouthEast => new CubeCoordinate(0, 1, -1),
                PhysicalDirection.SouthWest => new CubeCoordinate(-1, 1, 0),
                PhysicalDirection.West => new CubeCoordinate(-1, 0, 1),
                _ => throw new ArgumentException($"Direction {dir} is not valid for PointyTopped boards.", nameof(dir))
            };
        }
        else
        {
            return dir switch
            {
                PhysicalDirection.North => new CubeCoordinate(0, -1, 1),
                PhysicalDirection.NorthEast => new CubeCoordinate(1, -1, 0),
                PhysicalDirection.SouthEast => new CubeCoordinate(1, 0, -1),
                PhysicalDirection.South => new CubeCoordinate(0, 1, -1),
                PhysicalDirection.SouthWest => new CubeCoordinate(-1, 1, 0),
                PhysicalDirection.NorthWest => new CubeCoordinate(-1, 0, 1),
                _ => throw new ArgumentException($"Direction {dir} is not valid for FlatTopped boards.", nameof(dir))
            };
        }
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

    /// <summary>
    /// Physically links another board to this board on the specified edge, establishing neighbor tracking and 
    /// half-hex resolution.
    /// </summary>
    /// <param name="other">The board to join.</param>
    /// <param name="direction">The edge on this board to connect.</param>
    /// <param name="determinePrimaryHex">An optional selector function to determine which overlapping half-hex should act as the primary, physical hex.</param>
    /// <exception cref="InvalidOperationException">Thrown if the join validation fails.</exception>
    public void Join(Board<THexMetadata, TEdgeData> other, BoardEdge direction, Func<Hex<THexMetadata>, Hex<THexMetadata>, Hex<THexMetadata>>? determinePrimaryHex = null)
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

    private void LinkHalfHexes(Board<THexMetadata, TEdgeData> other, BoardEdge direction, Func<Hex<THexMetadata>, Hex<THexMetadata>, Hex<THexMetadata>>? determinePrimaryHex)
    {
        // For now, as a placeholder until physical coordinate mapping is implemented, 
        // we need to find the overlapping hexes. This requires identifying the border hexes on both ends.
        // We will implement the actual overlap pairing logic along with GetPhysicalNeighbor.
        // The core requirement is identifying the alias vs primary.
        
        Func<Hex<THexMetadata>, Hex<THexMetadata>, Hex<THexMetadata>> prioritySelector = determinePrimaryHex ?? ((h1, h2) => 
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

    /// <summary>
    /// Severs the physical link and half-hex overlaps with the board joined on the specified edge.
    /// </summary>
    /// <param name="direction">The edge to unlink.</param>
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

    private void UnlinkHalfHexes(Board<THexMetadata, TEdgeData> other, BoardEdge direction)
    {
        // Placeholder for breaking the alias bonds on the overlapping edge
        // foreach(var (myHex, otherHex) in GetOverlappingHexPairs(direction, other))
        // {
        //     myHex.PrimaryHexAlias = null;
        //     otherHex.PrimaryHexAlias = null;
        // }
    }

    /// <summary>
    /// Explicitly registers a hex to this board. The hex's logical location must be unique on the board.
    /// </summary>
    /// <param name="hex">The hex to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if the hex is null.</exception>
    /// <exception cref="ArgumentException">Thrown if a hex already exists at the target logical location.</exception>
    public void AddHex(Hex<THexMetadata> hex)
    {
        if (hex == null) throw new ArgumentNullException(nameof(hex));
        if (_hexes.ContainsKey(hex.Location))
        {
            throw new ArgumentException($"A hex already exists at location {hex.Location.Q}, {hex.Location.R}, {hex.Location.S}", nameof(hex));
        }
        _hexes[hex.Location] = hex;
    }

    /// <summary>
    /// Attempts to retrieve a hex residing locally on this board given a strictly logical coordinate.
    /// </summary>
    /// <param name="location">The logical coordinate relative to this board's origin.</param>
    /// <returns>The hex if found; otherwise, <c>null</c>.</returns>
    public Hex<THexMetadata>? GetHexAt(CubeCoordinate location)
    {
        _hexes.TryGetValue(location, out var hex);
        return hex;
    }

    /// <summary>
    /// Removes a hex at the specified logical coordinate from this board.
    /// </summary>
    /// <param name="location">The logical coordinate of the hex to remove.</param>
    /// <returns><c>true</c> if a hex was found and removed; otherwise, <c>false</c>.</returns>
    public bool RemoveHexAt(CubeCoordinate location)
    {
        return _hexes.Remove(location);
    }

    /// <summary>
    /// Registers or updates metadata for a physical edge between two hexes.
    /// Order of coordinates does not matter.
    /// </summary>
    public void SetEdgeData(CubeCoordinate a, CubeCoordinate b, TEdgeData data)
    {
        _edges[NormalizeEdge(a, b)] = data;
    }

    /// <summary>
    /// Retrieves metadata for a physical edge between two hexes.
    /// Returns default if no data is registered.
    /// </summary>
    public TEdgeData? GetEdgeData(CubeCoordinate a, CubeCoordinate b)
    {
        _edges.TryGetValue(NormalizeEdge(a, b), out var data);
        return data;
    }

    private (CubeCoordinate, CubeCoordinate) NormalizeEdge(CubeCoordinate a, CubeCoordinate b)
    {
        // Consistent ordering to ensure A-B and B-A are the same key.
        return (a.Q < b.Q || (a.Q == b.Q && a.R < b.R)) ? (a, b) : (b, a);
    }
}
