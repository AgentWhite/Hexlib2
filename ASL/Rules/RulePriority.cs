namespace ASL.Rules;

/// <summary>
/// Defines the execution priority of a rule within a pipeline.
/// Lower numeric values run first.
/// </summary>
public enum RulePriority
{
    /// <summary>The "physics" of the game — foundational rules that always apply (e.g., A4.1, B13.1).</summary>
    Baseline = 100,

    /// <summary>Optional core rules that may be in effect (Night, Weather, Slopes).</summary>
    OptionalCore = 150,

    /// <summary>Scenario Specific Rules from the scenario card.</summary>
    ScenarioSet = 200,

    /// <summary>Rules that modify the output of earlier rules (multipliers, add-ons).</summary>
    Modifier = 300,

    /// <summary>The final word — hard inhibitors or aborts that override everything else.</summary>
    FinalOverride = 400
}
