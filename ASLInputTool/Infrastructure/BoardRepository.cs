using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ASL;
using ASL.Persistence;

namespace ASLInputTool.Infrastructure;

/// <summary>
/// In-memory implementation of the board repository.
/// </summary>
public class BoardRepository : IBoardRepository
{
    private readonly List<AslBoard> _boards = new();

    /// <inheritdoc />
    public event EventHandler<string>? BoardSaved;

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

    /// <inheritdoc />
    public async Task SaveToDiskAsync(AslBoard board, string? sourceImagePath, string? originalName = null)
    {
        string baseFolder = SettingsManager.Instance.Settings.BoardsFolder;
        if (string.IsNullOrWhiteSpace(baseFolder)) return;

        // Handle Renaming
        if (!string.IsNullOrEmpty(originalName) && originalName != board.Name)
        {
            string oldFolder = Path.Combine(baseFolder, originalName);
            string newFolder = Path.Combine(baseFolder, board.Name);

            if (Directory.Exists(oldFolder) && !Directory.Exists(newFolder))
            {
                try
                {
                    Directory.Move(oldFolder, newFolder);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to rename board folder from '{originalName}' to '{board.Name}': {ex.Message}", ex);
                }
            }
        }

        string boardFolder = Path.Combine(baseFolder, board.Name);
        if (!Directory.Exists(boardFolder))
        {
            Directory.CreateDirectory(boardFolder);
        }

        // 1. Save Hex Data (.board)
        var boardDto = BoardDto.FromBoard(board.Board);
        var jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };
        string boardJson = JsonSerializer.Serialize(boardDto, jsonOptions);
        string boardFilePath = Path.Combine(boardFolder, $"{board.Name}.board");
        
        // If we renamed, the old file name might still exist as originalName.board
        if (!string.IsNullOrEmpty(originalName) && originalName != board.Name)
        {
            string oldFilePath = Path.Combine(boardFolder, $"{originalName}.board");
            if (File.Exists(oldFilePath)) File.Delete(oldFilePath);
        }

        await File.WriteAllTextAsync(boardFilePath, boardJson);

        // 2. Handle Image
        string? imageFileName = null;
        if (!string.IsNullOrEmpty(sourceImagePath) && File.Exists(sourceImagePath))
        {
            imageFileName = Path.GetFileName(sourceImagePath);
            string destImagePath = Path.Combine(boardFolder, imageFileName);
            
            // Only copy if different or doesn't exist (avoid locking issues if same file)
            if (!File.Exists(destImagePath) || !string.Equals(Path.GetFullPath(sourceImagePath), Path.GetFullPath(destImagePath), StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(sourceImagePath, destImagePath, true);
            }
        }
        else
        {
            // If image path was not provided, look for existing images in the folder
            var files = Directory.GetFiles(boardFolder, "*.*", SearchOption.TopDirectoryOnly);
            foreach (var f in files)
            {
                string ext = Path.GetExtension(f).ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp")
                {
                    imageFileName = Path.GetFileName(f);
                    break;
                }
            }
        }

        // 3. Save Metadata
        var metadata = new BoardMetadata
        {
            Name = board.Name,
            Type = board.Type,
            Width = board.Width,
            Height = board.Height,
            CanvasWidth = board.CanvasWidth,
            CanvasHeight = board.CanvasHeight,
            ImageFileName = imageFileName,
            IsFirstColShiftedDown = board.IsFirstColShiftedDown,
            IsTopRowHalf = board.IsTopRowHalf,
            IsBottomRowHalf = board.IsBottomRowHalf,
            IsLeftEdgeHalf = board.IsLeftEdgeHalf,
            IsRightEdgeHalf = board.IsRightEdgeHalf
        };
        string metadataJson = JsonSerializer.Serialize(metadata, jsonOptions);
        string metadataFilePath = Path.Combine(boardFolder, "metadata.json");
        await File.WriteAllTextAsync(metadataFilePath, metadataJson);

        BoardSaved?.Invoke(this, board.Name);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AslBoard>> ScanAndLoadAsync()
    {
        var boards = new List<AslBoard>();
        string baseFolder = SettingsManager.Instance.Settings.BoardsFolder;
        
        if (string.IsNullOrWhiteSpace(baseFolder) || !Directory.Exists(baseFolder))
            return boards;

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };

        foreach (var dir in Directory.GetDirectories(baseFolder))
        {
            string metadataPath = Path.Combine(dir, "metadata.json");
            if (File.Exists(metadataPath))
            {
                try
                {
                    string json = await File.ReadAllTextAsync(metadataPath);
                    var metadata = JsonSerializer.Deserialize<BoardMetadata>(json, jsonOptions);
                    if (metadata != null)
                    {
                        var board = new AslBoard
                        {
                            Name = metadata.Name,
                            Type = metadata.Type,
                            Width = metadata.Width,
                            Height = metadata.Height,
                            CanvasWidth = metadata.CanvasWidth,
                            CanvasHeight = metadata.CanvasHeight,
                            IsFirstColShiftedDown = metadata.IsFirstColShiftedDown,
                            IsTopRowHalf = metadata.IsTopRowHalf,
                            IsBottomRowHalf = metadata.IsBottomRowHalf,
                            IsLeftEdgeHalf = metadata.IsLeftEdgeHalf,
                            IsRightEdgeHalf = metadata.IsRightEdgeHalf
                        };
                        boards.Add(board);
                    }
                }
                catch
                {
                    // Skip invalid metadata
                }
            }
        }
        return boards;
    }

    /// <inheritdoc />
    public async Task LoadBoardDataAsync(AslBoard board)
    {
        string baseFolder = SettingsManager.Instance.Settings.BoardsFolder;
        string boardPath = Path.Combine(baseFolder, board.Name, $"{board.Name}.board");
        
        if (File.Exists(boardPath))
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() } };
            string json = await File.ReadAllTextAsync(boardPath);
            var dto = JsonSerializer.Deserialize<BoardDto>(json, jsonOptions);
            if (dto != null)
            {
                board.Board = dto.ToBoard();
                // Ensure dimensions match the file if they were edited outside
                board.Width = dto.Width;
                board.Height = dto.Height;
                // Re-sync structural metadata to the internal board
                board.PopulateBoard();
            }
        }
    }

    /// <inheritdoc />
    public async Task DeleteBoardAsync(AslBoard board)
    {
        string baseFolder = SettingsManager.Instance.Settings.BoardsFolder;
        string boardFolder = Path.Combine(baseFolder, board.Name);
        if (Directory.Exists(boardFolder))
        {
            try
            {
                await Task.Run(() => Directory.Delete(boardFolder, true));
            }
            catch (IOException ex)
            {
                throw new IOException($"Failed to delete board folder '{board.Name}'. Some files (like images) might be locked by another process: {ex.Message}", ex);
            }
        }
    }
}
