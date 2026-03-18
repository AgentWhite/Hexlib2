using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Abstract base class for all ViewModels.
/// Implements INotifyPropertyChanged to support WPF data binding.
/// </summary>
public abstract class ViewModelBase : INotifyPropertyChanged
{
    /// <summary>
    /// Gets the name of the view to be displayed in navigation elements (like tabs).
    /// </summary>
    public string? DisplayName { get; protected set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed. Automatically populated via <see cref="CallerMemberNameAttribute"/>.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Sets a property and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">Property type.</typeparam>
    /// <param name="storage">Reference to the backing field.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">Property name.</param>
    /// <returns>True if the value was changed, false otherwise.</returns>
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value)) return false;
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
