namespace ASL;

/// <summary>
/// Represents a participant in an ASL game.
/// </summary>
public class Player
{
    public string Name { get; set; } = string.Empty;

    public Player(string name)
    {
        Name = name;
    }
}
