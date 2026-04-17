using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel.DataAnnotations;
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
/// ViewModel for administering ASL game modules.
/// Handles module details like name, description, and box images.
/// </summary>
public class ModulesViewModel : CrudViewModelBase<AslModule>, IInitializeableFromRepository
{
    private string _fullName = string.Empty;
    private string _description = string.Empty;
    private Module? _moduleType;
    private string? _frontImage;
    private string? _backImage;
    private bool _isFinished;

    /// <summary>
    /// Gets or sets the full name of the module.
    /// </summary>
    public string FullName { get => _fullName; set { SetProperty(ref _fullName, value); ClearErrors(nameof(FullName)); } }

    /// <summary>
    /// Gets or sets the description of the module.
    /// </summary>
    public string Description { get => _description; set => SetProperty(ref _description, value); }

    /// <summary>
    /// Gets or sets the specific module type/ID.
    /// </summary>
    [Required(ErrorMessage = "Module type is required.")]
    public Module? ModuleType { get => _moduleType; set => SetProperty(ref _moduleType, value); }

    /// <summary>
    /// Gets or sets the path to the front box image.
    /// </summary>
    public string? FrontImage { get => _frontImage; set => SetProperty(ref _frontImage, value); }

    /// <summary>
    /// Gets or sets the path to the back box image.
    /// </summary>
    public string? BackImage { get => _backImage; set => SetProperty(ref _backImage, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the module is finished (all content entered).
    /// </summary>
    public bool IsFinished { get => _isFinished; set => SetProperty(ref _isFinished, value); }

    /// <summary>
    /// Gets the collection of module types that are still available (not yet assigned to a project module).
    /// </summary>
    public ObservableCollection<Module> AvailableModuleTypes { get; } = new();

    /// <summary>
    /// Command to pick the front box image.
    /// </summary>
    public RelayCommand PickFrontImageCommand { get; }

    /// <summary>
    /// Command to pick the back box image.
    /// </summary>
    public RelayCommand PickBackImageCommand { get; }

    /// <summary>
    /// Initializes the ViewModel's items from the central repository.
    /// </summary>
    public void InitializeFromRepository()
    {
        Items.Clear();
        foreach (var module in _repository.AllModules)
        {
            Items.Add(new SelectableItem<AslModule>(module, NotifySelectionChanged));
        }
        UpdateAvailableModuleTypes();
    }

    private readonly IModuleRepository _repository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModulesViewModel"/> class.
    /// </summary>
    public ModulesViewModel(IModuleRepository repository)
    {
        _repository = repository;
        DisplayName = "Modules";
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(true));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(false));
        
        Items.CollectionChanged += (s, e) => UpdateAvailableModuleTypes();
        UpdateAvailableModuleTypes();
    }

    private void UpdateAvailableModuleTypes()
    {
        var allTypes = Enum.GetValues<Module>();
        var usedTypes = Items.Select(m => m.Item.Module).ToHashSet();
        
        // Keep track of current selection to restore it if possible
        var currentSelection = ModuleType;

        AvailableModuleTypes.Clear();
        foreach (var type in allTypes)
        {
            // Show if it's not used OR if it's the one we are currently editing
            if (!usedTypes.Contains(type) || (EditingItem != null && EditingItem.Module == type))
            {
                AvailableModuleTypes.Add(type);
            }
        }

        // Try to restore previous selection
        if (currentSelection.HasValue && AvailableModuleTypes.Contains(currentSelection.Value))
        {
            _moduleType = currentSelection;
        }

        // Force a UI refresh because clearing the collection broke the ComboBox binding
        OnPropertyChanged(nameof(ModuleType));
    }

    /// <summary>
    /// Gets or sets a value indicating whether the "Add" form is currently visible.
    /// Also updates the list of available module types.
    /// </summary>
    public new bool IsAdding
    {
        get => base.IsAdding;
        set 
        { 
            base.IsAdding = value; 
            UpdateAvailableModuleTypes(); 
        }
    }

    private void ExecutePickImage(bool front)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = $"Select Module {(front ? "Front" : "Back")} Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            if (front) FrontImage = openDialog.FileName;
            else BackImage = openDialog.FileName;
        }
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        EditingItem = null;
        FullName = string.Empty;
        Description = string.Empty;
        ModuleType = null;
        UpdateAvailableModuleTypes();
        FrontImage = null;
        BackImage = null;
        IsFinished = false;
        ClearErrors();
    }

    /// <inheritdoc />
    protected override void PopulateForm(AslModule item)
    {
        EditingItem = item;
        FullName = item.FullName;
        Description = item.Description;
        UpdateAvailableModuleTypes();
        ModuleType = item.Module;
        FrontImage = item.FrontImage;
        BackImage = item.BackImage;
        IsFinished = item.IsFinished;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        ClearErrors();
        if (!ValidateAllProperties())
        {
            ShowToast("Please fix the validation errors.");
            return;
        }

        if (Items.Any(m => m.Item != EditingItem && m.Item.FullName.Equals(FullName, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(nameof(FullName), "A module with this name already exists.");
            ShowToast("Duplicate module name found!");
            return;
        }

        var module = new AslModule
        {
            FullName = FullName,
            Description = Description,
            Module = ModuleType!.Value,
            FrontImage = FrontImage,
            BackImage = BackImage,
            IsFinished = IsFinished
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
                    Items[index] = new SelectableItem<AslModule>(module, NotifySelectionChanged);
                    OnItemAdded(module);
                }
            }
        }
        else
        {
            Items.Add(new SelectableItem<AslModule>(module, NotifySelectionChanged));
            OnItemAdded(module);
        }

        IsAdding = false;
    }

    /// <inheritdoc />
    protected override void OnItemAdded(AslModule item) => _repository.Add(item);

    /// <inheritdoc />
    protected override void OnItemRemoved(AslModule item) => _repository.Remove(item);
}
