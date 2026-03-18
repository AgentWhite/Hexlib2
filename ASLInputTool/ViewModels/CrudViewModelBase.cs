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

    public string? ToastMessage { get => _toastMessage; set => SetProperty(ref _toastMessage, value); }
    public bool IsToastVisible { get => _isToastVisible; set => SetProperty(ref _isToastVisible, value); }

    public RelayCommand AddCommand { get; }
    public RelayCommand CancelCommand { get; }
    public RelayCommand SaveCommand { get; }

    protected CrudViewModelBase()
    {
        AddCommand = new RelayCommand(_ => { ResetForm(); IsAdding = true; });
        CancelCommand = new RelayCommand(_ => IsAdding = false);
        SaveCommand = new RelayCommand(p => OnSave(p));
    }

    /// <summary>
    /// Resets the entry form to its default state.
    /// </summary>
    protected abstract void ResetForm();

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
