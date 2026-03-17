namespace ASL.Counters;

public class Squad : MultiManCounter
{
    public Squad(string name, int firepower, int range, int morale, Nationality nationality) 
        : base(name, firepower, range, morale, nationality)
    {
    }
}
