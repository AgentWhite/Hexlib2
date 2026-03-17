namespace ASL.Counters;

public class Hero : BaseASLCounter
{
    public int Firepower { get; set; }
    public int Range { get; set; }

    public Hero(string name, int firepower, int range, int morale, Nationality nationality)
    {
        Name = name;
        Firepower = firepower;
        Range = range;
        Morale = morale;
        Nationality = nationality;
    }
}
