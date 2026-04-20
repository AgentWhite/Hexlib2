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
/// ViewModel for managing Leader counters.
/// </summary>
public class LeadersViewModel : UnitViewModelBase
{
    /// <inheritdoc />
    protected override string UnitCategoryFilter => "Leader";
    private string _name = string.Empty;
    private string _morale = string.Empty;
    private string _brokenMorale = string.Empty;
    private string _leadership = string.Empty;
    private Nationality _selectedNationality = Nationality.German;


    /// <summary>
    /// Gets or sets the name of the leader.
    /// </summary>
    [Required(ErrorMessage = "Leader name is required.")]
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    /// <summary>
    /// Gets or sets the morale value as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Morale is required.")]
    [Range(typeof(int), "1", "15", ErrorMessage = "Morale must be between 1 and 15.")]
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }

    /// <summary>
    /// Gets or sets the broken morale value as a string for UI binding.
    /// </summary>
    [Range(typeof(int), "1", "12", ErrorMessage = "Broken morale must be between 1 and 12.")]
    public string BrokenMorale { get => _brokenMorale; set => SetProperty(ref _brokenMorale, value); }


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
    /// Gets or sets the selected nationality for the leader.
    /// </summary>
    public Nationality SelectedNationality 
    { 
        get => _selectedNationality; 
        set 
        { 
            if (SetProperty(ref _selectedNationality, value))
            {
                OnPropertyChanged(nameof(IsBrokenMoraleEnabled));
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
    /// Initializes a new instance of the <see cref="LeadersViewModel"/> class.
    /// </summary>
    public LeadersViewModel(IUnitRepository repository, IModuleRepository moduleRepository) : base(repository, moduleRepository)
    {
        DisplayName = "Leaders";
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
        _name = string.Empty;
        _morale = string.Empty;
        _brokenMorale = string.Empty;
        _leadership = string.Empty;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(Leadership));
        SelectedNationality = Nationality.German;
        SelectedModule = ASL.Models.Modules.Module.BeyondValor;
        ImagePathFront = null;
        ImagePathBack = null;
    }

    /// <inheritdoc />
    protected override void PopulateForm(Unit item)
    {
        ClearErrors();
        _name = item.Name;
        _morale = (item.Infantry?.Morale ?? 0).ToString();
        _brokenMorale = item.Infantry?.BrokenMorale?.ToString() ?? string.Empty;
        _leadership = (item.Leadership?.Leadership ?? 0).ToString();
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(Leadership));
        SelectedNationality = item.Nationality;
        SelectedModule = item.Module;
        OnPropertyChanged(nameof(IsBrokenMoraleEnabled));
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
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
            ImagePathFront = ImagePathFront,
            ImagePathBack = ImagePathBack
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
