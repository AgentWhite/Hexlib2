namespace ASL.Counters;

public class Leader : BaseASLCounter
{
    public int Leadership { get; set; }

    public Leader(string name, int morale, int leadership, Nationality nationality)
    {
        Name = name;
        Morale = morale;
        Leadership = leadership;
        Nationality = nationality;
    }
}
