# <img alt='paper plane icons' src='./icon.png' height='10%' width='10%'> Fluent Scenario

FluentScenario is a C# library that provides a fluent BDD scenario testing framework.

[![Build and Publish NuGet Package](https://github.com/patmoreau/drifterapps-seeds-fluentscenario/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/patmoreau/drifterapps-seeds-fluentscenario/actions/workflows/ci-cd.yml)
[![CodeQL](https://github.com/patmoreau/drifterapps-seeds-fluentscenario/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/patmoreau/drifterapps-seeds-fluentscenario/actions/workflows/codeql-analysis.yml)
![Tests results](https://gist.githubusercontent.com/patmoreau/51a2fc9fd8b7ed500ed3b6aabe0fc2d6/raw/seeds-fluentscenario-tests-badge-net10.0.svg)

## Table of Contents

- [Installation](#installation)
- [Usage](#usage)
  - [Scenario basics](#scenario-basics)
  - [Scenario with execution methods](#scenario-with-execution-methods)
  - [Scenario with well named methods](#scenario-with-well-named-methods)
  - [Scenario with parameters and results](#scenario-with-parameters-and-results)
- [Contributing](#contributing)
- [License](#license)

## Installation

To install FluentScenario, you can use the NuGet package manager:

```sh
dotnet add package DrifterApps.Seeds.FluentScenario
```

If you use [FluentAssertions](https://fluentassertions.com) to assert your tests. You can use the NuGet package manager:

```sh
dotnet add package DrifterApps.Seeds.FluentScenario.FluentAssertions
```

## Usage

### Scenario basics

Here is a basic test scenario [ScenarioBasicsTests](./tests/FluentScenario.Tests/Samples/ScenarioBasicsTests.cs):

```csharp
[Fact]
public async Task SimpleScenario()
{
    int temperatureInside = 0;
    int temperatureOutside = 0;

    await ScenarioRunner.Create("when the weather is too cold", _scenarioOutput)
        .Given("I want to go play outside", () => temperatureInside = 19)
        .When("the temperature is below 0C", () => temperatureOutside = -5)
        .Then("I stay inside if the temperature difference is greater than 20C", () => (temperatureInside - temperatureOutside).Should().BeGreaterThan(20))
        .PlayAsync();
}
```

This will produce this output:

```console
✓ SCENARIO for when the weather is too cold
✓ GIVEN I want to go play outside
✓ WHEN the temperature is below 0C
✓ THEN I stay inside if the temperature difference is greater than 20C
```

### Scenario with execution methods

For scenarios where some steps could be shared between multiple scenarions, you can create a set of execution methods for those common steps [ScenarioWithExecutionMethodsTests](./tests/FluentScenario.Tests/Samples/ScenarioWithExecutionMethodsTests.cs):

```csharp
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
```

This will produce this output:

```console
✓ SCENARIO for when the weather is too cold
✓ GIVEN I want to go play outside
✓ WHEN the temperature is below 0C
✓ THEN I stay inside if the temperature difference is greater than 20C
```

### Scenario with well named methods

Some of us like to properly name our methods so they are clear to the reader what the intent of the method is.

In this case, you can forgo a description and the ScenarioRunner will use the name of your calling methods to create the description [ScenarioWithWellNamedMethodsTests](./tests/FluentScenario.Tests/Samples/ScenarioWithWellNamedMethodsTests.cs):

```csharp
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
```

This will produce this output:

```console
✓ SCENARIO for When The Weather Is Too Cold
✓ GIVEN I Want To Go Play Outside
✓ WHEN The Temperature Is Below0 C
✓ THEN I Stay Inside If The Temperature Difference Is Greater Than20 C
```

### Scenario with parameters and results

To start a scenario, you can pass values in the Create method. And you can also pass values between steps.

Each values passed as a parameter is a [Enzure<>](/src/FluentScenario/Ensure.cs) value object. This value object is there to help ensuring that the value received is a valid value.

Here's an example [ScenarioWithParametersAndResultsTests](./tests/FluentScenario.Tests/Samples/ScenarioWithParametersAndResultsTests.cs):

```csharp
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
```

This will produce this output:

```console
✓ SCENARIO for when the weather is too cold
✓ GIVEN I want to go play outside
✓ WHEN the temperature difference is great
✓ THEN I stay inside if the temperature difference is greater than 20C
```

## Contributing

Contributions are welcome! Please read the contributing guidelines for more information.

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.
