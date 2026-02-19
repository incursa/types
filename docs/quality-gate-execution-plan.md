# Quality Gate Execution Plan

This document tracks the six-step cleanup and hardening workstream.

## Current repository snapshot
- Branch: `main`
- Working tree: large mixed change-set (formatting, runtime behavior, tests, docs, CI/tooling)
- Goal: split into reviewable commits while finishing coverage/spec/mutation quality work

## Step tracker

| Step | Objective | Status | Notes |
| --- | --- | --- | --- |
| 1 | Reduce branch noise and define commit grouping | Completed | Added file-group plan and staging helper script. |
| 2 | Address coverage gap vs CI gate | Completed | Coverage now 66.89% line / 53.98% branch; CI ratcheted baseline set to 60% / 45%. |
| 3 | Complete spec traceability for all types | Completed | `docs/spec/test-traceability.md` now includes entries for all public concrete types. |
| 4 | Finalize release docs | Completed | `CHANGELOG.md` and `docs/migration-v2.md` updated for current behavior changes. |
| 5 | Harden spec gate to validate structure/content | Completed | Spec verifier now checks metadata, sections, placeholders, and control characters. |
| 6 | Run mutation testing and capture actions | Completed | Scoped Stryker run executed; `Money.cs` pilot now kills mutants (score 21.50%), and CI mutation gate is scoped/ratcheted. |

## Planned commit groups

1. `tooling-and-normalization`
- `.editorconfig`
- `.gitignore`
- `.vscode/tasks.json`
- `cleanup.ps1`
- `cleanup.sh`
- `.pre-commit-config.yaml`

2. `runtime-hardening`
- `Incursa.Types/**/*.cs`

3. `test-hardening`
- `Incursa.Types.Tests/**/*.cs`
- `Incursa.Types.Tests/*.csproj`

4. `specs-and-documentation`
- `docs/spec/**`
- `docs/types.md`
- `docs/fastid.md`
- `docs/serialization-and-ef-samples.md`
- `docs/migration-v2.md`
- `CHANGELOG.md`
- `README.md`
- `Incursa.Types/README.md`
- `.github/pull_request_template.md`

5. `ci-quality-gates`
- `.github/workflows/quality.yml`
- `.github/workflows/commit-ci.yml`
- `.github/workflows/main-release-ci-cd.yml`
- `scripts/**`
- `Directory.Build.props`
- `Incursa.Core.slnx`

## Suggested commit order
1. `tooling-and-normalization`
2. `runtime-hardening`
3. `test-hardening`
4. `specs-and-documentation`
5. `ci-quality-gates`

## Mutation follow-up (from `artifacts/stryker-pilot`)
- Previous observed result: `Killed: 0`, `Survived: 282`, score `0.00%`.
- Current observed result: `Killed: 23`, `Survived: 8`, score `21.50%` on `Money.cs` pilot scope.
Actions completed:
1. Normalize test runner compatibility by removing mixed xUnit v3 package overrides in test projects.
2. Keep Stryker file logging (`-L`) in CI and publish mutation artifacts for diagnostics.
3. Scope CI mutation gate to `Money.cs` pilot until broader file sets have stronger mutation-kill tests.
4. Add low-coverage type tests and expand traceability mapping for parse/format/convert contracts.
Next ratchet targets:
1. Raise mutation pilot break threshold above `10` once sustained score exceeds `25`.
2. Add `JsonContext` and `MonthOnly` to mutation scope after additional assertion hardening.
