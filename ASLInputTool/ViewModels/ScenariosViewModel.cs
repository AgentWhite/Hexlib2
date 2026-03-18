using System.Collections.ObjectModel;
using ASL;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the Scenario entry and list view.
/// Handles historical scenarios including place, date, and description.
/// </summary>
public class ScenariosViewModel : ViewModelBase
{
    private string _name = string.Empty;
    private string _reference = string.Empty;
    private string _place = string.Empty;
    private string _date = string.Empty;
    private string _descriptionText = string.Empty;
    private string? _toastMessage;
    private bool _isToastVisible;

    public string Name { get => _name; set { SetProperty(ref _name, value); ClearErrors(nameof(Name)); } }
    public string Reference { get => _reference; set { SetProperty(ref _reference, value); ClearErrors(nameof(Reference)); } }
    public string Place { get => _place; set => SetProperty(ref _place, value); }
    public string Date { get => _date; set => SetProperty(ref _date, value); }
    public string DescriptionText { get => _descriptionText; set => SetProperty(ref _descriptionText, value); }

    public string? ToastMessage { get => _toastMessage; set => SetProperty(ref _toastMessage, value); }
    public bool IsToastVisible { get => _isToastVisible; set => SetProperty(ref _isToastVisible, value); }

    /// <summary>
    /// Gets the collection of scenarios that have been entered.
    /// </summary>
    public ObservableCollection<Scenario> Scenarios { get; } = new();

    private bool _isAddingScenario;

    /// <summary>
    /// Gets or sets a value indicating whether the user is currently in the "Add Scenario" form view.
    /// When false, the UI displays the list (table) of scenarios instead.
    /// </summary>
    public bool IsAddingScenario
    {
        get => _isAddingScenario;
        set { SetProperty(ref _isAddingScenario, value); if (!value) ClearErrors(); }
    }

    /// <summary>
    /// Command to switch the view from the list to the "Add Scenario" form.
    /// </summary>
    public RelayCommand AddScenarioCommand { get; }

    /// <summary>
    /// Command to cancel the current add operation and return to the list view.
    /// </summary>
    public RelayCommand CancelAddCommand { get; }

    /// <summary>
    /// Command to save the current scenario and return to the list view.
    /// </summary>
    public RelayCommand SaveScenarioCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosViewModel"/> class.
    /// </summary>
    public ScenariosViewModel()
    {
        DisplayName = "Scenarios";
        IsAddingScenario = false;

        AddScenarioCommand = new RelayCommand(_ => { ResetForm(); IsAddingScenario = true; });
        CancelAddCommand = new RelayCommand(_ => IsAddingScenario = false);
        SaveScenarioCommand = new RelayCommand(_ => SaveScenario());
    }

    private void ResetForm()
    {
        Name = string.Empty;
        Reference = string.Empty;
        Place = string.Empty;
        Date = string.Empty;
        DescriptionText = string.Empty;
        ClearErrors();
    }

    private async void ShowToast(string message)
    {
        ToastMessage = message;
        IsToastVisible = true;
        await Task.Delay(3000);
        IsToastVisible = false;
    }

    private void SaveScenario()
    {
        ClearErrors();
        bool hasValidationError = false;

        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "Name is required.");
            hasValidationError = true;
        }

        if (string.IsNullOrWhiteSpace(Reference))
        {
            AddError(nameof(Reference), "Reference is required.");
            hasValidationError = true;
        }

        if (hasValidationError)
        {
            ShowToast("Please fill in all required fields.");
            return;
        }

        if (Scenarios.Any(s => s.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(nameof(Name), "A scenario with this name already exists.");
            ShowToast("Duplicate scenario name found!");
            return;
        }

        if (Scenarios.Any(s => s.Reference.Equals(Reference, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(nameof(Reference), "A scenario with this reference already exists.");
            ShowToast("Duplicate scenario reference found!");
            return;
        }

        var scenario = new Scenario
        {
            Name = Name,
            Reference = Reference,
            Description = new ScenarioDescription(Place, Date, DescriptionText)
        };
        Scenarios.Add(scenario);
        IsAddingScenario = false;
    }
}
