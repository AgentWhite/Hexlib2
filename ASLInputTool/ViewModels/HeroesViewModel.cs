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
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    /// <summary>
    /// Gets or sets the firepower rating of the hero.
    /// </summary>
    public string Firepower { get => _firepower; set => SetProperty(ref _firepower, value); }
    /// <summary>
    /// Gets or sets the range rating of the hero.
    /// </summary>
    public string Range { get => _range; set => SetProperty(ref _range, value); }
    /// <summary>
    /// Gets or sets the morale rating of the hero.
    /// </summary>
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    /// <summary>
    /// Gets or sets the selected nationality of the hero.
    /// </summary>
    public Nationality SelectedNationality { get => _selectedNationality; set => SetProperty(ref _selectedNationality, value); }
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
        Name = string.Empty;
        Firepower = string.Empty;
        Range = string.Empty;
        Morale = string.Empty;
        SelectedNationality = Nationality.German;
        ImagePathFront = null;
        ImagePathBack = null;
    }

    /// <inheritdoc />
    protected override void PopulateForm(Hero item)
    {
        Name = item.Name;
        Firepower = item.Firepower.ToString();
        Range = item.Range.ToString();
        Morale = item.Morale.ToString();
        SelectedNationality = item.Nationality;
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        int fp = int.TryParse(Firepower, out int f) ? f : 1;
        int range = int.TryParse(Range, out int r) ? r : 4;
        int morale = int.TryParse(Morale, out int m) ? m : 9;

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
