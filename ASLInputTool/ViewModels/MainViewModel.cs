namespace ASLInputTool.ViewModels;

public class MainViewModel : ViewModelBase
{
    private ViewModelBase? _currentView;

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public List<ViewModelBase> NavigationItems { get; }
    
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

