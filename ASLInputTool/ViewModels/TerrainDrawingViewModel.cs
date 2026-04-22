using System.Windows.Media;
using ASL.Models.Board;

namespace ASLInputTool.ViewModels;

/// <summary>
/// Represents a custom-drawn terrain area on the map.
/// </summary>
public class TerrainDrawingViewModel : ViewModelBase
{
    private TerrainType _terrainType;
    private Geometry _geometry = Geometry.Empty;
    private bool _isSelected;
    private Brush _fill = Brushes.Transparent;

    /// <summary>
    /// Gets or sets a value indicating whether this drawing is currently selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Gets or sets the type of terrain represented by this drawing.
    /// </summary>
    public TerrainType TerrainType
    {
        get => _terrainType;
        set => SetProperty(ref _terrainType, value);
    }

    /// <summary>
    /// Gets or sets the combined geometry of all merged shapes for this terrain.
    /// </summary>
    public Geometry Geometry
    {
        get => _geometry;
        set => SetProperty(ref _geometry, value);
    }

    /// <summary>
    /// Gets or sets the fill brush for the drawing.
    /// </summary>
    public Brush Fill
    {
        get => _fill;
        set => SetProperty(ref _fill, value);
    }
    /// <summary>
    /// Converts this drawing to a DTO for persistence.
    /// </summary>
    public ASL.Persistence.TerrainDrawingDto ToDto()
    {
        return new ASL.Persistence.TerrainDrawingDto
        {
            TerrainType = TerrainType,
            // Use InvariantCulture to ensure '.' is always the decimal separator in the SVG string
            PathData = Geometry.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Creates a ViewModel from a DTO.
    /// </summary>
    /// <param name="dto">The terrain drawing DTO.</param>
    /// <param name="fill">The fill brush to apply.</param>
    /// <returns>A new <see cref="TerrainDrawingViewModel"/> instance.</returns>
    public static TerrainDrawingViewModel FromDto(ASL.Persistence.TerrainDrawingDto dto, Brush fill)
    {
        // SVG path syntax expects ' ' or ',' as delimiters. 
        // Some locales produce ';' for points or ',' for decimals.
        // We normalize ';' to ' ' and ensure decimals use '.' (though Geometry.Parse usually handles culture if not specified, 
        // but we want the most robust SVG-compliant string).
        
        string data = dto.PathData.Replace(';', ' ');
        // If the string was saved with commas as decimal separators (e.g. 536,144) 
        // but no semicolons were present, we have a problem because SVG uses commas for point coordinates.
        // However, standard Geometry.Parse should handle the system culture.
        // To be safe and forward-compatible with Invariant strings, we attempt to parse.
        
        try
        {
            return new TerrainDrawingViewModel
            {
                TerrainType = dto.TerrainType,
                Geometry = Geometry.Parse(data),
                Fill = fill
            };
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parsing LOS path data: {ex.Message}\nData: {data}");
            throw; // Re-throw to be caught by LoadLosData
        }
    }
}
