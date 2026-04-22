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

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using ASLInputTool.Infrastructure;
using System.IO;
using System.Runtime.CompilerServices;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Base class for ViewModels that manage <see cref="Unit"/> counters.
/// Provides shared logic for nationality filtering, image picking, and common validation.
/// </summary>
public abstract class UnitViewModelBase : CrudViewModelBase<Unit>, IInitializeableFromRepository
{
    /// <summary>The unit repository.</summary>
    protected readonly IUnitRepository Repository;
    private readonly IModuleRepository _moduleRepository;
    private Nationality? _selectedNationalityFilter;
    private ASL.Models.Modules.Module _selectedModule = ASL.Models.Modules.Module.BeyondValor;
    private string? _imagePathFront;
    private string? _imagePathBack;
    private string? _svgFront;
    private string? _svgBack;
    private bool _isCutterActive;
    private Geometry? _cutterGhostGeometry;
    private string? _activeCutterSide;
    private string? _unitCode;
    private readonly ObservableCollection<Point> _activePolygonPoints = new();

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
    /// Gets or sets the SVG content for the front side.
    /// </summary>
    public string? SvgFront { get => _svgFront; set => SetProperty(ref _svgFront, value); }

    /// <summary>
    /// Gets or sets the SVG content for the back side.
    /// </summary>
    public string? SvgBack { get => _svgBack; set => SetProperty(ref _svgBack, value); }

    /// <summary>
    /// Gets or sets the currently selected module.
    /// </summary>
    public ASL.Models.Modules.Module SelectedModule { get => _selectedModule; set => SetProperty(ref _selectedModule, value); }

    /// <summary>
    /// Gets the list of available modules defined in the project.
    /// </summary>
    public IEnumerable<AslModule> AvailableModules => _moduleRepository.AllModules;

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
    /// Gets the command to open the SVG editor.
    /// </summary>
    public RelayCommand OpenSvgEditorCommand { get; }

    /// <summary>
    /// Gets the command to activate the cutter tool.
    /// </summary>
    public RelayCommand ToggleCutterCommand { get; }

    /// <summary>
    /// Gets or sets the indicator of which side is currently being cut (Front/Back).
    /// </summary>
    public string? ActiveCutterSide { get => _activeCutterSide; set => SetProperty(ref _activeCutterSide, value); }

    /// <summary>
    /// Gets or sets a value indicating whether the polygon cutter tool is active.
    /// </summary>
    public bool IsCutterActive 
    { 
        get => _isCutterActive; 
        set 
        { 
            if (SetProperty(ref _isCutterActive, value) && !value)
            {
                ActivePolygonPoints.Clear();
                CutterGhostGeometry = null;
                ActiveCutterSide = null;
            }
        } 
    }

    /// <summary>
    /// Gets or sets the ghost geometry for the cutter tool.
    /// </summary>
    public Geometry? CutterGhostGeometry { get => _cutterGhostGeometry; set => SetProperty(ref _cutterGhostGeometry, value); }

    /// <inheritdoc/>
    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        
        // Background sync of SVG strings when stats change
        if (IsStatProperty(propertyName))
        {
            RefreshSvgStrings();
        }
    }

    private bool IsStatProperty(string? propertyName)
    {
        return propertyName == nameof(UnitCode) || 
               propertyName == "Firepower" || 
               propertyName == "Range" || 
               propertyName == "Morale" || 
               propertyName == "BrokenMorale" || 
               propertyName == "Leadership" ||
               propertyName == "SelectedScale" ||
               propertyName == "SelectedClass" ||
               propertyName == "HasAssaultFire" ||
               propertyName == "HasSprayingFire" ||
               propertyName == "HasELR" ||
               propertyName == "HasSmokeExponent" ||
               propertyName == "SmokePlacementExponent" ||
               propertyName == "CanSelfRally" ||
               propertyName == "WoundedRange";
    }

    private void RefreshSvgStrings()
    {
        // We use two dummy SvgEditorViewModels to perform the regeneration
        // one for Front and one for Back.
        if (!string.IsNullOrEmpty(SvgFront))
        {
            var frontVm = new SvgEditorViewModel { SvgContent = SvgFront, IsBackSide = false };
            SyncPropertiesToEditor(frontVm, true);
            SvgFront = frontVm.SvgContent;
        }

        if (!string.IsNullOrEmpty(SvgBack))
        {
            var backVm = new SvgEditorViewModel { SvgContent = SvgBack, IsBackSide = true };
            SyncPropertiesToEditor(backVm, false);
            SvgBack = backVm.SvgContent;
        }
    }

    /// <summary>
    /// Gets or sets a transient unit code (ID) primarily for visual testing.
    /// Updating this property immediately injects the code into the SVG counter.
    /// </summary>
    public string? UnitCode 
    { 
        get => _unitCode; 
        set => SetProperty(ref _unitCode, value);
    }

    /// <summary>
    /// Gets the collection of points for the active cutting polygon.
    /// </summary>
    public ObservableCollection<Point> ActivePolygonPoints => _activePolygonPoints;

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
    protected UnitViewModelBase(IUnitRepository repository, IModuleRepository moduleRepository)
    {
        Repository = repository;
        _moduleRepository = moduleRepository;
        FilteredItems = CollectionViewSource.GetDefaultView(Items);
        FilteredItems.Filter = FilterPredicate;

        ClearFilterCommand = new RelayCommand(_ => SelectedNationalityFilter = null);
        PickFrontImageCommand = new RelayCommand(_ => ExecutePickImage(0));
        PickBackImageCommand = new RelayCommand(_ => ExecutePickImage(1));
        OpenSvgEditorCommand = new RelayCommand(p => ExecuteOpenSvgEditor(p));
        ToggleCutterCommand = new RelayCommand(_ => IsCutterActive = !IsCutterActive);
    }

    private void ExecuteOpenSvgEditor(object? parameter)
    {
        if (GlobalEditorService.ActiveSvgEditorWindow != null)
        {
            GlobalEditorService.ActiveSvgEditorWindow.Activate();
            return;
        }

        bool isFront = "Front".Equals(parameter as string);
        var vm = new SvgEditorViewModel 
        { 
            Title = $"Edit SVG {(isFront ? "Front" : "Back")}",
            SvgContent = isFront ? SvgFront : SvgBack,
            IsBackSide = !isFront,
            ToggleCutterCommand = ToggleCutterCommand,
            IsCutterActive = IsCutterActive
        };

        SyncPropertiesToEditor(vm, isFront);

        var dialog = new ASLInputTool.Views.SvgEditorDialog { DataContext = vm, Owner = System.Windows.Application.Current.MainWindow };
        
        // Sync triggers from Unit -> Editor
        PropertyChangedEventHandler unitPropertyHandler = (s, e) =>
        {
            if (e.PropertyName == nameof(IsCutterActive)) vm.IsCutterActive = IsCutterActive;
            else if (IsStatProperty(e.PropertyName)) SyncPropertiesToEditor(vm, isFront);
        };
        this.PropertyChanged += unitPropertyHandler;

        // Sync changes from Editor -> Unit
        vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SvgEditorViewModel.SvgContent))
            {
                if (isFront) SvgFront = vm.SvgContent;
                else SvgBack = vm.SvgContent;
            }
            else if (e.PropertyName == nameof(SvgEditorViewModel.IsCutterActive))
            {
                IsCutterActive = vm.IsCutterActive;
            }
        };

        // Handle unloading and closing
        dialog.Closed += (s, e) => 
        {
            this.PropertyChanged -= unitPropertyHandler;
            vm.Unload();
        };
        
        vm.CloseAction = result => 
        {
            if (result == false)
            {
                // If cancelled, we might want to revert, but for block color it's fine
            }
            dialog.Close();
        };

        dialog.Show();
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
    protected override void OnItemAdded(Unit item)
    {
        Repository.Add(item);
        TriggerDiskSave(item.Module);
    }

    /// <inheritdoc />
    protected override void OnItemRemoved(Unit item)
    {
        Repository.Remove(item);
        TriggerDiskSave(item.Module);
    }

    /// <summary>
    /// Synchronizes internal stat properties to an SvgEditorViewModel.
    /// </summary>
    private void SyncPropertiesToEditor(SvgEditorViewModel vm, bool isFront)
    {
        if (this is SquadsViewModel squadVm)
        {
            vm.IsInfantry = true;
            if (isFront)
            {
                vm.CounterStyle = CounterStyle.Horizontal;
                vm.IsCrew = squadVm.SelectedScale == InfantryScale.Crew;
                vm.StatClass = squadVm.SelectedScale == InfantryScale.Crew ? string.Empty : squadVm.SelectedClass switch
                {
                    UnitClass.Elite => "E",
                    UnitClass.FirstLine => "1",
                    UnitClass.SecondLine => "2",
                    UnitClass.Conscript => "C",
                    UnitClass.Green => "G",
                    _ => ""
                };
                vm.StatFirepower = squadVm.Firepower;
                vm.StatRange = squadVm.Range;
                vm.StatMorale = squadVm.Morale;
                vm.HasAssaultFire = squadVm.HasAssaultFire;
                vm.HasSprayingFire = squadVm.HasSprayingFire;
                vm.HasELR = squadVm.HasELR;
                vm.StatSmoke = squadVm.HasSmokeExponent ? squadVm.SmokePlacementExponent : string.Empty;
                vm.StatUnitCode = UnitCode ?? string.Empty;
            }
            else
            {
                vm.CounterStyle = CounterStyle.Horizontal;
                vm.StatBPV = squadVm.BPV;
                vm.StatBrokenMorale = squadVm.BrokenMorale;
                vm.StatUnitCode = UnitCode ?? string.Empty;
                vm.HasSelfRally = squadVm.CanSelfRally;
            }
        }
        else if (this is LeadersViewModel leaderVm)
        {
            vm.IsInfantry = true;
            if (isFront)
            {
                vm.CounterStyle = CounterStyle.VerticalCCW;
                vm.StatClass = string.Empty;
                vm.StatMorale = leaderVm.Morale;
                vm.StatLeadership = leaderVm.Leadership;
                vm.StatUnitCode = UnitCode ?? string.Empty;
                vm.StatBrokenMorale = string.Empty;
            }
            else
            {
                vm.CounterStyle = CounterStyle.VerticalCCW;
                vm.StatBrokenMorale = leaderVm.BrokenMorale;
                vm.StatUnitCode = string.Empty;
                vm.StatMorale = string.Empty;
                vm.StatLeadership = string.Empty;
                vm.HasSelfRally = true;
            }
        }
        else if (this is HeroesViewModel heroVm)
        {
            vm.IsInfantry = true;
            if (isFront)
            {
                vm.CounterStyle = CounterStyle.VerticalCW;
                vm.StatClass = string.Empty;
                vm.StatFirepower = heroVm.Firepower;
                vm.StatRange = heroVm.Range;
                vm.StatMorale = heroVm.Morale;
                vm.StatUnitCode = UnitCode ?? string.Empty;
                vm.StatMovementFactor = string.Empty;
                vm.StatBrokenMorale = string.Empty;
            }
            else
            {
                vm.CounterStyle = CounterStyle.VerticalCW;
                vm.StatClass = string.Empty;
                vm.StatFirepower = heroVm.Firepower;
                vm.StatRange = heroVm.WoundedRange;
                vm.StatMorale = heroVm.BrokenMorale;
                vm.StatUnitCode = UnitCode ?? string.Empty;
                vm.StatMovementFactor = "3 MF";
                vm.StatBrokenMorale = string.Empty;
            }
        }
    }

    /// <summary>
    /// Triggers a persist operation for all units belonging to the specified module.
    /// </summary>
    protected async void TriggerDiskSave(ASL.Models.Modules.Module moduleType)
    {
        try
        {
            var module = _moduleRepository.AllModules.FirstOrDefault(m => m.Module == moduleType);
            if (module != null)
            {
                await Repository.SaveUnitsForModuleAsync(moduleType, module.FullName);
            }
        }
        catch (Exception ex)
        {
            // Fail gracefully to prevent UI thread crash
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                File.AppendAllText(logPath, $"\n--- Background Save Error {DateTime.Now} ---\n{ex.Message}\n{ex.StackTrace}\n");
            }
            catch { /* If even logging fails, we must stay silent to avoid crashing */ }
        }
    }
}
