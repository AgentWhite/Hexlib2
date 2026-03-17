namespace ASL.Counters;

public class Leader : BaseASLCounter
{
    public int Leadership { get; set; }

    public override string Stats => $"Morale: {Morale}, Leadership: {Leadership}";

    public Leader(string name, int morale, int leadership, Nationality nationality) : base(name, morale, nationality)
    {
        Leadership = leadership;
    }
}
