using ASL.Models;
using ASL.Models.Components;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Squad and Half-Squad MMC counters.
/// </summary>
public class SquadsViewModel : CrudViewModelBase<Unit>
{
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private string _brokenMorale = string.Empty;
    private string _bpv = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private UnitClass _selectedClass = UnitClass.FirstLine;
    private string? _imagePathFront;
    private string? _imagePathBack;
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
    public string Name { get => _name; set { SetProperty(ref _name, value); ValidateName(); } }

    /// <summary>
    /// Gets or sets the firepower value as a string for UI binding.
    /// </summary>
    public string Firepower { get => _firepower; set { SetProperty(ref _firepower, value); ValidateFirepower(); } }

    /// <summary>
    /// Gets or sets the range value as a string for UI binding.
    /// </summary>
    public string Range { get => _range; set { SetProperty(ref _range, value); ValidateRange(); } }

    /// <summary>
    /// Gets or sets the morale value as a string for UI binding.
    /// </summary>
    public string Morale { get => _morale; set { SetProperty(ref _morale, value); ValidateMorale(); } }

    /// <summary>
    /// Gets or sets the broken morale value as a string for UI binding.
    /// </summary>
    public string BrokenMorale { get => _brokenMorale; set { SetProperty(ref _brokenMorale, value); ValidateBrokenMorale(); } }

    /// <summary>
    /// Gets or sets the BPV value as a string for UI binding.
    /// </summary>
    public string BPV { get => _bpv; set { SetProperty(ref _bpv, value); ValidateBPV(); } }

    /// <summary>
    /// Gets or sets the selected nationality for the squad.
    /// </summary>
    public Nationality SelectedNationality 
    { 
        get => _selectedNationality; 
        set 
        { 
            if (SetProperty(ref _selectedNationality, value))
            {
                ValidateUniqueness();
            }
        } 
    }

    /// <summary>
    /// Gets or sets the selected unit class (Elite, 1st Line, etc.).
    /// </summary>
    public UnitClass SelectedClass { get => _selectedClass; set => SetProperty(ref _selectedClass, value); }

    /// <summary>
    /// Gets or sets the file path for the front image.
    /// </summary>
    public string? ImagePathFront { get => _imagePathFront; set => SetProperty(ref _imagePathFront, value); }

    /// <summary>
    /// Gets or sets the file path for the back image.
    /// </summary>
    public string? ImagePathBack { get => _imagePathBack; set => SetProperty(ref _imagePathBack, value); }

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

                OnPropertyChanged(nameof(CanHaveAssaultFire));
                OnPropertyChanged(nameof(CanHaveSprayingFire));
                OnPropertyChanged(nameof(CanHaveSelfRally));
                OnPropertyChanged(nameof(CanHaveELR));
                OnPropertyChanged(nameof(CanHaveSmoke));
            }
        } 
    }

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
                    ValidateSmoke();
                }
            }
        } 
    }

    /// <summary>
    /// Gets or sets the smoke placement exponent as a string.
    /// </summary>
    public string SmokePlacementExponent 
    { 
        get => _smokePlacementExponent; 
        set 
        { 
            if (SetProperty(ref _smokePlacementExponent, value))
            {
                ValidateSmoke();
            }
        } 
    }

    /// <summary>
    /// Gets the list of available nationalities.
    /// </summary>
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    /// <summary>
    /// Gets the list of available unit classes.
    /// </summary>
    public IEnumerable<UnitClass> UnitClasses => Enum.GetValues(typeof(UnitClass)).Cast<UnitClass>();

    /// <summary>
    /// Gets the list of available infantry scales (excluding SMC).
    /// </summary>
    public IEnumerable<InfantryScale> Scales => Enum.GetValues(typeof(InfantryScale))
        .Cast<InfantryScale>()
        .Where(s => s != InfantryScale.SMC);

    /// <summary>
    /// Command to pick the front image.
    /// </summary>
    public RelayCommand PickFrontImageCommand { get; }

    /// <summary>
    /// Command to pick the back image.
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

    private void ValidateName()
    {
        ClearErrors(nameof(Name));
        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "Unit identity is required.");
            ShowToast("Unit identity is required.");
        }
        else
        {
            ValidateUniqueness();
        }
    }

    private void ValidateFirepower()
    {
        ClearErrors(nameof(Firepower));
        if (string.IsNullOrWhiteSpace(Firepower))
        {
            AddError(nameof(Firepower), "Firepower is required.");
            ShowToast("Firepower is required.");
        }
        else if (!int.TryParse(Firepower, out int f) || f <= 0)
        {
            AddError(nameof(Firepower), "Firepower must be a positive number.");
            ShowToast("Firepower must be a positive number.");
        }
        else
        {
            ValidateUniqueness();
        }
    }

    private void ValidateRange()
    {
        ClearErrors(nameof(Range));
        if (string.IsNullOrWhiteSpace(Range))
        {
            AddError(nameof(Range), "Range is required.");
            ShowToast("Range is required.");
        }
        else if (!int.TryParse(Range, out int r) || r <= 0)
        {
            AddError(nameof(Range), "Range must be a positive number.");
            ShowToast("Range must be a positive number.");
        }
        else
        {
            ValidateUniqueness();
        }
    }

    private void ValidateMorale()
    {
        ClearErrors(nameof(Morale));
        if (string.IsNullOrWhiteSpace(Morale))
        {
            AddError(nameof(Morale), "Morale is required.");
            ShowToast("Morale is required.");
        }
        else if (!int.TryParse(Morale, out int m) || m <= 0)
        {
            AddError(nameof(Morale), "Morale must be a positive number.");
            ShowToast("Morale must be a positive number.");
        }
        else
        {
            ValidateUniqueness();
        }
    }

    private void ValidateBrokenMorale()
    {
        ClearErrors(nameof(BrokenMorale));
        if (string.IsNullOrWhiteSpace(BrokenMorale))
        {
            AddError(nameof(BrokenMorale), "Broken morale is required.");
            ShowToast("Broken morale is required.");
        }
        else if (!int.TryParse(BrokenMorale, out int m) || m <= 0)
        {
            AddError(nameof(BrokenMorale), "Broken morale must be a positive number.");
            ShowToast("Broken morale must be a positive number.");
        }
    }

    private void ValidateBPV()
    {
        ClearErrors(nameof(BPV));
        if (string.IsNullOrWhiteSpace(BPV))
        {
            AddError(nameof(BPV), "BPV is required.");
            ShowToast("BPV is required.");
        }
        else if (!int.TryParse(BPV, out int b) || b <= 0)
        {
            AddError(nameof(BPV), "BPV must be a positive number.");
            ShowToast("BPV must be a positive number.");
        }
    }

    private void ValidateSmoke()
    {
        ClearErrors(nameof(SmokePlacementExponent));
        if (HasSmokeExponent)
        {
            if (string.IsNullOrWhiteSpace(SmokePlacementExponent))
            {
                AddError(nameof(SmokePlacementExponent), "Smoke exponent is required when enabled.");
            }
            else if (!int.TryParse(SmokePlacementExponent, out int se) || se < 0)
            {
                AddError(nameof(SmokePlacementExponent), "Smoke exponent must be a non-negative number.");
            }
        }
    }

    private void ValidateUniqueness()
    {
        if (string.IsNullOrWhiteSpace(Name) || 
            !int.TryParse(Firepower, out int fp) || 
            !int.TryParse(Range, out int r) || 
            !int.TryParse(Morale, out int m))
        {
            return;
        }

        if (Items.Any(i => i.Item != EditingItem && 
                          i.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && 
                          i.Item.Nationality == SelectedNationality &&
                          (i.Item.FirePower?.Firepower ?? 0) == fp &&
                          (i.Item.FirePower?.Range ?? 0) == r &&
                          (i.Item.Infantry?.Morale ?? 0) == m))
        {
            AddError(nameof(Name), "An identical unit already exists for this nationality.");
            ShowToast("Duplicate unit detected!");
        }
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
        SelectedClass = UnitClass.FirstLine;
        SelectedScale = InfantryScale.Squad;
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
        ValidateName();
        ValidateFirepower();
        ValidateRange();
        ValidateMorale();
        ValidateBrokenMorale();
        ValidateBPV();
        ValidateSmoke();

        if (HasErrors)
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
                if (index >= 0) Items[index] = new SelectableItem<Unit>(unit, NotifySelectionChanged);
            }
        }
        else
        {
            Items.Add(new SelectableItem<Unit>(unit, NotifySelectionChanged));
        }
        
        IsAdding = false;
    }
}
