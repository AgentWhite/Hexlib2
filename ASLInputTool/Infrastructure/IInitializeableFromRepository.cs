namespace ASLInputTool.Infrastructure;

/// <summary>
/// Interface for ViewModels that need to be initialized from their respective repositories.
/// Used to allow MainViewModel to agnostically initialize all loaded views.
/// </summary>
public interface IInitializeableFromRepository
{
    /// <summary>
    /// Initializes the ViewModel's state or collections from its assigned data repository.
    /// </summary>
    void InitializeFromRepository();
}
