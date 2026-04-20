using ASL.Models;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Models.Components;
using ASL.Persistence;
using HexLib.Persistence;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Threading.Tasks;
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
    /// Command to define the global boards folder.
    /// </summary>
    public RelayCommand DefineBoardsFolderCommand { get; }
    
    /// <summary>
    /// Command to define the global modules folder.
    /// </summary>
    public RelayCommand DefineModulesFolderCommand { get; }

    /// <summary>
    /// Command to define the global scenarios folder.
    /// </summary>
    public RelayCommand DefineScenariosFolderCommand { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// Sets up the view locator and selects the first view.
    /// </summary>
    public MainViewModel()
    {
        _locator = new ViewModelLocator();
        CurrentView = NavigationItems.FirstOrDefault();

        DefineBoardsFolderCommand = new RelayCommand(_ => ExecuteDefineBoardsFolder());
        DefineModulesFolderCommand = new RelayCommand(_ => ExecuteDefineModulesFolder());
        DefineScenariosFolderCommand = new RelayCommand(_ => ExecuteDefineScenariosFolder());

        // Kick off initial scan and load
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        try
        {
            // 1. Scan and Load Boards
            var boards = await _locator.BoardRepository.ScanAndLoadAsync();
            _locator.BoardRepository.Initialize(boards);

            // 2. Scan and Load Modules
            var modules = await _locator.ModuleRepository.ScanAndLoadAsync();
            _locator.ModuleRepository.Initialize(modules);

            // 3. Scan and Load Scenarios
            var scenarios = await _locator.ScenarioRepository.ScanAndLoadAsync();
            _locator.ScenarioRepository.Initialize(scenarios);

            // 4. Load Units for each Module
            _locator.UnitRepository.Clear();
            foreach (var module in modules)
            {
                await _locator.UnitRepository.LoadUnitsForModuleAsync(module.FullName);
            }

            // 5. Trigger UI refreshes
            foreach (var vm in _locator.GetAll().OfType<IInitializeableFromRepository>())
            {
                vm.InitializeFromRepository();
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error during initialization: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
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

    private void ExecuteDefineModulesFolder()
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Title = "Define Modules Folder",
            InitialDirectory = SettingsManager.Instance.Settings.ModulesFolder
        };
        
        if (openFolderDialog.ShowDialog() == true)
        {
            SettingsManager.Instance.Settings.ModulesFolder = openFolderDialog.FolderName;
            SettingsManager.Instance.Save();
            System.Windows.MessageBox.Show($"Modules folder set to: {openFolderDialog.FolderName}", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            
            // Re-initialize to pick up modules from the new folder
            InitializeAsync();
        }
    }

    private void ExecuteDefineScenariosFolder()
    {
        var openFolderDialog = new OpenFolderDialog
        {
            Title = "Define Scenarios Folder",
            InitialDirectory = SettingsManager.Instance.Settings.ScenariosFolder
        };
        
        if (openFolderDialog.ShowDialog() == true)
        {
            SettingsManager.Instance.Settings.ScenariosFolder = openFolderDialog.FolderName;
            SettingsManager.Instance.Save();
            System.Windows.MessageBox.Show($"Scenarios folder set to: {openFolderDialog.FolderName}", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            
            // Re-initialize to pick up scenarios from the new folder
            InitializeAsync();
        }
    }
}
