// Integration patterns for real-world test scenarios.
//
// Demonstrates:
// - HTTP integration tests with typed result chaining
// - Async context setup with IStepRunner
// - Shared step libraries across test classes
// - PlayAsync<T> to return the scenario result

using System.Net;
using System.Net.Http.Json;
using DrifterApps.Seeds.FluentScenario;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Examples;

// ---------------------------------------------------------------------------
// HTTP API integration test — Ensure<T> chaining with HttpResponseMessage
// ---------------------------------------------------------------------------

// Assumes a WebApplicationFactory<TEntryPoint> is injected as _factory.
// Replace _factory.CreateClient() with your actual HTTP client setup.
public class ApiIntegrationTests(ITestOutputHelper output)
{
    private readonly IScenarioOutput _output = new XUnitScenarioOutput(output);

    [Fact]
    public async Task CreateProduct_Returns201WithLocation()
    {
        await ScenarioRunner.Create("create a product via API", _output)
            .Given<HttpClient>("an HTTP client is configured", () =>
                new HttpClient { BaseAddress = new Uri("https://api.example.com") })
            .When<HttpClient, HttpResponseMessage>("POST /products with a valid payload",
                async (Ensure<HttpClient> client) =>
                {
                    client.Should().BeValid();
                    var payload = new { Name = "Widget", Price = 9.99m };
                    return await client.Value.PostAsJsonAsync("/products", payload);
                })
            .Then<HttpResponseMessage>("the response is 201 Created",
                async (Ensure<HttpResponseMessage> response) =>
                {
                    response.Should().BeValid();
                    response.Value.StatusCode.Should().Be(HttpStatusCode.Created);
                    response.Value.Headers.Location.Should().NotBeNull();

                    var body = await response.Value.Content.ReadFromJsonAsync<CreatedProductResponse>();
                    body.Should().NotBeNull();
                    body!.Id.Should().NotBeEmpty();
                })
            .PlayAsync();
    }

    [Fact]
    public async Task GetNonExistentProduct_Returns404()
    {
        var unknownId = Guid.NewGuid();

        await ScenarioRunner.Create("get a non-existent product", _output)
            .Given<HttpClient>("an HTTP client is configured", () =>
                new HttpClient { BaseAddress = new Uri("https://api.example.com") })
            .When<HttpClient, HttpResponseMessage>($"GET /products/{unknownId}",
                async (Ensure<HttpClient> client) =>
                {
                    client.Should().BeValid();
                    return await client.Value.GetAsync($"/products/{unknownId}");
                })
            .Then<HttpResponseMessage>("the response is 404 Not Found",
                (Ensure<HttpResponseMessage> response) =>
                {
                    response.Should().BeValid();
                    response.Value.StatusCode.Should().Be(HttpStatusCode.NotFound);
                })
            .PlayAsync();
}
}

file record CreatedProductResponse(Guid Id, string Name, decimal Price);

// ---------------------------------------------------------------------------
// Shared step library — reusable across test classes
// ---------------------------------------------------------------------------

public static class UserSteps
{
    public const string UserIdKey = "userId";
    public const string UserEmailKey = "userEmail";
    public const string AuthTokenKey = "authToken";

    public static void ARegisteredUserExists(IStepRunner runner) =>
        runner.Execute("a registered user exists", () =>
        {
            runner.SetContextData(UserIdKey, Guid.NewGuid());
            runner.SetContextData(UserEmailKey, "testuser@example.com");
        });

    public static async Task TheUserIsAuthenticated(IStepRunner runner) =>
        await runner.Execute("the user is authenticated", async () =>
        {
            var email = runner.GetContextData<string>(UserEmailKey);
            // Simulate token generation
            var token = await Task.FromResult($"token-for-{email}");
            runner.SetContextData(AuthTokenKey, token);
        });

    public static void TheUserHasAValidToken(IStepRunner runner) =>
        runner.Execute("the user has a valid auth token", () =>
        {
            var token = runner.GetContextData<string>(AuthTokenKey);
            token.Should().NotBeNullOrWhiteSpace();
            token.Should().StartWith("token-for-");
        });
}

// --- Usage of shared steps --------------------------------------------------

public class AuthScenarios(ITestOutputHelper output)
{
    private readonly IScenarioOutput _output = new XUnitScenarioOutput(output);

    [Fact]
    public async Task UserCanAuthenticate()
    {
        await ScenarioRunner.Create("user authentication", _output)
            .Given(UserSteps.ARegisteredUserExists)
            .When(UserSteps.TheUserIsAuthenticated)
            .Then(UserSteps.TheUserHasAValidToken)
            .PlayAsync();
    }

    [Fact]
    public async Task LoggedOutUserHasNoToken()
    {
        await ScenarioRunner.Create("user logs out", _output)
            .Given(UserSteps.ARegisteredUserExists)
            .When(UserSteps.TheUserIsAuthenticated)
            .And("the user logs out", (IStepRunner runner) =>
                runner.Execute("clear the token", () =>
                    runner.SetContextData(UserSteps.AuthTokenKey, string.Empty)))
            .Then("the token is empty", (IStepRunner runner) =>
                runner.Execute("token is empty string", () =>
                {
                    var token = runner.GetContextData<string>(UserSteps.AuthTokenKey);
                    token.Should().BeEmpty();
                }))
            .PlayAsync();
    }
}

// ---------------------------------------------------------------------------
// PlayAsync<T> — return scenario result for external assertions
// ---------------------------------------------------------------------------

public class PlayAsyncReturnValue(ITestOutputHelper output)
{
    private readonly IScenarioOutput _output = new XUnitScenarioOutput(output);

    [Fact]
    public async Task PricingPipeline_ReturnsCorrectTotal()
    {
        var result = await ScenarioRunner.Create("pricing pipeline returns total", _output)
            .Given<decimal>("base price is 100.00", () => 100m)
            .When<decimal, decimal>("10% discount applied",
                (Ensure<decimal> price) =>
                {
                    price.Should().BeValid();
                    return price.Value * 0.9m;
                })
            .Then<decimal, decimal>("VAT at 20% added",
                (Ensure<decimal> discounted) =>
                {
                    discounted.Should().BeValid();
                    return discounted.Value * 1.2m;
                })
            .PlayAsync<decimal>();

        // Assert the final result outside the chain
        result.Should().BeValid();
        result.Value.Should().Be(108m);
    }
}
