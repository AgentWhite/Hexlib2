
namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Hero counters.
/// </summary>
public class HeroesViewModel : InfantryViewModelBase
{
    /// <inheritdoc />
    protected override string UnitCategoryFilter => "Hero";
    
    private string _woundedRange = string.Empty;

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
    /// Initializes a new instance of the <see cref="HeroesViewModel"/> class.
    /// </summary>
    public HeroesViewModel(IUnitRepository repository, IModuleRepository moduleRepository) : base(repository, moduleRepository)
    {
        DisplayName = "Heroes";
    }

    /// <inheritdoc />
    protected override void OnNationalityChanged(Nationality newNationality)
    {
        OnPropertyChanged(nameof(CanHaveBrokenMorale));
        if (newNationality == Nationality.Japanese)
        {
            ClearErrors(nameof(BrokenMorale));
        }
        else
        {
            ValidateProperty(BrokenMorale, nameof(BrokenMorale));
        }
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
        Name = string.Empty;
        Firepower = string.Empty;
        Range = string.Empty;
        Morale = string.Empty;
        BrokenMorale = string.Empty;
        WoundedRange = string.Empty;
        SelectedNationality = Nationality.German;
        SelectedModule = ASL.Models.Modules.Module.BeyondValor;
        ImagePathFront = null;
        ImagePathBack = null;
        SvgFront = null;
        SvgBack = null;
        OnPropertyChanged(nameof(CanHaveBrokenMorale));
    }

    /// <inheritdoc />
    protected override void PopulateForm(Unit item)
    {
        ClearErrors();
        Name = item.Name;
        Firepower = (item.FirePower?.Firepower ?? 0).ToString();
        Range = (item.FirePower?.Range ?? 0).ToString();
        Morale = (item.Infantry?.Morale ?? 0).ToString();
        BrokenMorale = (item.Infantry?.BrokenMorale ?? 0).ToString();
        WoundedRange = (item.Hero?.WoundedRange ?? 0).ToString();
        
        SelectedNationality = item.Nationality;
        SelectedModule = item.Module;
        ImagePathFront = item.Visual?.ImagePathFront;
        ImagePathBack = item.Visual?.ImagePathBack;
        SvgFront = item.Visual?.SvgFront;
        SvgBack = item.Visual?.SvgBack;
        OnPropertyChanged(nameof(CanHaveBrokenMorale));
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
