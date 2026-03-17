namespace ASL.Counters;

public class HalfSquad : MultiManCounter
{
    public HalfSquad(string name, int firepower, int range, int morale, Nationality nationality) 
        : base(name, firepower, range, morale, nationality)
    {
    }
}
