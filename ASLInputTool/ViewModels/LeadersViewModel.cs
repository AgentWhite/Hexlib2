
namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Leader counters.
/// </summary>
public class LeadersViewModel : InfantryViewModelBase
{
    /// <inheritdoc />
    protected override string UnitCategoryFilter => "Leader";
    
    private string _leadership = string.Empty;

    /// <summary>
    /// Gets or sets the leadership modifier as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Leadership is required.")]
    [Range(typeof(int), "-3", "3", ErrorMessage = "Leadership must be between -3 and 3.")]
    public string Leadership { get => _leadership; set => SetProperty(ref _leadership, value); }

    /// <summary>
    /// Gets a value indicating whether the Broken Morale field should be enabled (disabled for Japanese).
    /// </summary>
    public bool IsBrokenMoraleEnabled => SelectedNationality != Nationality.Japanese;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeadersViewModel"/> class.
    /// </summary>
    public LeadersViewModel(IUnitRepository repository, IModuleRepository moduleRepository) : base(repository, moduleRepository)
    {
        DisplayName = "Leaders";
    }

    /// <inheritdoc />
    protected override void OnNationalityChanged(Nationality newNationality)
    {
        OnPropertyChanged(nameof(IsBrokenMoraleEnabled));
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
    protected override bool ValidateAllProperties()
    {
        bool isValid = base.ValidateAllProperties();

        if (Items.Any(i => i.Item != EditingItem && 
                           i.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && 
                           i.Item.Nationality == SelectedNationality))
        {
            AddError(nameof(Name), "A leader with this name already exists for this nationality.");
            isValid = false;
        }

        if (SelectedNationality != Nationality.Japanese && string.IsNullOrWhiteSpace(BrokenMorale))
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
        Morale = string.Empty;
        BrokenMorale = string.Empty;
        Leadership = string.Empty;
        SelectedNationality = Nationality.German;
        SelectedModule = ASL.Models.Modules.Module.BeyondValor;
        ImagePathFront = null;
        ImagePathBack = null;
        SvgFront = null;
        SvgBack = null;
        Firepower = "0"; // Leaders have no inherent FP
        Range = "0";     // Leaders have no inherent Range
        OnPropertyChanged(nameof(IsBrokenMoraleEnabled));
    }

    /// <inheritdoc />
    protected override void PopulateForm(Unit item)
    {
        ClearErrors();
        Name = item.Name;
        Morale = (item.Infantry?.Morale ?? 0).ToString();
        BrokenMorale = item.Infantry?.BrokenMorale?.ToString() ?? string.Empty;
        Leadership = (item.Leadership?.Leadership ?? 0).ToString();
        
        SelectedNationality = item.Nationality;
        SelectedModule = item.Module;
        ImagePathFront = item.Visual?.ImagePathFront;
        ImagePathBack = item.Visual?.ImagePathBack;
        SvgFront = item.Visual?.SvgFront;
        SvgBack = item.Visual?.SvgBack;
        Firepower = "0";
        Range = "0";
        OnPropertyChanged(nameof(IsBrokenMoraleEnabled));
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        if (!ValidateAllProperties())
        {
            ShowToast("Please fix the validation errors.");
            return;
        }

        int m = int.Parse(Morale);
        int? bm = SelectedNationality == Nationality.Japanese ? null : (int.TryParse(BrokenMorale, out int bmv) ? bmv : 0);
        int l = int.Parse(Leadership);

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
            Morale = m, 
            BrokenMorale = bm, 
            AslClass = UnitClass.Elite,
            Scale = InfantryScale.SMC,
            CanSelfRally = true
        });
        unit.AddComponent(new LeadershipComponent { Leadership = l });

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
