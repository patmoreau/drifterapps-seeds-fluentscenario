// Pattern 1: Inline lambdas with explicit descriptions
//
// Best for: quick, one-off scenarios where steps won't be reused.
// State is shared via captured local variables.

using DrifterApps.Seeds.FluentScenario;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Examples;

public class BasicScenario(ITestOutputHelper output)
{
    private readonly IScenarioOutput _output = new XUnitScenarioOutput(output);

    [Fact]
    public async Task SimpleWeatherDecision()
    {
        var temperatureInside = 0;
        var temperatureOutside = 0;

        await ScenarioRunner.Create("when the weather is too cold", _output)
            .Given("I want to go play outside", () => temperatureInside = 19)
            .When("the temperature is below 0C", () => temperatureOutside = -5)
            .Then("I stay inside if the difference is greater than 20C",
                () => (temperatureInside - temperatureOutside).Should().BeGreaterThan(20))
            .PlayAsync();
    }

    [Fact]
    public async Task ScenarioWithAndKeyword()
    {
        var total = 0;

        // And inherits the role of the preceding keyword:
        // Given ... And ... → both are Given steps
        // When  ... And ... → both are When steps
        // Then  ... And ... → both are Then steps
        await ScenarioRunner.Create("order total calculation", _output)
            .Given("a base price of 100", () => total = 100)
            .And("a discount of 10 is applied", () => total -= 10)
            .When("VAT at 20% is added", () => total = (int)(total * 1.2m))
            .Then("the total is 108", () => total.Should().Be(108))
            .And("the total is greater than 100", () => total.Should().BeGreaterThan(100))
            .PlayAsync();
    }

    [Fact]
    public async Task AsyncSteps()
    {
        string? data = null;

        await ScenarioRunner.Create("async data loading", _output)
            .Given("data is loaded asynchronously", async () =>
            {
                data = await Task.FromResult("loaded-data");
            })
            .Then("the data is available", () =>
            {
                data.Should().Be("loaded-data");
            })
            .PlayAsync();
    }
}

// Minimal IScenarioOutput adapter for xUnit.
// Create once per project (e.g. in a shared test helper class).
public sealed class XUnitScenarioOutput(ITestOutputHelper output) : IScenarioOutput
{
    public void WriteLine(string message) => output.WriteLine(message);
    public void WriteLine(string format, params object[] args) => output.WriteLine(format, args);
}
