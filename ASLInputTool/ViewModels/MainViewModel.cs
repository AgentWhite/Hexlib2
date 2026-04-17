using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Units;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Board;
using ASL.Models.Components;
using ASL.Persistence;
using HexLib.Persistence;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using ASLInputTool.Helpers;
using ASLInputTool.Infrastructure;

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
    /// Command to define the global boards folder.
    /// </summary>
    public RelayCommand DefineBoardsFolderCommand { get; }
    
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
        DefineBoardsFolderCommand = new RelayCommand(_ => ExecuteDefineBoardsFolder());
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
                    Counters = _locator.UnitRepository.AllUnits.ToList(),
                    Scenarios = _locator.ScenarioRepository.AllScenarios.ToList(),
                    Modules = _locator.ModuleRepository.AllModules.ToList(),
                    Boards = _locator.BoardRepository.AllBoards.ToList()
                };

                var projectToSave = _saveManager.PrepareProjectForSaving(sourceProject, saveDialog.FileName);

                string json = _saveManager.SerializeProject(projectToSave);
                File.WriteAllText(saveDialog.FileName, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to save project: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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
                string json = File.ReadAllText(openDialog.FileName);
                var project = _saveManager.DeserializeProject(json);

                if (project != null)
                {
                    // Initialize repositories
                    _locator.UnitRepository.Initialize(project.Counters);
                    _locator.ScenarioRepository.Initialize(project.Scenarios);
                    _locator.ModuleRepository.Initialize(project.Modules ?? new List<AslModule>());
                    _locator.BoardRepository.Initialize(project.Boards ?? new List<AslBoard>());

                    // Process data (fix image paths etc)
                    _locator.UnitRepository.ProcessData(openDialog.FileName);
                    _locator.ScenarioRepository.ProcessData(openDialog.FileName);
                    _locator.ModuleRepository.ProcessData(openDialog.FileName);
                    _locator.BoardRepository.ProcessData(openDialog.FileName);

                    // Initialize child ViewModels from repositories
                    foreach (var vm in _locator.GetAll().OfType<IInitializeableFromRepository>())
                    {
                        vm.InitializeFromRepository();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to load project: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private void ExecuteDefineBoardsFolder()
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Title = "Define Boards Folder",
            InitialDirectory = SettingsManager.Instance.Settings.BoardsFolder
        };

        if (openFolderDialog.ShowDialog() == true)
        {
            SettingsManager.Instance.Settings.BoardsFolder = openFolderDialog.FolderName;
            SettingsManager.Instance.Save();
            System.Windows.MessageBox.Show($"Boards folder set to: {openFolderDialog.FolderName}", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
    }
}
