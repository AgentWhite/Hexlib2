namespace ASL.Counters;

public class Squad : MultiManCounter
{
    public Squad(string name, int fp, int range, int morale, UnitClass unitClass, Nationality nationality) 
        : base(fp, range, morale, name, unitClass, nationality)
    {
    }
}
