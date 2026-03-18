using ASL;
using ASL.Counters;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ASLInputTool.ViewModels;

public class SquadsViewModel : CrudViewModelBase<MultiManCounter>
{
    private string _name = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string _morale = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private UnitClass _selectedClass = UnitClass.FirstLine;
    private string? _imagePathFront;
    private string? _imagePathBack;
    private bool _isHalfSquad;
    private bool _hasAssaultFire;
    private bool _hasSprayingFire;
    private bool _canSelfRally;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Firepower { get => _firepower; set => SetProperty(ref _firepower, value); }
    public string Range { get => _range; set => SetProperty(ref _range, value); }
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    public Nationality SelectedNationality { get => _selectedNationality; set => SetProperty(ref _selectedNationality, value); }
    public UnitClass SelectedClass { get => _selectedClass; set => SetProperty(ref _selectedClass, value); }
    public string? ImagePathFront { get => _imagePathFront; set => SetProperty(ref _imagePathFront, value); }
    public string? ImagePathBack { get => _imagePathBack; set => SetProperty(ref _imagePathBack, value); }
    public bool IsHalfSquad { get => _isHalfSquad; set => SetProperty(ref _isHalfSquad, value); }
    public bool HasAssaultFire { get => _hasAssaultFire; set => SetProperty(ref _hasAssaultFire, value); }
    public bool HasSprayingFire { get => _hasSprayingFire; set => SetProperty(ref _hasSprayingFire, value); }
    public bool CanSelfRally { get => _canSelfRally; set => SetProperty(ref _canSelfRally, value); }

    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();
    public IEnumerable<UnitClass> UnitClasses => Enum.GetValues(typeof(UnitClass)).Cast<UnitClass>();

    public RelayCommand PickFrontImageCommand { get; }
    public RelayCommand PickBackImageCommand { get; }

    public SquadsViewModel()
    {
        DisplayName = "Squads";
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(true));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(false));
    }

    private void ExecutePickImage(bool front)
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = front ? "Select Squad Front Image" : "Select Squad Back Image"
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
        Firepower = string.Empty;
        Range = string.Empty;
        Morale = string.Empty;
        SelectedNationality = Nationality.German;
        SelectedClass = UnitClass.FirstLine;
        ImagePathFront = null;
        ImagePathBack = null;
        IsHalfSquad = false;
        HasAssaultFire = false;
        HasSprayingFire = false;
        CanSelfRally = false;
    }

    protected override void PopulateForm(MultiManCounter item)
    {
        Name = item.Name;
        Firepower = item.Firepower.ToString();
        Range = item.Range.ToString();
        Morale = item.Morale.ToString();
        SelectedNationality = item.Nationality;
        SelectedClass = item.AslClass;
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
        IsHalfSquad = item is HalfSquad;
        HasAssaultFire = item.HasAssaultFire;
        HasSprayingFire = item.HasSprayingFire;
        CanSelfRally = item.CanSelfRally;
    }

    protected override void OnSave(object? parameter)
    {
        int fp = int.TryParse(Firepower, out int f) ? f : 4;
        int range = int.TryParse(Range, out int r) ? r : 6;
        int morale = int.TryParse(Morale, out int m) ? m : 7;

        MultiManCounter counter;
        if (IsHalfSquad)
        {
            counter = new HalfSquad(Name, fp, range, morale, SelectedClass, SelectedNationality);
        }
        else
        {
            counter = new Squad(Name, fp, range, morale, SelectedClass, SelectedNationality);
        }
        
        counter.ImagePathFront = ImagePathFront;
        counter.ImagePathBack = ImagePathBack;
        counter.HasAssaultFire = HasAssaultFire;
        counter.HasSprayingFire = HasSprayingFire;
        counter.CanSelfRally = CanSelfRally;

        if (EditingItem != null)
        {
            int index = Items.IndexOf(EditingItem);
            if (index >= 0) Items[index] = counter;
        }
        else
        {
            Items.Add(counter);
        }
        
        IsAdding = false;
    }
}
