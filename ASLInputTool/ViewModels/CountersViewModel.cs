using System.Collections.ObjectModel;
using System.Linq;
using ASL;
using ASL.Counters;
using System;
using System.Collections.Generic;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for the Counter entry and list view.
/// Handles management of SMC and MMC counters.
/// </summary>
public class CountersViewModel : CrudViewModelBase<BaseASLCounter>
{
    private Nationality _selectedNationality;
    private UnitClass _selectedClass;
    private string _name = string.Empty;
    private string _morale = string.Empty;
    private string _leadership = string.Empty;
    private string _firepower = string.Empty;
    private string _range = string.Empty;
    private string? _imagePath;
    private bool _isLeader = true;
    private bool _hasAssaultFire;
    private bool _hasSprayingFire;
    private bool _canSelfRally;

    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    public string Leadership { get => _leadership; set => SetProperty(ref _leadership, value); }
    public string Firepower { get => _firepower; set => SetProperty(ref _firepower, value); }
    public string Range { get => _range; set => SetProperty(ref _range, value); }
    public string? ImagePath { get => _imagePath; set => SetProperty(ref _imagePath, value); }
    public bool IsLeader { get => _isLeader; set => SetProperty(ref _isLeader, value); }
    public bool HasAssaultFire { get => _hasAssaultFire; set => SetProperty(ref _hasAssaultFire, value); }
    public bool HasSprayingFire { get => _hasSprayingFire; set => SetProperty(ref _hasSprayingFire, value); }
    public bool CanSelfRally { get => _canSelfRally; set => SetProperty(ref _canSelfRally, value); }

    public RelayCommand PickImageCommand { get; }

    /// <summary>
    /// Gets or sets the currently selected nationality in the entry form.
    /// </summary>
    public Nationality SelectedNationality
    {
        get => _selectedNationality;
        set => SetProperty(ref _selectedNationality, value);
    }

    /// <summary>
    /// Gets or sets the currently selected unit class in the entry form.
    /// </summary>
    public UnitClass SelectedClass
    {
        get => _selectedClass;
        set => SetProperty(ref _selectedClass, value);
    }

    /// <summary>
    /// Gets the list of available nationalities for selection.
    /// </summary>
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    /// <summary>
    /// Gets the list of available unit classes for selection.
    /// </summary>
    public IEnumerable<UnitClass> UnitClasses => Enum.GetValues(typeof(UnitClass)).Cast<UnitClass>();

    /// <summary>
    /// Initializes a new instance of the <see cref="CountersViewModel"/> class.
    /// </summary>
    public CountersViewModel()
    {
        DisplayName = "Counters";
        _selectedNationality = Nationality.German;
        _selectedClass = UnitClass.FirstLine;
        PickImageCommand = new RelayCommand(_ => ExecutePickImage());
    }

    private void ExecutePickImage()
    {
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg, *.png)|*.jpg;*.png",
            Title = "Select Counter Image"
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
        Firepower = string.Empty;
        Range = string.Empty;
        ImagePath = null;
        IsLeader = true;
        HasAssaultFire = false;
        HasSprayingFire = false;
        CanSelfRally = false;
        SelectedNationality = Nationality.German;
        SelectedClass = UnitClass.FirstLine;
    }

    protected override void PopulateForm(BaseASLCounter item)
    {
        Name = item.Name;
        Morale = item.Morale.ToString();
        SelectedNationality = item.Nationality;
        ImagePath = item.ImagePath;

        if (item is Leader leader)
        {
            IsLeader = true;
            Leadership = leader.Leadership.ToString();
        }
        else if (item is Hero hero)
        {
            IsLeader = false;
            Firepower = hero.Firepower.ToString();
            Range = hero.Range.ToString();
        }
        else if (item is MultiManCounter mmc)
        {
            // For now, only Squad is directly handled, but MultiManCounter properties are here
            IsLeader = false; // MMCs use the MMC tab in UI
            Firepower = mmc.Firepower.ToString();
            Range = mmc.Range.ToString();
            SelectedClass = mmc.AslClass;
            HasAssaultFire = mmc.HasAssaultFire;
            HasSprayingFire = mmc.HasSprayingFire;
            CanSelfRally = mmc.CanSelfRally;
        }
    }

    protected override void OnSave(object? parameter)
    {
        string? type = parameter as string;
        if (type == "SMC")
        {
            SaveSMC();
        }
        else if (type == "MMC")
        {
            SaveMMC();
        }
    }

    private void SaveSMC()
    {
        int morale = int.TryParse(Morale, out int m) ? m : 7;
        int leadership = int.TryParse(Leadership, out int l) ? l : 0;
        int firepower = int.TryParse(Firepower, out int f) ? f : 1;
        int range = int.TryParse(Range, out int r) ? r : 4;

        BaseASLCounter counter;
        if (IsLeader)
        {
            counter = new Leader(Name, morale, leadership, SelectedNationality);
        }
        else
        {
            counter = new Hero(Name, firepower, range, morale, SelectedNationality);
        }
        counter.ImagePath = ImagePath;

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

    private void SaveMMC()
    {
        int fp = int.TryParse(Firepower, out int f) ? f : 4;
        int range = int.TryParse(Range, out int r) ? r : 6;
        int morale = int.TryParse(Morale, out int m) ? m : 7;

        var squad = new Squad(Name, fp, range, morale, SelectedClass, SelectedNationality)
        {
            HasAssaultFire = HasAssaultFire,
            HasSprayingFire = HasSprayingFire,
            CanSelfRally = CanSelfRally,
            ImagePath = ImagePath
        };

        if (EditingItem != null)
        {
            int index = Items.IndexOf(EditingItem);
            if (index >= 0) Items[index] = squad;
        }
        else
        {
            Items.Add(squad);
        }

        IsAdding = false;
    }
}
