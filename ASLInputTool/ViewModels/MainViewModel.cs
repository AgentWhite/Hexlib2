using ASL;
using ASL.Models;
using ASL.Models.Components;
using ASL.Persistence;
using HexLib.Persistence;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace ASLInputTool.ViewModels;

/// <summary>
/// The main ViewModel for the ASL Input Tool.
/// Manages top-level navigation between different views (Counters, Scenarios).
/// </summary>
public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _currentView;
    private readonly ASLSaveManager _saveManager;
    private readonly ViewModelLocator _locator;

    /// <summary>
    /// Gets or sets the currently active view model.
    /// </summary>
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    /// <summary>
    /// Gets the list of available navigation items (Views).
    /// </summary>
    public List<ViewModelBase> NavigationItems => _locator.GetAll().ToList();

    /// <summary>
    /// Command to save the current project to a file.
    /// </summary>
    public RelayCommand SaveCommand { get; }

    /// <summary>
    /// Command to load a project from a file.
    /// </summary>
    public RelayCommand LoadCommand { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// Sets up the view locator and save manager, and selects the first view.
    /// </summary>
    public MainViewModel()
    {
        _locator = new ViewModelLocator();
        _saveManager = new ASLSaveManager(new FileStorageAdapter(Path.GetTempPath()));

        CurrentView = NavigationItems.FirstOrDefault();

        SaveCommand = new RelayCommand(_ => ExecuteSave());
        LoadCommand = new RelayCommand(_ => ExecuteLoad());
    }

    private void ExecuteSave()
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "ASL Project files (*.asl)|*.asl",
            DefaultExt = "asl",
            Title = "Save ASL Project"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                var sourceProject = new ASLProject
                {
                    Counters = new List<Unit>(),
                    Scenarios = _locator.Get<ScenariosViewModel>().Items.Select(i => i.Item).ToList(),
                    Modules = _locator.Get<ModulesViewModel>().Items.Select(i => i.Item).ToList()
                };

                sourceProject.Counters.AddRange(_locator.Get<LeadersViewModel>().Items.Select(i => i.Item));
                sourceProject.Counters.AddRange(_locator.Get<HeroesViewModel>().Items.Select(i => i.Item));
                sourceProject.Counters.AddRange(_locator.Get<SquadsViewModel>().Items.Select(i => i.Item));
                sourceProject.Counters.AddRange(_locator.Get<SupportWeaponViewModel>().Items.Select(i => i.Item));

                var projectToSave = _saveManager.PrepareProjectForSaving(sourceProject, saveDialog.FileName);

                string json = _saveManager.SerializeProject(projectToSave);
                File.WriteAllText(saveDialog.FileName, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save project: {ex.Message}\n\n{ex.StackTrace}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private void ExecuteLoad()
    {
        var openDialog = new OpenFileDialog
        {
            Filter = "ASL Project files (*.asl)|*.asl",
            DefaultExt = "asl",
            Title = "Load ASL Project"
        };

        if (openDialog.ShowDialog() == true)
        {
            try
            {
                string projectDir = Path.GetDirectoryName(openDialog.FileName) ?? string.Empty;
                string json = File.ReadAllText(openDialog.FileName);
                var project = _saveManager.DeserializeProject(json);

                if (project != null)
                {
                    var leadersVm = _locator.Get<LeadersViewModel>();
                    var heroesVm = _locator.Get<HeroesViewModel>();
                    var squadsVm = _locator.Get<SquadsViewModel>();
                    var supportWeaponsVm = _locator.Get<SupportWeaponViewModel>();
                    var scenariosVm = _locator.Get<ScenariosViewModel>();
                    var modulesVm = _locator.Get<ModulesViewModel>();

                    foreach (var c in project.Counters)
                    {
                        if (!string.IsNullOrEmpty(c.ImagePathFront) && !Path.IsPathRooted(c.ImagePathFront))
                            c.ImagePathFront = Path.GetFullPath(Path.Combine(projectDir, c.ImagePathFront!));

                        if (!string.IsNullOrEmpty(c.ImagePathBack) && !Path.IsPathRooted(c.ImagePathBack))
                            c.ImagePathBack = Path.GetFullPath(Path.Combine(projectDir, c.ImagePathBack!));
                    }

                    foreach (var s in project.Scenarios.Where(s => !string.IsNullOrEmpty(s.ImagePath)))
                    {
                        if (!Path.IsPathRooted(s.ImagePath))
                            s.ImagePath = Path.GetFullPath(Path.Combine(projectDir, s.ImagePath!));
                    }

                    foreach (var m in project.Modules.Where(m => m != null))
                    {
                        if (!string.IsNullOrEmpty(m.FrontImage) && !Path.IsPathRooted(m.FrontImage))
                            m.FrontImage = Path.GetFullPath(Path.Combine(projectDir, m.FrontImage!));
                        
                        if (!string.IsNullOrEmpty(m.BackImage) && !Path.IsPathRooted(m.BackImage))
                            m.BackImage = Path.GetFullPath(Path.Combine(projectDir, m.BackImage!));
                    }

                    leadersVm.Items.Clear();
                    squadsVm.Items.Clear();
                    supportWeaponsVm.Items.Clear();
                    heroesVm.Items.Clear();

                    foreach (var unit in project.Counters)
                    {
                        if (unit.IsLeader)
                        {
                            leadersVm.Items.Add(new SelectableItem<Unit>(unit, leadersVm.NotifySelectionChanged));
                        }
                        else if (unit.IsHero)
                        {
                            heroesVm.Items.Add(new SelectableItem<Unit>(unit, heroesVm.NotifySelectionChanged));
                        }
                        else if (unit.IsSquad || unit.IsHalfSquad || unit.IsCrew)
                        {
                            squadsVm.Items.Add(new SelectableItem<Unit>(unit, squadsVm.NotifySelectionChanged));
                        }
                        else if (unit.IsSupportWeapon)
                        {
                            supportWeaponsVm.Items.Add(new SelectableItem<Unit>(unit, supportWeaponsVm.NotifySelectionChanged));
                        }
                    }
                    

                    scenariosVm.Items.Clear();
                    foreach (var s in project.Scenarios) scenariosVm.Items.Add(new SelectableItem<Scenario>(s, scenariosVm.NotifySelectionChanged));

                    modulesVm.Items.Clear();
                    foreach (var m in project.Modules.Where(m => m != null)) modulesVm.Items.Add(new SelectableItem<AslModule>(m, modulesVm.NotifySelectionChanged));
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load project: {ex.Message}\n\n{ex.StackTrace}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}

// ViewModels are now in their own files

