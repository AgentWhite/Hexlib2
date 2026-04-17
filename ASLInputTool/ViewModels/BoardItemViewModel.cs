using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ASL;
using ASL.Core;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Infrastructure;
using ASL.Services;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Tool-specific wrapper for an AslBoard to handle UI presentation (thumbnails, paths).
/// </summary>
public class BoardItemViewModel : ViewModelBase
{
    private readonly AslBoard _item;
    private ImageSource? _thumbnail;
    private bool _isSelected;
    private string? _localImagePath;

    /// <summary>Gets the underlying AslBoard.</summary>
    public AslBoard Item => _item;

    /// <summary>Gets or sets the original name of the board (used to detect renames on disk).</summary>
    public string OriginalName { get; set; }

    /// <summary>Gets or sets a value indicating whether this item is selected in the UI.</summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>Gets the absolute path to the folder containing this board's data.</summary>
    public string BoardFolderPath 
    { 
        get
        {
            string baseFolder = SettingsManager.Instance.Settings.BoardsFolder;
            return string.IsNullOrEmpty(baseFolder) ? string.Empty : Path.Combine(baseFolder, _item.Name);
        }
    }

    /// <summary>Gets the absolute path to the localized background image.</summary>
    public string? LocalImagePath 
    { 
        get => _localImagePath; 
        private set => SetProperty(ref _localImagePath, value);
    }

    /// <summary>Gets the board name from the underlying item.</summary>
    public string Name => _item.Name;

    /// <summary>Gets the board type from the underlying item.</summary>
    public MapType Type => _item.Type;

    /// <summary>Gets the board canvas width.</summary>
    public int CanvasWidth => _item.CanvasWidth;

    /// <summary>Gets the board canvas height.</summary>
    public int CanvasHeight => _item.CanvasHeight;

    /// <summary>Gets a human-readable string for height x width dimensions.</summary>
    public string DimensionsDisplay => $"{_item.Width} x {_item.Height}";

    /// <summary>Gets the thumbnail image source, lazy-loaded.</summary>
    public ImageSource? Thumbnail
    {
        get
        {
            if (_thumbnail == null && !string.IsNullOrEmpty(LocalImagePath) && File.Exists(LocalImagePath))
            {
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(LocalImagePath, UriKind.Absolute);
                    bitmap.DecodePixelWidth = 100; // Efficient thumbnail size
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    _thumbnail = bitmap;
                }
                catch
                {
                    // Fallback or empty
                }
            }
            return _thumbnail;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardItemViewModel"/> class.
    /// </summary>
    /// <param name="board">The board.</param>
    public BoardItemViewModel(AslBoard board)
    {
        _item = board;
        OriginalName = board.Name;
        RefreshMetadata();
    }

    /// <summary>
    /// Re-scans the board folder for metadata and images to refresh UI state.
    /// </summary>
    public void RefreshMetadata()
    {
        string folder = BoardFolderPath;
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;

        _thumbnail = null; // Reset lazy-load
        
        try
        {
            var files = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly);
            string? foundImage = null;
            foreach (var f in files)
            {
                string ext = Path.GetExtension(f).ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".jpeg" || ext == ".gif" || ext == ".bmp")
                {
                    foundImage = f;
                    break;
                }
            }
            LocalImagePath = foundImage;
        }
        catch { }

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Type));
        OnPropertyChanged(nameof(DimensionsDisplay));
        OnPropertyChanged(nameof(Thumbnail));
        OnPropertyChanged(nameof(CanvasWidth));
        OnPropertyChanged(nameof(CanvasHeight));
    }
}
