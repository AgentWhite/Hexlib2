using ASL.Models;
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
/// ViewModel for managing Equipment (LMG, MMG, HMG, etc.) counters.
/// </summary>
public class EquipmentViewModel : UnitViewModelBase
{
    protected override string UnitCategoryFilter => "Equipment";
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _rateOfFire = string.Empty;
    private string _breakdownNumber = string.Empty;
    private string _removalNumber = string.Empty;
    private string _repairNumber = string.Empty;
    private string _portageCost = string.Empty;
    private string _dismantledCost = string.Empty;
    private bool _hasSmokeExponent;
    private string _smokePlacementExponent = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private MachineGunType _selectedMachineGunType = MachineGunType.LMG;
    private bool _hasSprayingFire;
    private string? _dismantledImage;
    private bool _showDismantledImage;
    private System.Windows.Input.ICommand? _pickDismantledImageCommand;


    /// <summary>
    /// Gets or sets the name/type of the support weapon.
    /// </summary>
    [Required(ErrorMessage = "Id is required.")]
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    /// <summary>
    /// Gets or sets the file path to the image representing the unit when dismantled.
    /// </summary>
    public string? DismantledImage { get => _dismantledImage; set => SetProperty(ref _dismantledImage, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the dismantled image box should be shown.
    /// </summary>
    public bool ShowDismantledImage { get => _showDismantledImage; set => SetProperty(ref _showDismantledImage, value); }

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
    /// Gets or sets the rate of fire (ROF) value as a string for UI binding.
    /// </summary>
    [Range(typeof(int), "1", "5", ErrorMessage = "ROF must be between 1 and 5.")]
    public string RateOfFire { get => _rateOfFire; set => SetProperty(ref _rateOfFire, value); }

    /// <summary>
    /// Gets or sets the breakdown number as a string for UI binding.
    /// </summary>
    [Range(typeof(int), "0", "12", ErrorMessage = "Breakdown number must be between 0 and 12.")]
    public string BreakdownNumber { get => _breakdownNumber; set => SetProperty(ref _breakdownNumber, value); }

    /// <summary>
    /// Gets or sets the removal number as a string for UI binding.
    /// </summary>
    [Range(typeof(int), "0", "12", ErrorMessage = "Removal number must be between 0 and 12.")]
    public string RemovalNumber { get => _removalNumber; set => SetProperty(ref _removalNumber, value); }

    /// <summary>
    /// Gets or sets the repair number as a string for UI binding.
    /// </summary>
    [Range(typeof(int), "0", "12", ErrorMessage = "Repair number must be between 0 and 12.")]
    public string RepairNumber { get => _repairNumber; set => SetProperty(ref _repairNumber, value); }

    /// <summary>
    /// Gets or sets the portage cost as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Portage cost is required.")]
    [Range(typeof(int), "0", "10", ErrorMessage = "Portage cost must be between 0 and 10.")]
    public string PortageCost 
    { 
        get => _portageCost; 
        set 
        { 
            if (SetProperty(ref _portageCost, value)) 
                UpdateDismantledImageVisibility(); 
        } 
    }

    /// <summary>
    /// Gets or sets the portage cost when dismantled as a string for UI binding.
    /// </summary>
    [Range(typeof(int), "1", "10", ErrorMessage = "Dismantled cost must be between 1 and 10.")]
    public string DismantledCost 
    { 
        get => _dismantledCost; 
        set 
        {
            var previouslyHadDismantledCost = !string.IsNullOrWhiteSpace(_dismantledCost);
            if (SetProperty(ref _dismantledCost, value))
            {
                var currentlyHasDismantledCost = !string.IsNullOrWhiteSpace(_dismantledCost);
                if (currentlyHasDismantledCost && !previouslyHadDismantledCost)
                {
                    DismantledImage = string.Empty;
                }
                UpdateDismantledImageVisibility();
            }
        } 
    }

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
    /// Gets or sets the selected nationality for the weapon.
    /// </summary>
    public Nationality SelectedNationality 
    { 
        get => _selectedNationality; 
        set => SetProperty(ref _selectedNationality, value);
    }


    /// <summary>
    /// Gets the list of available machine gun types.
    /// </summary>
    public IEnumerable<MachineGunType> MachineGunTypes => Enum.GetValues(typeof(MachineGunType)).Cast<MachineGunType>();

    /// <summary>
    /// Gets or sets the selected machine gun type.
    /// </summary>
    public MachineGunType SelectedMachineGunType 
    { 
        get => _selectedMachineGunType; 
        set => SetProperty(ref _selectedMachineGunType, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the machine gun has spraying fire.
    /// </summary>
    public bool HasSprayingFire { get => _hasSprayingFire; set => SetProperty(ref _hasSprayingFire, value); }

    /// <summary>
    /// Gets the command for picking the dismantled unit image file.
    /// </summary>
    public System.Windows.Input.ICommand PickDismantledImageCommand { get => _pickDismantledImageCommand ??= new RelayCommand(_ => ExecutePickImage(2)); }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentViewModel"/> class.
    /// </summary>
    public EquipmentViewModel(IUnitRepository repository) : base(repository)
    {
        DisplayName = "Equipment";
    }

    /// <inheritdoc />
    protected override void OnImagePicked(int imageType, string filePath)
    {
        if (imageType == 0) ImagePathFront = filePath;
        else if (imageType == 1) ImagePathBack = filePath;
        else if (imageType == 2) DismantledImage = filePath;
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        Name = string.Empty;
        Firepower = string.Empty;
        Range = string.Empty;
        RateOfFire = string.Empty;
        BreakdownNumber = string.Empty;
        RemovalNumber = string.Empty;
        RepairNumber = string.Empty;
        PortageCost = string.Empty;
        DismantledCost = string.Empty;
        HasSmokeExponent = false;
        SmokePlacementExponent = string.Empty;
        SelectedNationality = Nationality.German;
        SelectedMachineGunType = MachineGunType.LMG;
        HasSprayingFire = false;
        ImagePathFront = null;
        ImagePathBack = null;
        DismantledImage = string.Empty;
        ShowDismantledImage = false;
        ClearErrors();
    }

    /// <inheritdoc />
    protected override void PopulateForm(Unit item)
    {
        Name = item.Name;
        SelectedNationality = item.Nationality;
        ImagePathFront = item.ImagePathFront ?? string.Empty;
        ImagePathBack = item.ImagePathBack ?? string.Empty;

        var mg = item.GetComponent<MachineGunComponent>();
        if (mg != null)
        {
            SelectedMachineGunType = mg.Type;
            HasSprayingFire = mg.HasSprayingFire;
        }
        
        var fp = item.FirePower;
        if (fp != null)
        {
            Firepower = fp.Firepower.ToString();
            Range = fp.Range.ToString();
            RateOfFire = fp.RateOfFire?.ToString() ?? string.Empty;
        }

        var breakdown = item.Breakdown;
        if (breakdown != null)
        {
            BreakdownNumber = breakdown.BreakdownNumber.ToString();
            RemovalNumber = breakdown.RemovalNumber.ToString();
            RepairNumber = breakdown.RepairNumber.ToString();
        }

        var portage = item.Portage;
        if (portage != null)
        {
            PortageCost = portage.AssembledCost.ToString();
            DismantledCost = portage.DismantledCost?.ToString() ?? string.Empty;
        }

        DismantledImage = item.Portage?.DismantledImage ?? string.Empty;
        UpdateDismantledImageVisibility();

        var smoke = item.GetComponent<SmokeProviderComponent>();
        HasSmokeExponent = smoke != null;
        SmokePlacementExponent = smoke?.CapabilityNumber.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        if (string.IsNullOrWhiteSpace(BreakdownNumber)) BreakdownNumber = "12";
        if (string.IsNullOrWhiteSpace(RemovalNumber)) RemovalNumber = "0";
        if (string.IsNullOrWhiteSpace(RepairNumber)) RepairNumber = "0";
        if (string.IsNullOrWhiteSpace(PortageCost)) PortageCost = "0";

        if (!ValidateAllProperties())
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

        unit.AddComponent(new MachineGunComponent 
        { 
            Type = SelectedMachineGunType,
            HasSprayingFire = HasSprayingFire
        });

        unit.AddComponent(new BreakdownComponent
        {
            BreakdownNumber = int.Parse(BreakdownNumber),
            RemovalNumber = int.Parse(RemovalNumber),
            RepairNumber = int.Parse(RepairNumber)
        });

        unit.AddComponent(new PortageComponent
        {
            AssembledCost = int.Parse(PortageCost),
            DismantledCost = int.TryParse(DismantledCost, out int dc) ? dc : null,
            DismantledImage = DismantledImage,
            IsDismantled = false
        });

        unit.AddComponent(new FirePowerComponent
        {
            Firepower = int.Parse(Firepower),
            Range = int.Parse(Range),
            RateOfFire = int.TryParse(RateOfFire, out int rof) ? rof : null
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

    protected override bool ValidateAllProperties()
    {
        bool isValid = base.ValidateAllProperties();

        if (!string.IsNullOrWhiteSpace(Name))
        {
            if (Items.Any(i => i.Item != EditingItem && 
                              i.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && 
                              i.Item.Nationality == SelectedNationality &&
                              i.Item.GetComponent<MachineGunComponent>()?.Type == SelectedMachineGunType))
            {
                AddError(nameof(Name), "Equipment with this Id, Nationality and Type already exists.");
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

    private void UpdateDismantledImageVisibility()
    {
        ShowDismantledImage = !string.IsNullOrWhiteSpace(PortageCost) && !string.IsNullOrWhiteSpace(DismantledCost);
    }
}
