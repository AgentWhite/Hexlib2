using System.Collections.ObjectModel;
using ASL;
using ASL.Core;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Infrastructure;
using ASL.Services;
using ASL.Models;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the Scenario entry and list view.
/// Handles historical scenarios including place, date, description, and sides.
/// </summary>
public class ScenariosViewModel : CrudViewModelBase<Scenario>, IInitializeableFromRepository
{
    private string _name = string.Empty;
    private string _reference = string.Empty;
    private string _place = string.Empty;
    private string _date = string.Empty;
    private string _descriptionText = string.Empty;
    private string _aftermath = string.Empty;
    private string? _imagePath;
    private int _turns = 1;
    private bool _hasHalfTurn;

    // Scenario Side A
    private string _sideAName = string.Empty;
    private Side _sideASide = Side.Attacker;
    private Nationality _sideANationality = Nationality.German;
    private Insignia? _sideAInsignia;

    // Scenario Side B
    private string _sideBName = string.Empty;
    private Side _sideBSide = Side.Defender;
    private Nationality _sideBNationality = Nationality.Russian;
    private Insignia? _sideBInsignia;

    // Scenario Formations
    /// <summary> Gets the collection of formations for Side A. </summary>
    public ObservableCollection<Formation> SideAFormations { get; } = new();
    /// <summary> Gets the collection of formations for Side B. </summary>
    public ObservableCollection<Formation> SideBFormations { get; } = new();

    /// <summary>
    /// Gets a value indicating whether both side names are defined.
    /// Used to control visibility of advanced side settings.
    /// </summary>
    public bool AreSidesDefined => !string.IsNullOrWhiteSpace(SideAName) && !string.IsNullOrWhiteSpace(SideBName);

    /// <summary>
    /// Gets or sets the name of the scenario.
    /// </summary>
    public string Name { get => _name; set { SetProperty(ref _name, value); ValidateName(); } }
    /// <summary>
    /// Gets or sets the reference ID or publication source for the scenario.
    /// </summary>
    public string Reference { get => _reference; set { SetProperty(ref _reference, value); ValidateReference(); } }
    /// <summary>
    /// Gets or sets the historical place where the scenario occurred.
    /// </summary>
    public string Place { get => _place; set => SetProperty(ref _place, value); }
    /// <summary>
    /// Gets or sets the date/year of the scenario.
    /// </summary>
    public string Date { get => _date; set => SetProperty(ref _date, value); }
    /// <summary>
    /// Gets or sets the historical description text.
    /// </summary>
    public string DescriptionText { get => _descriptionText; set => SetProperty(ref _descriptionText, value); }
    /// <summary>
    /// Gets or sets the aftermath text for the scenario.
    /// </summary>
    public string Aftermath { get => _aftermath; set => SetProperty(ref _aftermath, value); }
    /// <summary>
    /// Gets or sets the path to the image representing the scenario (e.g., from the scenario card).
    /// </summary>
    public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }

    /// <summary>
    /// Command to pick an image for the scenario.
    /// </summary>
    public RelayCommand PickImageCommand { get; }

    /// <summary>
    /// Gets or sets the number of turns for the scenario.
    /// </summary>
    public int Turns { get => _turns; set { SetProperty(ref _turns, value); ValidateTurns(); } }

    /// <summary>
    /// Gets or sets a value indicating whether the final turn is a half-turn.
    /// </summary>
    public bool HasHalfTurn { get => _hasHalfTurn; set => SetProperty(ref _hasHalfTurn, value); }

    /// <summary>
    /// Gets or sets the name for Side A.
    /// </summary>
    public string SideAName { get => _sideAName; set { SetProperty(ref _sideAName, value); ValidateSideAName(); OnPropertyChanged(nameof(AreSidesDefined)); } }
    
    /// <summary>
    /// Gets or sets the insignia for Side A.
    /// </summary>
    public Insignia? SideAInsignia { get => _sideAInsignia; set => SetProperty(ref _sideAInsignia, value); }
    
    /// <summary>
    /// Gets or sets the tactical side for Side A.
    /// </summary>
    public Side SideASide
    {
        get => _sideASide;
        set
        {
            if (SetProperty(ref _sideASide, value))
            {
                SideBSide = value == Side.Attacker ? Side.Defender : Side.Attacker;
            }
        }
    }

    /// <summary>
    /// Gets or sets the nationality for Side A.
    /// </summary>
    public Nationality SideANationality { get => _sideANationality; set => SetProperty(ref _sideANationality, value); }

    /// <summary>
    /// Gets or sets the name for Side B.
    /// </summary>
    public string SideBName { get => _sideBName; set { SetProperty(ref _sideBName, value); ValidateSideBName(); OnPropertyChanged(nameof(AreSidesDefined)); } }

    /// <summary>
    /// Gets or sets the insignia for Side B.
    /// </summary>
    public Insignia? SideBInsignia { get => _sideBInsignia; set => SetProperty(ref _sideBInsignia, value); }

    /// <summary>
    /// Gets or sets the tactical side for Side B.
    /// </summary>
    public Side SideBSide
    {
        get => _sideBSide;
        set
        {
            if (SetProperty(ref _sideBSide, value))
            {
                SideASide = value == Side.Attacker ? Side.Defender : Side.Attacker;
            }
        }
    }

    /// <summary>
    /// Gets or sets the nationality for Side B.
    /// </summary>
    public Nationality SideBNationality { get => _sideBNationality; set => SetProperty(ref _sideBNationality, value); }

    /// <summary>
    /// Gets the available sides for selection.
    /// </summary>
    public IEnumerable<Side> AvailableSides => Enum.GetValues<Side>();

    /// <summary>
    /// Gets the available nationalities for selection.
    /// </summary>
    public IEnumerable<Nationality> AvailableNationalities => Enum.GetValues<Nationality>();

    /// <summary>
    /// Command to select an insignia for a side.
    /// </summary>
    public RelayCommand SelectInsigniaCommand { get; }

    /// <summary>
    /// Command to add a new formation to a side.
    /// </summary>
    public RelayCommand AddFormationCommand { get; }

    /// <summary>
    /// Command to remove a formation from a side.
    /// </summary>
    public RelayCommand RemoveFormationCommand { get; }

    private readonly IScenarioRepository _repository;

    /// <inheritdoc />
    protected override bool CanAdd => !string.IsNullOrWhiteSpace(SettingsManager.Instance.Settings.ScenariosFolder);

    /// <summary>
    /// Initializes the ViewModel's items from the central repository.
    /// </summary>
    public void InitializeFromRepository()
    {
        _ = _repository.LoadInsigniasAsync();
        Items.Clear();
        foreach (var scenario in _repository.AllScenarios)
        {
            Items.Add(new SelectableItem<Scenario>(scenario, NotifySelectionChanged));
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosViewModel"/> class.
    /// </summary>
    public ScenariosViewModel(IScenarioRepository repository)
    {
        _repository = repository;
        DisplayName = "Scenarios";
        PickImageCommand = new RelayCommand(_ => ExecutePickImage());
        SelectInsigniaCommand = new RelayCommand(p => ExecuteSelectInsignia(p));
        AddFormationCommand = new RelayCommand(p => ExecuteAddFormation(p));
        RemoveFormationCommand = new RelayCommand(p => ExecuteRemoveFormation(p));

        SettingsManager.Instance.SettingsChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(CanAdd));
            AddCommand.RaiseCanExecuteChanged();
        };
    }

    private void ValidateName()
    {
        ClearErrors(nameof(Name));
        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "Scenario name is required.");
            ShowToast("Scenario name is required.");
        }
        else if (Items.Any(s => s.Item != EditingItem && s.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(nameof(Name), "A scenario with this name already exists.");
            ShowToast("Duplicate scenario name found!");
        }
    }

    private void ValidateReference()
    {
        ClearErrors(nameof(Reference));
        if (string.IsNullOrWhiteSpace(Reference))
        {
            AddError(nameof(Reference), "Reference is required.");
            ShowToast("Reference is required.");
        }
        else if (Items.Any(s => s.Item != EditingItem && s.Item.Reference.Equals(Reference, StringComparison.OrdinalIgnoreCase)))
        {
            AddError(nameof(Reference), "A scenario with this reference already exists.");
            ShowToast("Duplicate scenario reference found!");
        }
    }

    private void ValidateTurns()
    {
        ClearErrors(nameof(Turns));
        if (Turns < 1)
        {
            AddError(nameof(Turns), "Number of turns must be at least 1.");
            ShowToast("Invalid number of turns!");
        }
    }

    private void ValidateSideAName()
    {
        ClearErrors(nameof(SideAName));
        if (string.IsNullOrWhiteSpace(SideAName)) AddError(nameof(SideAName), "Side A name is required.");
    }

    private void ValidateSideBName()
    {
        ClearErrors(nameof(SideBName));
        if (string.IsNullOrWhiteSpace(SideBName)) AddError(nameof(SideBName), "Side B name is required.");
    }

    private void ValidateSides()
    {
        ValidateSideAName();
        ValidateSideBName();
    }

    private void ExecutePickImage()
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = "Select Scenario Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            ImagePath = openDialog.FileName;
        }
    }

    private void ExecuteSelectInsignia(object? parameter)
    {
        var side = parameter as string;
        var vm = new InsigniaSelectionViewModel(_repository);
        if (side == "SideA") vm.SelectedInsignia = SideAInsignia;
        else if (side == "SideB") vm.SelectedInsignia = SideBInsignia;

        var dialog = new ASLInputTool.Views.InsigniaSelectionDialog(vm)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            if (side == "SideA") SideAInsignia = vm.Result;
            else if (side == "SideB") SideBInsignia = vm.Result;
        }
    }

    private void ExecuteAddFormation(object? parameter)
    {
        var side = parameter as string;
        var vm = new NameInputDialogViewModel { Prompt = "Enter Formation Name:" };
        var dialog = new ASLInputTool.Views.NameInputDialog(vm)
        {
            Owner = System.Windows.Application.Current.MainWindow
        };

        if (dialog.ShowDialog() == true)
        {
            var formation = new Formation { Name = vm.Name };
            if (side == "SideA") SideAFormations.Add(formation);
            else if (side == "SideB") SideBFormations.Add(formation);
        }
    }

    private void ExecuteRemoveFormation(object? parameter)
    {
        if (parameter is Formation formation)
        {
            if (SideAFormations.Contains(formation)) SideAFormations.Remove(formation);
            else if (SideBFormations.Contains(formation)) SideBFormations.Remove(formation);
        }
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        _name = string.Empty;
        _reference = string.Empty;
        _place = string.Empty;
        _date = string.Empty;
        _descriptionText = string.Empty;
        _aftermath = string.Empty;
        _turns = 1;
        _hasHalfTurn = false;
        _sideBNationality = Nationality.Russian;
        _sideAInsignia = null;
        _sideBInsignia = null;
        SideAFormations.Clear();
        SideBFormations.Clear();

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Reference));
        OnPropertyChanged(nameof(Place));
        OnPropertyChanged(nameof(Date));
        OnPropertyChanged(nameof(DescriptionText));
        OnPropertyChanged(nameof(Aftermath));
        OnPropertyChanged(nameof(Turns));
        OnPropertyChanged(nameof(HasHalfTurn));
        OnPropertyChanged(nameof(SideAName));
        OnPropertyChanged(nameof(SideASide));
        OnPropertyChanged(nameof(SideANationality));
        OnPropertyChanged(nameof(SideAInsignia));
        OnPropertyChanged(nameof(SideBName));
        OnPropertyChanged(nameof(SideBSide));
        OnPropertyChanged(nameof(SideBNationality));
        OnPropertyChanged(nameof(SideBInsignia));
        OnPropertyChanged(nameof(AreSidesDefined));

        ImagePath = null;
        ClearErrors();
    }

    /// <inheritdoc />
    protected override void PopulateForm(Scenario item)
    {
        _name = item.Name;
        _reference = item.Reference;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Reference));
        ImagePath = item.ImagePath;
        Place = item.Description.Place;
        Date = item.Description.Date;
        DescriptionText = item.Description.DescriptionText;
        Aftermath = item.Description.Aftermath;
        Turns = item.Turns;
        HasHalfTurn = item.HasHalfTurn;

        if (item.ScenarioSides.Count >= 2)
        {
            _sideAName = item.ScenarioSides[0].Name;
            _sideASide = item.ScenarioSides[0].Side;
            _sideANationality = item.ScenarioSides[0].Nationality;
            _sideAInsignia = item.ScenarioSides[0].Insignia;
            _sideBName = item.ScenarioSides[1].Name;
            _sideBSide = item.ScenarioSides[1].Side;
            _sideBNationality = item.ScenarioSides[1].Nationality;
            _sideBInsignia = item.ScenarioSides[1].Insignia;

            SideAFormations.Clear();
            foreach (var f in item.ScenarioSides[0].Formations) SideAFormations.Add(f);
            SideBFormations.Clear();
            foreach (var f in item.ScenarioSides[1].Formations) SideBFormations.Add(f);
        }
        else
        {
            _sideAName = string.Empty;
            _sideBName = string.Empty;
            _sideASide = Side.Attacker;
            _sideBSide = Side.Defender;
            _sideANationality = Nationality.German;
            _sideBNationality = Nationality.Russian;
            _sideAInsignia = null;
            _sideBInsignia = null;
            SideAFormations.Clear();
            SideBFormations.Clear();
        }

        OnPropertyChanged(nameof(SideAName));
        OnPropertyChanged(nameof(SideASide));
        OnPropertyChanged(nameof(SideANationality));
        OnPropertyChanged(nameof(SideAInsignia));
        OnPropertyChanged(nameof(SideBName));
        OnPropertyChanged(nameof(SideBSide));
        OnPropertyChanged(nameof(SideBNationality));
        OnPropertyChanged(nameof(SideBInsignia));
        OnPropertyChanged(nameof(AreSidesDefined));

        ClearErrors();
    }

    /// <inheritdoc />
    protected override async void OnSave(object? parameter)
    {
        ValidateName();
        ValidateReference();
        ValidateSides();
        ValidateTurns();

        if (HasErrors)
        {
            ShowToast("Please fix the validation errors.");
            return;
        }

        string? originalName = EditingItem?.Name;
        var scenario = new Scenario
        {
            Name = Name,
            Reference = Reference,
            ImagePath = ImagePath,
            Turns = Turns,
            HasHalfTurn = HasHalfTurn,
            Description = new ScenarioDescription(Place, Date, DescriptionText, Aftermath),
            ScenarioSides = new List<ScenarioSide>
            {
                new ScenarioSide 
                { 
                    Name = SideAName, Side = SideASide, Nationality = SideANationality, 
                    Insignia = SideAInsignia, Formations = SideAFormations.ToList() 
                },
                new ScenarioSide 
                { 
                    Name = SideBName, Side = SideBSide, Nationality = SideBNationality, 
                    Insignia = SideBInsignia, Formations = SideBFormations.ToList() 
                }
            }
        };

        if (EditingItem != null)
        {
            var wrapper = Items.FirstOrDefault(i => i.Item == EditingItem);
            if (wrapper != null)
            {
                int index = Items.IndexOf(wrapper);
                if (index >= 0)
                {
                    Items[index] = new SelectableItem<Scenario>(scenario, NotifySelectionChanged);
                }
            }
        }
        else
        {
            Items.Add(new SelectableItem<Scenario>(scenario, NotifySelectionChanged));
        }

        try
        {
            await _repository.SaveToDiskAsync(scenario, originalName);
            _repository.Initialize(Items.Select(i => i.Item).ToList());
            ShowToast($"Scenario '{scenario.Name}' saved to disk.");
        }
        catch (Exception ex)
        {
            ShowToast("Error saving scenario: " + ex.Message);
        }

        IsAdding = false;
    }

    /// <inheritdoc />
    protected override void OnItemAdded(Scenario item) => _repository.Add(item);

    /// <inheritdoc />
    protected override async void OnItemRemoved(Scenario item)
    {
        _repository.Remove(item);
        await _repository.DeleteFromDiskAsync(item);
    }
}
