using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Squad and Half-Squad MMC counters.
/// </summary>
public class SquadsViewModel : UnitViewModelBase
{
    /// <inheritdoc />
    protected override string UnitCategoryFilter => "Infantry";
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private string _brokenMorale = string.Empty;
    private string _bpv = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private UnitClass _selectedClass = UnitClass.FirstLine;
    private InfantryScale _selectedScale = InfantryScale.Squad;
    private bool _hasAssaultFire;
    private bool _hasSprayingFire;
    private bool _canSelfRally;
    private bool _hasELR;
    private bool _hasSmokeExponent;
    private string _smokePlacementExponent = string.Empty;


    /// <summary>
    /// Gets or sets the name/identity of the squad.
    /// </summary>
    [Required(ErrorMessage = "Unit identity is required.")]
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    /// <summary>
    /// Gets or sets the firepower value as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Firepower is required.")]
    [Range(typeof(int), "1", "30", ErrorMessage = "Firepower must be between 1 and 30.")]
    public string Firepower { get => _firepower; set => SetProperty(ref _firepower, value); }

    /// <summary>
    /// Gets or sets the range value as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Range is required.")]
    [Range(typeof(int), "0", "50", ErrorMessage = "Range must be between 0 and 50.")]
    public string Range { get => _range; set => SetProperty(ref _range, value); }

    /// <summary>
    /// Gets or sets the morale value as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Morale is required.")]
    [Range(typeof(int), "0", "10", ErrorMessage = "Morale must be between 0 and 10.")]
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }

    /// <summary>
    /// Gets or sets the broken morale value as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Broken morale is required.")]
    [Range(typeof(int), "1", "12", ErrorMessage = "Broken morale must be between 1 and 12.")]
    public string BrokenMorale { get => _brokenMorale; set => SetProperty(ref _brokenMorale, value); }

    /// <summary>
    /// Gets or sets the BPV value as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "BPV is required.")]
    [Range(typeof(int), "1", "100", ErrorMessage = "BPV must be between 1 and 100.")]
    public string BPV { get => _bpv; set => SetProperty(ref _bpv, value); }

    /// <summary>
    /// Gets or sets the selected nationality for the squad.
    /// </summary>
    public Nationality SelectedNationality 
    { 
        get => _selectedNationality; 
        set => SetProperty(ref _selectedNationality, value);
    }

    /// <summary>
    /// Gets or sets the selected unit class (Elite, 1st Line, etc.).
    /// </summary>
    public UnitClass SelectedClass { get => _selectedClass; set => SetProperty(ref _selectedClass, value); }


    /// <summary>
    /// Gets or sets the scale of the infantry unit.
    /// </summary>
    public InfantryScale SelectedScale 
    { 
        get => _selectedScale; 
        set 
        { 
            if (SetProperty(ref _selectedScale, value))
            {
                // Reset traits not allowed for the new scale
                if (!CanHaveAssaultFire) HasAssaultFire = false;
                if (!CanHaveSprayingFire) HasSprayingFire = false;
                if (!CanHaveSelfRally) CanSelfRally = false;
                if (!CanHaveSmoke) HasSmokeExponent = false;
                if (!CanHaveELR) HasELR = false;

                if (value == InfantryScale.Crew)
                {
                    SelectedClass = UnitClass.Elite;
                }

                OnPropertyChanged(nameof(CanHaveAssaultFire));
                OnPropertyChanged(nameof(CanHaveSprayingFire));
                OnPropertyChanged(nameof(CanHaveSelfRally));
                OnPropertyChanged(nameof(CanHaveELR));
                OnPropertyChanged(nameof(CanHaveSmoke));
                OnPropertyChanged(nameof(IsClassSelectionEnabled));
            }
        } 
    }

    /// <summary>
    /// Gets a value indicating whether Class selection is enabled.
    /// </summary>
    public bool IsClassSelectionEnabled => SelectedScale != InfantryScale.Crew;

    /// <summary>
    /// Gets a value indicating whether Assault Fire can be applied.
    /// </summary>
    public bool CanHaveAssaultFire => SelectedScale == InfantryScale.Squad;

    /// <summary>
    /// Gets a value indicating whether Spraying Fire can be applied.
    /// </summary>
    public bool CanHaveSprayingFire => SelectedScale == InfantryScale.Squad;

    /// <summary>
    /// Gets a value indicating whether Self Rally can be applied.
    /// </summary>
    public bool CanHaveSelfRally => SelectedScale == InfantryScale.Squad || SelectedScale == InfantryScale.Crew;

    /// <summary>
    /// Gets a value indicating whether ELR can be applied.
    /// </summary>
    public bool CanHaveELR => SelectedScale == InfantryScale.Squad || SelectedScale == InfantryScale.HalfSquad;

    /// <summary>
    /// Gets a value indicating whether Smoke capability can be applied.
    /// </summary>
    public bool CanHaveSmoke => SelectedScale == InfantryScale.Squad;

    /// <summary>
    /// Gets or sets a value indicating whether the unit has Assault Fire.
    /// </summary>
    public bool HasAssaultFire { get => _hasAssaultFire; set => SetProperty(ref _hasAssaultFire, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the unit has Spraying Fire.
    /// </summary>
    public bool HasSprayingFire { get => _hasSprayingFire; set => SetProperty(ref _hasSprayingFire, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the unit can Self Rally.
    /// </summary>
    public bool CanSelfRally { get => _canSelfRally; set => SetProperty(ref _canSelfRally, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the unit has ELR.
    /// </summary>
    public bool HasELR { get => _hasELR; set => SetProperty(ref _hasELR, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the unit has a smoke placement exponent.
    /// </summary>
    public bool HasSmokeExponent 
    { 
        get => _hasSmokeExponent; 
        set 
        { 
            if (SetProperty(ref _hasSmokeExponent, value))
            {
                if (!value) 
                {
                    SmokePlacementExponent = string.Empty;
                    ClearErrors(nameof(SmokePlacementExponent));
                }
            }
        } 
    }

    /// <summary>
    /// Gets or sets the smoke placement exponent as a string.
    /// </summary>
    [Range(typeof(int), "1", "9", ErrorMessage = "Smoke exponent must be between 1 and 9.")]
    public string SmokePlacementExponent 
    { 
        get => _smokePlacementExponent; 
        set => SetProperty(ref _smokePlacementExponent, value); 
    }

    /// <summary>
    /// Gets the list of available infantry scales.
    /// </summary>
    public IEnumerable<InfantryScale> Scales => Enum.GetValues(typeof(InfantryScale)).Cast<InfantryScale>();

    /// <summary>
    /// Gets the list of available unit classes.
    /// </summary>
    public IEnumerable<UnitClass> UnitClasses => Enum.GetValues(typeof(UnitClass)).Cast<UnitClass>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SquadsViewModel"/> class.
    /// </summary>
    public SquadsViewModel(IUnitRepository repository, IModuleRepository moduleRepository) : base(repository, moduleRepository)
    {
        DisplayName = "Squads";
    }

    /// <inheritdoc />
    protected override void ValidateProperty(object? value, string? propertyName)
    {
        base.ValidateProperty(value, propertyName);

        if (propertyName == nameof(SmokePlacementExponent))
        {
            if (HasSmokeExponent && string.IsNullOrWhiteSpace(value as string))
            {
                AddError(nameof(SmokePlacementExponent), "Smoke exponent is required.");
            }
        }
    }

    /// <inheritdoc />
    protected override bool ValidateAllProperties()
    {
        bool isValid = base.ValidateAllProperties();

        // Extra enforcement for BPV to bypass strange TryValidateObject skipping bug
        if (string.IsNullOrWhiteSpace(BPV))
        {
            AddError(nameof(BPV), "BPV is required.");
            isValid = false;
        }

        if (!string.IsNullOrWhiteSpace(Name) && 
            int.TryParse(Firepower, out int fp) && 
            int.TryParse(Range, out int r) && 
            int.TryParse(Morale, out int m))
        {
            if (Items.Any(i => i.Item != EditingItem && 
                              i.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && 
                              i.Item.Nationality == SelectedNationality &&
                              (i.Item.FirePower?.Firepower ?? 0) == fp &&
                              (i.Item.FirePower?.Range ?? 0) == r &&
                              (i.Item.Infantry?.Morale ?? 0) == m))
            {
                AddError(nameof(Name), "An identical unit already exists for this nationality.");
                isValid = false;
            }
        }

        if (HasSmokeExponent && string.IsNullOrWhiteSpace(SmokePlacementExponent))
        {
            AddError(nameof(SmokePlacementExponent), "Smoke exponent is required.");
            isValid = false;
        }

        return isValid;
    }

    /// <inheritdoc />
    protected override void OnImagePicked(int imageType, string filePath)
    {
        if (imageType == 0) ImagePathFront = filePath;
        else if (imageType == 1) ImagePathBack = filePath;
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        ClearErrors();
        _name = string.Empty;
        _firepower = string.Empty;
        _range = string.Empty;
        _morale = string.Empty;
        _brokenMorale = string.Empty;
        _bpv = string.Empty;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Firepower));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(BPV));
        SelectedNationality = Nationality.German;
        SelectedModule = ASL.Models.Modules.Module.BeyondValor;
        SelectedScale = InfantryScale.Squad;
        SelectedClass = UnitClass.FirstLine;
        ImagePathFront = null;
        ImagePathBack = null;
        HasAssaultFire = false;
        HasSprayingFire = false;
        CanSelfRally = false;
        HasELR = false;
        HasSmokeExponent = false;
        SmokePlacementExponent = string.Empty;
    }

    /// <inheritdoc />
    protected override void PopulateForm(Unit item)
    {
        ClearErrors();
        _name = item.Name;
        _firepower = (item.FirePower?.Firepower ?? 0).ToString();
        _range = (item.FirePower?.Range ?? 0).ToString();
        _morale = (item.Infantry?.Morale ?? 0).ToString();
        _brokenMorale = (item.Infantry?.BrokenMorale ?? 0).ToString();
        _bpv = (item.Bpv?.BPV ?? 0).ToString();
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Firepower));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(BPV));
        SelectedNationality = item.Nationality;
        SelectedModule = item.Module;
        SelectedClass = item.Infantry?.AslClass ?? UnitClass.SecondLine;
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
        var infantry = item.Infantry;
        SelectedScale = infantry?.Scale ?? InfantryScale.Squad;
        HasAssaultFire = infantry?.HasAssaultFire ?? false;
        HasSprayingFire = infantry?.HasSprayingFire ?? false;
        CanSelfRally = infantry?.CanSelfRally ?? false;
        HasELR = infantry?.HasELR ?? false;
        var smoke = item.GetComponent<SmokeProviderComponent>();
        HasSmokeExponent = smoke != null;
        SmokePlacementExponent = smoke?.CapabilityNumber.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        if (!ValidateAllProperties())
        {
            ShowToast("Please fix the validation errors.");
            return;
        }

        int fpValue = int.Parse(Firepower);
        int rValue = int.Parse(Range);
        int mValue = int.Parse(Morale);
        int bmValue = int.Parse(BrokenMorale);
        int bpvValue = int.Parse(BPV);

        var unit = new Unit
        {
            Name = Name,
            Nationality = SelectedNationality,
            Module = SelectedModule,
            UnitType = UnitType.MMC,
            ImagePathFront = ImagePathFront,
            ImagePathBack = ImagePathBack
        };

        unit.AddComponent(new InfantryComponent
        {
            Morale = mValue,
            BrokenMorale = bmValue,
            AslClass = SelectedClass,
            Scale = SelectedScale,
            HasAssaultFire = HasAssaultFire,
            HasSprayingFire = HasSprayingFire,
            CanSelfRally = CanSelfRally,
            HasELR = HasELR
        });

        if (HasSmokeExponent && int.TryParse(SmokePlacementExponent, out int se))
        {
            unit.AddComponent(new SmokeProviderComponent { CapabilityNumber = se, SmokeType = SmokeType.White });
        }

        unit.AddComponent(new FirePowerComponent { Firepower = fpValue, Range = rValue });
        unit.AddComponent(new BPVComponent { BPV = bpvValue });

        if (EditingItem != null)
        {
            var wrapper = Items.FirstOrDefault(i => i.Item == EditingItem);
            if (wrapper != null)
            {
                int index = Items.IndexOf(wrapper);
                if (index >= 0)
                {
                    OnItemRemoved(EditingItem);
                    Items[index] = new SelectableItem<Unit>(unit, NotifySelectionChanged);
                    OnItemAdded(unit);
                }
            }
        }
        else
        {
            Items.Add(new SelectableItem<Unit>(unit, NotifySelectionChanged));
            OnItemAdded(unit);
        }
        
        IsAdding = false;
    }
}
