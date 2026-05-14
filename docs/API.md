# API Reference — DrifterApps.Seeds.FluentScenario

## Namespaces

| Namespace | Package | Contents |
|---|---|---|
| `DrifterApps.Seeds.FluentScenario` | `DrifterApps.Seeds.FluentScenario` | Core types |
| `FluentAssertions` | `DrifterApps.Seeds.FluentScenario.FluentAssertions` | `EnsureAssertions<T>` extensions |

---

## ScenarioRunner

`public partial class ScenarioRunner : IScenarioRunner, IStepRunner`

The main orchestrator. Collect steps with `Given`/`When`/`Then`/`And`, then execute with `PlayAsync()`.

### Factory Methods

```csharp
// Explicit description
public static ScenarioRunner Create(string description, IScenarioOutput scenarioOutput)

// Description from calling method name (CallerMemberName)
public static ScenarioRunner Create(IScenarioOutput scenarioOutput,
    [CallerMemberName] string description = "")

// Explicit description with typed initial input
public static ScenarioRunner Create<T>(string description, T input, IScenarioOutput scenarioOutput)

// Description from calling method name, with typed initial input
public static ScenarioRunner Create<T>(T input, IScenarioOutput scenarioOutput,
    [CallerMemberName] string description = "")
```

**Parameters**
- `description` — Scenario label printed in output as `✓ SCENARIO for <description>`.
- `scenarioOutput` — Sink for step-by-step output. Implement `IScenarioOutput` for your test framework.
- `input` — Initial value passed to the first step as `Ensure<T>`.

---

### Execution Methods

```csharp
// Execute all steps. Throws on first step failure.
Task PlayAsync()

// Execute all steps and return the final step's result.
Task<Ensure<T>> PlayAsync<T>()
```

Steps execute sequentially. Each step receives the previous step's return value as its `Ensure<T>` input. If any step throws, the exception is logged (marked ✗) then rethrown.

---

### Step Methods — Given / When / Then / And

All four keywords share the same overload set. Replace `Given` with `When`, `Then`, or `And` as needed. `And` routes to the previous keyword's role.

#### IStepRunner delegation

```csharp
IScenarioRunner Given(Action<IStepRunner> step)
IScenarioRunner When(Action<IStepRunner> step)
IScenarioRunner Then(Action<IStepRunner> step)
IScenarioRunner And(Action<IStepRunner> step)
```

Pass a method that accepts `IStepRunner`. Use `runner.Execute(...)` inside to register sub-steps.

#### No input, no output

```csharp
IScenarioRunner Given(string description, Action step)
IScenarioRunner Given(string description, Func<Task> step)
IScenarioRunner Given(Action step, [CallerMemberName] string description = "")
IScenarioRunner Given(Func<Task> step, [CallerMemberName] string description = "")
```

Use when the step has no return value and does not consume the previous step's result.

#### No input, with output

```csharp
IScenarioRunner Given<T>(string description, Func<T> step)
IScenarioRunner Given<T>(string description, Func<Task<T>> step)
IScenarioRunner Given<T>(Func<T> step, [CallerMemberName] string description = "")
IScenarioRunner Given<T>(Func<Task<T>> step, [CallerMemberName] string description = "")
```

Use when the step produces a value for the next step, but does not consume any input.

#### With Ensure<T> input, no output

```csharp
IScenarioRunner Given<T>(string description, Action<Ensure<T>> step)
IScenarioRunner Given<T>(string description, Func<Ensure<T>, Task> step)
IScenarioRunner Given<T>(Action<Ensure<T>> step, [CallerMemberName] string description = "")
IScenarioRunner Given<T>(Func<Ensure<T>, Task> step, [CallerMemberName] string description = "")
```

Use when the step consumes the previous step's result but does not produce a new value.

#### With Ensure<T> input, with output

```csharp
IScenarioRunner Given<T, T2>(string description, Func<Ensure<T>, T2> step)
IScenarioRunner Given<T, T2>(string description, Func<Ensure<T>, Task<T2>> step)
IScenarioRunner Given<T, T2>(Func<Ensure<T>, T2> step, [CallerMemberName] string description = "")
IScenarioRunner Given<T, T2>(Func<Ensure<T>, Task<T2>> step, [CallerMemberName] string description = "")
```

Use when the step both consumes input from the previous step and produces output for the next step.

---

## IScenarioRunner

`public interface IScenarioRunner : IRunnerContext`

Exposes all `Given`/`When`/`Then`/`And`/`PlayAsync` methods. Returned by `ScenarioRunner.Create()`. Not intended to be implemented externally.

---

## IStepRunner

`public interface IStepRunner : IRunnerContext`

Passed to `Action<IStepRunner>` overloads. Lets you group related sub-steps under a single BDD keyword.

### Execute Methods

Mirrors the step method signatures on `IScenarioRunner`. Replace `Given` with `Execute`:

```csharp
// No input, no output
IStepRunner Execute(string description, Action stepExecution)
IStepRunner Execute(string description, Func<Task> stepExecution)
IStepRunner Execute(Action stepExecution, [CallerMemberName] string description = "")
IStepRunner Execute(Func<Task> stepExecution, [CallerMemberName] string description = "")

// No input, with output
IStepRunner Execute<T>(string description, Func<T> stepExecution)
IStepRunner Execute<T>(string description, Func<Task<T>> stepExecution)
IStepRunner Execute<T>(Func<T> stepExecution, [CallerMemberName] string description = "")
IStepRunner Execute<T>(Func<Task<T>> stepExecution, [CallerMemberName] string description = "")

// With Ensure<T> input, no output
IStepRunner Execute<T>(string description, Action<Ensure<T>> stepExecution)
IStepRunner Execute<T>(string description, Func<Ensure<T>, Task> stepExecution)
IStepRunner Execute<T>(Action<Ensure<T>> stepExecution, [CallerMemberName] string description = "")
IStepRunner Execute<T>(Func<Ensure<T>, Task> stepExecution, [CallerMemberName] string description = "")

// With Ensure<T> input, with output
IStepRunner Execute<T, T2>(string description, Func<Ensure<T>, T2> stepExecution)
IStepRunner Execute<T, T2>(string description, Func<Ensure<T>, Task<T2>> stepExecution)
IStepRunner Execute<T, T2>(Func<Ensure<T>, T2> stepExecution, [CallerMemberName] string description = "")
IStepRunner Execute<T, T2>(Func<Ensure<T>, Task<T2>> stepExecution, [CallerMemberName] string description = "")
```

---

## IRunnerContext

`public interface IRunnerContext`

Shared by both `IScenarioRunner` and `IStepRunner`. Provides a key-value store for sharing state across steps when not using return-value chaining.

```csharp
void SetContextData(string contextKey, object data)
T GetContextData<T>(string contextKey)
```

**Notes**
- `SetContextData` replaces any existing value at the key.
- `GetContextData<T>` performs an unchecked cast. The stored value must match `T` exactly.
- The context is shared between the `ScenarioRunner` and all `IStepRunner` instances within the same scenario.

---

## IScenarioOutput

`public interface IScenarioOutput`

Output sink for step results. Implement once per test framework.

```csharp
void WriteLine(string message)
void WriteLine(string format, params object[] args)
```

---

## Ensure\<T\>

`public readonly struct Ensure<TValue> : IEquatable<Ensure<TValue>>`

A readonly struct wrapping a value passed between steps. Carries a validity flag so receiving steps can distinguish "a value arrived" from "no value arrived."

### Properties

```csharp
// True if the wrapped value is a valid instance of TValue
// (or null when TValue is a nullable type)
public bool IsValid { get; }

// Returns the wrapped value. Throws InvalidOperationException if !IsValid.
[MemberNotNull]
public TValue Value { get; }

// True if TValue is a nullable reference or value type
public bool IsNullable { get; }
```

### Static Methods

```csharp
// Creates an Ensure<TValue> from an object?, validating the type
public static Ensure<TValue> From(object? value)
```

### Instance Methods

```csharp
// Returns this instance (identity, for interface consistency)
public Ensure<TValue> ToEnsure()

// Extracts the value, throwing if !IsValid
public TValue FromEnsure()

// Equality
public override bool Equals(object? obj)
public bool Equals(Ensure<TValue> other)
public override int GetHashCode()
```

### Operators

```csharp
// Unwrap: Ensure<T> → T (throws if !IsValid)
public static implicit operator TValue(Ensure<TValue> value)

// Wrap: T → Ensure<T>
public static implicit operator Ensure<TValue>(TValue value)

public static bool operator ==(Ensure<TValue> left, Ensure<TValue> right)
public static bool operator !=(Ensure<TValue> left, Ensure<TValue> right)
```

### Validity Matrix

| Source | `IsValid` | `Value` |
|---|---|---|
| `Create<T>(someValue)` where `someValue` matches `T` | `true` | `someValue` |
| First step, no `Create<T>` input, non-nullable `T` | `false` | throws |
| Step returned `null` for non-nullable `T` | `false` | throws |
| Step returned `null` for nullable `T?` | `true` | `null` |

---

## EnsureAssertions\<T\> (FluentAssertions package)

`public class EnsureAssertions<TValue>`

Provides FluentAssertions-style assertions on `Ensure<T>`. Available via the `.Should()` extension method after adding the `DrifterApps.Seeds.FluentScenario.FluentAssertions` package.

### Extension Method

```csharp
// In namespace: FluentAssertions
public static EnsureAssertions<TValue> Should<TValue>(this Ensure<TValue> instance)
```

### Assertion Methods

```csharp
// Asserts IsValid == true
AndConstraint<EnsureAssertions<TValue>> BeValid(
    string because = "", params object[] becauseArgs)

// Asserts IsValid == false
AndConstraint<EnsureAssertions<TValue>> BeInvalid(
    string because = "", params object[] becauseArgs)

// Asserts Value equals expectedValue (implies IsValid == true)
AndConstraint<EnsureAssertions<TValue>> HaveValue(
    TValue expectedValue, string because = "", params object[] becauseArgs)

// Asserts Value is null
AndConstraint<EnsureAssertions<TValue>> BeNull(
    string because = "", params object[] becauseArgs)

// Asserts Value is not null
AndConstraint<EnsureAssertions<TValue>> NotBeNull(
    string because = "", params object[] becauseArgs)
```

### Chaining to the Inner Value

```csharp
// Access the Ensure<T> subject after BeValid() to continue asserting the inner value
ensure.Should().BeValid().And.Subject.Value.Should().Be(42);
ensure.Should().BeValid().And.Subject.Value.Should().BeGreaterThan(0);
```

---

## Output Format

Each step produces one line of output:

```
✓ SCENARIO for <description>
✓ GIVEN <step description>
✓ WHEN <step description>
✓ THEN <step description>
✗ THEN <failing step description>
```

Success lines are printed in green (`[32m`). Failure lines are printed in red (`[31m`). Both reset with `[0m`.
