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
    private string _name = string.Empty;
    private string _morale = string.Empty;
    private string _leadership = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private bool _isLeader = true;
    private bool _hasAssaultFire;
    private bool _hasSprayingFire;
    private bool _canSelfRally;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    public string Leadership { get => _leadership; set => SetProperty(ref _leadership, value); }
    public string Firepower { get => _firepower; set => SetProperty(ref _firepower, value); }
    public string Range { get => _range; set => SetProperty(ref _range, value); }
    public bool IsLeader { get => _isLeader; set => SetProperty(ref _isLeader, value); }
    public bool HasAssaultFire { get => _hasAssaultFire; set => SetProperty(ref _hasAssaultFire, value); }
    public bool HasSprayingFire { get => _hasSprayingFire; set => SetProperty(ref _hasSprayingFire, value); }
    public bool CanSelfRally { get => _canSelfRally; set => SetProperty(ref _canSelfRally, value); }

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
    /// Command to save the current counter and return to the list view.
    /// </summary>
    public RelayCommand SaveCounterCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountersViewModel"/> class.
    /// </summary>
    public CountersViewModel()
    {
        DisplayName = "Counters";
        _selectedNationality = Nationality.German;
        _selectedClass = UnitClass.FirstLine;
        IsAddingCounter = false;

        AddCounterCommand = new RelayCommand(_ => { ResetForm(); IsAddingCounter = true; });
        CancelAddCommand = new RelayCommand(_ => IsAddingCounter = false);
        SaveCounterCommand = new RelayCommand(p => SaveCounter(p));
    }

    private void ResetForm()
    {
        Name = string.Empty;
        Morale = string.Empty;
        Leadership = string.Empty;
        Firepower = string.Empty;
        Range = string.Empty;
        IsLeader = true;
        HasAssaultFire = false;
        HasSprayingFire = false;
        CanSelfRally = false;
        SelectedNationality = Nationality.German;
        SelectedClass = UnitClass.FirstLine;
    }

    private void SaveCounter(object? parameter)
    {
        string? type = parameter as string;
        if (type == "SMC")
        {
            SaveSMC();
        }
        else if (type == "MMC")
        {
            SaveMMC();
        }
    }

    private void SaveSMC()
    {
        int morale = int.TryParse(Morale, out int m) ? m : 7;
        int leadership = int.TryParse(Leadership, out int l) ? l : 0;
        int firepower = int.TryParse(Firepower, out int f) ? f : 1;
        int range = int.TryParse(Range, out int r) ? r : 4;

        BaseASLCounter counter;
        if (IsLeader)
        {
            counter = new Leader(Name, morale, leadership, SelectedNationality);
        }
        else
        {
            counter = new Hero(Name, firepower, range, morale, SelectedNationality);
        }
        Counters.Add(counter);
        IsAddingCounter = false;
    }

    private void SaveMMC()
    {
        int fp = int.TryParse(Firepower, out int f) ? f : 4;
        int range = int.TryParse(Range, out int r) ? r : 6;
        int morale = int.TryParse(Morale, out int m) ? m : 7;

        var squad = new Squad(Name, fp, range, morale, SelectedClass, SelectedNationality)
        {
            HasAssaultFire = HasAssaultFire,
            HasSprayingFire = HasSprayingFire,
            CanSelfRally = CanSelfRally
        };
        Counters.Add(squad);
        IsAddingCounter = false;
    }
}
