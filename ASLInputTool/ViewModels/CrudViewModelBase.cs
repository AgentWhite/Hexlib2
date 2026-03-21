using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Abstract base class for ViewModels that manage a list of items with Add/Cancel/Save functionality.
/// </summary>
/// <typeparam name="T">The type of item being managed.</typeparam>
public abstract class CrudViewModelBase<T> : ViewModelBase
{
    private bool _isAdding;
    private string? _toastMessage;
    private bool _isToastVisible;

    /// <summary>Initializes a new instance of the class.</summary>
    protected CrudViewModelBase()
    {
        AddCommand = new RelayCommand(_ => { EditingItem = default; ResetForm(); IsAdding = true; });
        CancelCommand = new RelayCommand(_ => { EditingItem = default; IsAdding = false; });
        SaveCommand = new RelayCommand(p => OnSave(p));
        EditCommand = new RelayCommand(p => ExecuteEdit(p));
        DeleteSelectedCommand = new RelayCommand(_ => OnDeleteSelected());
        
        Items.CollectionChanged += (s, e) => OnPropertyChanged(nameof(HasSelectedItems));
    }

    /// <summary>
    /// Gets the collection of items wrapped for selection.
    /// </summary>
    public ObservableCollection<SelectableItem<T>> Items { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the "Add" form is currently visible.
    /// </summary>
    public virtual bool IsAdding
    {
        get => _isAdding;
        set { SetProperty(ref _isAdding, value); if (!value) ClearErrors(); }
    }

    /// <summary>
    /// Gets a value indicating whether any items are currently selected.
    /// </summary>
    public bool HasSelectedItems => Items.Any(i => i.IsSelected);

    /// <summary>
    /// Gets or sets the message to display in the UI toast notification.
    /// </summary>
    public string? ToastMessage { get => _toastMessage; set => SetProperty(ref _toastMessage, value); }
    
    /// <summary>
    /// Gets or sets a value indicating whether the UI toast notification is visible.
    /// </summary>
    public bool IsToastVisible { get => _isToastVisible; set => SetProperty(ref _isToastVisible, value); }

    /// <summary>
    /// Command to start adding a new item.
    /// </summary>
    public RelayCommand AddCommand { get; }
    
    /// <summary>
    /// Command to cancel the current add/edit operation.
    /// </summary>
    public RelayCommand CancelCommand { get; }
    
    /// <summary>
    /// Command to save the current item.
    /// </summary>
    public RelayCommand SaveCommand { get; }
    
    /// <summary>
    /// Command to start editing an existing item.
    /// </summary>
    public RelayCommand EditCommand { get; }

    /// <summary>
    /// Command to delete selected items.
    /// </summary>
    public RelayCommand DeleteSelectedCommand { get; }

    /// <summary>
    /// Gets or sets the item currently being edited.
    /// </summary>
    protected T? EditingItem { get; set; }

    /// <summary>
    /// Executes the edit command for a specific item.
    /// </summary>
    /// <param name="parameter">The SelectableItem wrapper of the item to edit.</param>
    private void ExecuteEdit(object? parameter)
    {
        if (parameter is SelectableItem<T> wrapper)
        {
            EditingItem = wrapper.Item;
            PopulateForm(wrapper.Item);
            IsAdding = true;
        }
    }

    /// <summary>
    /// Handles the deletion of all currently selected items.
    /// </summary>
    private void OnDeleteSelected()
    {
        var selected = Items.Where(i => i.IsSelected).ToList();
        if (!selected.Any()) return;

        string itemType = typeof(T).Name;
        string message = $"Do you really want to delete {selected.Count} {itemType}{(selected.Count > 1 ? "s" : "")}?";
        
        var result = MessageBox.Show(message, "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            foreach (var wrapper in selected)
            {
                Items.Remove(wrapper);
                OnItemRemoved(wrapper.Item);
            }
            OnPropertyChanged(nameof(HasSelectedItems));
        }
    }

    /// <summary>
    /// Utility to update selection state notification.
    /// </summary>
    public void NotifySelectionChanged() => OnPropertyChanged(nameof(HasSelectedItems));

    /// <summary>
    /// Resets the entry form to its default state.
    /// </summary>
    protected abstract void ResetForm();

    /// <summary>
    /// Populates the entry form with data from an existing item.
    /// </summary>
    /// <param name="item">The item to populate the form from.</param>
    protected abstract void PopulateForm(T item);

    /// <summary>
    /// Logic to handle the save operation.
    /// </summary>
    /// <param name="parameter">The command parameter.</param>
    protected abstract void OnSave(object? parameter);

    /// <summary>
    /// Called when an item is added to the managed collection.
    /// </summary>
    protected virtual void OnItemAdded(T item) { }

    /// <summary>
    /// Called when an item is removed from the managed collection.
    /// </summary>
    protected virtual void OnItemRemoved(T item) { }

    /// <summary>
    /// Displays a temporary toast notification in the UI.
    /// </summary>
    /// <param name="message">The message to display.</param>
    protected async void ShowToast(string message)
    {
        ToastMessage = message;
        IsToastVisible = true;
        await Task.Delay(3000);
        IsToastVisible = false;
    }
}
