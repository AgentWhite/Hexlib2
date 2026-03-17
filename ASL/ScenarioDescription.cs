namespace ASL;

/// <summary>
/// Contains the historical context and descriptive metadata for an ASL Scenario.
/// </summary>
public class ScenarioDescription
{
    public string Place { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public DateTime? DateValue { get; set; }
    public string HistoricalDescription { get; set; } = string.Empty;

    public ScenarioDescription(string place, string date, string historicalDescription, DateTime? dateValue = null)
    {
        Place = place;
        Date = date;
        DateValue = dateValue;
        HistoricalDescription = historicalDescription;
    }
}
