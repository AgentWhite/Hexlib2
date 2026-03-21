using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ASLInputTool.ViewModels;

public class ToHitRowViewModel : INotifyPropertyChanged
{
    private string _toHit = string.Empty;

    public int Range { get; }

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

    public ToHitRowViewModel(int range, string toHit = "")
    {
        Range = range;
        _toHit = toHit;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
