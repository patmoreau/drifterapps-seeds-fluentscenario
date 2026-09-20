@AGENTS.md

## Claude Code

The instructions above apply in full. A few Claude-specific notes:

- Verify against the source, not from memory of this file — `src/FluentScenario/*.cs` and
  the XML doc comments are the truth. Documentation in `docs/` can drift; when it
  disagrees with the code, the code wins and the doc is a bug to fix.
- Prefer `dotnet build` / `dotnet test` from the repo root over per-project invocations;
  the suite is small enough to run after every change, as the TDD cycle requires.
- Before proposing a new package, check `Directory.Packages.props` — it may already be
  there. Adding one needs approval either way.
- When adding a keyword overload, grep all four of `ScenarioRunner.Given/When/Then/And.cs`
  plus `ScenarioRunner.StepRunner.cs` — the overload sets are meant to stay identical.
- Commits: conventional-commit format from `docs/contributing/git-instructions.md`,
  structural and behavioral changes never mixed, and never push.
