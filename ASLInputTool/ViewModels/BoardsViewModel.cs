using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using ASL;
using ASL.Models.Maps;
using ASLInputTool.Infrastructure;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for administering ASL boards.
/// </summary>
public class BoardsViewModel : CrudViewModelBase<BoardItemViewModel>, IInitializeableFromRepository
{
    private bool _isEditMode;
    /// <summary>Gets or sets a value indicating whether the form is in edit mode.</summary>
    public bool IsEditMode
    {
        get => _isEditMode;
        set 
        { 
            if (SetProperty(ref _isEditMode, value)) 
            {
                OnPropertyChanged(nameof(CanGenerate)); 
                OnPropertyChanged(nameof(IsCanvasDimensionsEditable));
                ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>Gets a value indicating whether a board can be generated (not in edit mode).</summary>
    public bool CanGenerate => !IsEditMode;

    /// <summary>Command to delete a board.</summary>
    public ICommand DeleteCommand { get; }

    private string _name = string.Empty;
    private MapType _type = MapType.Standard;
    private int _canvasWidth = 1000;
    private int _canvasHeight = 1000;
    private int _width = 33;
    private int _height = 10;
    private bool _isFirstColShiftedDown = true;
    private bool _isTopRowHalf = true;
    private bool _isBottomRowHalf = true;
    private bool _isLeftEdgeHalf = true;
    private bool _isRightEdgeHalf = true;
    private string _imageFileName = string.Empty;
    private string _imageFullPath = string.Empty;
    private bool _isImageDimensionLocked = false;
    private BoardEditorViewModel? _editor;
    private readonly IBoardRepository _repository;

    /// <summary>Gets or sets the name of the board.</summary>
    public string Name
    {
        get => _name;
        set { SetProperty(ref _name, value); ClearErrors(nameof(Name)); }
    }

    /// <summary>Gets or sets the type of the board.</summary>
    public MapType Type
    {
        get => _type;
        set 
        { 
            if (SetProperty(ref _type, value))
            {
                OnPropertyChanged(nameof(IsDimensionsVisible));
                if (value == MapType.Standard || value == MapType.HalfBoard || value == MapType.BonusPack || value == MapType.StarterPack)
                {
                    Width = (value == MapType.Standard) ? 33 : 17;
                    if (value == MapType.Standard || value == MapType.HalfBoard) Height = 10;
                    else if (value == MapType.BonusPack) Height = 20;
                    else if (value == MapType.StarterPack) Height = 22;

                    IsFirstColShiftedDown = true; // Column A is Low, Column B is High/Halved
                    IsTopRowHalf = true;
                    IsBottomRowHalf = true;
                    IsLeftEdgeHalf = true;
                    IsRightEdgeHalf = true;
                }
            }
        }
    }

    /// <summary>Gets the available board categories.</summary>
    public Array AvailableBoardTypes => Enum.GetValues(typeof(MapType));

    /// <summary>Gets or sets the horizontal dimension.</summary>
    public int Width { get => _width; set { if (SetProperty(ref _width, value)) UpdateActiveBoard(); } }
    /// <summary>Gets or sets the vertical dimension.</summary>
    public int Height { get => _height; set { if (SetProperty(ref _height, value)) UpdateActiveBoard(); } }
    /// <summary>Gets or sets the pixel width of the background canvas.</summary>
    public int CanvasWidth { get => _canvasWidth; set => SetProperty(ref _canvasWidth, value); }
    /// <summary>Gets or sets the pixel height of the background canvas.</summary>
    public int CanvasHeight { get => _canvasHeight; set => SetProperty(ref _canvasHeight, value); }
    
    /// <summary>Gets whether the canvas dimensions are editable.</summary>
    public bool IsCanvasDimensionsEditable => !IsEditMode && !IsImageDimensionLocked;
    
    /// <summary>Gets or sets whether dimensions are shown (Standard is fixed).</summary>
    public bool IsDimensionsVisible => Type == MapType.NonStandard;

    /// <summary>Gets or sets the filename of the background image (for display).</summary>
    public string ImageFileName { get => _imageFileName; set => SetProperty(ref _imageFileName, value); }
    /// <summary>Gets or sets the absolute path of the background image.</summary>
    public string ImageFullPath { get => _imageFullPath; set => SetProperty(ref _imageFullPath, value); }
    /// <summary>Gets or sets value indicating if image is locked.</summary>
    public bool IsImageDimensionLocked 
    { 
        get => _isImageDimensionLocked; 
        set 
        { 
            if (SetProperty(ref _isImageDimensionLocked, value)) OnPropertyChanged(nameof(IsCanvasDimensionsEditable)); 
        } 
    }

    /// <summary>Gets the brush for the background preview.</summary>
    public Brush? ImageBrush
    {
        get
        {
            if (string.IsNullOrEmpty(ImageFullPath) || !System.IO.File.Exists(ImageFullPath)) return null;
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(ImageFullPath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // This prevents file locking
                bitmap.EndInit();
                bitmap.Freeze();
                return new ImageBrush(bitmap) { Opacity = 1.0 };
            }
            catch { return null; }
        }
    }

    /// <summary>Gets or sets a value indicating whether the top row contains half-hexes.</summary>
    public bool IsTopRowHalf { get => _isTopRowHalf; set { if (SetProperty(ref _isTopRowHalf, value)) UpdateActiveBoard(); } }
    /// <summary>Gets or sets a value indicating whether the bottom row contains half-hexes.</summary>
    public bool IsBottomRowHalf { get => _isBottomRowHalf; set { if (SetProperty(ref _isBottomRowHalf, value)) UpdateActiveBoard(); } }
    /// <summary>Gets or sets a value indicating whether the left edge contains half-hexes.</summary>
    public bool IsLeftEdgeHalf { get => _isLeftEdgeHalf; set { if (SetProperty(ref _isLeftEdgeHalf, value)) UpdateActiveBoard(); } }
    /// <summary>Gets or sets a value indicating whether the right edge contains half-hexes.</summary>
    public bool IsRightEdgeHalf { get => _isRightEdgeHalf; set { if (SetProperty(ref _isRightEdgeHalf, value)) UpdateActiveBoard(); } }
    /// <summary>Gets or sets a value indicating if column A is shifted down.</summary>
    public bool IsFirstColShiftedDown { get => _isFirstColShiftedDown; set { if (SetProperty(ref _isFirstColShiftedDown, value)) UpdateActiveBoard(); } }

    /// <summary>Gets or sets the editor.</summary>
    public BoardEditorViewModel? Editor { get => _editor; set => SetProperty(ref _editor, value); }

    /// <summary>Gets the generation command.</summary>
    public RelayCommand GenerateCommand { get; }
    /// <summary>Gets the image pick command.</summary>
    public RelayCommand PickImageCommand { get; }
    /// <summary>Gets the clear image command.</summary>
    public RelayCommand ClearImageCommand { get; }

    /// <summary>Initializes a new instance of the BoardsViewModel.</summary>
    public BoardsViewModel(IBoardRepository repository)
    {
        _repository = repository;
        DisplayName = "Boards";
        GenerateCommand = new RelayCommand(OnGenerate, _ => CanGenerate);
        PickImageCommand = new RelayCommand(OnPickImage);
        ClearImageCommand = new RelayCommand(OnClearImage);
        DeleteCommand = new RelayCommand(OnDelete, _ => IsEditMode || Items.Any(i => i.IsSelected));
        
        SettingsManager.Instance.SettingsChanged += OnSettingsChanged;
        _repository.BoardSaved += OnBoardSaved;
        InitializeFromRepository();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        OnPropertyChanged(nameof(CanAdd));
        RefreshBoards();
    }

    private void OnBoardSaved(object? sender, string boardName)
    {
        // Redraw list to ensure renames/metadata are reflected
        RefreshBoards();
    }

    private async void RefreshBoards()
    {
        Items.Clear();
        var loaded = await _repository.ScanAndLoadAsync();
        foreach (var board in loaded) 
        {
            Items.Add(new SelectableItem<BoardItemViewModel>(new BoardItemViewModel(board), NotifySelectionChanged));
        }
    }

    /// <inheritdoc />
    public void InitializeFromRepository() => RefreshBoards();

    /// <inheritdoc />
    protected override bool CanAdd => !string.IsNullOrWhiteSpace(SettingsManager.Instance.Settings.BoardsFolder);

    private void OnPickImage(object? parameter)
    {
        var openFileDialog = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif" };
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
            catch (Exception ex) { ShowToast("Failed to read image dimensions: " + ex.Message); }
        }
    }

    private void OnClearImage(object? parameter)
    {
        ImageFileName = string.Empty;
        ImageFullPath = string.Empty;
        IsImageDimensionLocked = false;
    }

    private void UpdateActiveBoard()
    {
        if (Editor == null) return;
        
        var board = Editor.Board;
        board.Width = Width;
        board.Height = Height;
        board.IsFirstColShiftedDown = IsFirstColShiftedDown;
        board.IsTopRowHalf = IsTopRowHalf;
        board.IsBottomRowHalf = IsBottomRowHalf;
        board.IsLeftEdgeHalf = IsLeftEdgeHalf;
        board.IsRightEdgeHalf = IsRightEdgeHalf;
        
        Editor.RefreshGrid();
    }

    private void OnGenerate(object? parameter)
    {
        ClearErrors();
        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "Board name is required.");
            ShowToast("Please enter a board name.");
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

        board.PopulateBoard();
        Editor = new BoardEditorViewModel(board, _repository, ImageFullPath);
        ShowToast("Board Editor initialized for: " + Name);
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        IsEditMode = false;
        Name = ""; 
        Type = MapType.Standard;
        IsFirstColShiftedDown = true; // Column A is Low, Column B is High/Halved
        IsTopRowHalf = true; 
        IsBottomRowHalf = true;
        IsLeftEdgeHalf = true; 
        IsRightEdgeHalf = true;
        Width = 33; 
        Height = 10; 
        CanvasWidth = 3000; 
        CanvasHeight = 2000;
        ImageFullPath = "";
        ImageFileName = "";
        IsImageDimensionLocked = false;
        Editor = null;
        OnPropertyChanged(nameof(ImageBrush));
    }

    /// <inheritdoc />
    protected override async void PopulateForm(BoardItemViewModel boardVm)
    {
        IsEditMode = true;
        var board = boardVm.Item;

        Name = board.Name; 
        Type = board.Type;
        Width = board.Width; 
        Height = board.Height;
        CanvasWidth = board.CanvasWidth; 
        CanvasHeight = board.CanvasHeight;
        IsFirstColShiftedDown = board.IsFirstColShiftedDown;
        IsTopRowHalf = board.IsTopRowHalf;
        IsBottomRowHalf = board.IsBottomRowHalf;
        IsLeftEdgeHalf = board.IsLeftEdgeHalf;
        IsRightEdgeHalf = board.IsRightEdgeHalf;
        ImageFullPath = boardVm.LocalImagePath ?? "";
        ImageFileName = System.IO.Path.GetFileName(ImageFullPath);
        IsImageDimensionLocked = !string.IsNullOrEmpty(ImageFullPath);
        
        if (board.Board == null || board.Board.Hexes.Count == 0)
        {
            await _repository.LoadBoardDataAsync(board);
        }

        Editor = new BoardEditorViewModel(board, _repository, ImageFullPath, boardVm.OriginalName);
        OnPropertyChanged(nameof(ImageBrush));
    }

    private async void OnDelete(object? parameter)
    {
        BoardItemViewModel? boardItemVm = null;
        if (IsEditMode && EditingItem != null)
        {
            boardItemVm = EditingItem;
        }
        else
        {
            var wrapper = Items.FirstOrDefault(i => i.IsSelected);
            if (wrapper != null) boardItemVm = wrapper.Item;
        }

        if (boardItemVm == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete board '{boardItemVm.Item.Name}' and all its files?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                // Clear the editor if it's currently editing the board being deleted
                if (Editor != null && Editor.Board == boardItemVm.Item)
                {
                    Editor = null;
                }

                await _repository.DeleteBoardAsync(boardItemVm.Item);
                
                var wrapper = Items.FirstOrDefault(i => i.Item == boardItemVm);
                if (wrapper != null) Items.Remove(wrapper);
                
                if (EditingItem == boardItemVm)
                {
                    IsEditMode = false;
                    EditingItem = null;
                    ResetForm();
                }
                ShowToast($"Board '{boardItemVm.Item.Name}' deleted.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <inheritdoc />
    protected override async void OnSave(object? parameter)
    {
        ClearErrors();
        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "Board name is required.");
            ShowToast("Please enter a board name.");
            return;
        }

        AslBoard board;
        string? originalName = null;

        if (IsEditMode && EditingItem != null)
        {
            board = EditingItem.Item;
            originalName = EditingItem.OriginalName;
            board.Name = Name;
            // Dims/Type locked via UI
        }
        else
        {
            board = new AslBoard 
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
        }

        try 
        {
            if (board.Board == null) board.PopulateBoard();
            await _repository.SaveToDiskAsync(board, ImageFullPath, originalName);
            ShowToast($"Board '{board.Name}' saved to disk.");
        }
        catch (Exception ex) { ShowToast("Failed to save board: " + ex.Message); }

        IsAdding = false;
        IsEditMode = false;
        EditingItem = null;
    }
}
