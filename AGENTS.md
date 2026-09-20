# FluentScenario — Agent Instructions

## Engineering principles
@docs/contributing/engineering-principles.md

## Git workflow
@docs/contributing/git-instructions.md

## What this repository is

This repo **is** the source of `DrifterApps.Seeds.FluentScenario` — a fluent BDD
(Given-When-Then) scenario framework for .NET test suites. You are working *on* the
library, not merely *with* it.

| Path | Contents |
|---|---|
| `src/FluentScenario/` | The library: `ScenarioRunner` (partials), `IScenarioRunner`, `IStepRunner`, `IRunnerContext`, `Ensure<T>`, `StepDefinition`, `IScenarioOutput` |
| `src/FluentScenario.FluentAssertions/` | Assertions package: `Ensure<T>.Should()` → `BeValid()`, `BeInvalid()`, `HaveValue()`, `BeNull()`, `NotBeNull()` |
| `tests/FluentScenario.Tests/` | xUnit v3 test suite (the only test project) |
| `tests/FluentScenario.Tests/Samples/` | Executable demonstrations of each API pattern |
| `examples/` | Standalone usage examples referenced from the docs |
| `docs/`, `README.md`, `llms.txt` | Published documentation, for people **using** the library — packed into the NuGet package, see "Documentation is part of the API" below |
| `docs/contributing/` | Process docs, for people **working on** this repo — never packed |

Assembly/namespace: `DrifterApps.Seeds.FluentScenario`. Target: `net10.0` only —
`Directory.Build.props` owns `TargetFramework`; individual projects must not override it.
SDK pinned in `global.json` (10.0.401, `rollForward: latestMinor`).

## Build and test

```bash
dotnet build                      # whole solution
dotnet test                       # whole suite — fast, always run it after each change
dotnet format --verify-no-changes --severity error   # what the linter workflow enforces
dotnet format --severity error                       # same check, but fixes in place
```

Test runner is **Microsoft.Testing.Platform** (`global.json` → `test.runner`), with
xUnit v3 (`xunit.v3.mtp-v2`). There are no long-running tests to exclude — run them all.

### The build is strict

`Directory.Build.props` sets `TreatWarningsAsErrors`, `CodeAnalysisTreatWarningsAsErrors`,
`AnalysisMode=All`, `EnableNETAnalyzers`, `EnforceCodeStyleInBuild`, and `Nullable=enable`.
A warning **is** a build failure, so "all warnings resolved" in the git workflow is
satisfied by a clean `dotnet build` — never silence one with a blanket `NoWarn`.
Suppress a rule only at the narrowest scope with a justification, the way
`#pragma warning disable CA1515` is scoped around `UnitTestAttribute`.

`GenerateDocumentationFile` is on: every public member needs an XML doc comment.

## Package management

Versions are centralized — `ManagePackageVersionsCentrally=true`. Add a
`<PackageReference Include="X" />` with **no** `Version` attribute to the csproj, and the
`<PackageVersion>` to `Directory.Packages.props`. Per the engineering principles, do not
add a new dependency without approval.

`FluentAssertions` is pinned to 7.x deliberately (8.x changed its license). Do not bump it
to 8 or beyond.

## Test conventions

Match the existing suite:

- Class carries `[UnitTest]` (the local `ITraitAttribute` adding trait `Category=Unit`).
- Global usings already cover `Bogus`, `DrifterApps.Seeds.FluentScenario`,
  `FluentAssertions`, `NSubstitute`, `Xunit` — do not re-import them per file.
- Random data comes from a `private readonly Faker _faker = new();`, never hard-coded
  literals, unless the literal is the thing under test.
- Test names describe behavior:
  `GivenEnsure_WhenNonNullValue_ThenShouldBeValid`.
- Bodies use explicit `// arrange` / `// act` / `// assert` sections.
- Parameterized cases use `public static TheoryData<…>` properties with `[Theory]`.
- `IScenarioOutput` is substituted with NSubstitute in unit tests; the `Samples/` tests use
  the real xUnit `ITestOutputHelper` wrapper.
- Test files mirror the source file they cover.

New behavior needs unit tests in `tests/FluentScenario.Tests/`; a new *API pattern* also
needs a demonstration in `Samples/`. CI reports coverage with thresholds `60 80`. Follow
the TDD cycle in the engineering principles (failing test first).

## Library design constraints

See `docs/ARCHITECTURE.md` before changing any of these:

- `ScenarioRunner` is split into partials by concern: `ScenarioRunner.cs` (context, step
  execution, output), `ScenarioRunner.Given/When/Then/And.cs` (keyword overloads),
  `ScenarioRunner.StepRunner.cs` (`IStepRunner`), `ScenarioRunner.RunnerContext.cs`
  (context storage). Put new members in the partial matching their concern — do not pile
  everything into `ScenarioRunner.cs`.
- `Ensure<T>` is a `readonly struct` wrapping values passed between steps: `IsValid`,
  `Value` (throws `InvalidOperationException` when invalid), implicit conversions both
  ways. Keep it a struct. Never cast `object?` directly in a step — go through
  `Ensure<T>.From()`.
- Each keyword (`Given`, `When`, `Then`, `And`) carries the **same** overload set:
  sync/async × with/without description (`[CallerMemberName]`) × with/without `Ensure<T>`
  input × with/without typed output, plus the `IStepRunner` delegation overload. Adding an
  overload to one keyword means adding it to all four, and to `IStepRunner.Execute`.
- `[CallerMemberName]` overloads convert the method name to a sentence via the
  source-generated `CamelToSentenceRegex()`.
- Steps are collected lazily into `List<StepDefinition>`; nothing runs until `PlayAsync()`.
  Execution is sequential, each step's result feeding the next as `Ensure<T>`; a failure is
  written to `IScenarioOutput` and rethrown.
- Output goes through `IScenarioOutput` only — never `Console` — so consumers can adapt any
  test framework.

## Documentation is part of the API

`Directory.Build.props` packs `README.md`, `llms.txt`, `docs/ARCHITECTURE.md`,
`docs/AI-GUIDELINES.md`, `docs/API.md`, and `docs/EXAMPLES.md` into the NuGet package.
When the public surface changes, update in the same change:

- `docs/API.md` — signatures, overloads, conversions
- `docs/EXAMPLES.md` and `examples/` — usage patterns
- `docs/AI-GUIDELINES.md` — rules for assistants consuming the library
- `docs/ARCHITECTURE.md` — only when a design decision changes
- `README.md` and `llms.txt` — only when the overview or doc map changes
- XML doc comments on the members themselves

Treat a doc-only correction as its own `docs:` commit, separate from behavior.

## Usage reference

Do not restate the library's usage rules here — they live in, and are kept current by, the
published docs:

- **`docs/API.md`** — every type, overload set, and conversion
- **`docs/EXAMPLES.md`** — scenario patterns beyond the basics
- **`docs/AI-GUIDELINES.md`** — when to apply the pattern, anti-patterns, gotchas
- **`docs/ARCHITECTURE.md`** — why the design is what it is, and what the library will not do
- **`llms.txt`** — the condensed map of all of the above

The short version, which the code in this repo must also obey: one `ScenarioRunner` per
test; chain `Given`/`When`/`Then`/`And`; always `await PlayAsync()`; pass values between
steps as `Ensure<T>` return values *or* through `SetContextData`/`GetContextData<T>` —
pick one per scenario; assert `Ensure<T>` validity before reading `.Value`.
