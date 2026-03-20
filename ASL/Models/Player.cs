namespace ASL.Models;

/// <summary>
/// Represents a participant in an ASL game.
/// </summary>
public class Player
{
    /// <summary>
    /// Gets or sets the player's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Player"/> class.
    /// </summary>
    /// <param name="name">The name of the player.</param>
    public Player(string name)
    {
        Name = name;
    }

    /// <summary>
    /// Gets or sets the scenario side for the player. Null means that
    /// no side has been picked for this player. 
    /// </summary>
    public ScenarioSide? ScenarioSide { get; set; }  
    
}
