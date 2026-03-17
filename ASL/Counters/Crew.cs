namespace ASL.Counters;

public class Crew : MultiManCounter
{
    public Crew(string name, int fp, int range, int morale, Nationality nationality) 
        : base(fp, range, morale, name, UnitClass.Elite, nationality)
    {
    }
}
