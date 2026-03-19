namespace ASLInputTool.ViewModels;

/// <summary>
/// A wrapper for any item that adds a selection state for UI interaction.
/// </summary>
/// <typeparam name="T">The type of the wrapped item.</typeparam>
public class SelectableItem<T> : ViewModelBase
{
    private bool _isSelected;
    private readonly System.Action? _onSelectionChanged;

    /// <summary>
    /// Gets or sets a value indicating whether the item is selected in the UI.
    /// </summary>
    public bool IsSelected 
    { 
        get => _isSelected; 
        set 
        { 
            if (SetProperty(ref _isSelected, value))
            {
                _onSelectionChanged?.Invoke();
            }
        } 
    }

    /// <summary>
    /// Gets the wrapped item.
    /// </summary>
    public T Item { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectableItem{T}"/> class.
    /// </summary>
    /// <param name="item">The item to wrap.</param>
    /// <param name="onSelectionChanged">Optional callback when selection state changes.</param>
    public SelectableItem(T item, System.Action? onSelectionChanged = null)
    {
        Item = item;
        _onSelectionChanged = onSelectionChanged;
    }
}
