namespace ASL.Infrastructure;

/// <summary>
/// Represents a single dice roll entry in the journal.
/// </summary>
/// <param name="Type">The description or type of the roll.</param>
/// <param name="ColoredDie">The value of the colored die (or the single die value).</param>
/// <param name="WhiteDie">The value of the white die, if applicable.</param>
public record DiceEntry(string Type, int ColoredDie, int? WhiteDie = null);