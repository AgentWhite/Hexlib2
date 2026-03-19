using ASL;
using ASL.Counters;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Hero counters.
/// </summary>
public class HeroesViewModel : CrudViewModelBase<Hero>
{
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private string? _imagePathFront;
    private string? _imagePathBack;

    /// <summary>
    /// Gets or sets the name of the hero.
    /// </summary>
    public string Name { get => _name; set { SetProperty(ref _name, value); ValidateName(); } }
    /// <summary>
    /// Gets or sets the firepower rating of the hero.
    /// </summary>
    public string Firepower { get => _firepower; set { SetProperty(ref _firepower, value); ValidateFirepower(); } }
    /// <summary>
    /// Gets or sets the range rating of the hero.
    /// </summary>
    public string Range { get => _range; set { SetProperty(ref _range, value); ValidateRange(); } }
    /// <summary>
    /// Gets or sets the morale rating of the hero.
    /// </summary>
    public string Morale { get => _morale; set { SetProperty(ref _morale, value); ValidateMorale(); } }
    /// <summary>
    /// Gets or sets the selected nationality of the hero.
    /// </summary>
    public Nationality SelectedNationality 
    { 
        get => _selectedNationality; 
        set 
        { 
            if (SetProperty(ref _selectedNationality, value))
            {
                ValidateName();
            }
        } 
    }
    /// <summary>
    /// Gets or sets the path to the front image of the hero.
    /// </summary>
    public string? ImagePathFront { get => _imagePathFront; set => SetProperty(ref _imagePathFront, value); }
    /// <summary>
    /// Gets or sets the path to the back image of the hero.
    /// </summary>
    public string? ImagePathBack { get => _imagePathBack; set => SetProperty(ref _imagePathBack, value); }

    /// <summary>
    /// Gets the list of available nationalities.
    /// </summary>
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    /// <summary>
    /// Command to pick the front image for the hero.
    /// </summary>
    public RelayCommand PickFrontImageCommand { get; }
    
    /// <summary>
    /// Command to pick the back image for the hero.
    /// </summary>
    public RelayCommand PickBackImageCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HeroesViewModel"/> class.
    /// </summary>
    public HeroesViewModel()
    {
        DisplayName = "Heroes";
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(true));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(false));
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
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Firepower));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(Morale));
        SelectedNationality = Nationality.German;
        ImagePathFront = null;
        ImagePathBack = null;
    }

    /// <inheritdoc />
    protected override void PopulateForm(Hero item)
    {
        ClearErrors();
        _name = item.Name;
        _firepower = item.Firepower.ToString();
        _range = item.Range.ToString();
        _morale = item.Morale.ToString();
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Firepower));
        OnPropertyChanged(nameof(Range));
        OnPropertyChanged(nameof(Morale));
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

        if (HasErrors)
        {
            ShowToast("Please fix the validation errors.");
            return;
        }

        int fp = int.Parse(Firepower);
        int range = int.Parse(Range);
        int morale = int.Parse(Morale);

        var hero = new Hero(Name, fp, range, morale, SelectedNationality)
        {
            ImagePathFront = ImagePathFront,
            ImagePathBack = ImagePathBack
        };

        if (EditingItem != null)
        {
            var wrapper = Items.FirstOrDefault(i => i.Item == EditingItem);
            if (wrapper != null)
            {
                int index = Items.IndexOf(wrapper);
                if (index >= 0) Items[index] = new SelectableItem<Hero>(hero, NotifySelectionChanged);
            }
        }
        else
        {
            Items.Add(new SelectableItem<Hero>(hero, NotifySelectionChanged));
        }
        
        IsAdding = false;
    }
}
