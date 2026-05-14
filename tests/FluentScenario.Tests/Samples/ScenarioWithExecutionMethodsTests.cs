namespace FluentScenario.Tests.Samples;

[UnitTest]
public class ScenarioWithExecutionMethodsTests(ITestOutputHelper testOutputHelper)
{
    private readonly IScenarioOutput _scenarioOutput = new SampleScenarioOutput(testOutputHelper);

    [Fact]
    public async Task ScenarioWithExecutionMethods()
    {
        await ScenarioRunner.Create("when the weather is too cold", _scenarioOutput)
            .Given(PlayOutsideMethod)
            .When(TemperatureBelow0CMethod)
            .Then(StayInsideMethod)
            .PlayAsync();
    }

    private static void PlayOutsideMethod(IStepRunner runner)
    {
        runner.Execute("I want to go play outside", () =>
        {
            runner.SetContextData("temperatureInside", 19);
        });
    }

    private static void TemperatureBelow0CMethod(IStepRunner runner)
    {
        runner.Execute("the temperature is below 0C", () =>
        {
            runner.SetContextData("temperatureOutside", -5);
        });
    }

    private static void StayInsideMethod(IStepRunner runner)
    {
        runner.Execute("I stay inside if the temperature difference is greater than 20C", () =>
        {
            var temperatureInside = runner.GetContextData<int>("temperatureInside");
            var temperatureOutside = runner.GetContextData<int>("temperatureOutside");
            (temperatureInside - temperatureOutside).Should().BeGreaterThan(20);
        });
    }
}
