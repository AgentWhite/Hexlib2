using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// Defines the behavior for board persistence and administration.
/// </summary>
public interface IBoardRepository
{
    /// <summary>Raised when a board is successfully saved to disk.</summary>
    event System.EventHandler<string>? BoardSaved;

    /// <summary>Gets all currently known boards.</summary>
    IEnumerable<ASL.AslBoard> AllBoards { get; }

    /// <summary>Adds a board to the repository.</summary>
    void Add(ASL.AslBoard board);

    /// <summary>Removes a board from the repository.</summary>
    void Remove(ASL.AslBoard board);

    /// <summary>Initializes the repository with a collection of boards.</summary>
    void Initialize(IEnumerable<ASL.AslBoard> boards);

    /// <summary>Processes potential board data based on application file paths.</summary>
    void ProcessData(string projectFilePath);

    /// <summary>
    /// Saves the board data and metadata to disk, optionally renaming the folder if the name changed.
    /// </summary>
    /// <param name="board">The board to save.</param>
    /// <param name="sourceImagePath">Optional source path for a new background image to copy into the board folder.</param>
    /// <param name="originalName">Optional original name to detect renames and move folders.</param>
    Task SaveToDiskAsync(ASL.AslBoard board, string? sourceImagePath, string? originalName = null);

    /// <summary>Scans the default board folder and loads all metadata.</summary>
    Task<IEnumerable<ASL.AslBoard>> ScanAndLoadAsync();

    /// <summary>Loads the full hex data for a specific board.</summary>
    Task LoadBoardDataAsync(ASL.AslBoard board);

    /// <summary>Deletes a board and its associated folder from disk.</summary>
    Task DeleteBoardAsync(ASL.AslBoard board);
}
