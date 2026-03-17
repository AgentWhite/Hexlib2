using HexLib;

namespace ASL.Counters;

public abstract class BaseASLCounter : ICounter
{
    public string Name { get; protected set; } = string.Empty;
    public int Morale { get; set; }
    public Nationality Nationality { get; set; }
}
