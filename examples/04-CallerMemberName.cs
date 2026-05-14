// Pattern 3: CallerMemberName — method names become step descriptions
//
// Best for: self-documenting tests where the method name clearly expresses intent.
// The framework converts PascalCase method names to sentences automatically.
//
// Naming convention: name methods as complete readable sentences in PascalCase.
// Example: GivenAnActiveUserExists → "given an active user exists"

using DrifterApps.Seeds.FluentScenario;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Examples;

public class CallerMemberNamePattern(ITestOutputHelper output)
{
    private readonly IScenarioOutput _output = new XUnitScenarioOutput(output);

    // The test method name becomes the scenario description when no string is passed.
    // WhenTheWeatherIsTooCold → "When The Weather Is Too Cold"
    [Fact]
    public async Task WhenTheWeatherIsTooCold()
    {
        // ScenarioRunner.Create(_output) — no description: method name is used
        await ScenarioRunner.Create(_output)
            .Given(IWantToGoPlayOutside)
            .When(TheTemperatureIsBelow0C)
            .Then(IStayInsideIfTheDifferenceIsGreaterThan20Degrees)
            .PlayAsync();
    }

    // Output:
    // ✓ SCENARIO for When The Weather Is Too Cold
    // ✓ GIVEN I Want To Go Play Outside
    // ✓ WHEN The Temperature Is Below0 C
    // ✓ THEN I Stay Inside If The Difference Is Greater Than20 Degrees

    // Note: numbers attached to letters produce slightly odd spacing.
    // For precision, pass an explicit description string instead.

    // --- Step methods (no description string — CallerMemberName used) --------

    private static void IWantToGoPlayOutside(IStepRunner runner) =>
        runner.Execute(() =>                           // method name "IWantToGoPlayOutside" → description
            runner.SetContextData("temperatureInside", 19));

    private static void TheTemperatureIsBelow0C(IStepRunner runner) =>
        runner.Execute(() =>
            runner.SetContextData("temperatureOutside", -5));

    private static void IStayInsideIfTheDifferenceIsGreaterThan20Degrees(IStepRunner runner) =>
        runner.Execute(() =>
        {
            var inside = runner.GetContextData<int>("temperatureInside");
            var outside = runner.GetContextData<int>("temperatureOutside");
            (inside - outside).Should().BeGreaterThan(20);
        });

    // --- Mixing CallerMemberName with explicit descriptions ------------------

    // When a method name is slightly awkward (e.g. contains numbers), use an explicit
    // description for that step only while keeping CallerMemberName for others.
    [Fact]
    public async Task TemperatureCheck()
    {
        await ScenarioRunner.Create(_output)
            .Given(IWantToGoPlayOutside)
            .When("the temperature is below 0°C",   // explicit to avoid number formatting quirk
                async () =>
                {
                    await Task.CompletedTask;        // step methods can also be inline
                })
            .Then(IStayInsideIfTheDifferenceIsGreaterThan20Degrees)
            .PlayAsync();
    }

    // --- Async step methods with CallerMemberName ----------------------------

    [Fact]
    public async Task AnOrderIsPlacedSuccessfully()
    {
        await ScenarioRunner.Create(_output)
            .Given(AnOrderIsCreated)
            .When(TheOrderIsSubmitted)
            .Then(TheOrderStatusIsConfirmed)
            .PlayAsync();
    }

    private static async Task AnOrderIsCreated(IStepRunner runner) =>
        await runner.Execute(async () =>
        {
            await Task.Delay(0);                     // simulate async setup
            runner.SetContextData("orderId", Guid.NewGuid());
        });

    private static async Task TheOrderIsSubmitted(IStepRunner runner) =>
        await runner.Execute(async () =>
        {
            await Task.Delay(0);                     // simulate async action
            runner.SetContextData("orderStatus", "Submitted");
        });

    private static void TheOrderStatusIsConfirmed(IStepRunner runner) =>
        runner.Execute(() =>
        {
            var status = runner.GetContextData<string>("orderStatus");
            status.Should().Be("Submitted");
        });
}
