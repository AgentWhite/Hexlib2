using System.Collections.ObjectModel;
using ASL;
using System.Linq;
using System;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the Scenario entry and list view.
/// Handles historical scenarios including place, date, and description.
/// </summary>
public class ScenariosViewModel : CrudViewModelBase<Scenario>
{
    private string _name = string.Empty;
    private string _reference = string.Empty;
    private string _place = string.Empty;
    private string _date = string.Empty;
    private string _descriptionText = string.Empty;
    private string _aftermath = string.Empty;
    private string? _imagePath;

    /// <summary>
    /// Gets or sets the name of the scenario.
    /// </summary>
    public string Name { get => _name; set { SetProperty(ref _name, value); ClearErrors(nameof(Name)); } }
    /// <summary>
    /// Gets or sets the reference ID or publication source for the scenario.
    /// </summary>
    public string Reference { get => _reference; set { SetProperty(ref _reference, value); ClearErrors(nameof(Reference)); } }
    /// <summary>
    /// Gets or sets the historical place where the scenario occurred.
    /// </summary>
    public string Place { get => _place; set => SetProperty(ref _place, value); }
    /// <summary>
    /// Gets or sets the date/year of the scenario.
    /// </summary>
    public string Date { get => _date; set => SetProperty(ref _date, value); }
    /// <summary>
    /// Gets or sets the historical description text.
    /// </summary>
    public string DescriptionText { get => _descriptionText; set => SetProperty(ref _descriptionText, value); }
    /// <summary>
    /// Gets or sets the aftermath text for the scenario.
    /// </summary>
    public string Aftermath { get => _aftermath; set => SetProperty(ref _aftermath, value); }
    /// <summary>
    /// Gets or sets the path to the image representing the scenario (e.g., from the scenario card).
    /// </summary>
    public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }

    /// <summary>
    /// Command to pick an image for the scenario.
    /// </summary>
    public RelayCommand PickImageCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenariosViewModel"/> class.
    /// </summary>
    public ScenariosViewModel()
    {
        DisplayName = "Scenarios";
        PickImageCommand = new RelayCommand(_ => ExecutePickImage());
    }

    private void ExecutePickImage()
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = "Select Scenario Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            ImagePath = openDialog.FileName;
        }
    }

    /// <inheritdoc />
    protected override void ResetForm()
    {
        Name = string.Empty;
        Reference = string.Empty;
        Place = string.Empty;
        Date = string.Empty;
        DescriptionText = string.Empty;
        Aftermath = string.Empty;
        ImagePath = null;
        ClearErrors();
    }

    /// <inheritdoc />
    protected override void PopulateForm(Scenario item)
    {
        Name = item.Name;
        Reference = item.Reference;
        ImagePath = item.ImagePath;
        Place = item.Description.Place;
        Date = item.Description.Date;
        DescriptionText = item.Description.DescriptionText;
        Aftermath = item.Description.Aftermath;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        ClearErrors();
        bool hasValidationError = false;

        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "Name is required.");
            hasValidationError = true;
        }

        if (string.IsNullOrWhiteSpace(Reference))
        {
            AddError(nameof(Reference), "Reference is required.");
            hasValidationError = true;
        }

        if (hasValidationError)
        {
            ShowToast("Please fill in all required fields.");
            return;
        }

        // Only check for duplicates if we aren't editing the item itself
        if (Items.Any(s => s != EditingItem && (s.Name.Equals(Name, StringComparison.OrdinalIgnoreCase))))
        {
            AddError(nameof(Name), "A scenario with this name already exists.");
            ShowToast("Duplicate scenario name found!");
            return;
        }

        if (Items.Any(s => s != EditingItem && (s.Reference.Equals(Reference, StringComparison.OrdinalIgnoreCase))))
        {
            AddError(nameof(Reference), "A scenario with this reference already exists.");
            ShowToast("Duplicate scenario reference found!");
            return;
        }

        var scenario = new Scenario
        {
            Name = Name,
            Reference = Reference,
            ImagePath = ImagePath,
            Description = new ScenarioDescription(Place, Date, DescriptionText, Aftermath)
        };

        if (EditingItem != null)
        {
            int index = Items.IndexOf(EditingItem);
            if (index >= 0) Items[index] = scenario;
        }
        else
        {
            Items.Add(scenario);
        }

        IsAdding = false;
    }
}
