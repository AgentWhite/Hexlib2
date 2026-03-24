using System.Collections.Generic;
using ASL;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// In-memory implementation of the board repository.
/// </summary>
public class BoardRepository : IBoardRepository
{
    private readonly List<AslBoard> _boards = new();

    /// <inheritdoc />
    public IEnumerable<AslBoard> AllBoards => _boards;

    /// <inheritdoc />
    public void Add(AslBoard board)
    {
        if (!_boards.Contains(board))
        {
            _boards.Add(board);
        }
    }

    /// <inheritdoc />
    public void Remove(AslBoard board)
    {
        _boards.Remove(board);
    }

    /// <inheritdoc />
    public void Initialize(IEnumerable<AslBoard> boards)
    {
        _boards.Clear();
        _boards.AddRange(boards);
    }

    /// <inheritdoc />
    public void ProcessData(string projectFilePath)
    {
        // Currently boards only have a Name property, so no special processing is needed.
    }
}
