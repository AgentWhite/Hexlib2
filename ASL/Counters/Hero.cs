namespace ASL.Counters;

public class Hero : BaseASLCounter
{
    public int Firepower { get; set; }
    public int Range { get; set; }

    public override string Stats => $"FP: {Firepower}, Range: {Range}, Morale: {Morale}";

    public Hero(string name, int fp, int range, int morale, Nationality nationality) : base(name, morale, nationality)
    {
        Firepower = fp;
        Range = range;
    }
}
