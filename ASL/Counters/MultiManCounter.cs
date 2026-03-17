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

    public override string Stats => $"FP: {Firepower}, Range: {Range}, Morale: {Morale}";

    protected MultiManCounter(int fp, int range, int morale, string identity, UnitClass @class, Nationality nationality) 
        : base(identity, morale, nationality)
    {
        Firepower = fp;
        Range = range;
        Identity = identity;
        Class = @class;
    }
}
