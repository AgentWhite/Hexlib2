namespace HexLib;

/// <summary>
/// Represents a generic unit, marker, or token placed on a Hex in the wargaming simulation.
/// </summary>
public interface ICounter
{
    /// <summary>
    /// The display name or unique identifier of the counter.
    /// </summary>
    string Name { get; }
}
