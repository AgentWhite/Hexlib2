using ASL;
using ASL.Counters;
using System.Collections.Generic;
using System;
using System.Linq;

namespace ASLInputTool.ViewModels;

/// <summary>
/// ViewModel for managing Leader counters.
/// </summary>
public class LeadersViewModel : CrudViewModelBase<Leader>
{
    private string _name = string.Empty;
    private string _morale = string.Empty;
    private string _leadership = string.Empty;
    private Nationality _selectedNationality = Nationality.German;
    private string? _imagePathFront;
    private string? _imagePathBack;

    /// <summary>
    /// Gets or sets the name of the leader.
    /// </summary>
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    /// <summary>
    /// Gets or sets the morale rating of the leader.
    /// </summary>
    public string Morale { get => _morale; set => SetProperty(ref _morale, value); }
    /// <summary>
    /// Gets or sets the leadership modifier string (e.g., "-1", "0").
    /// </summary>
    public string Leadership { get => _leadership; set => SetProperty(ref _leadership, value); }
    /// <summary>
    /// Gets or sets the selected nationality of the leader.
    /// </summary>
    public Nationality SelectedNationality { get => _selectedNationality; set => SetProperty(ref _selectedNationality, value); }
    /// <summary>
    /// Gets or sets the path to the front image of the leader.
    /// </summary>
    public string? ImagePathFront { get => _imagePathFront; set => SetProperty(ref _imagePathFront, value); }
    /// <summary>
    /// Gets or sets the path to the back image of the leader.
    /// </summary>
    public string? ImagePathBack { get => _imagePathBack; set => SetProperty(ref _imagePathBack, value); }

    /// <summary>
    /// Gets the list of available nationalities.
    /// </summary>
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    /// <summary>
    /// Command to pick the front image for the leader.
    /// </summary>
    public RelayCommand PickFrontImageCommand { get; }

    /// <summary>
    /// Command to pick the back image for the leader.
    /// </summary>
    public RelayCommand PickBackImageCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LeadersViewModel"/> class.
    /// </summary>
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

    /// <inheritdoc />
    protected override void ResetForm()
    {
        Name = string.Empty;
        Morale = string.Empty;
        Leadership = string.Empty;
        SelectedNationality = Nationality.German;
        ImagePathFront = null;
        ImagePathBack = null;
    }

    /// <inheritdoc />
    protected override void PopulateForm(Leader item)
    {
        Name = item.Name;
        Morale = item.Morale.ToString();
        Leadership = item.Leadership.ToString();
        SelectedNationality = item.Nationality;
        ImagePathFront = item.ImagePathFront;
        ImagePathBack = item.ImagePathBack;
    }

    /// <inheritdoc />
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
            var wrapper = Items.FirstOrDefault(i => i.Item == EditingItem);
            if (wrapper != null)
            {
                int index = Items.IndexOf(wrapper);
                if (index >= 0) Items[index] = new SelectableItem<Leader>(leader, NotifySelectionChanged);
            }
        }
        else
        {
            Items.Add(new SelectableItem<Leader>(leader, NotifySelectionChanged));
        }
        
        IsAdding = false;
    }
}
