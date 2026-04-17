using ASL.Models.Units;
using ASL.Models.Board;

namespace ASL.Models.Contexts;

public class MovementContext
{
    public Unit MovingUnit { get; set; }
    public Location Origin { get; set; }
    public Location Destination { get; set; }
    /// <summary>
    /// The hexside being passed if any. 
    /// </summary>
    public ASLEdgeData? CrossedHexside { get; set; }
    /// <summary>
    /// Final movement cost of impulse.
    /// </summary>
    public int FinalCost { get; set; }
}