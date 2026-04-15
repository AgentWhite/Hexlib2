using System;
using System.Collections.ObjectModel;
using System.Linq;
using ASL;
using ASL.Models.Maps;
using ASLInputTool.Infrastructure;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for administering ASL boards.
/// </summary>
public class BoardsViewModel : CrudViewModelBase<AslBoard>, IInitializeableFromRepository
{
    private string _name = string.Empty;
    private MapType _type = MapType.Standard;
    private int _canvasWidth = 1000;
    private int _canvasHeight = 1000;
    private bool _isFirstColShiftedDown = true;
    private bool _isTopRowHalf = true;
    /// <summary>
    /// Gets or sets a value indicating whether the top row contains half-hexes.
    /// </summary>
    public bool IsTopRowHalf
    {
        get => _isTopRowHalf;
        set => SetProperty(ref _isTopRowHalf, value);
    }

    private bool _isBottomRowHalf = true;
    /// <summary>
    /// Gets or sets a value indicating whether the bottom row contains half-hexes.
    /// </summary>
    public bool IsBottomRowHalf
    {
        get => _isBottomRowHalf;
        set => SetProperty(ref _isBottomRowHalf, value);
    }

    private bool _isLeftEdgeHalf = true;
    /// <summary>
    /// Gets or sets a value indicating whether the left edge contains half-hexes.
    /// </summary>
    public bool IsLeftEdgeHalf
    {
        get => _isLeftEdgeHalf;
        set => SetProperty(ref _isLeftEdgeHalf, value);
    }

    private bool _isRightEdgeHalf = true;
    /// <summary>
    /// Gets or sets a value indicating whether the right edge contains half-hexes.
    /// </summary>
    public bool IsRightEdgeHalf
    {
        get => _isRightEdgeHalf;
        set => SetProperty(ref _isRightEdgeHalf, value);
    }

    private string _imageFileName = string.Empty;
    private bool _isImageDimensionLocked = false;
    private BoardEditorViewModel? _editor;
    private readonly IBoardRepository _repository;

    /// <summary>
    /// Gets or sets the name of the board.
    /// </summary>
    public string Name
    {
        get => _name;
        set { SetProperty(ref _name, value); ClearErrors(nameof(Name)); }
    }

    /// <summary>
    /// Gets or sets the type of the board.
    /// </summary>
    public MapType Type
    {
        get => _type;
        set 
        { 
            if (SetProperty(ref _type, value))
            {
                OnPropertyChanged(nameof(IsDimensionsVisible));
                if (value == MapType.Standard)
                {
                    Width = 33;
                    Height = 10;
                    IsFirstColShiftedDown = true;
                    IsTopRowHalf = true;
                    IsBottomRowHalf = true;
                    IsLeftEdgeHalf = true;
                    IsRightEdgeHalf = true;
                }
            }
        }
    }

    private int _width = 33;
    private int _height = 10;

    /// <summary>
    /// Gets or sets the width of the board.
    /// </summary>
    public int Width { get => _width; set => SetProperty(ref _width, value); }

    /// <summary>
    /// Gets or sets the height of the board.
    /// </summary>
    public int Height { get => _height; set => SetProperty(ref _height, value); }

    /// <summary>
    /// Gets a value indicating whether the dimensions are visible for the current board type.
    /// </summary>
    public bool IsDimensionsVisible => Type != MapType.Standard;

    /// <summary>
    /// Gets the list of available board types.
    /// </summary>
    public ObservableCollection<MapType> AvailableBoardTypes { get; } = new(Enum.GetValues<MapType>());

    /// <summary>
    /// Gets or sets the canvas width.
    /// </summary>
    public int CanvasWidth { get => _canvasWidth; set => SetProperty(ref _canvasWidth, value); }

    /// <summary>
    /// Gets or sets the canvas height.
    /// </summary>
    public int CanvasHeight { get => _canvasHeight; set => SetProperty(ref _canvasHeight, value); }

    /// <summary>
    /// Gets or sets the file name of the picked image.
    /// </summary>
    public string ImageFileName
    {
        get => _imageFileName;
        set => SetProperty(ref _imageFileName, value);
    }

    private string _imageFullPath = string.Empty;
    /// <summary>
    /// Gets or sets the absolute path to the picked image.
    /// </summary>
    public string ImageFullPath
    {
        get => _imageFullPath;
        set => SetProperty(ref _imageFullPath, value);
    }

    /// <summary>
    /// Gets or sets whether the canvas dimensions are locked by the picked image.
    /// </summary>
    public bool IsImageDimensionLocked
    {
        get => _isImageDimensionLocked;
        set 
        {
            if (SetProperty(ref _isImageDimensionLocked, value))
            {
                OnPropertyChanged(nameof(IsCanvasDimensionsEditable));
            }
        }
    }

    /// <summary>
    /// Gets whether canvas dimensions can be edited manually.
    /// </summary>
    public bool IsCanvasDimensionsEditable => !IsImageDimensionLocked;

    /// <summary>
    /// Gets or sets a value indicating whether the first column is shifted down.
    /// </summary>
    public bool IsFirstColShiftedDown { get => _isFirstColShiftedDown; set => SetProperty(ref _isFirstColShiftedDown, value); }

    /// <summary>
    /// Gets or sets the currently active board editor.
    /// </summary>
    public BoardEditorViewModel? Editor { get => _editor; set => SetProperty(ref _editor, value); }

    /// <summary>
    /// Gets the command to generate the board.
    /// </summary>
    public RelayCommand GenerateCommand { get; }

    /// <summary>
    /// Command to pick a background image to derive canvas dimensions.
    /// </summary>
    public RelayCommand PickImageCommand { get; }

    /// <summary>
    /// Command to clear the picked image.
    /// </summary>
    public RelayCommand ClearImageCommand { get; }

    /// <summary>
    /// Initializes the ViewModel's items from the central repository.
    /// </summary>
    public void InitializeFromRepository()
    {
        Items.Clear();
        foreach (var board in _repository.AllBoards)
        {
            Items.Add(new SelectableItem<AslBoard>(board, NotifySelectionChanged));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BoardsViewModel"/> class.
    /// </summary>
    /// <param name="repository">The board repository.</param>
    public BoardsViewModel(IBoardRepository repository)
    {
        _repository = repository;
        DisplayName = "Boards";
        GenerateCommand = new RelayCommand(OnGenerate);
        PickImageCommand = new RelayCommand(OnPickImage);
        ClearImageCommand = new RelayCommand(OnClearImage);
    }

    private void OnPickImage(object? parameter)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Board Image",
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                using (var stream = System.IO.File.OpenRead(openFileDialog.FileName))
                {
                    var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                    CanvasWidth = decoder.Frames[0].PixelWidth;
                    CanvasHeight = decoder.Frames[0].PixelHeight;
                }
                ImageFileName = System.IO.Path.GetFileName(openFileDialog.FileName);
                ImageFullPath = openFileDialog.FileName;
                IsImageDimensionLocked = true;
                ClearErrors(nameof(CanvasWidth));
                ClearErrors(nameof(CanvasHeight));
            }
            catch (Exception ex)
            {
                ShowToast("Failed to read image dimensions: " + ex.Message);
            }
        }
    }

    private void OnClearImage(object? parameter)
    {
        ImageFileName = string.Empty;
        ImageFullPath = string.Empty;
        IsImageDimensionLocked = false;
    }

    private void OnGenerate(object? parameter)
    {
        var board = new AslBoard 
        { 
            Name = Name, 
            Type = Type, 
            Width = Width, 
            Height = Height,
            CanvasWidth = CanvasWidth,
            CanvasHeight = CanvasHeight,
            IsFirstColShiftedDown = IsFirstColShiftedDown,
            IsTopRowHalf = IsTopRowHalf,
            IsBottomRowHalf = IsBottomRowHalf,
            IsLeftEdgeHalf = IsLeftEdgeHalf,
            IsRightEdgeHalf = IsRightEdgeHalf
        };
        board.PopulateBoard();
        Editor = new BoardEditorViewModel(board, ImageFullPath);
        ShowToast("Board Editor initialized for: " + Name);
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        EditingItem = null;
        Name = string.Empty;
        Type = MapType.Standard;
        Width = 33;
        Height = 10;
        CanvasWidth = 1000;
        CanvasHeight = 1000;
        IsFirstColShiftedDown = true;
        IsTopRowHalf = true;
        IsBottomRowHalf = true;
        IsLeftEdgeHalf = true;
        IsRightEdgeHalf = true;
        ImageFileName = string.Empty;
        ImageFullPath = string.Empty;
        IsImageDimensionLocked = false;
        Editor = null;
        ClearErrors();
    }

    /// <inheritdoc />
    protected override void PopulateForm(AslBoard item)
    {
        EditingItem = item;
        Name = item.Name;
        Type = item.Type;
        Width = item.Width;
        Height = item.Height;
        CanvasWidth = item.CanvasWidth;
        CanvasHeight = item.CanvasHeight;
        IsFirstColShiftedDown = item.IsFirstColShiftedDown;
        Editor = null;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        ClearErrors();
        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "Board name is required.");
            ShowToast("Please enter a board name.");
            return;
        }

        if (Items.Any(m => m.Item != EditingItem && m.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(nameof(Name), "A board with this name already exists.");
            ShowToast("Duplicate board name found!");
            return;
        }

        var board = new AslBoard 
        { 
            Name = Name, 
            Type = Type, 
            Width = Width, 
            Height = Height,
            CanvasWidth = CanvasWidth,
            CanvasHeight = CanvasHeight,
            IsFirstColShiftedDown = IsFirstColShiftedDown,
            IsTopRowHalf = IsTopRowHalf,
            IsBottomRowHalf = IsBottomRowHalf,
            IsLeftEdgeHalf = IsLeftEdgeHalf,
            IsRightEdgeHalf = IsRightEdgeHalf
        };

        if (EditingItem != null)
        {
            var wrapper = Items.FirstOrDefault(i => i.Item == EditingItem);
            if (wrapper != null)
            {
                int index = Items.IndexOf(wrapper);
                if (index >= 0)
                {
                    OnItemRemoved(EditingItem);
                    Items[index] = new SelectableItem<AslBoard>(board, NotifySelectionChanged);
                    OnItemAdded(board);
                }
            }
        }
        else
        {
            Items.Add(new SelectableItem<AslBoard>(board, NotifySelectionChanged));
            OnItemAdded(board);
        }

        IsAdding = false;
    }

    /// <inheritdoc />
    protected override void OnItemAdded(AslBoard item) => _repository.Add(item);

    /// <inheritdoc />
    protected override void OnItemRemoved(AslBoard item) => _repository.Remove(item);
}
