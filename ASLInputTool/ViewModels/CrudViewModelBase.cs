using System.Collections.ObjectModel;
using System.Threading.Tasks;

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
    }

    /// <summary>
    /// Gets the collection of items.
    /// </summary>
    public ObservableCollection<T> Items { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the "Add" form is currently visible.
    /// </summary>
    public bool IsAdding
    {
        get => _isAdding;
        set { SetProperty(ref _isAdding, value); if (!value) ClearErrors(); }
    }

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

    protected T? EditingItem { get; set; }

    private void ExecuteEdit(object? parameter)
    {
        if (parameter is T item)
        {
            EditingItem = item;
            PopulateForm(item);
            IsAdding = true;
        }
    }

    /// <summary>
    /// Resets the entry form to its default state.
    /// </summary>
    protected abstract void ResetForm();

    /// <summary>
    /// Populates the entry form with data from an existing item.
    /// </summary>
    protected abstract void PopulateForm(T item);

    /// <summary>
    /// Logic to handle the save operation.
    /// </summary>
    protected abstract void OnSave(object? parameter);

    protected async void ShowToast(string message)
    {
        ToastMessage = message;
        IsToastVisible = true;
        await Task.Delay(3000);
        IsToastVisible = false;
    }
}
