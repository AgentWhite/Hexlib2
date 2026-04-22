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

    /// <summary>
    /// Updates the Unit Code text element within the front SVG content.
    /// Expects a text element with id="unit-code-overlay".
    /// </summary>
    /// <param name="code">The code to inject.</param>
    public void SetUnitCode(string code)
    {
        if (string.IsNullOrEmpty(SvgFront)) return;

        // Targeted replacement for the tagged unit code element
        // Look for the id and the subsequent > closing the open tag
        string targetId = "id=\"unit-code-overlay\"";
        int idIndex = SvgFront.IndexOf(targetId);
        if (idIndex == -1) return;

        int closeTagIndex = SvgFront.IndexOf(">", idIndex);
        if (closeTagIndex == -1) return;

        int endTextIndex = SvgFront.IndexOf("</text>", closeTagIndex);
        if (endTextIndex == -1) return;

        // Replace the content between > and </text>
        string newSvg = SvgFront.Substring(0, closeTagIndex + 1) + 
                        code + 
                        SvgFront.Substring(endTextIndex);
        
        SvgFront = newSvg;
    }
}
