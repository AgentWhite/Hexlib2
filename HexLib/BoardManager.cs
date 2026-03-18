namespace HexLib;

/// <summary>
/// Manages a topological graph of independent Boards, translating their local coordinate spaces 
/// into a unified global coordinate plane for overarching geometric math and pathfinding.
/// </summary>
/// <typeparam name="THexMetadata">User-defined type for hex terrain or data.</typeparam>
/// <typeparam name="TEdgeData">User-defined type for hexside data.</typeparam>
public class BoardManager<THexMetadata, TEdgeData>
{
    private readonly Dictionary<string, Board<THexMetadata, TEdgeData>> _boards = new Dictionary<string, Board<THexMetadata, TEdgeData>>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, (int GlobalOffsetX, int GlobalOffsetY)> _boardPositions = new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The primary board defining the origin (0,0) of the global offset space.
    /// </summary>
    public Board<THexMetadata, TEdgeData>? AnchorBoard { get; private set; }

    /// <summary>
    /// Gets all boards currently managed by this instance.
    /// </summary>
    public IReadOnlyCollection<Board<THexMetadata, TEdgeData>> Boards => _boards.Values;

    /// <summary>
    /// Establishes the provided board as the (0,0) anchor of the global map, automatically importing 
    /// any boards already connected to it via its Neighbors graph.
    /// </summary>
    /// <param name="board">The board to set as the Anchor.</param>
    public void SetAnchorBoard(Board<THexMetadata, TEdgeData> board)
    {
        if (board == null) throw new ArgumentNullException(nameof(board));
        if (AnchorBoard != null) throw new InvalidOperationException("An Anchor board has already been set.");
        if (_boards.ContainsKey(board.Name)) throw new ArgumentException($"A board named '{board.Name}' is already registered.");

        AnchorBoard = board;
        RegisterBoardWithCluster(board, 0, 0);
    }

    /// <summary>
    /// Adds a detached board explicitly at the provided global physical offsets.
    /// Automatically imports any boards already connected to it.
    /// </summary>
    public void AddBoard(Board<THexMetadata, TEdgeData> board, int globalOffsetX, int globalOffsetY)
    {
        if (board == null) throw new ArgumentNullException(nameof(board));
        if (AnchorBoard == null && _boards.Count > 0) throw new InvalidOperationException("Initial boards must be added via SetAnchorBoard.");
        if (_boards.ContainsKey(board.Name)) throw new ArgumentException($"A board named '{board.Name}' is already registered.");

        RegisterBoardWithCluster(board, globalOffsetX, globalOffsetY);
    }

    /// <summary>
    /// Recursively registers a board and all its connected neighbors into the manager.
    /// </summary>
    private void RegisterBoardWithCluster(Board<THexMetadata, TEdgeData> rootBoard, int rootOffsetX, int rootOffsetY)
    {
        var pendingQueue = new Queue<(Board<THexMetadata, TEdgeData> board, int ox, int oy)>();
        pendingQueue.Enqueue((rootBoard, rootOffsetX, rootOffsetY));

        while (pendingQueue.Count > 0)
        {
            var current = pendingQueue.Dequeue();
            
            if (_boards.ContainsKey(current.board.Name)) continue; // Already processed this node in the graph

            // Register
            current.board.Manager = this;
            _boards[current.board.Name] = current.board;
            _boardPositions[current.board.Name] = (current.ox, current.oy);

            // Queue connected neighbors
            foreach (var neighborKvp in current.board.Neighbors)
            {
                var direction = neighborKvp.Key;
                var neighborBoard = neighborKvp.Value;

                if (_boards.ContainsKey(neighborBoard.Name)) continue;

                // Calculate where the neighbor is on the global offset plane based on the direction they are connected
                var (nox, noy) = CalculateNeighborGlobalOffset(current.board, current.ox, current.oy, neighborBoard, direction);
                pendingQueue.Enqueue((neighborBoard, nox, noy));
            }
        }
    }

    /// <summary>
    /// Calculates the global offset of a neighbor board relative to a source board based on join direction.
    /// </summary>
    private (int ox, int oy) CalculateNeighborGlobalOffset(Board<THexMetadata, TEdgeData> sourceBoard, int sourceOx, int sourceOy, Board<THexMetadata, TEdgeData> targetBoard, BoardEdge directionFromSource)
    {
        if (directionFromSource == BoardEdge.Right)
        {
            return (sourceOx + sourceBoard.Width, sourceOy);
        }
        else if (directionFromSource == BoardEdge.Left)
        {
            return (sourceOx - targetBoard.Width, sourceOy);
        }
        else if (directionFromSource == BoardEdge.Bottom)
        {
            return (sourceOx, sourceOy + sourceBoard.Height);
        }
        else if (directionFromSource == BoardEdge.Top)
        {
            return (sourceOx, sourceOy - targetBoard.Height);
        }
        
        throw new InvalidOperationException($"Invalid join direction: {directionFromSource}");
    }

    /// <summary>
    /// Explicitly joins a detached board to an already managed board, calculating and registering its global offset.
    /// </summary>
    public void JoinBoard(Board<THexMetadata, TEdgeData> newBoard, Board<THexMetadata, TEdgeData> existingBoard, BoardEdge directionFromExisting)
    {
        if (newBoard == null) throw new ArgumentNullException(nameof(newBoard));
        if (existingBoard == null) throw new ArgumentNullException(nameof(existingBoard));
        if (!_boards.ContainsKey(existingBoard.Name)) throw new InvalidOperationException($"Board '{existingBoard.Name}' is not managed by this BoardManager.");
        if (_boards.ContainsKey(newBoard.Name)) throw new InvalidOperationException($"Board '{newBoard.Name}' is already managed. Use Board.Join for internal connections.");

        var existingPos = _boardPositions[existingBoard.Name];
        var (newOx, newOy) = CalculateNeighborGlobalOffset(existingBoard, existingPos.GlobalOffsetX, existingPos.GlobalOffsetY, newBoard, directionFromExisting);

        existingBoard.Join(newBoard, directionFromExisting);
        RegisterBoardWithCluster(newBoard, newOx, newOy);
    }

    /// <summary>
    /// Unregisters and physically disconnects a board from the manager's global graph.
    /// Re-anchors the global plane if the removed board was the anchor.
    /// </summary>
    /// <param name="boardName">The unique name of the board to remove.</param>
    public void RemoveBoard(string boardName)
    {
        if (!_boards.TryGetValue(boardName, out var boardToRemove))
        {
            return;
        }

        // Break connections with all neighboring boards physically
        foreach (var direction in boardToRemove.Neighbors.Keys.ToList())
        {
            boardToRemove.Unlink(direction);
        }

        _boards.Remove(boardName);
        _boardPositions.Remove(boardName);
        boardToRemove.Manager = null;

        // If we just removed the anchor, we must either shift the graph or clear it.
        // If there are no boards left, we naturally just clear the anchor.
        if (AnchorBoard == boardToRemove)
        {
            if (_boards.Count == 0)
            {
                AnchorBoard = null;
            }
            else
            {
                // Find a new arbitrary board to act as the Anchor
                var newAnchor = _boards.Values.First();
                AnchorBoard = newAnchor;

                // Shift all remaining boards so the new Anchor sits perfectly at (0,0)
                var currentAnchorPos = _boardPositions[newAnchor.Name];
                int deltaX = -currentAnchorPos.GlobalOffsetX;
                int deltaY = -currentAnchorPos.GlobalOffsetY;

                foreach (var boardKey in _boards.Keys.ToList()) // ToList to avoid concurrent modification issues
                {
                    var pos = _boardPositions[boardKey];
                    _boardPositions[boardKey] = (pos.GlobalOffsetX + deltaX, pos.GlobalOffsetY + deltaY);
                }
            }
        }
    }

    /// <summary>
    /// Translates a given Hex's local logical coordinate into the Manager's unified global CubeCoordinate space.
    /// </summary>
    public CubeCoordinate ToGlobalCoordinate(Hex<THexMetadata> hex)
    {
        // To find the global coordinate of a hex:
        // 1. We must find its parent board. 
        var parentBoard = _boards.Values.FirstOrDefault(b => b.GetHexAt(hex.Location) == hex);
        if (parentBoard == null)
        {
            throw new InvalidOperationException("The provided hex does not belong to any board managed by this BoardManager.");
        }

        var offset = _boardPositions[parentBoard.Name];
        var localPhysicalCube = GetPhysicalCubeFromLogical(hex.Location, parentBoard.Orientation);
        var localPhysicalOffset = localPhysicalCube.ToOffset(parentBoard.TopOrientation);

        int globalX = localPhysicalOffset.col + offset.GlobalOffsetX;
        int globalY = localPhysicalOffset.row + offset.GlobalOffsetY;

        var globalOrientation = AnchorBoard?.TopOrientation ?? parentBoard.TopOrientation;
        return HexMath.OffsetToCube(globalX, globalY, globalOrientation);
    }

    /// <summary>
    /// Translates a logical cube coordinate to a physical cube coordinate based on board rotation.
    /// </summary>
    private CubeCoordinate GetPhysicalCubeFromLogical(CubeCoordinate logical, BoardOrientation orientation)
    {
        return orientation switch
        {
            BoardOrientation.Degree90 => logical.Rotate60(),   // Inverse of Rotate300
            BoardOrientation.Degree180 => logical.Rotate180(), // Inverse of Rotate180
            BoardOrientation.Degree270 => logical.Rotate240(), // Inverse of Rotate120
            _ => logical
        };
    }

    /// <summary>
    /// Measures the exact physical distance in hexes between two hexes on the global map graph.
    /// </summary>
    public int GetDistance(Hex<THexMetadata> a, Hex<THexMetadata> b)
    {
        var globalA = ToGlobalCoordinate(a);
        var globalB = ToGlobalCoordinate(b);
        return globalA.DistanceTo(globalB);
    }
}
