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
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Defines the categories of equipment.
/// </summary>
public enum EquipmentType
{
    /// <summary>Machine guns (LMG, MMG, HMG).</summary>
    MachineGun,
    /// <summary>Radio sets.</summary>
    Radio,
    /// <summary>Field telephones.</summary>
    Telephone,
    /// <summary>Light Anti-Tank weapons (Bazooka, Panzerschreck, etc.).</summary>
    LightAntiTank
}

/// <summary>
/// ViewModel for managing Equipment (LMG, MMG, HMG, Radios, etc.) counters.
/// </summary>
public class EquipmentViewModel : SupportWeaponViewModelBase
{
    private bool _hasSmokeExponent;
    private string _smokePlacementExponent = string.Empty;
    private MachineGunType _selectedMachineGunType = MachineGunType.LMG;
    private bool _hasSprayingFire;
    
    // New fields for multiple equipment types
    private EquipmentType _selectedEquipmentType = EquipmentType.MachineGun;
    private string _contactNumber = string.Empty;

    // Light Anti Tank fields
    private LightAntiTankWeaponType _selectedLightAntiTankWeaponType = LightAntiTankWeaponType.Baz43;
    private bool _isShapedCharge;
    private bool _hasBackBlast;
    private bool _hasToHitTable;
    private bool _canEnableToHitTable;
    private System.Collections.ObjectModel.ObservableCollection<ToHitRowViewModel> _privateToHitTable = new();

    /// <summary>Gets the available equipment types.</summary>
    public IEnumerable<EquipmentType> EquipmentTypes => Enum.GetValues(typeof(EquipmentType)).Cast<EquipmentType>();

    /// <summary>Gets the available light anti-tank weapon types.</summary>
    public IEnumerable<LightAntiTankWeaponType> LightAntiTankWeaponTypes => Enum.GetValues(typeof(LightAntiTankWeaponType)).Cast<LightAntiTankWeaponType>();

    /// <summary>Gets or sets the selected equipment type.</summary>
    public EquipmentType SelectedEquipmentType
    {
        get => _selectedEquipmentType;
        set
        {
            if (SetProperty(ref _selectedEquipmentType, value))
            {
                OnPropertyChanged(nameof(IsMachineGun));
                OnPropertyChanged(nameof(IsRadioOrPhone));
                OnPropertyChanged(nameof(IsLightAntiTank));
                OnPropertyChanged(nameof(ShowPortage));
                UpdateToHitTableState();
            }
        }
    }

    /// <summary>Gets a value indicating whether the selected equipment is a machine gun.</summary>
    public bool IsMachineGun => SelectedEquipmentType == EquipmentType.MachineGun;

    /// <summary>Gets a value indicating whether the selected equipment is a radio or telephone.</summary>
    public bool IsRadioOrPhone => SelectedEquipmentType == EquipmentType.Radio || SelectedEquipmentType == EquipmentType.Telephone;

    /// <summary>Gets a value indicating whether the selected equipment is a light anti-tank weapon.</summary>
    public bool IsLightAntiTank => SelectedEquipmentType == EquipmentType.LightAntiTank;

    /// <summary>Gets a value indicating whether portage costs should be shown.</summary>
    public bool ShowPortage => SelectedEquipmentType != EquipmentType.Telephone;

    /// <summary>Gets or sets the contact number (for Radios/Phones).</summary>
    [Range(typeof(int), "1", "12", ErrorMessage = "Contact number must be between 1 and 12.")]
    public string ContactNumber { get => _contactNumber; set => SetProperty(ref _contactNumber, value); }

    /// <summary>Gets or sets the selected LATW type.</summary>
    public LightAntiTankWeaponType SelectedLightAntiTankWeaponType { get => _selectedLightAntiTankWeaponType; set => SetProperty(ref _selectedLightAntiTankWeaponType, value); }

    /// <summary>Gets or sets a value indicating whether the weapon uses a shaped charge.</summary>
    public bool IsShapedCharge { get => _isShapedCharge; set => SetProperty(ref _isShapedCharge, value); }

    /// <summary>Gets or sets a value indicating whether the weapon has a backblast.</summary>
    public bool HasBackBlast { get => _hasBackBlast; set => SetProperty(ref _hasBackBlast, value); }
    
    /// <summary>Gets or sets a value indicating whether a custom to-hit table is used.</summary>
    public bool HasToHitTable 
    { 
        get => _hasToHitTable; 
        set 
        {
            if (SetProperty(ref _hasToHitTable, value))
            {
                if (value) RebuildToHitTable();
                else PrivateToHitTable.Clear();
            }
        }
    }

    /// <summary>Gets or sets a value indicating whether a to-hit table can be enabled.</summary>
    public bool CanEnableToHitTable { get => _canEnableToHitTable; set => SetProperty(ref _canEnableToHitTable, value); }

    /// <summary>Gets or sets the collection of to-hit values for each range.</summary>
    public System.Collections.ObjectModel.ObservableCollection<ToHitRowViewModel> PrivateToHitTable
    {
        get => _privateToHitTable;
        set => SetProperty(ref _privateToHitTable, value);
    }

    /// <summary>Overriding Range to trigger to-hit table updates.</summary>
    public override string Range 
    { 
        get => base.Range; 
        set 
        {
            if (SetProperty(ref _range, value, nameof(Range))) { UpdateToHitTableState(); }
        }
    }

    private void UpdateToHitTableState()
    {
        if (!IsLightAntiTank) return;
        
        if (int.TryParse(Range, out int r) && r >= 0)
        {
            CanEnableToHitTable = true;
            if (HasToHitTable) RebuildToHitTable();
        }
        else
        {
            CanEnableToHitTable = false;
            HasToHitTable = false;
        }
    }

    private void RebuildToHitTable()
    {
        if (!int.TryParse(Range, out int rangeVal)) return;

        var tempDict = PrivateToHitTable.ToDictionary(r => r.Range, r => r.ToHit);
        PrivateToHitTable.Clear();

        for (int i = 0; i <= rangeVal; i++)
        {
            string th = tempDict.TryGetValue(i, out var existing) ? existing : string.Empty;
            PrivateToHitTable.Add(new ToHitRowViewModel(i, th));
        }
    }

    /// <summary>Gets or sets a value indicating whether the weapon has a smoke placement exponent.</summary>
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

    /// <summary>Gets or sets the smoke placement exponent as a string.</summary>
    [Range(typeof(int), "1", "9", ErrorMessage = "Smoke exponent must be between 1 and 9.")]
    public string SmokePlacementExponent  
    { 
        get => _smokePlacementExponent; 
        set => SetProperty(ref _smokePlacementExponent, value);
    }

    /// <summary>Gets the list of available machine gun types.</summary>
    public IEnumerable<MachineGunType> MachineGunTypes => Enum.GetValues(typeof(MachineGunType)).Cast<MachineGunType>();

    /// <summary>Gets or sets the selected machine gun type.</summary>
    public MachineGunType SelectedMachineGunType 
    { 
        get => _selectedMachineGunType; 
        set => SetProperty(ref _selectedMachineGunType, value);
    }

    /// <summary>Gets or sets a value indicating whether the machine gun has spraying fire.</summary>
    public bool HasSprayingFire { get => _hasSprayingFire; set => SetProperty(ref _hasSprayingFire, value); }

    /// <summary>Initializes a new instance of the <see cref="EquipmentViewModel"/> class.</summary>
    public EquipmentViewModel(IUnitRepository repository) : base(repository)
    {
        DisplayName = "Equipment";
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        Name = string.Empty;
        SelectedEquipmentType = EquipmentType.MachineGun;
        ContactNumber = string.Empty;
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
        
        SelectedLightAntiTankWeaponType = LightAntiTankWeaponType.Baz43;
        IsShapedCharge = false;
        HasBackBlast = false;
        HasToHitTable = false;
        CanEnableToHitTable = false;
        PrivateToHitTable.Clear();

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
        var radio = item.GetComponent<RadioComponent>();
        var latw = item.GetComponent<LightAntiTankWeaponComponent>();
        var portage = item.Portage;

        if (mg != null)
        {
            SelectedEquipmentType = EquipmentType.MachineGun;
            SelectedMachineGunType = mg.Type;
            HasSprayingFire = mg.HasSprayingFire;
        }
        else if (latw != null)
        {
            SelectedEquipmentType = EquipmentType.LightAntiTank;
            SelectedLightAntiTankWeaponType = latw.WeaponType;
            IsShapedCharge = latw.IsShapedCharge;
            HasBackBlast = latw.HasBackBlast;
            
            if (latw.PrivateToHitTable != null && latw.PrivateToHitTable.Any())
            {
                PrivateToHitTable.Clear();
                foreach (var kvp in latw.PrivateToHitTable.OrderBy(k => k.Key))
                {
                    PrivateToHitTable.Add(new ToHitRowViewModel(kvp.Key, kvp.Value.ToString()));
                }
                HasToHitTable = true;
                CanEnableToHitTable = true;
            }
        }
        else if (radio != null)
        {
            SelectedEquipmentType = portage != null ? EquipmentType.Radio : EquipmentType.Telephone;
            ContactNumber = radio.ContactNumber.ToString();
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
        if (ShowPortage && string.IsNullOrWhiteSpace(PortageCost)) PortageCost = "0";

        if (!ValidateAllProperties())
        {
            ShowToast("Please fix the validation errors.");
            return;
        }

        var unit = new Unit
        {
            Name = Name,
            Nationality = SelectedNationality,
            UnitType = UnitType.Ordnance, // All equipment (MG, Radio, Phone) is Ordnance
            ImagePathFront = ImagePathFront,
            ImagePathBack = ImagePathBack
        };

        if (IsMachineGun)
        {
            unit.AddComponent(new MachineGunComponent 
            { 
                Type = SelectedMachineGunType,
                HasSprayingFire = HasSprayingFire
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
        }
        else if (IsLightAntiTank)
        {
            var latw = new LightAntiTankWeaponComponent
            {
                WeaponType = SelectedLightAntiTankWeaponType,
                HasBackBlast = HasBackBlast,
                IsShapedCharge = IsShapedCharge
            };

            if (HasToHitTable && PrivateToHitTable.Any())
            {
                latw.PrivateToHitTable = new Dictionary<int, int>();
                foreach (var row in PrivateToHitTable)
                {
                    if (int.TryParse(row.ToHit, out int th))
                    {
                        latw.PrivateToHitTable[row.Range] = th;
                    }
                }
            }
            unit.AddComponent(latw);
            
            unit.AddComponent(new FirePowerComponent
            {
                Firepower = int.Parse(Firepower),
                Range = int.Parse(Range),
                RateOfFire = int.TryParse(RateOfFire, out int rof) ? rof : null
            });
        }
        else if (IsRadioOrPhone)
        {
            unit.AddComponent(new RadioComponent { ContactNumber = int.Parse(ContactNumber) });
        }

        unit.AddComponent(new BreakdownComponent
        {
            BreakdownNumber = int.Parse(BreakdownNumber),
            RemovalNumber = int.Parse(RemovalNumber),
            RepairNumber = int.Parse(RepairNumber)
        });

        if (ShowPortage)
        {
            unit.AddComponent(new PortageComponent
            {
                AssembledCost = int.Parse(PortageCost),
                DismantledCost = int.TryParse(DismantledCost, out int dc) ? dc : null,
                DismantledImage = DismantledImage,
                IsDismantled = false
            });
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

    /// <inheritdoc />
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

        if (IsMachineGun || IsLightAntiTank)
        {
            if (string.IsNullOrWhiteSpace(Firepower))
            {
                AddError(nameof(Firepower), "Firepower is required.");
                isValid = false;
            }
            if (string.IsNullOrWhiteSpace(Range))
            {
                AddError(nameof(Range), "Range is required.");
                isValid = false;
            }
            
            if (IsLightAntiTank && HasToHitTable)
            {
                bool tableValid = true;
                foreach (var row in PrivateToHitTable)
                {
                    if (!int.TryParse(row.ToHit, out int th) || th < 0)
                    {
                        tableValid = false;
                        break;
                    }
                }
                if (!tableValid)
                {
                    AddError(nameof(HasToHitTable), "All To-Hit numbers must be valid non-negative integers.");
                    isValid = false;
                }
            }
        }
        
        if (IsRadioOrPhone)
        {
            if (string.IsNullOrWhiteSpace(ContactNumber))
            {
                AddError(nameof(ContactNumber), "Contact number is required.");
                isValid = false;
            }
        }

        return isValid;
    }
}
