using System.Collections.ObjectModel;
using System.Linq;
using ASL;
using ASL.Counters;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the Counter entry and list view.
/// Handles management of SMC and MMC counters.
/// </summary>
public class CountersViewModel : ViewModelBase
{
    private Nationality _selectedNationality;
    private UnitClass _selectedClass;

    /// <summary>
    /// Gets or sets the currently selected nationality in the entry form.
    /// </summary>
    public Nationality SelectedNationality
    {
        get => _selectedNationality;
        set => SetProperty(ref _selectedNationality, value);
    }

    /// <summary>
    /// Gets or sets the currently selected unit class in the entry form.
    /// </summary>
    public UnitClass SelectedClass
    {
        get => _selectedClass;
        set => SetProperty(ref _selectedClass, value);
    }

    /// <summary>
    /// Gets the list of available nationalities for selection.
    /// </summary>
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    /// <summary>
    /// Gets the list of available unit classes for selection.
    /// </summary>
    public IEnumerable<UnitClass> UnitClasses => Enum.GetValues(typeof(UnitClass)).Cast<UnitClass>();

    /// <summary>
    /// Gets the collection of counters that have been entered.
    /// </summary>
    public ObservableCollection<BaseASLCounter> Counters { get; } = new();

    private bool _isAddingCounter;

    /// <summary>
    /// Gets or sets a value indicating whether the user is currently in the "Add Counter" form view.
    /// When false, the UI displays the list (table) of counters instead.
    /// </summary>
    public bool IsAddingCounter
    {
        get => _isAddingCounter;
        set => SetProperty(ref _isAddingCounter, value);
    }

    /// <summary>
    /// Command to switch the view from the list to the "Add Counter" form.
    /// </summary>
    public RelayCommand AddCounterCommand { get; }

    /// <summary>
    /// Command to cancel the current add operation and return to the list view.
    /// </summary>
    public RelayCommand CancelAddCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountersViewModel"/> class.
    /// </summary>
    public CountersViewModel()
    {
        DisplayName = "Counters";
        _selectedNationality = Nationality.German;
        _selectedClass = UnitClass.FirstLine;
        IsAddingCounter = false;

        AddCounterCommand = new RelayCommand(_ => IsAddingCounter = true);
        CancelAddCommand = new RelayCommand(_ => IsAddingCounter = false);
    }
}
