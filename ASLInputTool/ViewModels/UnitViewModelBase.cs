using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Base class for ViewModels that manage <see cref="Unit"/> counters.
/// Provides shared logic for nationality filtering, image picking, and common validation.
/// </summary>
public abstract class UnitViewModelBase : CrudViewModelBase<Unit>, IInitializeableFromRepository
{
    /// <summary>The unit repository.</summary>
    protected readonly IUnitRepository Repository;
    private Nationality? _selectedNationalityFilter;
    private string? _imagePathFront;
    private string? _imagePathBack;

    /// <summary>
    /// Gets or sets the nationality used to filter the list.
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
    /// Gets the filtered view of items.
    /// </summary>
    public ICollectionView FilteredItems { get; }

    /// <summary>
    /// Gets or sets the file path for the front image.
    /// </summary>
    public string? ImagePathFront { get => _imagePathFront; set => SetProperty(ref _imagePathFront, value); }

    /// <summary>
    /// Gets or sets the file path for the back image.
    /// </summary>
    public string? ImagePathBack { get => _imagePathBack; set => SetProperty(ref _imagePathBack, value); }

    /// <summary>
    /// Gets the command to clear the nationality filter.
    /// </summary>
    public RelayCommand ClearFilterCommand { get; }

    /// <summary>
    /// Gets the command to pick the front image.
    /// </summary>
    public RelayCommand PickFrontImageCommand { get; }

    /// <summary>
    /// Gets the command to pick the back image.
    /// </summary>
    public RelayCommand PickBackImageCommand { get; }

    /// <summary>
    /// Gets the list of available nationalities.
    /// </summary>
    public IEnumerable<Nationality> Nationalities => Enum.GetValues(typeof(Nationality)).Cast<Nationality>();

    /// <summary>
    /// Gets the unit category used to filter units for this specific view.
    /// </summary>
    protected abstract string UnitCategoryFilter { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitViewModelBase"/> class.
    /// </summary>
    protected UnitViewModelBase(IUnitRepository repository)
    {
        Repository = repository;
        FilteredItems = CollectionViewSource.GetDefaultView(Items);
        FilteredItems.Filter = FilterPredicate;

        ClearFilterCommand = new RelayCommand(_ => SelectedNationalityFilter = null);
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(0));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(1));
    }

    /// <summary>
    /// Predicate logic for filtering items by nationality.
    /// </summary>
    protected virtual bool FilterPredicate(object obj)
    {
        if (obj is SelectableItem<Unit> wrapper)
        {
            if (SelectedNationalityFilter == null) return true;
            return wrapper.Item.Nationality == SelectedNationalityFilter;
        }
        return true;
    }

    /// <summary>
    /// Executes the image picking logic for different image types.
    /// </summary>
    /// <param name="imageType">0: Front, 1: Back, 2: Dismantled (if applicable).</param>
    protected virtual void ExecutePickImage(int imageType)
    {
        string dialogTitle = imageType switch
        {
            0 => "Select Front Image",
            1 => "Select Back Image",
            2 => "Select Dismantled Image",
            _ => "Select Image"
        };

        var openDialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
            Title = dialogTitle
        };

        if (openDialog.ShowDialog() == true)
        {
            OnImagePicked(imageType, openDialog.FileName);
        }
    }

    /// <summary>
    /// Called when an image has been picked via the dialog.
    /// </summary>
    protected abstract void OnImagePicked(int imageType, string filePath);



    /// <summary>
    /// Loads units from the repository that match the unit category.
    /// </summary>
    public void InitializeFromRepository()
    {
        Items.Clear();
        foreach (var unit in Repository.GetUnitsByCategory(UnitCategoryFilter))
        {
            Items.Add(new SelectableItem<Unit>(unit, NotifySelectionChanged));
        }
    }

    /// <inheritdoc />
    protected override void OnItemAdded(Unit item) => Repository.Add(item);

    /// <inheritdoc />
    protected override void OnItemRemoved(Unit item) => Repository.Remove(item);
}
