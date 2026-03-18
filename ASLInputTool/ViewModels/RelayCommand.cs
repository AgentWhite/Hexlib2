using System;
using System.Windows.Input;

namespace ASLInputTool.ViewModels;

/// <summary>
/// A simple implementation of <see cref="ICommand"/> used to bridge UI actions to ViewModel logic.
/// Allows passing an action to be executed and an optional predicate to determine if it can run.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    /// <param name="execute">The logic to be performed when the command is invoked.</param>
    /// <param name="canExecute">The logic to determine if the command can be executed.</param>
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

    /// <inheritdoc />
    public void Execute(object? parameter) => _execute(parameter);
}
