using System.Collections.ObjectModel;
using ASL;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the Scenario entry and list view.
/// Handles historical scenarios including place, date, and description.
/// </summary>
public class ScenariosViewModel : ViewModelBase
{
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
        set => SetProperty(ref _isAddingScenario, value);
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
    /// Initializes a new instance of the <see cref="ScenariosViewModel"/> class.
    /// </summary>
    public ScenariosViewModel()
    {
        DisplayName = "Scenarios";
        IsAddingScenario = false;

        AddScenarioCommand = new RelayCommand(_ => IsAddingScenario = true);
        CancelAddCommand = new RelayCommand(_ => IsAddingScenario = false);
    }
}
