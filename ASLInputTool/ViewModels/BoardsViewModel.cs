using System;
using System.Collections.ObjectModel;
using System.Linq;
using ASL;
using ASL.Models.Maps;
using ASLInputTool.Infrastructure;

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
                    Height = 11;
                }
            }
        }
    }

    private int _width = 33;
    private int _height = 11;

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
    /// Gets or sets the currently active board editor.
    /// </summary>
    public BoardEditorViewModel? Editor { get => _editor; set => SetProperty(ref _editor, value); }

    /// <summary>
    /// Gets the command to generate the board.
    /// </summary>
    public RelayCommand GenerateCommand { get; }

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
            CanvasHeight = CanvasHeight
        };
        board.PopulateBoard();
        Editor = new BoardEditorViewModel(board);
        ShowToast("Board Editor initialized for: " + Name);
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        EditingItem = null;
        Name = string.Empty;
        Type = MapType.Standard;
        Width = 33;
        Height = 11;
        CanvasWidth = 1000;
        CanvasHeight = 1000;
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
            CanvasHeight = CanvasHeight
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
