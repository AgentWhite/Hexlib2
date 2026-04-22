using ASL.Models.Units;
using ASL.Models.Board;

namespace ASL.Models.Contexts;

/// <summary>
/// Represents the context for a unit movement action.
/// </summary>
public class MovementContext
{
    /// <summary>Gets or sets the unit currently performing movement.</summary>
    public required Unit MovingUnit { get; set; }
    /// <summary>Gets or sets the starting location of the movement.</summary>
    public required Location Origin { get; set; }
    /// <summary>Gets or sets the target location of the movement.</summary>
    public required Location Destination { get; set; }
    /// <summary>
    /// The hexside being passed if any. 
    /// </summary>
    public ASLEdgeData? CrossedHexside { get; set; }
    /// <summary>
    /// Final movement cost of impulse.
    /// </summary>
    public int FinalCost { get; set; }
}