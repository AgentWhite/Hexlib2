namespace ASL.Counters;

public class HalfSquad : MultiManCounter
{
    public HalfSquad(string name, int fp, int range, int morale, UnitClass unitClass, Nationality nationality) 
        : base(fp, range, morale, name, unitClass, nationality)
    {
    }
}
