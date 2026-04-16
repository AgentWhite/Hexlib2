using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a single row in a to-hit table.
/// </summary>
public class ToHitRowViewModel : INotifyPropertyChanged
{
    private string _toHit = string.Empty;

    /// <summary>Gets the range value for this row.</summary>
    public int Range { get; }

    /// <summary>Gets or sets the to-hit number string.</summary>
    public string ToHit
    {
        get => _toHit;
        set
        {
            if (_toHit != value)
            {
                _toHit = value;
                OnPropertyChanged();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToHitRowViewModel"/> class.
    /// </summary>
    /// <param name="range">The range value.</param>
    /// <param name="toHit">The to-hit string.</param>
    public ToHitRowViewModel(int range, string toHit = "")
    {
        Range = range;
        _toHit = toHit;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
