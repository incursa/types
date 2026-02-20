---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/quality-gate-execution-plan.md"
  path: /docs/quality-gate-execution-plan.md
---

# Quality Gate Execution Plan

This document tracks the six-step cleanup and hardening workstream.

## Current repository snapshot
- Branch: `main`
- Working tree: clean after grouped commits and quality-gate ratchets
- Goal: split into reviewable commits while finishing coverage/spec/mutation quality work

## Step tracker

| Step | Objective | Status | Notes |
| --- | --- | --- | --- |
| 1 | Reduce branch noise and define commit grouping | Completed | Added file-group plan and staging helper script. |
| 2 | Address coverage gap vs CI gate | Completed | Coverage now 70.07% line / 56.82% branch (`72.0%`/`60.2%` by reportgenerator); CI ratcheted baseline set to 65% / 50%. |
| 3 | Complete spec traceability for all types | Completed | `docs/spec/test-traceability.md` now includes entries for all public concrete types. |
| 4 | Finalize release docs | Completed | `CHANGELOG.md` and `docs/migration-v2.md` updated for current behavior changes. |
| 5 | Harden spec gate to validate structure/content | Completed | Spec verifier now checks metadata, sections, placeholders, and control characters. |
| 6 | Run mutation testing and capture actions | Completed | Stryker wave-B core (`Money`, `JsonContext`, `MonthOnly`) kills `113`/`114` tested mutants (score `58.25%`). Expanded scope candidate (`+ Percentage + TimeZoneId`) kills `151`/`157` tested mutants (score `54.91%`), and CI mutation scope now includes all five files at ratcheted thresholds. |

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

## Mutation follow-up (from `artifacts/stryker-wave-b`)
- Previous observed result: `Killed: 0`, `Survived: 282`, score `0.00%`.
- Wave A result: `Killed: 23`, `Survived: 8`, score `21.50%` on `Money.cs`.
- Wave B previous result: `Killed: 58`, `Survived: 18`, score `31.18%` on `Money.cs` + `JsonContext.cs` + `MonthOnly.cs`.
- Wave B tightened result: `Killed: 113`, `Survived: 1`, score `58.25%` on `Money.cs` + `JsonContext.cs` + `MonthOnly.cs`.
- Expanded scope result: `Killed: 151`, `Survived: 6`, score `54.91%` on `Money.cs` + `JsonContext.cs` + `MonthOnly.cs` + `Percentage.cs` + `TimeZoneId.cs`.
Actions completed:
1. Normalize test runner compatibility by removing mixed xUnit v3 package overrides in test projects.
2. Keep Stryker file logging (`-L`) in CI and publish mutation artifacts for diagnostics.
3. Expand CI mutation scope from `Money.cs` to wave B files with ratcheted break threshold.
4. Add wave-1 low-coverage type tests and expand traceability mapping for parse/format/convert contracts.
5. Approve wave-1 specifications (`JsonContext`, `MonthOnly`, `BvFile`, `EmailAddress`, `EncryptedString`, `TimeZoneId`, `Url`).
6. Add mutation-targeted assertions for strict comparison operators, converter fallbacks, and parse-boundary behavior in `Money`, `JsonContext`, and `MonthOnly`.
7. Raise CI mutation threshold from `15` to `35` (`threshold-high` to `55`) for sustained enforcement.
8. Pilot `Percentage` and `TimeZoneId` mutation runs, harden targeted assertions, and expand CI mutation scope to include both files.
Next ratchet targets:
1. Raise mutation wave-B break threshold above `35` once sustained score exceeds `60` across multiple PRs.
2. Raise mutation break threshold to `40` once expanded five-file scope sustains `>=55` score for multiple PRs.
