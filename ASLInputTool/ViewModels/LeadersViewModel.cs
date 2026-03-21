using ASL.Models;
using ASL.Models.Components;
using System;
using System.Linq;
using System.ComponentModel;
using System.Windows.Data;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Leader counters.
/// </summary>
public class LeadersViewModel : CrudViewModelBase<Unit>
{
    private string _name = string.Empty;
    private string _morale = string.Empty;
    private string _brokenMorale = string.Empty;
    private string _bpv = string.Empty;
    private string _leadership = string.Empty;
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
    /// Gets or sets the name of the leader.
    /// </summary>
    public string Name { get => _name; set { SetProperty(ref _name, value); ValidateName(); } }

    /// <summary>
    /// Gets or sets the morale value as a string for UI binding.
    /// </summary>
    public string Morale { get => _morale; set { SetProperty(ref _morale, value); ValidateMorale(); } }

    /// <summary>
    /// Gets or sets the broken morale value as a string for UI binding.
    /// </summary>
    public string BrokenMorale { get => _brokenMorale; set { SetProperty(ref _brokenMorale, value); ValidateBrokenMorale(); } }

    /// <summary>
    /// Gets or sets the BPV value as a string for UI binding.
    /// </summary>
    public string BPV { get => _bpv; set { SetProperty(ref _bpv, value); ValidateBPV(); } }

    /// <summary>
    /// Gets or sets the leadership modifier as a string for UI binding.
    /// </summary>
    public string Leadership { get => _leadership; set { SetProperty(ref _leadership, value); ValidateLeadership(); } }

    /// <summary>
    /// Gets a value indicating whether the Broken Morale field should be enabled (disabled for Japanese).
    /// </summary>
    public bool IsBrokenMoraleEnabled => SelectedNationality != Nationality.Japanese;

    /// <summary>
    /// Gets or sets the selected nationality for the leader.
    /// </summary>
    public Nationality SelectedNationality 
    { 
        get => _selectedNationality; 
        set 
        { 
            if (SetProperty(ref _selectedNationality, value))
            {
                OnPropertyChanged(nameof(IsBrokenMoraleEnabled));
                if (SelectedNationality == Nationality.Japanese)
                {
                    ClearErrors(nameof(BrokenMorale));
                }
                else if (!string.IsNullOrWhiteSpace(BrokenMorale))
                {
                    ValidateBrokenMorale();
                }
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
    /// Initializes a new instance of the <see cref="LeadersViewModel"/> class.
    /// </summary>
    public LeadersViewModel()
    {
        DisplayName = "Leaders";
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
            AddError(nameof(Name), "Leader name is required.");
            ShowToast("Leader name is required.");
            return;
        }
        
        if (Items.Any(i => i.Item != EditingItem && 
                           i.Item.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && 
                           i.Item.Nationality == SelectedNationality))
        {
            AddError(nameof(Name), "A leader with this name already exists for this nationality.");
            ShowToast("Duplicate leader name!");
        }
    }

    private void ValidateMorale()
    {
        ClearErrors(nameof(Morale));
        if (string.IsNullOrWhiteSpace(Morale))
        {
            AddError(nameof(Morale), "Morale is required.");
            ShowToast("Morale is required.");
            return;
        }
        
        if (!int.TryParse(Morale, out int m) || m <= 0)
        {
            AddError(nameof(Morale), "Morale must be a positive number.");
            ShowToast("Morale must be a positive number.");
        }
    }

    private void ValidateBrokenMorale()
    {
        ClearErrors(nameof(BrokenMorale));
        if (SelectedNationality == Nationality.Japanese)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(BrokenMorale))
        {
            AddError(nameof(BrokenMorale), "Broken morale is required.");
            ShowToast("Broken morale is required.");
            return;
        }
        
        if (!int.TryParse(BrokenMorale, out int m) || m <= 0)
        {
            AddError(nameof(BrokenMorale), "Broken morale must be a positive number.");
            ShowToast("Broken morale must be a positive number.");
        }
    }

    private void ValidateBPV()
    {
        ClearErrors(nameof(BPV));
        if (string.IsNullOrWhiteSpace(BPV))
        {
            AddError(nameof(BPV), "BPV is required.");
            ShowToast("BPV is required.");
            return;
        }
        
        if (!int.TryParse(BPV, out int b) || b <= 0)
        {
            AddError(nameof(BPV), "BPV must be a positive number.");
            ShowToast("BPV must be a positive number.");
        }
    }

    private void ValidateLeadership()
    {
        ClearErrors(nameof(Leadership));
        if (string.IsNullOrWhiteSpace(Leadership))
        {
            AddError(nameof(Leadership), "Leadership is required.");
            ShowToast("Leadership is required.");
            return;
        }
        
        if (!int.TryParse(Leadership, out _))
        {
            AddError(nameof(Leadership), "Leadership must be a number.");
            ShowToast("Leadership must be a number.");
        }
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

    /// <inheritdoc />
    protected override void ResetForm()
    {
        ClearErrors();
        _name = string.Empty;
        _morale = string.Empty;
        _brokenMorale = string.Empty;
        _bpv = string.Empty;
        _leadership = string.Empty;
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(BPV));
        OnPropertyChanged(nameof(Leadership));
        SelectedNationality = Nationality.German;
        ImagePathFront = null;
        ImagePathBack = null;
    }

    /// <inheritdoc />
    protected override void PopulateForm(Unit item)
    {
        ClearErrors();
        _name = item.Name;
        _morale = (item.Infantry?.Morale ?? 0).ToString();
        _brokenMorale = item.Infantry?.BrokenMorale?.ToString() ?? string.Empty;
        _bpv = (item.Bpv?.BPV ?? 0).ToString();
        _leadership = (item.Leadership?.Leadership ?? 0).ToString();
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(Morale));
        OnPropertyChanged(nameof(BrokenMorale));
        OnPropertyChanged(nameof(BPV));
        OnPropertyChanged(nameof(Leadership));
        SelectedNationality = item.Nationality;
        OnPropertyChanged(nameof(IsBrokenMoraleEnabled));
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
    }

    /// <inheritdoc />
    protected override void OnSave(object? parameter)
    {
        ValidateName();
        ValidateMorale();
        ValidateBrokenMorale();
        ValidateBPV();
        ValidateLeadership();

        if (HasErrors)
        {
            ShowToast("Please fix the validation errors.");
            return;
        }

        int m = int.Parse(Morale);
        int? bm = SelectedNationality == Nationality.Japanese ? null : (int.TryParse(BrokenMorale, out int bmv) ? bmv : 0);
        int bpvValue = int.Parse(BPV);
        int l = int.Parse(Leadership);

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
            Morale = m, 
            BrokenMorale = bm, 
            AslClass = UnitClass.Elite,
            Scale = InfantryScale.SMC,
            CanSelfRally = true
        });
        unit.AddComponent(new LeadershipComponent { Leadership = l });
        unit.AddComponent(new BPVComponent { BPV = bpvValue });

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
