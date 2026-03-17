namespace ASL;

/// <summary>
/// Represents the starting state and definition of an ASL scenario.
/// </summary>
public class Scenario
{
    public string Name { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public ScenarioDescription Description { get; set; }

    public Scenario(string name, string reference, ScenarioDescription description)
    {
        Name = name;
        Reference = reference;
        Description = description;
    }
}
