using System;

using System;

namespace ASL.Models.Scenarios;

/// <summary>
/// Contains the historical context of a scenario, divided into location, date, and description.
/// </summary>
public class ScenarioDescription
{
    /// <summary>
    /// Gets or sets the geographical location where the scenario takes place.
    /// </summary>
    public string Place { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the textual representation of the date (e.g., "May 1940").
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a specific <see cref="DateTime"/> for rules processing if applicable.
    /// </summary>
    public DateTime? PreciseDate { get; set; }

    /// <summary>
    /// Gets or sets the detailed historical narrative of the events.
    /// </summary>
    public string DescriptionText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the textual description of the historical outcome/aftermath.
    /// </summary>
    public string Aftermath { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioDescription"/> class.
    /// </summary>
    public ScenarioDescription() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScenarioDescription"/> class with specified values.
    /// </summary>
    public ScenarioDescription(string place, string date, string descriptionText, string aftermath = "", DateTime? preciseDate = null)
    {
        Place = place;
        Date = date;
        DescriptionText = descriptionText;
        Aftermath = aftermath;
        PreciseDate = preciseDate;
    }
}
