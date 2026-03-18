using ASL;
using ASL.Counters;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ASLInputTool.ViewModels;

public class LeadersViewModel : CrudViewModelBase<Leader>
{
    private string _name = string.Empty;
    private string _morale = string.Empty;
    private string _leadership = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private string? _imagePath;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    public string Leadership { get => _leadership; set => SetProperty(ref _leadership, value); }
    public Nationality SelectedNationality { get => _selectedNationality; set => SetProperty(ref _selectedNationality, value); }
    public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }

    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    public RelayCommand PickImageCommand { get; }

    public LeadersViewModel()
    {
        DisplayName = "Leaders";
        PickImageCommand = new RelayCommand(_ => ExecutePickImage());
    }

    private void ExecutePickImage()
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = "Select Leader Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            ImagePath = openDialog.FileName;
        }
    }

    protected override void ResetForm()
    {
        Name = string.Empty;
        Morale = string.Empty;
        Leadership = string.Empty;
        SelectedNationality = Nationality.German;
        ImagePath = null;
    }

    protected override void PopulateForm(Leader item)
    {
        Name = item.Name;
        Morale = item.Morale.ToString();
        Leadership = item.Leadership.ToString();
        SelectedNationality = item.Nationality;
        ImagePath = item.ImagePath;
    }

    protected override void OnSave(object? parameter)
    {
        int morale = int.TryParse(Morale, out int m) ? m : 7;
        int leadership = int.TryParse(Leadership, out int l) ? l : 0;

        var leader = new Leader(Name, morale, leadership, SelectedNationality)
        {
            ImagePath = ImagePath
        };

        if (EditingItem != null)
        {
            int index = Items.IndexOf(EditingItem);
            if (index >= 0) Items[index] = leader;
        }
        else
        {
            Items.Add(leader);
        }
        
        IsAdding = false;
    }
}
