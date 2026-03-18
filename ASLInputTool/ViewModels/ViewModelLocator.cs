using System;
using System.Collections.Generic;

namespace ASLInputTool.ViewModels;

/// <summary>
/// A simple service locator for ViewModels to support dynamic navigation and DI-like behavior.
/// </summary>
public class ViewModelLocator
{
    private readonly Dictionary<Type, ViewModelBase> _viewModels = new();

    public ViewModelLocator()
    {
        // Register default instances
        Register<LeadersViewModel>(new LeadersViewModel());
        Register<HeroesViewModel>(new HeroesViewModel());
        Register<SquadsViewModel>(new SquadsViewModel());
        Register<ScenariosViewModel>(new ScenariosViewModel());
    }

    public void Register<T>(T instance) where T : ViewModelBase
    {
        _viewModels[typeof(T)] = instance;
    }

    public T Get<T>() where T : ViewModelBase
    {
        return (T)_viewModels[typeof(T)];
    }

    public IEnumerable<ViewModelBase> GetAll() => _viewModels.Values;
}
