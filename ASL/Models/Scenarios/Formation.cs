namespace ASL.Models.Scenarios;

/// <summary>
/// Represents a combat formation or organizational unit within a scenario side.
/// </summary>
public class Formation
{
    /// <summary>
    /// Gets or sets the name or designation of the formation.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Experience Level Rating (ELR) for this formation as specified by the scenario.
    /// When an Original Morale Check DR exceeds (unit Morale + ELR), units subject to ELR are replaced
    /// with a lower-quality counter. Typical scenario values are 0–5.
    /// </summary>
    public int ELR { get; set; }
}
