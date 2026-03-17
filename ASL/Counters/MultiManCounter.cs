namespace ASL.Counters;

public abstract class MultiManCounter : BaseASLCounter
{
    public int Firepower { get; set; }
    public int Range { get; set; }
    public string Identity { get; set; } = string.Empty;
    public virtual UnitClass Class { get; set; }
    
    public bool HasAssaultFire { get; set; }
    public bool HasSprayingFire { get; set; }
    public bool HasELR { get; set; }
    public int BrokenMoraleLevel { get; set; }
    public bool CanSelfRally { get; set; }
    public bool HasSmokeExponent { get; set; }
    public int SmokePlacementExponent { get; set; }
    public int BPV { get; set; }

    protected MultiManCounter(string name, int firepower, int range, int morale, Nationality nationality)
    {
        Name = name;
        Firepower = firepower;
        Range = range;
        Morale = morale;
        Nationality = nationality;
    }
}
