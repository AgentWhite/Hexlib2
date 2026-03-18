using ASL;
using ASL.Persistence;
using HexLib.Persistence;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace ASLInputTool.ViewModels;

/// <summary>
/// The main ViewModel for the ASL Input Tool.
/// Manages top-level navigation between different views (Counters, Scenarios).
/// </summary>
public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _currentView;
    private readonly ASLSaveManager _saveManager;

    /// <summary>
    /// Gets or sets the currently active view model being displayed in the main content area.
    /// </summary>
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    /// <summary>
    /// Gets the list of available view models for navigation.
    /// Used by the TabControl for item generation.
    /// </summary>
    public List<ViewModelBase> NavigationItems { get; }

    public RelayCommand SaveCommand { get; }
    public RelayCommand LoadCommand { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// Sets up the initial navigation state.
    /// </summary>
    public MainViewModel()
    {
        // Use a dummy adapter for memory-only project serialization
        _saveManager = new ASLSaveManager(new FileStorageAdapter(Path.GetTempPath()));

        NavigationItems = new List<ViewModelBase>
        {
            new CountersViewModel(),
            new ScenariosViewModel()
        };

        CurrentView = NavigationItems[0];

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
            var project = new ASLProject
            {
                Counters = (NavigationItems[0] as CountersViewModel)?.Counters.ToList() ?? new(),
                Scenarios = (NavigationItems[1] as ScenariosViewModel)?.Scenarios.ToList() ?? new()
            };

            string json = _saveManager.SerializeProject(project);
            File.WriteAllText(saveDialog.FileName, json);
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
            string json = File.ReadAllText(openDialog.FileName);
            var project = _saveManager.DeserializeProject(json);

            if (project != null)
            {
                var countersVm = NavigationItems[0] as CountersViewModel;
                var scenariosVm = NavigationItems[1] as ScenariosViewModel;

                if (countersVm != null)
                {
                    countersVm.Counters.Clear();
                    foreach (var c in project.Counters) countersVm.Counters.Add(c);
                }

                if (scenariosVm != null)
                {
                    scenariosVm.Scenarios.Clear();
                    foreach (var s in project.Scenarios) scenariosVm.Scenarios.Add(s);
                }
            }
        }
    }
}

// ViewModels are now in their own files

