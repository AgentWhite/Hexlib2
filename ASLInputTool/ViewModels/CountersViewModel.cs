using System.Collections.ObjectModel;
using System.Linq;
using ASL;
using ASL.Counters;

namespace ASLInputTool.ViewModels;

public class CountersViewModel : ViewModelBase
{
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    private Nationality _selectedNationality;
    public Nationality SelectedNationality
    {
        get => _selectedNationality;
        set => SetProperty(ref _selectedNationality, value);
    }

    public IEnumerable<UnitClass> UnitClasses => Enum.GetValues(typeof(UnitClass)).Cast<UnitClass>();

    private UnitClass _selectedClass;
    public UnitClass SelectedClass
    {
        get => _selectedClass;
        set => SetProperty(ref _selectedClass, value);
    }

    public ObservableCollection<BaseASLCounter> Counters { get; } = new();

    private bool _isAddingCounter;
    public bool IsAddingCounter
    {
        get => _isAddingCounter;
        set => SetProperty(ref _isAddingCounter, value);
    }

    public RelayCommand AddCounterCommand { get; }
    public RelayCommand CancelAddCommand { get; }

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
