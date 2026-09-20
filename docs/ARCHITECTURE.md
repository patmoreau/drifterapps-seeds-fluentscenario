# Architecture — DrifterApps.Seeds.FluentScenario

## Purpose

FluentScenario structures test code into readable BDD (Behavior-Driven Development) scenarios without requiring a dedicated BDD tool (SpecFlow, Reqnroll, etc.). It targets teams who want expressive test names and Given-When-Then organisation while staying entirely within C# and their existing test runner.

---

## Package Boundaries

```
DrifterApps.Seeds.FluentScenario           (core)
  └─ ScenarioRunner
  └─ Ensure<T>
  └─ IScenarioRunner / IStepRunner / IRunnerContext / IScenarioOutput

DrifterApps.Seeds.FluentScenario.FluentAssertions   (optional extension)
  └─ EnsureAssertions<T>   (extends Ensure<T> with .Should())
```

The FluentAssertions package has a hard dependency on FluentAssertions 7.x. The core package has no third-party runtime dependencies.

---

## Core Design Decisions

### 1. Builder Pattern with Deferred Execution

Steps are registered in a `List<StepDefinition>` during the fluent chain. Nothing runs until `PlayAsync()` is called. This separation:

- Makes step registration side-effect-free.
- Allows the full chain to be constructed before any I/O or assertions occur.
- Ensures a single, predictable execution point.

```
.Given(...)  →  collect StepDefinition
.When(...)   →  collect StepDefinition
.Then(...)   →  collect StepDefinition
.PlayAsync() →  execute all in order
```

### 2. Partial Class Organisation

`ScenarioRunner` is intentionally split across seven files to prevent a single massive file while keeping a single class:

| File | Responsibility |
|---|---|
| `ScenarioRunner.cs` | `Create()` factories, `PlayAsync()`, output |
| `ScenarioRunner.Given.cs` | 16 `Given()` overloads |
| `ScenarioRunner.When.cs` | 16 `When()` overloads |
| `ScenarioRunner.Then.cs` | 16 `Then()` overloads |
| `ScenarioRunner.And.cs` | 16 `And()` overloads |
| `ScenarioRunner.StepRunner.cs` | 16 `Execute()` overloads (`IStepRunner` impl) |
| `ScenarioRunner.RunnerContext.cs` | `SetContextData` / `GetContextData` |

New overloads for a given keyword go in the matching file. Core execution logic stays in `ScenarioRunner.cs`.

### 3. Ensure\<T\> as a Value Boundary

Passing raw `object?` between steps would be stringly-typed and unsafe. `Ensure<T>` adds:

- **Validity signalling**: a step that received no input gets `IsValid == false`.
- **Type safety**: the generic parameter forces callers to declare the expected type.
- **Zero allocation on the happy path**: it's a `readonly struct`.
- **Implicit conversion**: can be used directly as `T` in most contexts, reducing ceremony.

The struct also supports nullable types (`T?`) so `null` is representable as a valid value when the caller declares nullability explicitly.

### 4. Two Context-Sharing Mechanisms

| Mechanism | How it works | When to use |
|---|---|---|
| Return-value chaining | Step returns `T`; next step receives `Ensure<T>` | Linear transformation pipelines |
| `SetContextData` / `GetContextData<T>` | Shared dictionary on the runner | `IStepRunner` methods, fan-out state |

Both are available simultaneously; the recommendation is to pick one per scenario for clarity.

### 5. CallerMemberName for Self-Documenting Tests

All step and `Execute()` overloads have a variant that omits the `description` string. Instead, `[CallerMemberName]` captures the calling method's name at compile time. A regex converts the method name from PascalCase to a sentence:

```
WhenTheWeatherIsTooCold  →  "when the weather is too cold"
```

Regex: `(?<!^)([A-Z]|[0-9].*)|_` (source-generated for performance).

This lets developers write well-named methods once and get description output for free, at zero runtime cost.

### 6. And() Keyword Routing

`And()` has no keyword of its own. It inspects the `Command` of the previous `StepDefinition` and routes to the appropriate label:

```
previous = Given → And creates a Given step
previous = When  → And creates a When step
previous = Then  → And creates a Then step
```

This ensures output reads naturally (`GIVEN ... AND ...`) without requiring the developer to repeat the previous keyword.

### 7. IStepRunner Delegation

`Action<IStepRunner>` overloads allow grouping multiple sub-steps under one BDD keyword. This pattern:

- Enables step-method libraries shared across test classes.
- Preserves the single context dictionary (same `ScenarioRunner` instance).
- Lets each sub-step have its own description and success/failure output line.

`ScenarioRunner` implements both `IScenarioRunner` and `IStepRunner`. When a `Given(Action<IStepRunner>)` call is made, the runner sets an internal `_stepCommand` field, then passes itself as the `IStepRunner`. The `Execute()` methods on `IStepRunner` use `_stepCommand` to create steps with the correct keyword label.

---

## Execution Flow

```
PlayAsync()
  │
  ├─ print "✓ SCENARIO for <description>"
  │
  ├─ foreach StepDefinition in _steps:
  │     │
  │     ├─ invoke step.Step(previousResult)
  │     │     returns Task<object?>
  │     │
  │     ├─ on success:
  │     │     print "✓ <KEYWORD> <description>"
  │     │     previousResult = returnedValue
  │     │
  │     └─ on exception:
  │           print "✗ <KEYWORD> <description>"
  │           rethrow
  │
  └─ (optional) wrap final result in Ensure<T> for PlayAsync<T>()
```

Each step lambda is normalised to `Func<object?, Task<object?>>` inside `StepDefinition.Create()`. This uniform signature allows the runner to be completely generic while the outer API remains strongly typed.

---

## Method Overload Matrix

Each keyword supports 4 input/output combinations × 2 async variants × 2 description variants = 16 overloads:

| Input | Output | Sync | Async | Explicit desc | CallerMemberName |
|---|---|---|---|---|---|
| None | None | `Action` | `Func<Task>` | ✓ | ✓ |
| None | `T` | `Func<T>` | `Func<Task<T>>` | ✓ | ✓ |
| `Ensure<T>` | None | `Action<Ensure<T>>` | `Func<Ensure<T>, Task>` | ✓ | ✓ |
| `Ensure<T>` | `T2` | `Func<Ensure<T>, T2>` | `Func<Ensure<T>, Task<T2>>` | ✓ | ✓ |

Plus one `IStepRunner` delegation overload per keyword (no CallerMemberName variant).

Total: 65 overloads across Given/When/Then/And/Execute.

This overload set is intentional. The goal is zero friction for any test shape. Adding new overloads should follow the same matrix rather than introducing special-purpose signatures.

---

## FluentAssertions Integration Design

`EnsureAssertions<T>` extends `ReferenceTypeAssertions<Ensure<T>, EnsureAssertions<T>>` from FluentAssertions. Placing it in the `FluentAssertions` namespace (with `#pragma disable IDE0130`) means callers only need `using FluentAssertions;` — no extra using statement for the integration package.

The extension method `Should()` is defined on `Ensure<T>` (a struct), not on `object`, so it won't interfere with FluentAssertions' own `Should()` on arbitrary objects.

---

## Performance Characteristics

| Concern | Detail |
|---|---|
| `Ensure<T>` allocation | Zero allocation on valid values (readonly struct) |
| `StepDefinition` | Internal `record`, allocated once per step registration |
| `_steps` list | `List<StepDefinition>` — amortised O(1) append |
| Context dictionary | `Dictionary<string, object>` — standard hash map |
| CamelToSentence regex | Source-generated (`[GeneratedRegex]`), compiled once |
| Step execution | Sequential, no parallelism — predictable ordering |
| Output | Synchronous `WriteLine` calls — no buffering |

For high-throughput unit test suites: each `ScenarioRunner` instance allocates one list and one dictionary. For integration tests with async I/O, the overhead is negligible relative to the I/O cost.

---

## Extension Points

| Extension point | How |
|---|---|
| Output framework | Implement `IScenarioOutput` |
| Custom assertions | Standard FluentAssertions extension methods on `Ensure<T>` |
| Context access | `IRunnerContext` is public; can be received via DI if needed |
| Step sharing | Static methods / classes accepting `IStepRunner` |

There is intentionally no plugin or hook system. The library stays thin; advanced behaviour is composed using ordinary C# methods.

---

## What This Library Is Not

- **Not a BDD feature-file parser.** There are no `.feature` files or Gherkin. Descriptions are plain strings or method names.
- **Not a test runner.** It runs inside xUnit, NUnit, MSTest, or any other runner. It does not discover or execute tests.
- **Not parallel-safe within a single scenario.** Steps are sequential by design. If you need parallel assertions, do them inside a single step.
- **Not a production runtime component.** It belongs exclusively in test projects.
