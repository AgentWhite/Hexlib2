using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ASL.Models.Scenarios;
using ASLInputTool.Infrastructure;
using Microsoft.Win32;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the Insignia selection dialog.
/// Allows choosing an existing insignia or creating a new one.
/// </summary>
public class InsigniaSelectionViewModel : ViewModelBase
{
    private readonly IScenarioRepository _repository;
    private Insignia? _selectedInsignia;
    private bool _isCreatingNew;
    private string _newInsigniaName = string.Empty;
    private string? _newInsigniaImagePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="InsigniaSelectionViewModel"/> class.
    /// </summary>
    /// <param name="repository">The repository to manage insignias.</param>
    public InsigniaSelectionViewModel(IScenarioRepository repository)
    {
        _repository = repository;
        AvailableInsignias = _repository.AllInsignias;
        
        ToggleNewCommand = new RelayCommand(_ => IsCreatingNew = !IsCreatingNew);
        PickImageCommand = new RelayCommand(_ => ExecutePickImage());
        SaveCommand = new RelayCommand(async _ => await ExecuteSave(), _ => CanSave());
        OkCommand = new RelayCommand(_ => ExecuteOk(), _ => SelectedInsignia != null);
        CancelCommand = new RelayCommand(_ => ExecuteCancel());
    }

    /// <summary>
    /// Gets the collection of available insignias.
    /// </summary>
    public ObservableCollection<Insignia> AvailableInsignias { get; }

    /// <summary>
    /// Gets or sets the currently selected insignia from the list.
    /// </summary>
    public Insignia? SelectedInsignia
    {
        get => _selectedInsignia;
        set => SetProperty(ref _selectedInsignia, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the user is creating a new insignia.
    /// </summary>
    public bool IsCreatingNew
    {
        get => _isCreatingNew;
        set => SetProperty(ref _isCreatingNew, value);
    }

    /// <summary>
    /// Gets or sets the name for a new insignia being created.
    /// </summary>
    public string NewInsigniaName
    {
        get => _newInsigniaName;
        set { if (SetProperty(ref _newInsigniaName, value)) ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
    }

    /// <summary>
    /// Gets or sets the image path for a new insignia being created.
    /// </summary>
    public string? NewInsigniaImagePath
    {
        get => _newInsigniaImagePath;
        set { if (SetProperty(ref _newInsigniaImagePath, value)) ((RelayCommand)SaveCommand).RaiseCanExecuteChanged(); }
    }

    /// <summary>
    /// Command to toggle the 'Create New' section.
    /// </summary>
    public ICommand ToggleNewCommand { get; }

    /// <summary>
    /// Command to pick an image for a new insignia.
    /// </summary>
    public ICommand PickImageCommand { get; }

    /// <summary>
    /// Command to save a new insignia.
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// Command to confirm selection.
    /// </summary>
    public ICommand OkCommand { get; }

    /// <summary>
    /// Command to cancel the dialog.
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// Gets the insignia that was finally selected or created.
    /// </summary>
    public Insignia? Result { get; private set; }

    /// <summary>
    /// Occurs when the dialog requests to be closed.
    /// The boolean parameter indicates whether the result was confirmed (True) or cancelled (False).
    /// </summary>
    public event EventHandler<bool>? RequestClose;

    private void ExecutePickImage()
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = "Select Insignia Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            NewInsigniaImagePath = openDialog.FileName;
        }
    }

    private bool CanSave() => !string.IsNullOrWhiteSpace(NewInsigniaName) && !string.IsNullOrWhiteSpace(NewInsigniaImagePath);

    private async Task ExecuteSave()
    {
        var insignia = new Insignia
        {
            Name = NewInsigniaName,
            ImagePath = NewInsigniaImagePath
        };

        await _repository.SaveInsigniaAsync(insignia);
        Result = insignia;
        RequestClose?.Invoke(this, true);
    }

    private void ExecuteOk()
    {
        Result = SelectedInsignia;
        RequestClose?.Invoke(this, true);
    }

    private void ExecuteCancel()
    {
        RequestClose?.Invoke(this, false);
    }
}
