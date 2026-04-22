using System.Collections.Generic;
using ASL.Models.Units;

namespace ASL.Models.Board;
/// <summary>
/// Represents a specific location (hex) on the ASL board with its features and occupants.
/// </summary>
public class Location
{
    /// <summary>Gets or sets the elevation level of this location.</summary>
    public int Level { get; set; } = 0;
    /// <summary>Gets or sets the combined terrain features of this location.</summary>
    public LocationFeatures Features { get; set; }
    /// <summary>Gets the list of units currently occupying this location.</summary>
    public List<Unit> Units { get; set; } = new();





}
