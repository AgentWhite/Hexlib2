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
    private string? _imagePath;

    public string Name { get => _name; set { SetProperty(ref _name, value); ClearErrors(nameof(Name)); } }
    public string Reference { get => _reference; set { SetProperty(ref _reference, value); ClearErrors(nameof(Reference)); } }
    public string Place { get => _place; set => SetProperty(ref _place, value); }
    public string Date { get => _date; set => SetProperty(ref _date, value); }
    public string DescriptionText { get => _descriptionText; set => SetProperty(ref _descriptionText, value); }
    public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }

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

    protected override void ResetForm()
    {
        Name = string.Empty;
        Reference = string.Empty;
        Place = string.Empty;
        Date = string.Empty;
        DescriptionText = string.Empty;
        ImagePath = null;
        ClearErrors();
    }

    protected override void PopulateForm(Scenario item)
    {
        Name = item.Name;
        Reference = item.Reference;
        ImagePath = item.ImagePath;
        Place = item.Description.Place;
        Date = item.Description.Date;
        DescriptionText = item.Description.DescriptionText;
    }

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
            Description = new ScenarioDescription(Place, Date, DescriptionText)
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
