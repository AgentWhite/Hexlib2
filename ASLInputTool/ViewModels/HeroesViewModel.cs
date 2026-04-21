using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;
using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Hero counters.
/// </summary>
public class HeroesViewModel : UnitViewModelBase
{
    /// <inheritdoc />
    protected override string UnitCategoryFilter => "Hero";
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private string _brokenMorale = string.Empty;
    private string _woundedRange = string.Empty;
    private Nationality _selectedNationality = Nationality.German;


    /// <summary>
    /// Gets or sets the name of the hero.
    /// </summary>
    [Required(ErrorMessage = "Hero name is required.")]
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
    [Range(typeof(int), "1", "12", ErrorMessage = "Broken morale must be between 1 and 12.")]
    public string BrokenMorale { get => _brokenMorale; set => SetProperty(ref _brokenMorale, value); }

    /// <summary>
    /// Gets or sets the wounded range value as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Wounded range is required.")]
    [Range(typeof(int), "0", "50", ErrorMessage = "Wounded range must be between 0 and 50.")]
    public string WoundedRange { get => _woundedRange; set => SetProperty(ref _woundedRange, value); }

    /// <summary>
    /// Gets a value indicating whether this hero can have a broken morale value (false for Japanese).
    /// </summary>
    public bool CanHaveBrokenMorale => SelectedNationality != Nationality.Japanese;

    /// <summary>
    /// Gets or sets the selected nationality for the hero.
    /// </summary>
    public Nationality SelectedNationality 
    { 
        get => _selectedNationality; 
        set 
        { 
            if (SetProperty(ref _selectedNationality, value))
            {
                OnPropertyChanged(nameof(CanHaveBrokenMorale));
                if (SelectedNationality == Nationality.Japanese)
                {
                    ClearErrors(nameof(BrokenMorale));
                }
                else
                {
                    ValidateProperty(BrokenMorale, nameof(BrokenMorale));
                }
            }
        } 
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesViewModel"/> class.
    /// </summary>
    public HeroesViewModel(IUnitRepository repository, IModuleRepository moduleRepository) : base(repository, moduleRepository)
    {
        DisplayName = "Heroes";
    }

    /// <inheritdoc />
    protected override void ValidateProperty(object? value, string? propertyName)
    {
        base.ValidateProperty(value, propertyName);

        if (propertyName == nameof(BrokenMorale))
        {
            if (CanHaveBrokenMorale && string.IsNullOrWhiteSpace(value as string))
            {
                AddError(nameof(BrokenMorale), "Broken morale is required.");
            }
        }
    }

    /// <inheritdoc />
    protected override bool ValidateAllProperties()
    {
        bool isValid = base.ValidateAllProperties();

        if (Items.Any(i => i.Item != EditingItem && 
                           i.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && 
                           i.Item.Nationality == SelectedNationality))
        {
            AddError(nameof(Name), "A hero with this name already exists for this nationality.");
            isValid = false;
        }

        if (CanHaveBrokenMorale && string.IsNullOrWhiteSpace(BrokenMorale))
        {
            AddError(nameof(BrokenMorale), "Broken morale is required.");
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
        _woundedRange = string.Empty;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Firepower));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(WoundedRange));
        OnPropertyChanged(nameof(CanHaveBrokenMorale));
        SelectedNationality = Nationality.German;
        SelectedModule = ASL.Models.Modules.Module.BeyondValor;
        ImagePathFront = null;
        ImagePathBack = null;
        SvgFront = null;
        SvgBack = null;
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
        _woundedRange = (item.Hero?.WoundedRange ?? 0).ToString();
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Firepower));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(WoundedRange));
        OnPropertyChanged(nameof(CanHaveBrokenMorale));
        SelectedNationality = item.Nationality;
        SelectedModule = item.Module;
        ImagePathFront = item.Visual?.ImagePathFront;
        ImagePathBack = item.Visual?.ImagePathBack;
        SvgFront = item.Visual?.SvgFront;
        SvgBack = item.Visual?.SvgBack;
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
        int bmValue = CanHaveBrokenMorale ? int.Parse(BrokenMorale) : 0;
        
        var unit = new Unit
        {
            Name = Name,
            Nationality = SelectedNationality,
            Module = SelectedModule,
            UnitType = UnitType.SMC,
            Visual = new UnitVisual
            {
                ImagePathFront = ImagePathFront,
                ImagePathBack = ImagePathBack,
                SvgFront = SvgFront,
                SvgBack = SvgBack
            }
        };

        unit.AddComponent(new InfantryComponent 
        { 
            Morale = mValue, 
            BrokenMorale = bmValue, 
            AslClass = UnitClass.Elite,
            Scale = InfantryScale.SMC
        });
        unit.AddComponent(new HeroComponent { WoundedRange = int.Parse(WoundedRange) });
        unit.AddComponent(new FirePowerComponent { Firepower = fpValue, Range = rValue });

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
