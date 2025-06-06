using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace DrifterApps.Seeds.FluentScenario;

/// <summary>
///     Represents a runner for executing test scenarios.
/// </summary>
public sealed partial class ScenarioRunner : IScenarioRunner, IStepRunner
{
    internal const string SuccessCheck = "\u001b[32m\u2713\u001b[0m";
    internal const string FailCheck = "\u001b[31m\u2717\u001b[0m";

    private readonly Dictionary<string, object> _context = [];
    private readonly List<StepDefinition> _steps = [];
    private readonly IScenarioOutput _scenarioOutput;
    private string _stepCommand = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScenarioRunner" /> class.
    /// </summary>
    /// <param name="description">The description of the scenario.</param>
    /// <param name="scenarioOutput">The test output helper.</param>
    /// <exception cref="ArgumentNullException">Thrown when description or testOutputHelper is null.</exception>
    private ScenarioRunner(string description, IScenarioOutput scenarioOutput)
    {
        ValidateDescription(description);

        ArgumentNullException.ThrowIfNull(scenarioOutput);

        _steps.Add(StepDefinition.Create("Scenario", $"SCENARIO for {description}", () => { }));

        _scenarioOutput = scenarioOutput;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ScenarioRunner" /> class.
    /// </summary>
    /// <param name="description">The description of the scenario.</param>
    /// <param name="input">Input value to start the scenario</param>
    /// <param name="scenarioOutput">The test output helper.</param>
    /// <exception cref="ArgumentNullException">Thrown when description or testOutputHelper is null.</exception>
    private ScenarioRunner(string description, object? input, IScenarioOutput scenarioOutput)
    {
        ValidateDescription(description);

        ArgumentNullException.ThrowIfNull(scenarioOutput);

        _steps.Add(StepDefinition.Create("Scenario", $"SCENARIO for {description}", () => input));

        _scenarioOutput = scenarioOutput;
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentNullException(nameof(description),
                "Please explain your intent by documenting your scenario.");
        }
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ScenarioRunner" /> class.
    /// </summary>
    /// <param name="description">The description of the scenario.</param>
    /// <param name="scenarioOutput">The test output helper.</param>
    /// <returns>A new instance of <see cref="ScenarioRunner" />.</returns>
    public static ScenarioRunner Create(string description, IScenarioOutput scenarioOutput)
        => new(description, scenarioOutput);

    /// <summary>
    ///     Creates a new instance of the <see cref="ScenarioRunner" /> class.
    /// </summary>
    /// <param name="scenarioOutput">The test output helper.</param>
    /// <param name="description">The description of the scenario.</param>
    /// <returns>A new instance of <see cref="ScenarioRunner" />.</returns>
    public static ScenarioRunner Create(IScenarioOutput scenarioOutput, [CallerMemberName] string description = "")
        => new(CamelToSentence(description), scenarioOutput);

    /// <summary>
    ///     Creates a new instance of the <see cref="ScenarioRunner" /> class.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter for the scenario.</typeparam>
    /// <param name="description">The description of the scenario.</param>
    /// <param name="input">Input value to start the scenario</param>
    /// <param name="scenarioOutput">The test output helper.</param>
    /// <returns>A new instance of <see cref="ScenarioRunner" />.</returns>
    public static ScenarioRunner Create<T>(string description, T input, IScenarioOutput scenarioOutput)
        => new(description, input, scenarioOutput);

    /// <summary>
    ///     Creates a new instance of the <see cref="ScenarioRunner" /> class.
    /// </summary>
    /// <typeparam name="T">The type of the input parameter for the scenario.</typeparam>
    /// <param name="scenarioOutput">The test output helper.</param>
    /// <param name="input">Input value to start the scenario</param>
    /// <param name="description">The description of the scenario.</param>
    /// <returns>A new instance of <see cref="ScenarioRunner" />.</returns>
    public static ScenarioRunner Create<T>(T input, IScenarioOutput scenarioOutput,
        [CallerMemberName] string description = "")
        => new(CamelToSentence(description), input, scenarioOutput);

    /// <inheritdoc/>
    public async Task PlayAsync() => _ = await PlayAsync<object?>().ConfigureAwait(false);

    /// <inheritdoc/>
    public async Task<Ensure<T>> PlayAsync<T>()
    {
        var steps = _steps.ToList();
        _steps.Clear();

        object? currentResult = null;

        foreach (var step in steps)
        {
            try
            {
                currentResult = await step.Step(currentResult).ConfigureAwait(false);
                _scenarioOutput.WriteLine($"{SuccessCheck} {step.Description}");
            }
            catch (Exception)
            {
                _scenarioOutput.WriteLine($"{FailCheck} {step.Description}");
                throw;
            }
        }

        return Ensure<T>.From(currentResult);
    }

    /// <summary>
    ///     Adds a step to the scenario.
    /// </summary>
    /// <param name="stepDefinition">The step definition.</param>
    private ScenarioRunner AddStep(StepDefinition stepDefinition)
    {
        var previousStep = _steps.LastOrDefault();
        var textCommand = stepDefinition.Command.Equals(previousStep?.Command, StringComparison.OrdinalIgnoreCase)
            ? "and"
            : stepDefinition.Command.ToUpperInvariant();
        var text = $"{textCommand} {stepDefinition.Description}";
        _steps.Add(stepDefinition with
        {
            Description = text
        });
        return this;
    }

    /// <summary>
    /// Converts a camel case string to a sentence.
    /// </summary>
    /// <param name="description">The camel case string to convert.</param>
    /// <returns>The converted sentence.</returns>
    private static string CamelToSentence(string description) => CamelToSentenceRegex()
        .Replace(description, m => m.Groups[1].Success ? " " + m.Groups[1].Value : "").Trim()
        .ToLower(CultureInfo.CurrentCulture);

    /// <summary>
    /// Regular expression to match camel case patterns.
    /// </summary>
    [GeneratedRegex("(?<!^)([A-Z]|[0-9].*)|_")]
    private static partial Regex CamelToSentenceRegex();
}
