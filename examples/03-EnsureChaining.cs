// Pattern 4: Ensure<T> return-value chaining
//
// Best for: transformation pipelines where each step produces a typed value
// consumed by the next step. Eliminates the need for context storage in
// linear workflows.
//
// Requires: DrifterApps.Seeds.FluentScenario.FluentAssertions for .Should().BeValid()

using DrifterApps.Seeds.FluentScenario;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Examples;

public class EnsureChaining(ITestOutputHelper output)
{
    private readonly IScenarioOutput _output = new XUnitScenarioOutput(output);

    // --- Basic chaining -------------------------------------------------------

    [Theory]
    [InlineData(19)]
    public async Task TemperatureScenario(int insideTemp)
    {
        // Pass the initial value via Create<T>. The first step receives it as Ensure<int>.
        await ScenarioRunner.Create("temperature comparison", insideTemp, _output)
            .Given("capture outside temperature",
                (Ensure<int> inside) =>
                {
                    inside.Should().BeValid();
                    return (Inside: inside.Value, Outside: -5);   // return tuple to next step
                })
            .When("calculate the difference",
                (Ensure<(int Inside, int Outside)> temps) =>
                {
                    temps.Should().BeValid();
                    return temps.Value.Inside - temps.Value.Outside;
                })
            .Then("the difference is greater than 20",
                (Ensure<int> difference) =>
                {
                    difference.Should().BeValid().And.Subject.Value.Should().BeGreaterThan(20);
                })
            .PlayAsync();
    }

    // --- Multi-step transformation pipeline -----------------------------------

    [Fact]
    public async Task OrderPricingPipeline()
    {
        const decimal basePrice = 100m;

        await ScenarioRunner.Create("order pricing pipeline", basePrice, _output)
            .Given<decimal, decimal>("a 10% discount is applied",
                (Ensure<decimal> price) =>
                {
                    price.Should().BeValid();
                    return price.Value * 0.9m;
                })
            .When<decimal, decimal>("VAT at 20% is added",
                (Ensure<decimal> discounted) =>
                {
                    discounted.Should().BeValid();
                    return discounted.Value * 1.2m;
                })
            .And<decimal, decimal>("shipping of 5.00 is added",
                (Ensure<decimal> withVat) =>
                {
                    withVat.Should().BeValid();
                    return withVat.Value + 5m;
                })
            .Then<decimal>("the final price is 113.00",
                (Ensure<decimal> final) =>
                {
                    final.Should().BeValid();
                    final.Value.Should().Be(113m);
                })
            .PlayAsync();
    }

    // --- PlayAsync<T> — returning the final result ---------------------------

    [Fact]
    public async Task CreateEntity_ReturnsNewId()
    {
        var userId = Guid.NewGuid();

        // PlayAsync<T> returns the final step's result for assertion outside the chain
        var result = await ScenarioRunner.Create("create and return an entity ID", _output)
            .Given<Guid>("a user ID is generated", () => userId)
            .When<Guid, string>("the ID is converted to string",
                (Ensure<Guid> id) =>
                {
                    id.Should().BeValid();
                    return id.Value.ToString("N");
                })
            .PlayAsync<string>();

        result.Should().BeValid();
        result.Value.Should().HaveLength(32);
        result.Value.Should().Be(userId.ToString("N"));
    }

    // --- Nullable Ensure<T> --------------------------------------------------

    [Fact]
    public async Task NullableValueHandling()
    {
        string? maybeNull = null;

        await ScenarioRunner.Create("nullable value handling", maybeNull, _output)
            .Given("a nullable string input",
                (Ensure<string?> input) =>
                {
                    // For nullable types, null is valid
                    input.Should().BeValid();
                    input.Value.Should().BeNull();
                    return input.Value ?? "default";
                })
            .Then("the fallback value is used",
                (Ensure<string> result) =>
                {
                    result.Should().BeValid();
                    result.Value.Should().Be("default");
                })
            .PlayAsync();
    }

    // --- Ensure<T> without FluentAssertions ----------------------------------

    [Fact]
    public async Task EnsureWithoutFluentAssertions()
    {
        await ScenarioRunner.Create("manual ensure validation", 42, _output)
            .Given<int, string>("value is converted to string",
                (Ensure<int> input) =>
                {
                    // Manual check without .Should().BeValid()
                    if (!input.IsValid)
                        throw new InvalidOperationException("Expected a valid integer input.");

                    return input.Value.ToString();
                })
            .Then<string>("result has length 2",
                (Ensure<string> result) =>
                {
                    if (!result.IsValid)
                        throw new InvalidOperationException("Expected a valid string result.");

                    result.Value.Should().HaveLength(2);
                })
            .PlayAsync();
    }
}
