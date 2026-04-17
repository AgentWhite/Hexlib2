using System;
using ASL;
using ASL.Core;
using ASL.Models.Units;
using ASL.Models.Board;
using ASL.Models.Scenarios;
using ASL.Models.Modules;
using ASL.Models.Equipment;
using ASL.Infrastructure;
using ASL.Services;
using ASLInputTool.Infrastructure;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a selection of a specific edge of a hex.
/// </summary>
public class HexEdgeSelection
{
    /// <summary>Gets the hex associated with the selection.</summary>
    public HexViewModel Hex { get; }
    
    /// <summary>Gets the index of the selected edge (0-5).</summary>
    public int EdgeIndex { get; }
    
    /// <summary>Gets the view model for the hexside data.</summary>
    public HexsideViewModel Data { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HexEdgeSelection"/> class.
    /// </summary>
    /// <param name="hex">The hex.</param>
    /// <param name="edgeIndex">Index of the edge.</param>
    /// <param name="data">The hexside data.</param>
    public HexEdgeSelection(HexViewModel hex, int edgeIndex, HexsideViewModel data)
    {
        Hex = hex;
        EdgeIndex = edgeIndex;
        Data = data;
    }
}

/// <summary>
/// View model for managing properties of a specific hexside.
/// </summary>
public class HexsideViewModel : ViewModelBase
{
    private readonly ASLEdgeData _data;
    private readonly Action? _onChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="HexsideViewModel"/> class.
    /// </summary>
    /// <param name="data">The edge data.</param>
    /// <param name="onChanged">Callback for when data changes.</param>
    /// <param name="isHouseAvailable">Whether house connections are possible on this edge.</param>
    public HexsideViewModel(ASLEdgeData data, Action? onChanged, bool isHouseAvailable = true)
    {
        _data = data;
        _onChanged = onChanged;
        IsHouseConnectionAvailable = isHouseAvailable;
    }

    /// <summary>Gets a value indicating whether a house connection can be made at this hexside.</summary>
    public bool IsHouseConnectionAvailable { get; }

    /// <summary>Gets or sets a value indicating whether this hexside has a wall.</summary>
    public bool HasWall
    {
        get => _data.HasWall;
        set 
        { 
            _data.HasWall = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a hedge.</summary>
    public bool HasHedge
    {
        get => _data.HasHedge;
        set 
        { 
            _data.HasHedge = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has bocage.</summary>
    public bool HasBocage
    {
        get => _data.HasBocage;
        set 
        { 
            _data.HasBocage = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a paved road.</summary>
    public bool HasPavedRoad
    {
        get => _data.HasPavedRoad;
        set 
        { 
            _data.HasPavedRoad = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a dirt road.</summary>
    public bool HasDirtRoad
    {
        get => _data.HasDirtRoad;
        set 
        { 
            _data.HasDirtRoad = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a path.</summary>
    public bool HasPath
    {
        get => _data.HasPath;
        set 
        { 
            _data.HasPath = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a house connection.</summary>
    public bool HasHouse
    {
        get => _data.HasHouse;
        set 
        { 
            _data.HasHouse = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a stream.</summary>
    public bool HasStream
    {
        get => _data.HasStream;
        set 
        { 
            _data.HasStream = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a gully.</summary>
    public bool HasGully
    {
        get => _data.HasGully;
        set 
        { 
            _data.HasGully = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a canal.</summary>
    public bool HasCanal
    {
        get => _data.HasCanal;
        set 
        { 
            _data.HasCanal = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside is a rowhouse connection.</summary>
    public bool IsRowhouse
    {
        get => _data.IsRowhouse;
        set 
        { 
            _data.IsRowhouse = value; 
            if (value) HasHouse = true; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }

    /// <summary>Gets or sets a value indicating whether this hexside has a cliff.</summary>
    public bool HasCliff
    {
        get => _data.HasCliff;
        set 
        { 
            _data.HasCliff = value; 
            OnPropertyChanged(); 
            _onChanged?.Invoke();
        }
    }
}
