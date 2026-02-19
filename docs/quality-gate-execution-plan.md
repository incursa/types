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
| 2 | Address coverage gap vs CI gate | Completed | Coverage now 57.24% line / 29.52% branch; CI ratcheted baseline set to 52% / 29%. |
| 3 | Complete spec traceability for all types | Completed | `docs/spec/test-traceability.md` now includes entries for all public concrete types. |
| 4 | Finalize release docs | Completed | `CHANGELOG.md` and `docs/migration-v2.md` updated for current behavior changes. |
| 5 | Harden spec gate to validate structure/content | Completed | Spec verifier now checks metadata, sections, placeholders, and control characters. |
| 6 | Run mutation testing and capture actions | Completed | Scoped Stryker run executed; score 0.00%, follow-up listed below. |

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

## Mutation follow-up (from `artifacts/stryker-scoped`)
- Observed result: `Killed: 0`, `Survived: 282`, score `0.00%`.
Immediate actions:
1. Verify Stryker test-discovery/execution path for xUnit v3 project configuration. Status: Completed locally via `scripts/run-stryker-pilot.ps1` (`Money.cs`) with runner logs under `artifacts/stryker-pilot/logs`.
2. Enable Stryker file logging (`-L`) in CI and publish full logs for diagnosis. Status: Completed in quality workflow.
3. Start with a single-file pilot (`Money.cs`) and enforce non-zero mutation score before broad rollout.
4. Improve assertions for `Money`, `JsonContext`, `MonthOnly`, `ShortCode`, `VirtualPath`, and `Locale`.
