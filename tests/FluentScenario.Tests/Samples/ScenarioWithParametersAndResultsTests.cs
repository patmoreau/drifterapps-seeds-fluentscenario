namespace FluentScenario.Tests.Samples;

[UnitTest]
public class ScenarioWithParametersAndResultsTests(ITestOutputHelper testOutputHelper)
{
    private readonly IScenarioOutput _scenarioOutput = new SampleScenarioOutput(testOutputHelper);

    [Theory]
    [InlineData(19)]
    public async Task ScenarioWithParameters(int inside)
    {
        await ScenarioRunner.Create("when the weather is too cold", inside, _scenarioOutput)
            .Given("I want to go play outside", (Ensure<int> temperatureInside) =>
            {
                temperatureInside.Should().BeValid(); // ensure the temperature is valid
                return (temperatureInside.Value, -5);
            })
            .When("the temperature difference is great", (Ensure<(int inside, int outside)> temperatures) =>
            {
                temperatures.Should().BeValid(); // ensure the temperatures are valid
                return temperatures.Value.inside - temperatures.Value.outside;
            })
            .Then("I stay inside if the temperature difference is greater than 20C",
                (Ensure<int> temperatureDifference) =>
                {
                    temperatureDifference.Should().BeValid().And.Subject.Value.Should().BeGreaterThan(20);
                })
            .PlayAsync();
    }
}
