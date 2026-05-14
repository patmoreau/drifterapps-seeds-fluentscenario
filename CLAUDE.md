# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**DrifterApps.Seeds.FluentScenario** is a C# library providing a fluent BDD (Behavior-Driven Development) scenario testing framework. It enables developers to write expressive test scenarios using a Given-When-Then pattern with a clean, chainable API.

The library has two main NuGet packages:
- **DrifterApps.Seeds.FluentScenario**: Core BDD framework
- **DrifterApps.Seeds.FluentScenario.FluentAssertions**: Integration with FluentAssertions for extended assertion capabilities

## Build, Test, and Lint Commands

### Build
```bash
dotnet build                                    # Build all projects (Debug)
dotnet build --configuration Release            # Build in Release mode
dotnet build --framework net10.0                # Build specific framework
```

### Test
```bash
dotnet test                                     # Run all tests
dotnet test --configuration Release             # Test in Release mode
dotnet test --framework net10.0                 # Test specific framework
dotnet test --filter "ClassName=TestClass"     # Run specific test class
dotnet test --filter "FullyQualifiedName~TestMethodName"  # Run specific test
dotnet test --logger "console;verbosity=detailed"  # Verbose output
```

### Code Quality
```bash
dotnet format                                   # Check code formatting
dotnet format --verify-no-changes               # Verify formatting without changes
dotnet format --severity error                  # Check for formatting errors only
```

### Pack NuGet Packages
```bash
dotnet pack --configuration Release             # Create NuGet packages
dotnet pack --output ./nuget-packages           # Pack to specific directory
```

## Project Structure

```
src/
├── FluentScenario/                    # Core library
│   ├── ScenarioRunner.cs              # Main entry point (partial class)
│   ├── ScenarioRunner.*.cs            # Partial classes: Given, When, Then, And, StepRunner, RunnerContext
│   ├── IScenarioRunner.cs             # Public interface with all scenario methods
│   ├── IStepRunner.cs                 # Interface for step execution within runner methods
│   ├── IRunnerContext.cs              # Shared context interface (SetContextData/GetContextData)
│   ├── Ensure.cs                      # Value wrapper for type-safe parameter passing
│   ├── StepDefinition.cs              # Internal step representation
│   └── IScenarioOutput.cs             # Output abstraction (WriteLine methods)
│
└── FluentScenario.FluentAssertions/   # Extension library
    └── EnsureAssertionsExtensions.cs # Extension methods integrating with FluentAssertions

tests/
└── FluentScenario.Tests/
    ├── Samples/                       # Usage examples demonstrating API patterns
    │   ├── ScenarioBasicsTests.cs
    │   ├── ScenarioWithExecutionMethodsTests.cs
    │   ├── ScenarioWithWellNamedMethodsTests.cs
    │   └── ScenarioWithParametersAndResultsTests.cs
    ├── *Tests.cs                      # Unit tests for core classes
    ├── Mocks/                         # Test doubles
    └── Extensions/                    # Test helpers
```

## Architecture and Key Design Patterns

### Fluent Builder Pattern
The library uses a fluent builder pattern via `ScenarioRunner`. The main entry point is static `ScenarioRunner.Create()` methods which return an `IScenarioRunner`. Calling `.Given()`, `.When()`, `.Then()`, or `.And()` methods chains steps, with `.PlayAsync()` executing them sequentially.

### Partial Class Organization
`ScenarioRunner` is split across multiple files to manage complexity:
- **ScenarioRunner.cs**: Core logic (context, step execution, output)
- **ScenarioRunner.Given/When/Then/And.cs**: Overloads for each BDD keyword (each has 15+ overloads supporting different signatures)
- **ScenarioRunner.StepRunner.cs**: `IStepRunner` implementation for execution within runner methods
- **ScenarioRunner.RunnerContext.cs**: Shared context data storage

### The `Ensure<T>` Value Wrapper
`Ensure<T>` is a readonly struct that wraps values passed between steps. It provides type-safe validation:
- `IsValid`: Boolean indicating if the wrapped value matches type `T`
- `Value`: Property that throws `InvalidOperationException` if not valid
- Implicit conversion operators allow treating it as `T` directly
- Enables integration with FluentAssertions for validation within scenarios

### Step Execution Flow
1. Steps are collected in a `List<StepDefinition>` during builder calls
2. `.PlayAsync()` executes them sequentially, passing the previous step's result as input to the next
3. Results wrap values in `Ensure<T>` for type-safe consumption
4. Each step result is written to `IScenarioOutput` as success (✓) or failure (✗)
5. Exceptions in any step are caught, logged, and rethrown

### Method Overload Strategy
Each scenario keyword (Given, When, Then, And) supports many overloads:
- **With description + action**: `Given(string description, Action step)`
- **With description + typed return**: `Given<T>(string description, Func<T> step)`
- **With description + async**: `Given(string description, Func<Task> step)`
- **With description + Ensure input + output**: `Given<T, T2>(string description, Func<Ensure<T>, T2> step)`
- **With CallerMemberName**: `Given(Action step, [CallerMemberName] string description = "")` - uses method name as description

This allows developers to write tests with or without explicit descriptions and with or without step return values.

### Call Method Name to Description Conversion
When using the `CallerMemberName` overloads, method names are converted from camelCase to sentences:
- `WhenTheWeatherIsTooCold()` → "when the weather is too cold"
- Implemented via `CamelToSentenceRegex()` (partial method with generated regex)

### IScenarioOutput Abstraction
Tests inject implementations of `IScenarioOutput` for output handling:
- xUnit tests use a wrapper around `ITestOutputHelper`
- Allows custom implementations for different test frameworks

## Technology Stack

- **.NET**: Targets net8.0, net9.0, and net10.0 (managed via Directory.Build.props)
- **Testing Framework**: xUnit v3
- **Assertions**: FluentAssertions 7.2.2
- **Mocking**: NSubstitute 5.3.0
- **Code Generation**: Source generators (for regex patterns)
- **Test Data**: Bogus (fake data generator)
- **Code Coverage**: Coverlet

## Configuration Files

### Directory.Build.props
Centralized build settings applied to all projects:
- TargetFramework: net10.0 (base target)
- LangVersion: latest
- ImplicitUsings: enabled
- Nullable: enabled
- TreatWarningsAsErrors: true
- CodeAnalysisTreatWarningsAsErrors: true
- AnalysisMode: All (exhaustive analyzer checks)
- ManagePackageVersionsCentrally: true (via Directory.Packages.props)
- GenerateDocumentationFile: true (XML docs for IntelliSense)
- NuGet metadata (authors, license, repository URLs)

### Directory.Packages.props
Centralized package version management. All package versions defined here, referenced projects use simple `<PackageReference>` without versions.

### global.json
Specifies .NET 10.0.300 SDK with latestMinor rollforward strategy.

### GitVersion.yml
Semantic versioning configuration:
- Mode: ContinuousDelivery
- Scheme: MajorMinorPatch
- Commit message patterns for version bumps:
  - Breaking/major: `+semver: breaking` or `+semver: major`
  - Feature/minor: `+semver: feature` or `+semver: minor`
  - Fix/patch: `+semver: fix` or `+semver: patch`

## Code Style and Standards

Defined in `.editorconfig`:
- **Indentation**: 4 spaces, LF line endings
- **Expression-bodied members**: Preferred for properties, methods, accessors
- **Braces**: Required for all statements
- **Null checking**: Prefer null-check patterns over type checks
- **Pattern matching**: Enabled
- **Var usage**: Encouraged for built-in types when clear

Notable disabled diagnostics:
- CA1000: Static members on generic types (acceptable pattern here)
- IDE0130: Namespace format (file-scoped namespaces disabled)
- Some Sonarqube rules disabled

Strict enforcement:
- TreatWarningsAsErrors: true
- AnalysisLevel: latest

## CI/CD Pipeline

### Workflows (GitHub Actions)

**ci-cd.yml**: Main build and publish pipeline
- Triggers: push to main, PRs to main, release creation
- Jobs:
  1. `check_changes`: Determines if code changed (skips unnecessary steps)
  2. `set-version`: Uses GitVersion to determine semantic version
  3. `build`: Builds and tests on .NET 10.0
     - Generates test badges (pass/fail counts)
     - Uploads to Gist for display in README
  4. `tagging`: Creates git tags for versions on main
  5. `publish`: Publishes NuGet packages on release
  6. `create_check`: Reports check status to PR

**linter.yml**: Code formatting checks
- Runs `dotnet format --severity error` on C# files
- Fails if formatting issues found
- Uploads diagnostic report

**codeql-analysis.yml**: Security scanning (workflow exists, details not shown)

## Testing Patterns

### Sample Test Scenarios
The `Samples/` directory demonstrates the library's API:
1. **ScenarioBasicsTests**: Simple Given-When-Then with inline lambdas
2. **ScenarioWithExecutionMethodsTests**: Shared step methods via `IStepRunner.Execute()`
3. **ScenarioWithWellNamedMethodsTests**: Method names become descriptions (CallerMemberName)
4. **ScenarioWithParametersAndResultsTests**: Passing values between steps with Ensure<T>

### Test Organization
- Custom attribute `[UnitTest]` marks unit tests
- Tests use `ITestOutputHelper` from xUnit, wrapped in `SampleScenarioOutput`
- Each test class receives output helper via constructor injection

### Key Test Classes
- `ScenarioRunnerTests`: Tests scenario execution flow
- `StepRunnerTests`: Tests step definition and execution within runners
- `RunnerContextTests`: Tests context data storage/retrieval
- `EnsureTests`: Tests type-safe value wrapping
- `EnsureAssertionsExtensionsTests`: Tests FluentAssertions integration

## Important Development Notes

### Generic Overload Explosion
The library has ~60 method overloads per keyword (Given, When, Then, And) to support:
- Sync/async execution
- With/without description
- No input/With typed input
- No output/With typed output
- All combinations of above

This is intentional for API flexibility. When adding methods, ensure all meaningful combinations are included.

### Ensure<T> Usage
`Ensure<T>` is core to the framework's type safety. Always use it when:
- Passing values between steps
- Accepting input in step methods
- Integrating with assertions

Never directly cast `object?` in step methods; use `Ensure<T>.From()` for proper validation.

### XML Documentation
All public APIs must have XML documentation comments (enforced by GenerateDocumentationFile).
Use standard summary/param/returns tags for IntelliSense and NuGet package documentation.

### Partial Classes
When adding new overloads, add them to the appropriate ScenarioRunner.*.cs file:
- Step execution logic → ScenarioRunner.cs
- Given/When/Then/And overloads → ScenarioRunner.Given/When/Then/And.cs
- IStepRunner implementation → ScenarioRunner.StepRunner.cs

Do NOT add all code to ScenarioRunner.cs; maintain file organization.

### Testing New Features
New features must have:
1. Unit tests in `Tests/` (not in `Samples/`)
2. Sample/demonstration test if it's a new API pattern (in `Samples/`)
3. All frameworks tested: net8.0, net9.0, net10.0
4. Code coverage maintained (coverlet integrated)

## Dependencies to Know

### FluentAssertions Integration
The `FluentScenario.FluentAssertions` package extends `Ensure<T>` with assertion methods:
- `EnsureAssertionsExtensions.cs` provides `.Should()` method on `Ensure<T>`
- Allows: `input.Should().BeValid().And.Subject.Value.Should().Be(expected)`
- Optional dependency; core library doesn't require it

### Xunit
Tests use xUnit v3 with modern patterns:
- `[Fact]` for parameterless tests
- `[Theory]` with `[InlineData]` for parameterized tests
- `ITestOutputHelper` for console output (injected via constructor)

## Performance Considerations

- `StepDefinition`: Internal record, lightweight
- `Ensure<T>`: Readonly struct, zero-allocation when valid
- Step execution: Sequential, no parallelization
- Context storage: Dictionary<string, object>, minimal allocations
- Output: Single-threaded, synchronous WriteLine calls

For high-volume scenario testing, be aware that each scenario creates a fresh ScenarioRunner instance with new step lists.
