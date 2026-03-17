using System.Collections.ObjectModel;
using ASL;

namespace ASLInputTool.ViewModels;

public class ScenariosViewModel : ViewModelBase
{
    public ObservableCollection<Scenario> Scenarios { get; } = new();

    private bool _isAddingScenario;
    public bool IsAddingScenario
    {
        get => _isAddingScenario;
        set => SetProperty(ref _isAddingScenario, value);
    }

    public RelayCommand AddScenarioCommand { get; }
    public RelayCommand CancelAddCommand { get; }

    public ScenariosViewModel()
    {
        DisplayName = "Scenarios";
        IsAddingScenario = false;

        AddScenarioCommand = new RelayCommand(_ => IsAddingScenario = true);
        CancelAddCommand = new RelayCommand(_ => IsAddingScenario = false);
    }
}
