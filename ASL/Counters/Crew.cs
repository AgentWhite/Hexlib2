namespace ASL.Counters;

public class Crew : MultiManCounter
{
    /// <summary>
    /// A crew is always Elite class.
    /// </summary>
    public override UnitClass Class 
    { 
        get => UnitClass.Elite; 
        set { /* Ignore attempts to set class for a crew */ } 
    }

    public Crew(string name, int firepower, int range, int morale, Nationality nationality) 
        : base(name, firepower, range, morale, nationality)
    {
    }
}
