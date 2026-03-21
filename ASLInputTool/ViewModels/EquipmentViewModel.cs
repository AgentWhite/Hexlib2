using ASL.Models;
using ASL.Models.Components;
using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Equipment (LMG, MMG, HMG, etc.) counters.
/// </summary>
public class EquipmentViewModel : CrudViewModelBase<Unit>
{
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
    private string? _imagePathFront;
    private string? _imagePathBack;
    private string? _dismantledImage;
    private bool _showDismantledImage;
    private Nationality? _selectedNationalityFilter;

    /// <summary>
    /// Gets the filtered view of items.
    /// </summary>
    public ICollectionView FilteredItems => CollectionViewSource.GetDefaultView(Items);

    /// <summary>
    /// Gets or sets the nationality to filter the list by.
    /// </summary>
    public Nationality? SelectedNationalityFilter
    {
        get => _selectedNationalityFilter;
        set
        {
            if (SetProperty(ref _selectedNationalityFilter, value))
            {
                FilteredItems.Refresh();
            }
        }
    }

    /// <summary>
    /// Gets or sets the name/type of the support weapon.
    /// </summary>
    public string Name { get => _name; set { SetProperty(ref _name, value); ValidateName(); } }

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
    /// Gets or sets the removal number as a string for UI binding.
    /// </summary>
    public string RemovalNumber { get => _removalNumber; set { SetProperty(ref _removalNumber, value); ValidateRemovalNumber(); } }

    /// <summary>
    /// Gets or sets the repair number as a string for UI binding.
    /// </summary>
    public string RepairNumber { get => _repairNumber; set { SetProperty(ref _repairNumber, value); ValidateRepairNumber(); } }

    /// <summary>
    /// Gets or sets the portage cost as a string for UI binding.
    /// </summary>
    public string PortageCost { get => _portageCost; set { SetProperty(ref _portageCost, value); ValidatePortage(); } }

    /// <summary>
    /// Gets or sets the portage cost when dismantled as a string for UI binding.
    /// </summary>
    public string DismantledCost { get => _dismantledCost; set { SetProperty(ref _dismantledCost, value); ValidateDismantledPortage(); } }

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
    /// Gets the list of available machine gun types.
    /// </summary>
    public IEnumerable<MachineGunType> MachineGunTypes => Enum.GetValues(typeof(MachineGunType)).Cast<MachineGunType>();

    /// <summary>
    /// Gets or sets the selected machine gun type.
    /// </summary>
    public MachineGunType SelectedMachineGunType 
    { 
        get => _selectedMachineGunType; 
        set 
        { 
            if (SetProperty(ref _selectedMachineGunType, value))
            {
                ValidateUniqueness();
            }
        } 
    }

    /// <summary>
    /// Gets or sets a value indicating whether the machine gun has spraying fire.
    /// </summary>
    public bool HasSprayingFire { get => _hasSprayingFire; set => SetProperty(ref _hasSprayingFire, value); }

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
    /// Command to pick the dismantled image.
    /// </summary>
    public RelayCommand PickDismantledImageCommand { get; }

    /// <summary>
    /// Command to clear the nationality filter.
    /// </summary>
    public RelayCommand ClearFilterCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentViewModel"/> class.
    /// </summary>
    public EquipmentViewModel()
    {
        DisplayName = "Equipment";
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(0));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(1));
        PickDismantledImageCommand = new RelayCommand(_ => ExecutePickImage(2));
        ClearFilterCommand = new RelayCommand(_ => SelectedNationalityFilter = null);
        
        FilteredItems.Filter = obj =>
        {
            if (obj is SelectableItem<Unit> wrapper)
            {
                if (SelectedNationalityFilter == null) return true;
                return wrapper.Item.Nationality == SelectedNationalityFilter;
            }
            return true;
        };
    }

    private void ExecutePickImage(int imageType) // 0: Front, 1: Back, 2: Dismantled
    {
        string dialogTitle = imageType switch
        {
            0 => "Select Equipment Front Image",
            1 => "Select Equipment Back Image",
            2 => "Select Dismantled Image",
            _ => "Select Image"
        };

        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = dialogTitle
        };

        if (openDialog.ShowDialog() == true)
        {
            if (imageType == 0) ImagePathFront = openDialog.FileName;
            else if (imageType == 1) ImagePathBack = openDialog.FileName;
            else if (imageType == 2) DismantledImage = openDialog.FileName;
        }
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
        DismantledImage = item.Portage?.DismantledImage ?? string.Empty;

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

        UpdateDismantledImageVisibility();

        var smoke = item.GetComponent<SmokeProviderComponent>();
        HasSmokeExponent = smoke != null;
        SmokePlacementExponent = smoke?.CapabilityNumber.ToString() ?? string.Empty;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        ValidateName();
        
        if (string.IsNullOrWhiteSpace(BreakdownNumber)) BreakdownNumber = "12";

        // Required fields must not be empty on save
        if (string.IsNullOrWhiteSpace(Firepower)) AddError(nameof(Firepower), "Firepower is required.");
        if (string.IsNullOrWhiteSpace(Range)) AddError(nameof(Range), "Range is required.");
        if (string.IsNullOrWhiteSpace(RemovalNumber)) RemovalNumber = "0";
        if (string.IsNullOrWhiteSpace(RepairNumber)) RepairNumber = "0";
        if (string.IsNullOrWhiteSpace(PortageCost)) PortageCost = "0";

        ValidateFirepower();
        ValidateRange();
        ValidateROF();
        ValidateBreakdown();
        ValidateRemovalNumber();
        ValidateRepairNumber();
        ValidatePortage();
        ValidateDismantledPortage();
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
        if (string.IsNullOrWhiteSpace(Name)) AddError(nameof(Name), "Id is required.");
        else ValidateUniqueness();
    }

    private void ValidateUniqueness()
    {
        if (string.IsNullOrWhiteSpace(Name)) return;

        if (Items.Any(i => i.Item != EditingItem && 
                          i.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && 
                          i.Item.Nationality == SelectedNationality &&
                          i.Item.GetComponent<MachineGunComponent>()?.Type == SelectedMachineGunType))
        {
            AddError(nameof(Name), "Equipment with this Id, Nationality and Type already exists.");
        }
    }

    private void ValidateFirepower()
    {
        ClearErrors(nameof(Firepower));
        if (string.IsNullOrWhiteSpace(Firepower)) return;
        if (!int.TryParse(Firepower, out int fp) || fp <= 0) AddError(nameof(Firepower), "Must be a positive number.");
    }

    private void ValidateRange()
    {
        ClearErrors(nameof(Range));
        if (string.IsNullOrWhiteSpace(Range)) return;
        if (!int.TryParse(Range, out int r) || r <= 0) AddError(nameof(Range), "Must be a positive number.");
    }

    private void ValidateROF()
    {
        ClearErrors(nameof(RateOfFire));
        if (string.IsNullOrWhiteSpace(RateOfFire)) return;
        if (!int.TryParse(RateOfFire, out int rof) || rof <= 0) AddError(nameof(RateOfFire), "Must be a positive number.");
    }

    private void ValidateBreakdown()
    {
        ClearErrors(nameof(BreakdownNumber));
        if (string.IsNullOrWhiteSpace(BreakdownNumber)) return;
        if (!int.TryParse(BreakdownNumber, out int bn) || bn < 0) AddError(nameof(BreakdownNumber), "Must be a non-negative number.");
    }

    private void ValidateRemovalNumber()
    {
        ClearErrors(nameof(RemovalNumber));
        if (string.IsNullOrWhiteSpace(RemovalNumber)) return;
        if (!int.TryParse(RemovalNumber, out int rn) || rn < 0) AddError(nameof(RemovalNumber), "Must be a non-negative number.");
    }

    private void ValidateRepairNumber()
    {
        ClearErrors(nameof(RepairNumber));
        if (string.IsNullOrWhiteSpace(RepairNumber)) return;
        if (!int.TryParse(RepairNumber, out int rn) || rn < 0) AddError(nameof(RepairNumber), "Must be a non-negative number.");
    }

    private void ValidatePortage()
    {
        ClearErrors(nameof(PortageCost));
        if (string.IsNullOrWhiteSpace(PortageCost)) 
        {
            UpdateDismantledImageVisibility();
            return;
        }
        if (!int.TryParse(PortageCost, out int pc) || pc < 0) AddError(nameof(PortageCost), "Must be a non-negative number.");
        UpdateDismantledImageVisibility();
    }

    private void ValidateDismantledPortage()
    {
        var previouslyHadDismantledCost = !string.IsNullOrWhiteSpace(DismantledCost);
        
        ClearErrors(nameof(DismantledCost));
        if (!string.IsNullOrWhiteSpace(DismantledCost))
        {
            if (!int.TryParse(DismantledCost, out int dc) || dc <= 0) AddError(nameof(DismantledCost), "Must be a positive number.");
        }

        var currentlyHasDismantledCost = !string.IsNullOrWhiteSpace(DismantledCost);

        // Logic: if it reappears, clear the image
        if (currentlyHasDismantledCost && !previouslyHadDismantledCost)
        {
            DismantledImage = string.Empty;
        }

        UpdateDismantledImageVisibility();
    }

    private void UpdateDismantledImageVisibility()
    {
        ShowDismantledImage = !string.IsNullOrWhiteSpace(PortageCost) && !string.IsNullOrWhiteSpace(DismantledCost);
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
