using ASL;
using Xunit;

namespace ASL.Tests;

public class ScenarioTests
{
    [Fact]
    public void Player_CanBeCreatedWithName()
    {
        var player = new Player("John Doe");
        Assert.Equal("John Doe", player.Name);
    }

    [Fact]
    public void Scenario_CanBeFullyPopulated()
    {
        // Data from user example
        string name = "FIGHTING WITHDRAWAL";
        string reference = "ASL SCENARIO 1";
        string place = "SESTRORETSK ROAD, TERIJOKI";
        string date = "September 2nd, 1941";
        string historicalDesc = "The Finns, seeking restitution for the Winter War of 1939, had erupted across the borders and breached the Soviet Karelian Front even as the crisis to the south of Leningrad came. ... if they could.";

        var desc = new ScenarioDescription(place, date, historicalDesc);
        var scenario = new Scenario { Name = name, Reference = reference, Description = desc };

        Assert.Equal(name, scenario.Name);
        Assert.Equal(reference, scenario.Reference);
        Assert.Equal(place, scenario.Description.Place);
        Assert.Equal(date, scenario.Description.Date);
        Assert.Equal(historicalDesc, scenario.Description.DescriptionText);
    }

    [Fact]
    public void ASLDateParser_HandlesOrdinalSuffixes()
    {
        string dateText = "September 2nd, 1941";
        DateTime? parsed = ASLDateParser.Parse(dateText);

        Assert.NotNull(parsed);
        Assert.Equal(new DateTime(1941, 9, 2), parsed.Value);
    }

    [Fact]
    public void ScenarioDescription_CanStoreTypedDate()
    {
        var desc = new ScenarioDescription("Place", "Date", "Desc");
        desc.PreciseDate = ASLDateParser.Parse("September 2nd, 1941");

        Assert.Equal(new DateTime(1941, 9, 2), desc.PreciseDate!.Value);
    }
}
