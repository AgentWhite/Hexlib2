using ASL.Models;
using ASL.Models.Components;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Support Weapon (LMG, MMG, HMG, etc.) counters.
/// </summary>
public class SupportWeaponViewModel : CrudViewModelBase<Unit>
{
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _rateOfFire = string.Empty;
    private string _breakdownNumber = string.Empty;
    private string _portageCost = string.Empty;
    private bool _hasSmokeExponent;
    private string _smokePlacementExponent = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private string? _imagePathFront;
    private string? _imagePathBack;

    /// <summary>
    /// Gets or sets the name/type of the support weapon.
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
    /// Gets or sets the rate of fire (ROF) value as a string for UI binding.
    /// </summary>
    public string RateOfFire { get => _rateOfFire; set { SetProperty(ref _rateOfFire, value); ValidateROF(); } }

    /// <summary>
    /// Gets or sets the breakdown number as a string for UI binding.
    /// </summary>
    public string BreakdownNumber { get => _breakdownNumber; set { SetProperty(ref _breakdownNumber, value); ValidateBreakdown(); } }

    /// <summary>
    /// Gets or sets the portage cost as a string for UI binding.
    /// </summary>
    public string PortageCost { get => _portageCost; set { SetProperty(ref _portageCost, value); ValidatePortage(); } }

    /// <summary>
    /// Gets or sets a value indicating whether the weapon has a smoke placement exponent.
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
    /// Gets or sets the selected nationality for the weapon.
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
    /// Gets or sets the file path for the front image.
    /// </summary>
    public string? ImagePathFront { get => _imagePathFront; set => SetProperty(ref _imagePathFront, value); }

    /// <summary>
    /// Gets or sets the file path for the back image.
    /// </summary>
    public string? ImagePathBack { get => _imagePathBack; set => SetProperty(ref _imagePathBack, value); }

    /// <summary>
    /// Gets the list of available nationalities.
    /// </summary>
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    /// <summary>
    /// Command to pick the front image.
    /// </summary>
    public RelayCommand PickFrontImageCommand { get; }

    /// <summary>
    /// Command to pick the back image.
    /// </summary>
    public RelayCommand PickBackImageCommand { get; }

    /// <summary>
    /// Initializes a new instance of the class.
    /// </summary>
    public SupportWeaponViewModel()
    {
        DisplayName = "Support Weapons";
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(true));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(false));
    }

    private void ExecutePickImage(bool front)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = front ? "Select Weapon Front Image" : "Select Weapon Back Image"
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
        Firepower = "0";
        Range = "0";
        RateOfFire = "0";
        BreakdownNumber = "0";
        PortageCost = "0";
        HasSmokeExponent = false;
        SmokePlacementExponent = string.Empty;
        SelectedNationality = Nationality.German;
        ImagePathFront = null;
        ImagePathBack = null;
        ClearErrors();
    }

    /// <inheritdoc />
    protected override void PopulateForm(Unit item)
    {
        Name = item.Name;
        SelectedNationality = item.Nationality;
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
        
        var sw = item.SupportWeapon;
        if (sw != null)
        {
            Firepower = sw.FirePower.ToString();
            Range = sw.Range.ToString();
            RateOfFire = sw.RateOfFire.ToString();
            BreakdownNumber = sw.BreakdownNumber.ToString();
            PortageCost = sw.PortageCost.ToString();
        }

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
        ValidateROF();
        ValidateBreakdown();
        ValidatePortage();
        ValidateSmoke();

        if (HasErrors)
        {
            ShowToast("Please fix the validation errors.");
            return;
        }

        var unit = new Unit
        {
            Name = Name,
            Nationality = SelectedNationality,
            UnitType = UnitType.SMC, // Using SMC for Support Weapons for now
            ImagePathFront = ImagePathFront,
            ImagePathBack = ImagePathBack
        };

        unit.AddComponent(new SupportWeaponComponent
        {
            FirePower = int.Parse(Firepower),
            Range = int.Parse(Range),
            RateOfFire = int.Parse(RateOfFire),
            BreakdownNumber = int.Parse(BreakdownNumber),
            PortageCost = int.Parse(PortageCost)
        });

        if (HasSmokeExponent && int.TryParse(SmokePlacementExponent, out int se))
        {
            unit.AddComponent(new SmokeProviderComponent { CapabilityNumber = se, SmokeType = SmokeType.White });
        }

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

    private void ValidateName()
    {
        ClearErrors(nameof(Name));
        if (string.IsNullOrWhiteSpace(Name)) AddError(nameof(Name), "Name is required.");
        else ValidateUniqueness();
    }

    private void ValidateUniqueness()
    {
        if (Items.Any(i => i.Item != EditingItem && i.Item.Name == Name && i.Item.Nationality == SelectedNationality))
        {
            AddError(nameof(Name), "Weapon with this name and nationality already exists.");
        }
    }

    private void ValidateFirepower()
    {
        ClearErrors(nameof(Firepower));
        if (!int.TryParse(Firepower, out int fp) || fp < 0) AddError(nameof(Firepower), "Must be a non-negative number.");
    }

    private void ValidateRange()
    {
        ClearErrors(nameof(Range));
        if (!int.TryParse(Range, out int r) || r < 0) AddError(nameof(Range), "Must be a non-negative number.");
    }

    private void ValidateROF()
    {
        ClearErrors(nameof(RateOfFire));
        if (!int.TryParse(RateOfFire, out int rof) || rof < 0) AddError(nameof(RateOfFire), "Must be a non-negative number.");
    }

    private void ValidateBreakdown()
    {
        ClearErrors(nameof(BreakdownNumber));
        if (!int.TryParse(BreakdownNumber, out int bn) || bn < 0) AddError(nameof(BreakdownNumber), "Must be a non-negative number.");
    }

    private void ValidatePortage()
    {
        ClearErrors(nameof(PortageCost));
        if (!int.TryParse(PortageCost, out int pc) || pc < 0) AddError(nameof(PortageCost), "Must be a non-negative number.");
    }

    private void ValidateSmoke()
    {
        ClearErrors(nameof(SmokePlacementExponent));
        if (HasSmokeExponent)
        {
            if (string.IsNullOrWhiteSpace(SmokePlacementExponent)) AddError(nameof(SmokePlacementExponent), "Smoke exponent is required when enabled.");
            else if (!int.TryParse(SmokePlacementExponent, out int se) || se < 0) AddError(nameof(SmokePlacementExponent), "Must be a non-negative number.");
        }
    }
}
