using HexLib;

namespace ASL.Counters;

public abstract class BaseASLCounter : ICounter
{
    public string Name { get; protected set; } = string.Empty;
    public int Morale { get; set; }
    public Nationality Nationality { get; set; }
    public abstract string Stats { get; }

    protected BaseASLCounter(string name, int morale, Nationality nationality)
    {
        Name = name;
        Morale = morale;
        Nationality = nationality;
    }
}
