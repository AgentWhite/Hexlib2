using System;
using System.Collections.Generic;

namespace ASLInputTool.ViewModels;

/// <summary>
/// A simple service locator for ViewModels to support dynamic navigation and DI-like behavior.
/// </summary>
public class ViewModelLocator
{
    private readonly Dictionary<Type, ViewModelBase> _viewModels = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ViewModelLocator"/> class and registers default ViewModels.
    /// </summary>
    public ViewModelLocator()
    {
        // Register default instances
        Register<LeadersViewModel>(new LeadersViewModel());
        Register<HeroesViewModel>(new HeroesViewModel());
        Register<SquadsViewModel>(new SquadsViewModel());
        Register<EquipmentViewModel>(new EquipmentViewModel());
        Register<ScenariosViewModel>(new ScenariosViewModel());
        Register<ModulesViewModel>(new ModulesViewModel());
    }

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
