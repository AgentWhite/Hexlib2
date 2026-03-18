namespace ASLInputTool.ViewModels;

/// <summary>
/// The main ViewModel for the ASL Input Tool.
/// Manages top-level navigation between different views (Counters, Scenarios).
/// </summary>
public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _currentView;

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
    
    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// Sets up the initial navigation state.
    /// </summary>
    public MainViewModel()
    {
        NavigationItems = new List<ViewModelBase>
        {
            new CountersViewModel(),
            new ScenariosViewModel()
        };

        CurrentView = NavigationItems[0];
    }
}

// ViewModels are now in their own files

