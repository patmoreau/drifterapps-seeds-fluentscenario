// Pattern 2: Reusable IStepRunner methods
//
// Best for: steps that are shared across multiple tests.
// State is shared via SetContextData / GetContextData<T> on IRunnerContext.

using DrifterApps.Seeds.FluentScenario;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Examples;

public class ReusableSteps(ITestOutputHelper output)
{
    private readonly IScenarioOutput _output = new XUnitScenarioOutput(output);

    // --- Tests ---------------------------------------------------------------

    [Fact]
    public async Task WeatherScenario_TooCold()
    {
        await ScenarioRunner.Create("when the weather is too cold", _output)
            .Given(IWantToGoPlayOutside)
            .When(TemperatureBelow0C)
            .Then(IStayInside)
            .PlayAsync();
    }

    [Fact]
    public async Task WeatherScenario_NiceDay()
    {
        await ScenarioRunner.Create("when the weather is nice", _output)
            .Given(IWantToGoPlayOutside)
            .When(TemperatureAbove15C)
            .Then(IGoOutside)
            .PlayAsync();
    }

    // --- Reusable steps ------------------------------------------------------

    // Shared setup step — used by both tests above
    private static void IWantToGoPlayOutside(IStepRunner runner) =>
        runner.Execute("I want to go play outside", () =>
            runner.SetContextData("temperatureInside", 19));

    private static void TemperatureBelow0C(IStepRunner runner) =>
        runner.Execute("the temperature is below 0C", () =>
            runner.SetContextData("temperatureOutside", -5));

    private static void TemperatureAbove15C(IStepRunner runner) =>
        runner.Execute("the temperature is above 15C", () =>
            runner.SetContextData("temperatureOutside", 18));

    private static void IStayInside(IStepRunner runner) =>
        runner.Execute("I stay inside if the difference is greater than 20C", () =>
        {
            var inside = runner.GetContextData<int>("temperatureInside");
            var outside = runner.GetContextData<int>("temperatureOutside");
            (inside - outside).Should().BeGreaterThan(20);
        });

    private static void IGoOutside(IStepRunner runner) =>
        runner.Execute("I go outside if the difference is less than 5C", () =>
        {
            var inside = runner.GetContextData<int>("temperatureInside");
            var outside = runner.GetContextData<int>("temperatureOutside");
            Math.Abs(inside - outside).Should().BeLessThan(5);
        });
}

// --- Shared step library (cross-class reuse) ---------------------------------

// Put steps used across multiple test classes in a static class.
// Steps receive IStepRunner directly and manage their own context keys.
public static class CommonSteps
{
    public static void AnAuthenticatedUser(IStepRunner runner) =>
        runner.Execute("an authenticated user", () =>
        {
            runner.SetContextData("userId", Guid.NewGuid());
            runner.SetContextData("isAuthenticated", true);
        });

    public static void TheUserIsLoggedOut(IStepRunner runner) =>
        runner.Execute("the user is logged out", () =>
        {
            runner.SetContextData("isAuthenticated", false);
        });

    public static void TheSessionIsInvalid(IStepRunner runner) =>
        runner.Execute("the session is no longer valid", () =>
        {
            var isAuthenticated = runner.GetContextData<bool>("isAuthenticated");
            isAuthenticated.Should().BeFalse();
        });
}
