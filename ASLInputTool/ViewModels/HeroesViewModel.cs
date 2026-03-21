using ASL.Models;
using ASL.Models.Components;
using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Hero counters.
/// </summary>
public class HeroesViewModel : CrudViewModelBase<Unit>
{
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private string _brokenMorale = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private string? _imagePathFront;
    private string? _imagePathBack;
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
    /// Gets or sets the name of the hero.
    /// </summary>
    public string Name { get => _name; set { SetProperty(ref _name, value); ValidateName(); } }

    /// <summary>
    /// Gets or sets the firepower value as a string for UI binding.
    /// </summary>
    public string Firepower { get => _firepower; set { SetProperty(ref _firepower, value); ValidateFirepower(); } }

    /// <summary>
    /// Gets or sets the range value as a string for UI binding.
    /// </summary>
    public string Range { get => _range; set { SetProperty(ref _range, value); ValidateRange(); } }

    /// <summary>
    /// Gets or sets the morale value as a string for UI binding.
    /// </summary>
    public string Morale { get => _morale; set { SetProperty(ref _morale, value); ValidateMorale(); } }

    /// <summary>
    /// Gets or sets the broken morale value as a string for UI binding.
    /// </summary>
    public string BrokenMorale { get => _brokenMorale; set { SetProperty(ref _brokenMorale, value); ValidateBrokenMorale(); } }

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
                ValidateName();
                OnPropertyChanged(nameof(CanHaveBrokenMorale));
                ValidateBrokenMorale();
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
    /// Command to clear the nationality filter.
    /// </summary>
    public RelayCommand ClearFilterCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesViewModel"/> class.
    /// </summary>
    public HeroesViewModel()
    {
        DisplayName = "Heroes";
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(true));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(false));
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

    private void ValidateName()
    {
        ClearErrors(nameof(Name));
        if (string.IsNullOrWhiteSpace(Name))
        {
            AddError(nameof(Name), "Hero name is required.");
            ShowToast("Hero name is required.");
        }
        else if (Items.Any(i => i.Item != EditingItem && 
                               i.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && 
                               i.Item.Nationality == SelectedNationality))
        {
            AddError(nameof(Name), "A hero with this name already exists for this nationality.");
            ShowToast("Duplicate hero name!");
        }
    }

    private void ValidateFirepower()
    {
        ClearErrors(nameof(Firepower));
        if (string.IsNullOrWhiteSpace(Firepower))
        {
            AddError(nameof(Firepower), "Firepower is required.");
            ShowToast("Firepower is required.");
        }
        else if (!int.TryParse(Firepower, out int f) || f <= 0)
        {
            AddError(nameof(Firepower), "Firepower must be a positive number.");
            ShowToast("Firepower must be a positive number.");
        }
    }

    private void ValidateRange()
    {
        ClearErrors(nameof(Range));
        if (string.IsNullOrWhiteSpace(Range))
        {
            AddError(nameof(Range), "Range is required.");
            ShowToast("Range is required.");
        }
        else if (!int.TryParse(Range, out int r) || r <= 0)
        {
            AddError(nameof(Range), "Range must be a positive number.");
            ShowToast("Range must be a positive number.");
        }
    }

    private void ValidateMorale()
    {
        ClearErrors(nameof(Morale));
        if (string.IsNullOrWhiteSpace(Morale))
        {
            AddError(nameof(Morale), "Morale is required.");
            ShowToast("Morale is required.");
        }
        else if (!int.TryParse(Morale, out int m) || m <= 0)
        {
            AddError(nameof(Morale), "Morale must be a positive number.");
            ShowToast("Morale must be a positive number.");
        }
    }

    private void ValidateBrokenMorale()
    {
        ClearErrors(nameof(BrokenMorale));
        if (!CanHaveBrokenMorale) return;

        if (string.IsNullOrWhiteSpace(BrokenMorale))
        {
            AddError(nameof(BrokenMorale), "Broken morale is required.");
            ShowToast("Broken morale is required.");
        }
        else if (!int.TryParse(BrokenMorale, out int m) || m <= 0)
        {
            AddError(nameof(BrokenMorale), "Broken morale must be a positive number.");
            ShowToast("Broken morale must be a positive number.");
        }
    }

    private void ExecutePickImage(bool front)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = front ? "Select Hero Front Image" : "Select Hero Back Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            if (front) ImagePathFront = openDialog.FileName;
            else ImagePathBack = openDialog.FileName;
        }
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
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Firepower));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(CanHaveBrokenMorale));
        SelectedNationality = Nationality.German;
        ImagePathFront = null;
        ImagePathBack = null;
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
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Firepower));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(CanHaveBrokenMorale));
        SelectedNationality = item.Nationality;
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        ValidateName();
        ValidateFirepower();
        ValidateRange();
        ValidateMorale();
        ValidateBrokenMorale();

        if (HasErrors)
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
            UnitType = UnitType.SMC,
            ImagePathFront = ImagePathFront,
            ImagePathBack = ImagePathBack
        };

        unit.AddComponent(new InfantryComponent 
        { 
            Morale = mValue, 
            BrokenMorale = bmValue, 
            AslClass = UnitClass.Elite,
            Scale = InfantryScale.SMC
        });
        unit.AddComponent(new HeroComponent());
        unit.AddComponent(new FirePowerComponent { Firepower = fpValue, Range = rValue });

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
}
