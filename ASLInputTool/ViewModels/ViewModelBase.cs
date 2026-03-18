using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Abstract base class for all ViewModels.
/// Implements INotifyPropertyChanged and INotifyDataErrorInfo.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private string? _displayName;
    /// <summary>
    /// Gets the name of the view to be displayed in navigation elements (like tabs).
    /// </summary>
    public string? DisplayName 
    { 
        get => _displayName; 
        protected set => SetProperty(ref _displayName, value); 
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets a property value and raises the PropertyChanged event if the value changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">Reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>True if the value was changed, false otherwise.</returns>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    // --- Validation Support ---

    private readonly Dictionary<string, List<string>> _errors = new();

    /// <inheritdoc />
    public bool HasErrors => _errors.Any();

    /// <inheritdoc />
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    /// <inheritdoc />
    public IEnumerable GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName) || !_errors.ContainsKey(propertyName))
            return Enumerable.Empty<string>();

        return _errors[propertyName];
    }

    /// <summary>
    /// Adds a validation error for a property.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="error">The error message.</param>
    protected void AddError(string propertyName, string error)
    {
        if (!_errors.ContainsKey(propertyName))
            _errors[propertyName] = new List<string>();

        if (!_errors[propertyName].Contains(error))
        {
            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Clears validation errors for a property or all properties.
    /// </summary>
    /// <param name="propertyName">The name of the property to clear, or null to clear all errors.</param>
    protected void ClearErrors(string? propertyName = null)
    {
        if (propertyName == null)
        {
            var propertyNames = _errors.Keys.ToList();
            _errors.Clear();
            foreach (var name in propertyNames)
                OnErrorsChanged(name);
        }
        else if (_errors.ContainsKey(propertyName))
        {
            _errors.Remove(propertyName);
            OnErrorsChanged(propertyName);
        }
    }

    /// <summary>
    /// Raises the <see cref="ErrorsChanged"/> event.
    /// </summary>
    /// <param name="propertyName">The name of the property with changed errors.</param>
    protected virtual void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }
}
