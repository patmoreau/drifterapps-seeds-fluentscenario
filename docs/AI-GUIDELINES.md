# AI Guidelines — DrifterApps.Seeds.FluentScenario

Guidelines for AI coding assistants generating or modifying code that uses this library.

## What This Library Is

FluentScenario is a **BDD test framework** for C# that structures tests as Given-When-Then scenarios. It is not a production runtime library — it belongs in test projects only.

Two packages:
- `DrifterApps.Seeds.FluentScenario` — core, required
- `DrifterApps.Seeds.FluentScenario.FluentAssertions` — optional, adds `.Should()` on `Ensure<T>`

Target: `net10.0` only.

---

## Quick Decision Tree

```
Need to write a test?
  │
  ├─ Steps are one-offs, simple → Pattern 1: inline lambdas
  ├─ Steps are reused across tests → Pattern 2: IStepRunner methods
  ├─ Method names are self-documenting → Pattern 3: CallerMemberName
  └─ Values must flow between steps → Pattern 4: Ensure<T> chaining
```

---

## Pattern Cheat Sheet

### Pattern 1 — Inline lambdas

```csharp
await ScenarioRunner.Create("scenario description", _output)
    .Given("step description", () => { /* setup */ })
    .When("step description", () => { /* action */ })
    .Then("step description", () => { /* assertion */ })
    .PlayAsync();
```

### Pattern 2 — Reusable IStepRunner methods

```csharp
await ScenarioRunner.Create("scenario description", _output)
    .Given(SetupStep)
    .When(ActionStep)
    .Then(AssertionStep)
    .PlayAsync();

private static void SetupStep(IStepRunner runner) =>
    runner.Execute("step description", () =>
    {
        runner.SetContextData("key", value);
    });
```

### Pattern 3 — CallerMemberName (method name = description)

```csharp
await ScenarioRunner.Create(_output)   // no description → test method name used
    .Given(SetupStep)
    .When(ActionStep)
    .Then(AssertionStep)
    .PlayAsync();

private static void SetupStep(IStepRunner runner) =>
    runner.Execute(() =>               // no description → method name used
    {
        runner.SetContextData("key", value);
    });
```

### Pattern 4 — Ensure<T> value chaining

```csharp
await ScenarioRunner.Create("scenario description", initialValue, _output)
    .Given("step description", (Ensure<InputType> input) =>
    {
        input.Should().BeValid();      // assert valid before using
        return transformedValue;
    })
    .When("step description", (Ensure<TransformedType> value) =>
    {
        value.Should().BeValid();
        return result;
    })
    .Then("step description", (Ensure<ResultType> result) =>
    {
        result.Should().BeValid().And.Subject.Value.Should().Be(expected);
    })
    .PlayAsync();
```

---

## Rules for Generating Code

### Always

- `await` the `PlayAsync()` call — steps are collected lazily; nothing runs without it.
- Call `ScenarioRunner.Create(...)` exactly once per scenario. One runner per test.
- Check `Ensure<T>.IsValid` or call `.Should().BeValid()` before accessing `.Value`.
- Use `Ensure<T>` as the **parameter type** in step lambdas when receiving values from previous steps — never cast `object` directly.
- Keep each test focused on a single scenario. Use separate test methods for separate behaviors.

### Never

- Call `new ScenarioRunner(...)` — only the static `Create()` factory methods are public.
- Access `Ensure<T>.Value` without first validating `IsValid` or asserting validity.
- Call `GetContextData<T>(key)` with a type that doesn't match what was stored — it casts and throws at runtime.
- Mix `Ensure<T>` return-value chaining and `SetContextData`/`GetContextData` in the same scenario. Pick one approach.
- Put scenario runner code in production source projects — it belongs in test projects only.
- Skip `PlayAsync()` at the end of a chain — the scenario will never execute.

---

## `Ensure<T>` Rules

`Ensure<T>` is a readonly struct, not a class. Key behaviors:

| Scenario | `IsValid` | `Value` |
|---|---|---|
| `Create<T>(value)` with valid `T` | `true` | returns the value |
| First step with no `Create<T>` input | `false` | throws `InvalidOperationException` |
| Step preceding returned `null` for a non-nullable `T` | `false` | throws |
| Step preceding returned `null` for a nullable `T?` | `true` | returns `null` |

Implicit conversions exist in both directions, but don't rely on them when validity is uncertain.

---

## Context Storage Rules

Use `SetContextData` / `GetContextData<T>` when:
- Using Pattern 2 (IStepRunner methods) where steps cannot return values directly.
- Sharing state across multiple related steps that don't form a linear chain.

Rules:
- Keys are plain strings. Use consistent, descriptive keys within a scenario.
- `GetContextData<T>` performs an unchecked cast. The stored type must exactly match `T`.
- There is no `TryGetContextData`. If a key was never set, it will throw on access.
- Both `ScenarioRunner` (via `IRunnerContext`) and `IStepRunner` share the same context dictionary.

---

## `And()` Keyword

`And` is context-aware: it adopts the previous keyword's role (Given, When, or Then):

```csharp
.Given("first condition", () => { })
.And("second condition", () => { })    // treated as Given
.When("action", () => { })
.And("another action", () => { })      // treated as When
.Then("assertion", () => { })
.And("another assertion", () => { })   // treated as Then
```

---

## IScenarioOutput Adapters

The library does not depend on any specific test framework. Implement `IScenarioOutput` once per project:

### xUnit
```csharp
public sealed class XUnitScenarioOutput(ITestOutputHelper output) : IScenarioOutput
{
    public void WriteLine(string message) => output.WriteLine(message);
    public void WriteLine(string format, params object[] args) => output.WriteLine(format, args);
}
```

### NUnit
```csharp
public sealed class NUnitScenarioOutput : IScenarioOutput
{
    public void WriteLine(string message) => TestContext.Progress.WriteLine(message);
    public void WriteLine(string format, params object[] args) =>
        TestContext.Progress.WriteLine(format, args);
}
```

### MSTest
```csharp
public sealed class MSTestScenarioOutput(TestContext context) : IScenarioOutput
{
    public void WriteLine(string message) => context.WriteLine(message);
    public void WriteLine(string format, params object[] args) => context.WriteLine(format, args);
}
```

---

## Common Anti-Patterns

### Missing await on PlayAsync
```csharp
// WRONG — scenario never executes
ScenarioRunner.Create("test", _output)
    .Given("step", () => { })
    .PlayAsync();

// CORRECT
await ScenarioRunner.Create("test", _output)
    .Given("step", () => { })
    .PlayAsync();
```

### Unsafe Ensure<T> access
```csharp
// WRONG — throws if value didn't arrive
.Then("step", (Ensure<int> result) =>
{
    result.Value.Should().BeGreaterThan(0);
})

// CORRECT
.Then("step", (Ensure<int> result) =>
{
    result.Should().BeValid();
    result.Value.Should().BeGreaterThan(0);
})
```

### Wrong type in GetContextData
```csharp
runner.SetContextData("count", 42);          // stored as int
var count = runner.GetContextData<string>("count");  // WRONG — throws InvalidCastException
var count = runner.GetContextData<int>("count");     // CORRECT
```

### Reusing a runner across tests
```csharp
// WRONG — steps accumulate; tests bleed into each other
private readonly ScenarioRunner _runner = ScenarioRunner.Create("test", _output);

// CORRECT — create a new runner per test
await ScenarioRunner.Create("test", _output)
    .Given(...)
    .PlayAsync();
```

---

## FluentAssertions Integration Notes

Requires the `DrifterApps.Seeds.FluentScenario.FluentAssertions` package.

Available assertions on `Ensure<T>`:

```csharp
ensure.Should().BeValid();                          // asserts IsValid == true
ensure.Should().BeInvalid();                        // asserts IsValid == false
ensure.Should().HaveValue(expectedValue);           // asserts Value equals expected
ensure.Should().BeNull();                           // asserts Value is null
ensure.Should().NotBeNull();                        // asserts Value is not null

// Chain to assert the inner value
ensure.Should().BeValid().And.Subject.Value.Should().Be(42);
```

The `EnsureAssertions<T>` class lives in the `FluentAssertions` namespace intentionally — it feels native to the assertion library.

---

## Async Step Signatures

All step overloads have sync and async variants. Prefer async when your step calls `async` code:

```csharp
// Sync
.Given("description", () => { /* sync */ })
.Given<T>("description", () => value)
.Given<T, T2>("description", (Ensure<T> input) => output)

// Async
.Given("description", async () => { /* async */ })
.Given<T>("description", async () => await GetValueAsync())
.Given<T, T2>("description", async (Ensure<T> input) => await TransformAsync(input))
```

Do not use `.Result` or `.GetAwaiter().GetResult()` inside step lambdas — use `async`/`await`.

---

## CallerMemberName Conversion

When no description is provided, the calling method's name is converted to a sentence:

| Method name | Description output |
|---|---|
| `WhenTheWeatherIsTooCold` | "when the weather is too cold" |
| `GivenAUserExists` | "given a user exists" |
| `TheTemperatureIsBelow0C` | "the temperature is below0 c" |

Numbers attached to letters may produce unexpected spacing. For precise descriptions, pass an explicit string.
