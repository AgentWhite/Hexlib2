using ASL;
using ASL.Counters;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ASLInputTool.ViewModels;

public class HeroesViewModel : CrudViewModelBase<Hero>
{
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private string? _imagePath;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Firepower { get => _firepower; set => SetProperty(ref _firepower, value); }
    public string Range { get => _range; set => SetProperty(ref _range, value); }
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    public Nationality SelectedNationality { get => _selectedNationality; set => SetProperty(ref _selectedNationality, value); }
    public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }

    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    public RelayCommand PickImageCommand { get; }

    public HeroesViewModel()
    {
        DisplayName = "Heroes";
        PickImageCommand = new RelayCommand(_ => ExecutePickImage());
    }

    private void ExecutePickImage()
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = "Select Hero Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            ImagePath = openDialog.FileName;
        }
    }

    protected override void ResetForm()
    {
        Name = string.Empty;
        Firepower = string.Empty;
        Range = string.Empty;
        Morale = string.Empty;
        SelectedNationality = Nationality.German;
        ImagePath = null;
    }

    protected override void PopulateForm(Hero item)
    {
        Name = item.Name;
        Firepower = item.Firepower.ToString();
        Range = item.Range.ToString();
        Morale = item.Morale.ToString();
        SelectedNationality = item.Nationality;
        ImagePath = item.ImagePath;
    }

    protected override void OnSave(object? parameter)
    {
        int fp = int.TryParse(Firepower, out int f) ? f : 1;
        int range = int.TryParse(Range, out int r) ? r : 4;
        int morale = int.TryParse(Morale, out int m) ? m : 9;

        var hero = new Hero(Name, fp, range, morale, SelectedNationality)
        {
            ImagePath = ImagePath
        };

        if (EditingItem != null)
        {
            int index = Items.IndexOf(EditingItem);
            if (index >= 0) Items[index] = hero;
        }
        else
        {
            Items.Add(hero);
        }
        
        IsAdding = false;
    }
}
