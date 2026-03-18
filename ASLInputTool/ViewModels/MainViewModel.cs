using ASL;
using ASL.Counters;
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
    private readonly ViewModelLocator _locator;

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
    public List<ViewModelBase> NavigationItems => _locator.GetAll().ToList();

    public RelayCommand SaveCommand { get; }
    public RelayCommand LoadCommand { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// Sets up the initial navigation state.
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
                var allCounters = new List<BaseASLCounter>();
                allCounters.AddRange(_locator.Get<LeadersViewModel>().Items);
                allCounters.AddRange(_locator.Get<HeroesViewModel>().Items);
                allCounters.AddRange(_locator.Get<SquadsViewModel>().Items);

                var sourceProject = new ASLProject
                {
                    Counters = allCounters,
                    Scenarios = _locator.Get<ScenariosViewModel>().Items.ToList()
                };

                // Process images and get a version of the project with relative GUID-based paths
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
                    var scenariosVm = _locator.Get<ScenariosViewModel>();

                    // Resolve relative paths to absolute for the UI to display
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

                    // Clear and distribute counters
                    leadersVm.Items.Clear();
                    heroesVm.Items.Clear();
                    squadsVm.Items.Clear();

                    foreach (var counter in project.Counters)
                    {
                        switch (counter)
                        {
                            case Leader l: leadersVm.Items.Add(l); break;
                            case Hero h: heroesVm.Items.Add(h); break;
                            case MultiManCounter m: squadsVm.Items.Add(m); break;
                        }
                    }

                    scenariosVm.Items.Clear();
                    foreach (var s in project.Scenarios) scenariosVm.Items.Add(s);
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

