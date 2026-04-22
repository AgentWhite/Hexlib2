using System.ComponentModel.DataAnnotations;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Base class for infantry-style units (Squads, Heroes, Leaders) providing shared properties like Firepower, Range, and Morale.
/// </summary>
public abstract class InfantryViewModelBase : UnitViewModelBase
{
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private string _brokenMorale = string.Empty;
    private Nationality _selectedNationality = Nationality.German;

    /// <summary>
    /// Gets or sets the unit's name or identity.
    /// </summary>
    [Required(ErrorMessage = "Name is required.")]
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    /// <summary>
    /// Gets or sets the firepower value as a string for UI binding.
    /// </summary>
    [Required(ErrorMessage = "Firepower is required.")]
    [Range(typeof(int), "0", "30", ErrorMessage = "Firepower must be between 0 and 30.")]
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
    /// Gets or sets the selected nationality.
    /// </summary>
    public Nationality SelectedNationality 
    { 
        get => _selectedNationality; 
        set 
        { 
            if (SetProperty(ref _selectedNationality, value))
            {
                OnNationalityChanged(value);
            }
        } 
    }

    /// <summary>
    /// Invoked when the nationality changes.
    /// </summary>
    protected virtual void OnNationalityChanged(Nationality newNationality) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InfantryViewModelBase"/> class.
    /// </summary>
    protected InfantryViewModelBase(IUnitRepository repository, IModuleRepository moduleRepository) 
        : base(repository, moduleRepository) { }
}
