# Examples — DrifterApps.Seeds.FluentScenario

Real-world usage examples beyond the basic weather scenario. All examples assume xUnit with:

```csharp
private readonly IScenarioOutput _output;

public MyTests(ITestOutputHelper outputHelper)
{
    _output = new XUnitScenarioOutput(outputHelper);
}
```

See [examples/](../examples/) for complete compilable files.

---

## 1. HTTP API Integration Test

Test a REST API endpoint. Steps produce typed results that flow into subsequent assertions.

```csharp
[Fact]
public async Task CreateProduct_ReturnsCreatedWithLocation()
{
    await ScenarioRunner.Create("create a product via API", _output)
        .Given<HttpClient>("a configured HTTP client", () =>
            _factory.CreateClient())
        .When<HttpClient, HttpResponseMessage>("POST /products with valid payload",
            async (Ensure<HttpClient> client) =>
            {
                client.Should().BeValid();
                var payload = new { Name = "Widget", Price = 9.99m };
                return await client.Value.PostAsJsonAsync("/products", payload);
            })
        .Then<HttpResponseMessage>("response is 201 Created with a Location header",
            async (Ensure<HttpResponseMessage> response) =>
            {
                response.Should().BeValid();
                response.Value.StatusCode.Should().Be(HttpStatusCode.Created);
                response.Value.Headers.Location.Should().NotBeNull();
            })
        .PlayAsync();
}
```

---

## 2. Database Repository Test

Steps share context to coordinate setup and assertion phases.

```csharp
[Fact]
public async Task SaveAndRetrieveOrder()
{
    await ScenarioRunner.Create("save an order and retrieve it", _output)
        .Given(AnOrderIsPersisted)
        .When(TheOrderIsRetrievedById)
        .Then(TheRetrievedOrderMatchesTheOriginal)
        .PlayAsync();
}

private async Task AnOrderIsPersisted(IStepRunner runner)
{
    await runner.Execute("an order is persisted", async () =>
    {
        var order = new Order { CustomerId = Guid.NewGuid(), Amount = 150m };
        await _repository.SaveAsync(order);
        runner.SetContextData("orderId", order.Id);
        runner.SetContextData("expectedAmount", order.Amount);
    });
}

private async Task TheOrderIsRetrievedById(IStepRunner runner)
{
    await runner.Execute("the order is retrieved by ID", async () =>
    {
        var orderId = runner.GetContextData<Guid>("orderId");
        var order = await _repository.FindByIdAsync(orderId);
        runner.SetContextData("retrievedOrder", order);
    });
}

private void TheRetrievedOrderMatchesTheOriginal(IStepRunner runner)
{
    runner.Execute("the retrieved order matches the original", () =>
    {
        var order = runner.GetContextData<Order>("retrievedOrder");
        var expectedAmount = runner.GetContextData<decimal>("expectedAmount");

        order.Should().NotBeNull();
        order.Amount.Should().Be(expectedAmount);
    });
}
```

---

## 3. Domain Event Assertion

Verify domain events emitted during a use case using Ensure<T> chaining.

```csharp
[Fact]
public async Task PlacingAnOrderEmitsOrderPlacedEvent()
{
    await ScenarioRunner.Create("placing an order emits an event", _output)
        .Given<Order>("a customer with items in their cart", () =>
        {
            var cart = new Cart(customerId: Guid.NewGuid());
            cart.AddItem("SKU-001", quantity: 2, unitPrice: 19.99m);
            return cart.Checkout();
        })
        .When<Order, IReadOnlyList<IDomainEvent>>("the order is placed",
            async (Ensure<Order> order) =>
            {
                order.Should().BeValid();
                await _orderService.PlaceAsync(order.Value);
                return _eventStore.EventsFor(order.Value.Id);
            })
        .Then<IReadOnlyList<IDomainEvent>>("an OrderPlaced event is emitted",
            (Ensure<IReadOnlyList<IDomainEvent>> events) =>
            {
                events.Should().BeValid();
                events.Value.Should().ContainSingle(e => e is OrderPlacedEvent);
            })
        .PlayAsync();
}
```

---

## 4. Parameterized Tests with Theory

Use `[Theory]` with `[InlineData]` and pass the parameter into `Create<T>`.

```csharp
[Theory]
[InlineData("user@example.com", true)]
[InlineData("not-an-email", false)]
[InlineData("", false)]
public async Task EmailValidation(string email, bool expectedValid)
{
    await ScenarioRunner.Create("email validation", email, _output)
        .Given<string, ValidationResult>("the email is validated",
            (Ensure<string> input) =>
            {
                input.Should().BeValid();
                return _validator.Validate(input.Value);
            })
        .Then<ValidationResult>("the result matches expectation",
            (Ensure<ValidationResult> result) =>
            {
                result.Should().BeValid();
                result.Value.IsValid.Should().Be(expectedValid);
            })
        .PlayAsync();
}
```

---

## 5. Multi-Step Transformation Pipeline

Chain transformations through multiple steps using return-value passing.

```csharp
[Fact]
public async Task CsvFileIsImportedAndPersisted()
{
    await ScenarioRunner.Create("CSV file import pipeline", _output)
        .Given<string>("a CSV file is available", () =>
            File.ReadAllText("TestData/products.csv"))
        .When<string, IReadOnlyList<ProductDto>>("the CSV is parsed",
            (Ensure<string> csv) =>
            {
                csv.Should().BeValid();
                return _csvParser.Parse(csv.Value);
            })
        .And<IReadOnlyList<ProductDto>, IReadOnlyList<Product>>("records are mapped to domain objects",
            (Ensure<IReadOnlyList<ProductDto>> dtos) =>
            {
                dtos.Should().BeValid();
                return dtos.Value.Select(_mapper.Map<Product>).ToList();
            })
        .Then<IReadOnlyList<Product>>("all products are persisted",
            async (Ensure<IReadOnlyList<Product>> products) =>
            {
                products.Should().BeValid();
                await _repository.SaveAllAsync(products.Value);
                var count = await _repository.CountAsync();
                count.Should().Be(products.Value.Count);
            })
        .PlayAsync();
}
```

---

## 6. Authentication Flow

Test a multi-step authentication flow with context sharing.

```csharp
[Fact]
public async Task UserCanLoginAndAccessProtectedResource()
{
    await ScenarioRunner.Create("authenticated user accesses protected resource", _output)
        .Given(ARegisteredUserExists)
        .When(TheUserLogsIn)
        .And(TheUserAccessesProtectedEndpoint)
        .Then(TheResponseIsAuthorized)
        .PlayAsync();
}

private async Task ARegisteredUserExists(IStepRunner runner)
{
    await runner.Execute("a registered user exists", async () =>
    {
        var user = await _userService.CreateAsync("testuser@example.com", "SecurePass1!");
        runner.SetContextData("userId", user.Id);
        runner.SetContextData("password", "SecurePass1!");
    });
}

private async Task TheUserLogsIn(IStepRunner runner)
{
    await runner.Execute("the user logs in with correct credentials", async () =>
    {
        var email = "testuser@example.com";
        var password = runner.GetContextData<string>("password");
        var token = await _authService.LoginAsync(email, password);
        token.Should().NotBeNullOrWhiteSpace();
        runner.SetContextData("token", token);
    });
}

private async Task TheUserAccessesProtectedEndpoint(IStepRunner runner)
{
    await runner.Execute("the user accesses a protected endpoint", async () =>
    {
        var token = runner.GetContextData<string>("token");
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/api/profile");
        runner.SetContextData("response", response);
    });
}

private void TheResponseIsAuthorized(IStepRunner runner)
{
    runner.Execute("the response is 200 OK", () =>
    {
        var response = runner.GetContextData<HttpResponseMessage>("response");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    });
}
```

---

## 7. Shared Step Library

Extract reusable steps into a static helper class for use across multiple test files.

```csharp
// SharedSteps.cs — reusable across test classes
public static class OrderSteps
{
    public static void AnExistingCustomer(IStepRunner runner)
    {
        runner.Execute("an existing customer", () =>
        {
            var customer = CustomerFactory.CreateValid();
            runner.SetContextData("customerId", customer.Id);
        });
    }

    public static async Task AnOrderIsCreated(IStepRunner runner)
    {
        await runner.Execute("an order is created", async () =>
        {
            var customerId = runner.GetContextData<Guid>("customerId");
            var order = await OrderFactory.CreateForCustomerAsync(customerId);
            runner.SetContextData("orderId", order.Id);
        });
    }
}

// UsageInTest.cs
[Fact]
public async Task CancellingAnOrder_ChangesStatusToCancelled()
{
    await ScenarioRunner.Create("cancelling an order", _output)
        .Given(OrderSteps.AnExistingCustomer)
        .And(OrderSteps.AnOrderIsCreated)
        .When("the order is cancelled", async (IStepRunner runner) =>
        {
            await runner.Execute("cancel the order", async () =>
            {
                var orderId = runner.GetContextData<Guid>("orderId");
                await _orderService.CancelAsync(orderId);
            });
        })
        .Then("the order status is Cancelled", async (IStepRunner runner) =>
        {
            await runner.Execute("order status is verified", async () =>
            {
                var orderId = runner.GetContextData<Guid>("orderId");
                var order = await _repository.FindByIdAsync(orderId);
                order.Status.Should().Be(OrderStatus.Cancelled);
            });
        })
        .PlayAsync();
}
```

---

## 8. PlayAsync\<T\> — Returning a Final Value

Use `PlayAsync<T>()` when the test needs the scenario's result for further assertions outside the chain.

```csharp
[Fact]
public async Task CreateUser_ReturnsNewUserId()
{
    var result = await ScenarioRunner.Create("create a user and return the ID", _output)
        .Given("a valid user payload is prepared", () =>
            new CreateUserRequest { Name = "Alice", Email = "alice@example.com" })
        .When<CreateUserRequest, Guid>("the user is created",
            async (Ensure<CreateUserRequest> request) =>
            {
                request.Should().BeValid();
                return await _userService.CreateAsync(request.Value);
            })
        .PlayAsync<Guid>();

    result.Should().BeValid();
    result.Value.Should().NotBeEmpty();
}
```

---

## 9. CallerMemberName — Self-Documenting Test

When method names are descriptive, omit strings entirely for minimal boilerplate.

```csharp
[Fact]
public async Task AnActiveUserCanResetTheirPassword()
{
    await ScenarioRunner.Create(_output)
        .Given(AnActiveUserExists)
        .When(TheUserRequestsAPasswordReset)
        .Then(AResetEmailIsSent)
        .PlayAsync();
}

// Output: ✓ SCENARIO for An Active User Can Reset Their Password
//         ✓ GIVEN An Active User Exists
//         ✓ WHEN The User Requests A Password Reset
//         ✓ THEN A Reset Email Is Sent

private void AnActiveUserExists(IStepRunner runner) =>
    runner.Execute(() =>
    {
        runner.SetContextData("email", "user@example.com");
        _userRepository.Add(new User { Email = "user@example.com", IsActive = true });
    });

private async Task TheUserRequestsAPasswordReset(IStepRunner runner) =>
    await runner.Execute(async () =>
    {
        var email = runner.GetContextData<string>("email");
        await _passwordService.RequestResetAsync(email);
    });

private void AResetEmailIsSent(IStepRunner runner) =>
    runner.Execute(() =>
    {
        var email = runner.GetContextData<string>("email");
        _emailSender.SentEmails
            .Should().ContainSingle(m => m.To == email && m.Subject.Contains("reset"));
    });
```

---

## Common Patterns Summary

| Pattern | When to use | Context sharing |
|---|---|---|
| Inline lambdas + explicit descriptions | Quick, one-off scenarios | Captured variables |
| `IStepRunner` methods with explicit descriptions | Reusable step building blocks | `SetContextData` / `GetContextData` |
| `IStepRunner` methods + CallerMemberName | Self-documenting, step names match intent | `SetContextData` / `GetContextData` |
| `Ensure<T>` return chaining | Type-safe value flow, transformation pipelines | Return values |
| `PlayAsync<T>()` | Result needed outside the chain | Return values |
| Shared static step classes | Step reuse across multiple test files | `SetContextData` / `GetContextData` |

---

## Anti-Patterns to Avoid

### Splitting one scenario into multiple `PlayAsync` calls

```csharp
// WRONG — two separate scenarios, not one
await ScenarioRunner.Create("setup", _output).Given(...).PlayAsync();
await ScenarioRunner.Create("assertion", _output).Then(...).PlayAsync();

// CORRECT — one scenario
await ScenarioRunner.Create("scenario name", _output)
    .Given(...)
    .Then(...)
    .PlayAsync();
```

### Asserting inside Given or When for logic that belongs in Then

```csharp
// WRONG — assertion in setup
.Given("setup", () =>
{
    var result = DoSetup();
    result.Should().NotBeNull(); // assertion doesn't belong in Given
})

// CORRECT — verify prerequisites minimally, assert outcomes in Then
.Given("setup", () => DoSetup())
.Then("result is not null", (Ensure<SetupResult> r) =>
{
    r.Should().BeValid();
    r.Value.Should().NotBeNull();
})
```

### Skipping validation on Ensure<T> inputs

```csharp
// WRONG — will throw InvalidOperationException if value didn't arrive
.Then("assert", (Ensure<int> value) => value.Value.Should().BePositive())

// CORRECT
.Then("assert", (Ensure<int> value) =>
{
    value.Should().BeValid();
    value.Value.Should().BePositive();
})
```
