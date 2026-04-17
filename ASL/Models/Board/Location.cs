using System.Collections.Generic;
using ASL.Models.Units;

namespace ASL.Models.Board;
public class Location
{
    public int Level { get; set; } = 0;
    public LocationFeatures Features { get; set; }
    public List<Unit> Units { get; set; } = new();





}
