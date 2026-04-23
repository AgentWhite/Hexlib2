namespace ASL.Rules;

/// <summary>
/// Defines the execution priority of a rule within a pipeline.
/// Lower numeric values run first.
/// </summary>
public enum RulePriority
{
    /// <summary>Core rules that always apply.</summary>
    Core = 0,

    /// <summary>Scenario Special Rules that override or extend Core.</summary>
    SSR = 100
}
