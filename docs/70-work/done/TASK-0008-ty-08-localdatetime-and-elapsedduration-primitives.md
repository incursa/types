---
id: TASK-0008
type: task
status: done
priority: critical
owner: platform
created: 2026-11-27
updated: 2026-11-27
tags: []
related:
  specs:
    - /docs/spec/types/ElapsedDuration.md
    - /docs/spec/types/LocalDateTime.md
  adrs: []
  files: []
  prs:
    - "https://github.com/incursa/types/pull/11"
  issues: []
  branches:
    - users/samuelmcaravey/ty-08-temporal-primitives
title: TY-08 LocalDateTime and ElapsedDuration primitives
---

# TASK-0008 - TY-08 LocalDateTime and ElapsedDuration primitives

## Summary

Add two dependency-free exact temporal primitives without changing the
existing calendar-relative `Duration` or `Period`: signed Int64 100ns
`ElapsedDuration`, and Gregorian zone-free 100ns `LocalDateTime` with explicit
offset/named-zone resolution. The branch starts from merged TY-07 commit
`c11ba7d6c069a44cc0719cc735514dd27f06b896`.

Current SpecAttest candidates `REQ-TYPES-TIME-0001..0008`, `0025..0033`,
`0044`, and `0046..0047` were fetched before implementation. All remain draft
revision 1 / record version 1; this work records evidence without promoting
requirement lifecycle state.

## Requirement disposition

| Requirement(s) | TY-08 evidence |
| --- | --- |
| TIME-0001 | Separate public `ElapsedDuration` and `LocalDateTime`; legacy calendar `Duration`/`Period` untouched. |
| TIME-0002..0005 | Signed Int64 units, canonical zero, checked arithmetic, and exact equality/order. |
| TIME-0006..0008 | Seconds-only canonical grammar through seven digits; excess precision/calendar units rejected; exact TimeSpan boundary. |
| TIME-0025..0029 | Unspecified Gregorian fields, years 0001..9999, 100ns resolution, fixed seven-digit canonical text, field-only identity/order. |
| TIME-0030 | Exact parser rejects invalid dates, 24:00, leap seconds, and offset/zone suffixes. |
| TIME-0031 | Instant conversion exists only through explicit offset or `TimeZoneInfo`. |
| TIME-0032..0033 | Named-zone conversion rejects ambiguous and nonexistent local times by default. |
| TIME-0044 | Behavioral specs identify exact ISO-inspired subsets and deviations without claiming full ISO conformance. |
| TIME-0046 | Portable fixtures cover fractional/negative/bounds/leap days/offsets/DST ambiguity/nonexistence; Period cases remain TY-09 scope. |
| TIME-0047 | Shared primitive tick/value/canonical contracts are explicit for Tabular; current Tabular requires exact Int64/string application mapping, while native adapter qualification remains TY-10. |

## Verification

- Portable JSONL fixtures cover signed elapsed boundaries, fractional seconds,
  Gregorian range, leap days, and canonical 100ns encodings.
- Tests cover checked overflow, exact TimeSpan conversion, JSON, invalid
  calendar/time/offset forms, explicit offsets, named-zone unambiguous
  resolution, and default rejection of New York DST ambiguity/gaps.
- Read-only inspection of current `incursa/tabular-dotnet` confirms its closed
  scalar contract excludes `DateTime`/`TimeSpan`/duration objects and requires
  explicit application mapping. Specs and fixtures therefore define exact
  signed-unit and local-tick Int64 mappings plus canonical strings without
  claiming native admission before TY-10.
- `dotnet test Incursa.Core.slnx -c Release`: 456 passed, 0 failed, 0
  skipped using `DOTNET_ROLL_FORWARD=Major` on this workstation; connected CI
  supplies the pinned .NET 8 runtime.
- Coverage gate passed at 76.50% line / 63.51% branch overall (65% / 50%
  thresholds); ElapsedDuration measured 83.03% line / 85.52% branch and
  LocalDateTime 86.51% / 75.00%.
- Warn-as-error deterministic build, formatting, Workbench strict validation,
  type-spec verification, and diff checks pass.
- Package-only nupkg restore/build/run consumes both primitives and passes with
  zero runtime package dependencies.
- PR #11 connected validation passed Workbench, Windows/Ubuntu combined
  timezone/temporal matrix, warn-as-error build, 456 tests, coverage gates,
  package-only consumer, deterministic packaging, both mutation scopes, and
  aggregate Quality Report.

## Acceptance criteria

- [x] Portable fixtures cover range, precision, signed elapsed values, offsets,
  DST ambiguity, and invalid local times.
- [x] Primitives API is independently consumable and matches the documented
  Tabular exact scalar profiles.
- [x] Requirements map to code/tests or explicit TY-09/TY-10 deferral and
  Release solution plus connected CI results are recorded.
