---
id: TASK-0009
type: task
status: in-progress
priority: critical
owner: platform
created: 2026-11-27
updated: 2026-11-27
tags: []
related:
  specs:
    - /docs/spec/types/CalendarDuration.md
    - /docs/spec/types/ExactPeriod.md
    - /docs/migration-v2.md
    - /docs/spec/test-traceability.md
  adrs: []
  files: []
  prs:
    - "https://github.com/incursa/types/pull/12"
  issues: []
  branches:
    - users/samuelmcaravey/ty-09-calendar-duration-period
title: TY-09 strict calendar duration and exact period profiles
---

# TASK-0009 - TY-09 strict calendar duration and exact period profiles

## Summary

Add opt-in strict `CalendarDuration` and `ExactPeriod` profiles while retaining
the source, binary, behavior, and wire contracts of legacy `Duration` and
`Period`. Add explicit portable DST ambiguity/gap policies to `LocalDateTime`.

Current SpecAttest candidates `REQ-TYPES-TIME-0009..0024`, `0034..0047`, and
`REQ-TYPES-PKG-0013`, `0020` were fetched before implementation. They remain
draft revision 1 / record version 1; this task records evidence without
promoting requirement lifecycle state.

## Requirement disposition

| Requirement(s) | TY-09 evidence |
| --- | --- |
| TIME-0009..0014 | Signed integral months/days plus exact elapsed units, one-sign invariant, normalized Y/W input, exact fractional time, canonical zero/text, and structural identity. |
| TIME-0015..0019 | No context-free order; months then days then elapsed application, Gregorian clamp, explicit zone policy, and exact elapsed timeline. |
| TIME-0020..0024 | Legacy `Duration` retained; strict profile is separately named and migration is versioned by consumer schema. |
| TIME-0034..0040 | Exact half-open two-endpoint interval, reversed rejection, empty/touching rules, offset-sensitive identity, canonical offset endpoints, and exact elapsed. |
| TIME-0041..0043 | Start/elapsed and local start/calendar/zone factories with explicit resolution and reversed-result rejection. |
| TIME-0044..0047 | ISO-inspired deviations, migration compatibility dimensions, edge-case tests, and dependency-free primitive/type forwarders are documented. |
| PKG-0013, PKG-0020 | New public types are implemented in `Incursa.Types.Primitives`, forwarded through `Incursa.Types`, and included in dependency/package boundary tests. |

## Verification

- `dotnet test Incursa.Core.slnx -c Release`: 482 passed, 0 failed, 0
  skipped using `DOTNET_ROLL_FORWARD=Major` on this workstation; connected CI
  supplies the pinned .NET 8 runtime.
- Coverage gate passed at 77.46% line / 65.33% branch overall (65% / 50%
  thresholds).
- Warn-as-error Release build, formatting, Workbench strict validation,
  type-spec verification, and diff checks pass.
- Package-only nupkg restore/build/run consumes `CalendarDuration` and
  `ExactPeriod` and passes with zero runtime package dependencies.
- Connected Windows/Linux temporal validation and the full Quality Gate remain
  to be recorded from the pull request.

## Acceptance criteria

- [x] Legacy and strict profiles plus source/binary/behavior/wire migration are documented.
- [x] Current draft requirements map to implementation and automated tests.
- [x] Month-end, leap, negative, 100ns, empty/touching, offset-identity, and portable DST ambiguity/gap cases are covered.
