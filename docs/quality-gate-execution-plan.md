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
| 6 | Run mutation testing and capture actions | Completed | Stryker wave B scope (`Money`, `JsonContext`, `MonthOnly`) now kills mutants (score 31.18%), and CI mutation gate is scoped/ratcheted. |

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
- Wave B result: `Killed: 58`, `Survived: 18`, score `31.18%` on `Money.cs` + `JsonContext.cs` + `MonthOnly.cs`.
Actions completed:
1. Normalize test runner compatibility by removing mixed xUnit v3 package overrides in test projects.
2. Keep Stryker file logging (`-L`) in CI and publish mutation artifacts for diagnostics.
3. Expand CI mutation scope from `Money.cs` to wave B files with ratcheted break threshold.
4. Add wave-1 low-coverage type tests and expand traceability mapping for parse/format/convert contracts.
5. Approve wave-1 specifications (`JsonContext`, `MonthOnly`, `BvFile`, `EmailAddress`, `EncryptedString`, `TimeZoneId`, `Url`).
Next ratchet targets:
1. Raise mutation wave-B break threshold above `15` once sustained score exceeds `35`.
2. Add `Percentage` and `TimeZoneId` to mutation scope after additional assertion hardening.
