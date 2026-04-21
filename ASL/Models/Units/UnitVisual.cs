namespace ASL.Models.Units;

/// <summary>
/// Represents the visual assets associated with a unit counter, including traditional bitmaps and dynamic SVGs.
/// </summary>
public class UnitVisual
{
    /// <summary>
    /// Gets or sets the file path to the front image of the unit's counter.
    /// </summary>
    public string? ImagePathFront { get; set; }

    /// <summary>
    /// Gets or sets the file path to the back image of the unit's counter.
    /// </summary>
    public string? ImagePathBack { get; set; }

    /// <summary>
    /// Gets or sets the SVG content or reference for the front of the unit's counter.
    /// </summary>
    public string? SvgFront { get; set; }

    /// <summary>
    /// Gets or sets the SVG content or reference for the back of the unit's counter.
    /// </summary>
    public string? SvgBack { get; set; }
}
