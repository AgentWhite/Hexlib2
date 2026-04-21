using System;

namespace ASL.Models.Scenarios;

/// <summary>
/// Represents a shared insignia or emblem used by a scenario side.
/// </summary>
public class Insignia
{
    /// <summary>
    /// Gets or sets the display name of the insignia.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file path to the insignia image.
    /// </summary>
    public string? ImagePath { get; set; }
}
