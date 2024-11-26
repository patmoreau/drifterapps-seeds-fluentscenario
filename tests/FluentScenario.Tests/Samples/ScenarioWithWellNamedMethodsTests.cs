using Xunit.Abstractions;

namespace FluentScenario.Tests.Samples;

[UnitTest]
public class ScenarioWithWellNamedMethodsTests(ITestOutputHelper testOutputHelper)
{
    private readonly IScenarioOutput _scenarioOutput = new SampleScenarioOutput(testOutputHelper);

    [Fact]
    public async Task WhenTheWeatherIsTooCold()
    {
        await ScenarioRunner.Create(_scenarioOutput)
            .Given(IWantToGoPlayOutside)
            .When(TheTemperatureIsBelow0C)
            .Then(IStayInsideIfTheTemperatureDifferenceIsGreaterThan20C)
            .PlayAsync();
    }

    private static void IWantToGoPlayOutside(IStepRunner runner)
    {
        runner.Execute(() =>
        {
            runner.SetContextData("temperatureInside", 19);
        });
    }

    private static void TheTemperatureIsBelow0C(IStepRunner runner)
    {
        runner.Execute(() =>
        {
            runner.SetContextData("temperatureOutside", -5);
        });
    }

    private static void IStayInsideIfTheTemperatureDifferenceIsGreaterThan20C(IStepRunner runner)
    {
        runner.Execute(() =>
        {
            var temperatureInside = runner.GetContextData<int>("temperatureInside");
            var temperatureOutside = runner.GetContextData<int>("temperatureOutside");
            (temperatureInside - temperatureOutside).Should().BeGreaterThan(20);
        });
    }
}
