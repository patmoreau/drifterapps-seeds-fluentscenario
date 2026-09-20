---
name: add-keyword-overload
description: Add a step overload to Given, When, Then, And and IStepRunner, keeping all overload sets identical
---

# Add a step overload

> **Audience:** contributors *working on* this repository. Not packaged — see `AGENTS.md` at the repository root.

Every BDD keyword in this library exposes the **same** overload set. Adding an
overload to one keyword and not the others is the most common way this API goes
inconsistent, so this task is done for all four keywords at once or not at all.

Ask the user which signature to add before starting if it is not already stated
(sync or async, with or without a description, with or without an `Ensure<T>`
input, with or without a typed output).

## 1. Find the shape to copy

Read the closest existing overload in `src/FluentScenario/ScenarioRunner.Given.cs`.
Overloads come in families:

- no input, no output — `Given(string description, Action step)`
- no input, typed output — `Given<T>(string description, Func<T> step)`
- `Ensure<T>` input, no output — `Given<T>(string description, Action<Ensure<T>> step)`
- `Ensure<T>` input, typed output — `Given<T, T2>(string description, Func<Ensure<T>, T2> step)`
- `[CallerMemberName]` variants of each, which convert the method name to a
  sentence through `CamelToSentence`
- the `IStepRunner` delegation overload

Copy the closest family member rather than inventing a new call shape.

## 2. Add it in every place

Structural change first, and all of these in the same change:

| File | What to add |
|---|---|
| `src/FluentScenario/IScenarioRunner.cs` | the interface declaration, with full XML docs |
| `src/FluentScenario/ScenarioRunner.Given.cs` | the `Given` implementation |
| `src/FluentScenario/ScenarioRunner.When.cs` | the `When` implementation |
| `src/FluentScenario/ScenarioRunner.Then.cs` | the `Then` implementation |
| `src/FluentScenario/ScenarioRunner.And.cs` | the `And` implementation |
| `src/FluentScenario/IStepRunner.cs` | the matching `Execute` declaration |
| `src/FluentScenario/ScenarioRunner.StepRunner.cs` | the matching `Execute` implementation |

Keep each implementation in the partial that matches its keyword. Do not move
step-execution logic out of `ScenarioRunner.cs`.

`GenerateDocumentationFile` is on and warnings are errors, so every new public
member needs `<summary>`, `<param>`, `<typeparam>` and `<returns>` docs.

## 3. Verify parity mechanically

The four keyword files must hold the same signatures once the keyword name is
normalized away:

```bash
for k in Given When Then And; do
  grep -oE '^    public [^(]+\([^)]*\)' src/FluentScenario/ScenarioRunner.$k.cs \
    | sed "s/ $k/ KEYWORD/; s/ $k</ KEYWORD</" | sort > "/tmp/parity-$k.txt"
done
for k in When Then And; do
  diff "/tmp/parity-Given.txt" "/tmp/parity-$k.txt" || echo "MISMATCH in $k"
done
```

Every diff must be empty. A mismatch means one keyword was missed.

## 4. Test it

- A unit test per new overload in `tests/FluentScenario.Tests/` — cover the
  keyword itself, not only `Given`, when the overload changes dispatch.
- A demonstration in `tests/FluentScenario.Tests/Samples/` **only** if this is a
  new API *pattern* rather than one more member of an existing family.
- Follow the suite's conventions: `[UnitTest]` on the class, a `Faker` for
  random data, `// arrange` / `// act` / `// assert` sections, behavior-named
  test methods.

Write the failing test first, per the engineering principles.

## 5. Document it

- `docs/API.md` — add the signature to the overload tables.
- `llms.txt` — extend the overload list if the new shape is not already covered.
- `docs/EXAMPLES.md` and `examples/` — only if this introduces a new pattern.

## 6. Check before committing

```bash
dotnet build                                          # must report 0 warnings
dotnet test                                           # whole suite green
dotnet format --verify-no-changes --severity error    # what CI enforces
```

Commit structural and behavioral changes separately, conventional-commit
format, and do not push. See `docs/contributing/git-instructions.md`.

<!-- probe: stale on purpose -->
