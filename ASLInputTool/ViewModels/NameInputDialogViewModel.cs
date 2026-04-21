using System;
using System.Windows.Input;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for a simple input dialog to prompt for a name.
/// </summary>
public class NameInputDialogViewModel : ViewModelBase
{
    private string _name = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="NameInputDialogViewModel"/> class.
    /// </summary>
    public NameInputDialogViewModel()
    {
        OkCommand = new RelayCommand(_ => RequestClose?.Invoke(this, true), _ => !string.IsNullOrWhiteSpace(Name));
        CancelCommand = new RelayCommand(_ => RequestClose?.Invoke(this, false));
    }

    /// <summary>
    /// Gets or sets the name entered by the user.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(ref _name, value))
            {
                ((RelayCommand)OkCommand).RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the prompt text to display in the dialog.
    /// </summary>
    public string Prompt { get; set; } = "Enter Name:";

    /// <summary>
    /// Command to confirm the input.
    /// </summary>
    public ICommand OkCommand { get; }

    /// <summary>
    /// Command to cancel the dialog.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Occurs when the dialog requests to be closed.
    /// </summary>
    public event EventHandler<bool>? RequestClose;
}
