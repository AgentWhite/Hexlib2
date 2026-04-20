using System;
using System.Collections.Generic;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// A simple service locator for ViewModels to support dynamic navigation and DI-like behavior.
/// </summary>
public class ViewModelLocator
{
    private readonly Dictionary<Type, ViewModelBase> _viewModels = new();
    private readonly IUnitRepository _unitRepository;
    private readonly IScenarioRepository _scenarioRepository;
    private readonly IModuleRepository _moduleRepository;
    private readonly IBoardRepository _boardRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelLocator"/> class and registers default ViewModels.
    /// </summary>
    public ViewModelLocator()
    {
        // Initialize infrastructure
        _unitRepository = new UnitRepository();
        _scenarioRepository = new ScenarioRepository();
        _moduleRepository = new ModuleRepository();
        _boardRepository = new BoardRepository();

        // Register default instances with injected dependencies
        Register<LeadersViewModel>(new LeadersViewModel(_unitRepository, _moduleRepository));
        Register<HeroesViewModel>(new HeroesViewModel(_unitRepository, _moduleRepository));
        Register<SquadsViewModel>(new SquadsViewModel(_unitRepository, _moduleRepository));
        Register<EquipmentViewModel>(new EquipmentViewModel(_unitRepository, _moduleRepository));
        Register<ScenariosViewModel>(new ScenariosViewModel(_scenarioRepository));
        Register<ModulesViewModel>(new ModulesViewModel(_moduleRepository));
        Register<BoardsViewModel>(new BoardsViewModel(_boardRepository));
    }

    /// <summary>
    /// Gets the registered unit repository.
    /// </summary>
    public IUnitRepository UnitRepository => _unitRepository;

    /// <summary>
    /// Gets the registered scenario repository.
    /// </summary>
    public IScenarioRepository ScenarioRepository => _scenarioRepository;

    /// <summary>
    /// Gets the registered module repository.
    /// </summary>
    public IModuleRepository ModuleRepository => _moduleRepository;

    /// <summary>
    /// Gets the registered board repository.
    /// </summary>
    public IBoardRepository BoardRepository => _boardRepository;

    /// <summary>
    /// Registers a ViewModel instance.
    /// </summary>
    /// <typeparam name="T">The type of the ViewModel.</typeparam>
    /// <param name="instance">The instance to register.</param>
    public void Register<T>(T instance) where T : ViewModelBase
    {
        _viewModels[typeof(T)] = instance;
    }

    /// <summary>
    /// Retrieves a registered ViewModel instance.
    /// </summary>
    /// <typeparam name="T">The type of the ViewModel to retrieve.</typeparam>
    /// <returns>The registered instance.</returns>
    public T Get<T>() where T : ViewModelBase
    {
        return (T)_viewModels[typeof(T)];
    }

    /// <summary>
    /// Gets all registered ViewModel instances.
    /// </summary>
    /// <returns>An enumerable of all registered ViewModels.</returns>
    public IEnumerable<ViewModelBase> GetAll() => _viewModels.Values;
}
