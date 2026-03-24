using System.Collections.Generic;
using ASL;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Defines the contract for a repository managing ASL boards.
/// </summary>
public interface IBoardRepository
{
    /// <summary>
    /// Gets all boards currently in the repository.
    /// </summary>
    IEnumerable<AslBoard> AllBoards { get; }

    /// <summary>
    /// Adds a new board to the repository.
    /// </summary>
    /// <param name="board">The board to add.</param>
    void Add(AslBoard board);

    /// <summary>
    /// Removes a board from the repository.
    /// </summary>
    /// <param name="board">The board to remove.</param>
    void Remove(AslBoard board);

    /// <summary>
    /// Initializes the repository with a list of boards.
    /// </summary>
    /// <param name="boards">The initial collection of boards.</param>
    void Initialize(IEnumerable<AslBoard> boards);

    /// <summary>
    /// Processes board data (e.g., fixing relative paths) after loading.
    /// </summary>
    /// <param name="projectFilePath">Path to the project file for relative path resolution.</param>
    void ProcessData(string projectFilePath);
}
