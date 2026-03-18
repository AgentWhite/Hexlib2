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
    private string? _imagePathFront;
    private string? _imagePathBack;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    public string Leadership { get => _leadership; set => SetProperty(ref _leadership, value); }
    public Nationality SelectedNationality { get => _selectedNationality; set => SetProperty(ref _selectedNationality, value); }
    public string? ImagePathFront { get => _imagePathFront; set => SetProperty(ref _imagePathFront, value); }
    public string? ImagePathBack { get => _imagePathBack; set => SetProperty(ref _imagePathBack, value); }

    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    public RelayCommand PickFrontImageCommand { get; }
    public RelayCommand PickBackImageCommand { get; }

    public LeadersViewModel()
    {
        DisplayName = "Leaders";
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(true));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(false));
    }

    private void ExecutePickImage(bool front)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = front ? "Select Leader Front Image" : "Select Leader Back Image"
        };

        if (openDialog.ShowDialog() == true)
        {
            if (front) ImagePathFront = openDialog.FileName;
            else ImagePathBack = openDialog.FileName;
        }
    }

    protected override void ResetForm()
    {
        Name = string.Empty;
        Morale = string.Empty;
        Leadership = string.Empty;
        SelectedNationality = Nationality.German;
        ImagePathFront = null;
        ImagePathBack = null;
    }

    protected override void PopulateForm(Leader item)
    {
        Name = item.Name;
        Morale = item.Morale.ToString();
        Leadership = item.Leadership.ToString();
        SelectedNationality = item.Nationality;
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
    }

    protected override void OnSave(object? parameter)
    {
        int morale = int.TryParse(Morale, out int m) ? m : 7;
        int leadership = int.TryParse(Leadership, out int l) ? l : 0;

        var leader = new Leader(Name, morale, leadership, SelectedNationality)
        {
            ImagePathFront = ImagePathFront,
            ImagePathBack = ImagePathBack
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
