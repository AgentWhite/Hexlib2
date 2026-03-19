using ASL;
using ASL.Counters;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Squad and Half-Squad MMC counters.
/// </summary>
public class SquadsViewModel : CrudViewModelBase<MultiManCounter>
{
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private UnitClass _selectedClass = UnitClass.FirstLine;
    private string? _imagePathFront;
    private string? _imagePathBack;
    private bool _isHalfSquad;
    private bool _hasAssaultFire;
    private bool _hasSprayingFire;
    private bool _canSelfRally;
    private bool _hasSmokeExponent;
    private string _smokePlacementExponent = string.Empty;

    /// <summary>
    /// Gets or sets the name of the unit.
    /// </summary>
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    /// <summary>
    /// Gets or sets the firepower rating of the unit.
    /// </summary>
    public string Firepower { get => _firepower; set => SetProperty(ref _firepower, value); }
    /// <summary>
    /// Gets or sets the range rating of the unit.
    /// </summary>
    public string Range { get => _range; set => SetProperty(ref _range, value); }
    /// <summary>
    /// Gets or sets the morale rating of the unit.
    /// </summary>
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    /// <summary>
    /// Gets or sets the selected nationality of the unit.
    /// </summary>
    public Nationality SelectedNationality { get => _selectedNationality; set => SetProperty(ref _selectedNationality, value); }
    /// <summary>
    /// Gets or sets the unit class (e.g., Elite, First Line).
    /// </summary>
    public UnitClass SelectedClass { get => _selectedClass; set => SetProperty(ref _selectedClass, value); }
    /// <summary>
    /// Gets or sets the path to the front image of the counter.
    /// </summary>
    public string? ImagePathFront { get => _imagePathFront; set => SetProperty(ref _imagePathFront, value); }
    /// <summary>
    /// Gets or sets the path to the back image of the counter.
    /// </summary>
    public string? ImagePathBack { get => _imagePathBack; set => SetProperty(ref _imagePathBack, value); }
    /// <summary>
    /// Gets or sets a value indicating whether the unit is a half-squad.
    /// </summary>
    public bool IsHalfSquad { get => _isHalfSquad; set => SetProperty(ref _isHalfSquad, value); }
    /// <summary>
    /// Gets or sets a value indicating whether the unit has Assault Fire capability.
    /// </summary>
    public bool HasAssaultFire { get => _hasAssaultFire; set => SetProperty(ref _hasAssaultFire, value); }
    /// <summary>
    /// Gets or sets a value indicating whether the unit has Spraying Fire capability.
    /// </summary>
    public bool HasSprayingFire { get => _hasSprayingFire; set => SetProperty(ref _hasSprayingFire, value); }
    /// <summary>
    /// Gets or sets a value indicating whether the unit can Self-Rally.
    /// </summary>
    public bool CanSelfRally { get => _canSelfRally; set => SetProperty(ref _canSelfRally, value); }
    /// <summary>
    /// Gets or sets a value indicating whether the unit has a smoke exponent.
    /// </summary>
    public bool HasSmokeExponent 
    { 
        get => _hasSmokeExponent; 
        set 
        { 
            if (SetProperty(ref _hasSmokeExponent, value))
            {
                if (!value) SmokePlacementExponent = string.Empty;
            }
        } 
    }

    /// <summary>
    /// Gets or sets the smoke placement exponent for the unit.
    /// </summary>
    public string SmokePlacementExponent { get => _smokePlacementExponent; set => SetProperty(ref _smokePlacementExponent, value); }

    /// <summary>
    /// Gets the list of available nationalities.
    /// </summary>
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();
    /// <summary>
    /// Gets the list of available unit classes.
    /// </summary>
    public IEnumerable<UnitClass> UnitClasses => Enum.GetValues(typeof(UnitClass)).Cast<UnitClass>();

    /// <summary>
    /// Command to pick the front image for the unit.
    /// </summary>
    public RelayCommand PickFrontImageCommand { get; }
    /// <summary>
    /// Command to pick the back image for the unit.
    /// </summary>
    public RelayCommand PickBackImageCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SquadsViewModel"/> class.
    /// </summary>
    public SquadsViewModel()
    {
        DisplayName = "Squads";
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(true));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(false));
    }

    private void ExecutePickImage(bool front)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = front ? "Select Squad Front Image" : "Select Squad Back Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            if (front) ImagePathFront = openDialog.FileName;
            else ImagePathBack = openDialog.FileName;
        }
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        Name = string.Empty;
        Firepower = string.Empty;
        Range = string.Empty;
        Morale = string.Empty;
        SelectedNationality = Nationality.German;
        SelectedClass = UnitClass.FirstLine;
        ImagePathFront = null;
        ImagePathBack = null;
        IsHalfSquad = false;
        HasAssaultFire = false;
        HasSprayingFire = false;
        CanSelfRally = false;
        HasSmokeExponent = false;
        SmokePlacementExponent = string.Empty;
    }

    /// <inheritdoc />
    protected override void PopulateForm(MultiManCounter item)
    {
        Name = item.Name;
        Firepower = item.Firepower.ToString();
        Range = item.Range.ToString();
        Morale = item.Morale.ToString();
        SelectedNationality = item.Nationality;
        SelectedClass = item.AslClass;
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
        IsHalfSquad = item is HalfSquad;
        HasAssaultFire = item.HasAssaultFire;
        HasSprayingFire = item.HasSprayingFire;
        CanSelfRally = item.CanSelfRally;
        HasSmokeExponent = item.HasSmokeExponent;
        SmokePlacementExponent = item.SmokePlacementExponent > 0 ? item.SmokePlacementExponent.ToString() : string.Empty;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        int fp = int.TryParse(Firepower, out int f) ? f : 4;
        int range = int.TryParse(Range, out int r) ? r : 6;
        int morale = int.TryParse(Morale, out int m) ? m : 7;

        MultiManCounter counter;
        if (IsHalfSquad)
        {
            counter = new HalfSquad(Name, fp, range, morale, SelectedClass, SelectedNationality);
        }
        else
        {
            counter = new Squad(Name, fp, range, morale, SelectedClass, SelectedNationality);
        }
        
        counter.ImagePathFront = ImagePathFront;
        counter.ImagePathBack = ImagePathBack;
        counter.HasAssaultFire = HasAssaultFire;
        counter.HasSprayingFire = HasSprayingFire;
        counter.CanSelfRally = CanSelfRally;
        counter.HasSmokeExponent = HasSmokeExponent;
        counter.SmokePlacementExponent = int.TryParse(SmokePlacementExponent, out int se) ? se : 0;

        if (EditingItem != null)
        {
            var wrapper = Items.FirstOrDefault(i => i.Item == EditingItem);
            if (wrapper != null)
            {
                int index = Items.IndexOf(wrapper);
                if (index >= 0) Items[index] = new SelectableItem<MultiManCounter>(counter, NotifySelectionChanged);
            }
        }
        else
        {
            Items.Add(new SelectableItem<MultiManCounter>(counter, NotifySelectionChanged));
        }
        
        IsAdding = false;
    }
}
