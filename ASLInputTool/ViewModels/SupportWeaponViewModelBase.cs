using System;
using System.ComponentModel.DataAnnotations;
using ASL.Models;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Base class for equipment and support weapon ViewModels.
/// </summary>
public abstract class SupportWeaponViewModelBase : UnitViewModelBase
{
    /// <summary>The name backing field.</summary>
    protected string _name = string.Empty;
    /// <summary>The selected nationality backing field.</summary>
    protected Nationality _selectedNationality = Nationality.German;
    /// <summary>The firepower backing field.</summary>
    protected string _firepower = string.Empty;
    /// <summary>The range backing field.</summary>
    protected string _range = string.Empty;
    /// <summary>The rate of fire backing field.</summary>
    protected string _rateOfFire = string.Empty;
    /// <summary>The breakdown number backing field.</summary>
    protected string _breakdownNumber = string.Empty;
    /// <summary>The removal number backing field.</summary>
    protected string _removalNumber = string.Empty;
    /// <summary>The repair number backing field.</summary>
    protected string _repairNumber = string.Empty;
    /// <summary>The portage cost backing field.</summary>
    protected string _portageCost = string.Empty;
    /// <summary>The dismantled cost backing field.</summary>
    protected string _dismantledCost = string.Empty;
    /// <summary>The dismantled image backing field.</summary>
    protected string? _dismantledImage;
    /// <summary>The show dismantled image visibility backing field.</summary>
    protected bool _showDismantledImage;

    /// <inheritdoc />
    protected override string UnitCategoryFilter => "Equipment";

    /// <summary>Gets or sets the name/type of the support weapon.</summary>
    [Required(ErrorMessage = "Id is required.")]
    public string Name { get => _name; set => SetProperty(ref _name, value); }

    /// <summary>Gets or sets the selected nationality for the weapon.</summary>
    public Nationality SelectedNationality { get => _selectedNationality; set => SetProperty(ref _selectedNationality, value); }

    /// <summary>Gets or sets the firepower value.</summary>
    public string Firepower { get => _firepower; set => SetProperty(ref _firepower, value); }

    /// <summary>Gets or sets the range value.</summary>
    public virtual string Range { get => _range; set => SetProperty(ref _range, value); }

    /// <summary>Gets or sets the rate of fire.</summary>
    [Range(typeof(int), "1", "5", ErrorMessage = "ROF must be between 1 and 5.")]
    public string RateOfFire { get => _rateOfFire; set => SetProperty(ref _rateOfFire, value); }

    /// <summary>Gets or sets the breakdown number.</summary>
    [Range(typeof(int), "0", "12", ErrorMessage = "Breakdown number must be between 0 and 12.")]
    public string BreakdownNumber { get => _breakdownNumber; set => SetProperty(ref _breakdownNumber, value); }

    /// <summary>Gets or sets the removal number.</summary>
    [Range(typeof(int), "0", "12", ErrorMessage = "Removal number must be between 0 and 12.")]
    public string RemovalNumber { get => _removalNumber; set => SetProperty(ref _removalNumber, value); }

    /// <summary>Gets or sets the repair number.</summary>
    [Range(typeof(int), "0", "12", ErrorMessage = "Repair number must be between 0 and 12.")]
    public string RepairNumber { get => _repairNumber; set => SetProperty(ref _repairNumber, value); }

    /// <summary>Gets or sets the portage cost.</summary>
    public string PortageCost 
    { 
        get => _portageCost; 
        set { if (SetProperty(ref _portageCost, value)) UpdateDismantledImageVisibility(); } 
    }

    /// <summary>Gets or sets the dismantled portage cost.</summary>
    [Range(typeof(int), "1", "10", ErrorMessage = "Dismantled cost must be between 1 and 10.")]
    public string DismantledCost 
    { 
        get => _dismantledCost; 
        set 
        {
            var previouslyHadDismantledCost = !string.IsNullOrWhiteSpace(_dismantledCost);
            if (SetProperty(ref _dismantledCost, value))
            {
                if (!previouslyHadDismantledCost && !string.IsNullOrWhiteSpace(value)) DismantledImage = string.Empty;
                UpdateDismantledImageVisibility();
            }
        } 
    }

    /// <summary>Gets or sets the dismantled image path.</summary>
    public string? DismantledImage { get => _dismantledImage; set => SetProperty(ref _dismantledImage, value); }

    /// <summary>Gets or sets a value indicating whether the dismantled image box should be shown.</summary>
    public bool ShowDismantledImage { get => _showDismantledImage; set => SetProperty(ref _showDismantledImage, value); }

    /// <summary>Command for picking the dismantled image.</summary>
    public System.Windows.Input.ICommand PickDismantledImageCommand { get; }

    /// <summary>Initializes a new instance of the <see cref="SupportWeaponViewModelBase"/> class.</summary>
    protected SupportWeaponViewModelBase(IUnitRepository repository) : base(repository)
    {
        PickDismantledImageCommand = new RelayCommand(_ => ExecutePickImage(2));
    }

    /// <inheritdoc />
    protected override void OnImagePicked(int imageType, string filePath)
    {
        if (imageType == 0) ImagePathFront = filePath;
        else if (imageType == 1) ImagePathBack = filePath;
        else if (imageType == 2) DismantledImage = filePath;
    }

    /// <summary>Updates visibility of dismantled image based on cost existence.</summary>
    protected void UpdateDismantledImageVisibility()
    {
        ShowDismantledImage = !string.IsNullOrWhiteSpace(PortageCost) && !string.IsNullOrWhiteSpace(DismantledCost);
    }
}
