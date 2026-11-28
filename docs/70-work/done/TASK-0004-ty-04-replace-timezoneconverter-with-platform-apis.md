---
id: TASK-0004
type: task
status: done
priority: critical
owner: platform
created: 2026-11-27
updated: 2026-11-27
tags: []
related:
  specs:
    - /docs/spec/types/TimeZoneId.md
    - /docs/spec/compat-decisions.md
    - /docs/spec/test-traceability.md
    - /docs/migration-v2.md
  adrs: []
  files: []
  prs:
    - "https://github.com/incursa/types/pull/7"
  issues: []
  branches:
    - users/samuelmcaravey/ty-04-platform-timezones
title: TY-04 replace TimeZoneConverter with platform APIs
---

# TASK-0004 - TY-04 replace TimeZoneConverter with platform APIs

## Summary

Replace the bundled TimeZoneConverter mapping dependency with the .NET 8
`TimeZoneInfo` APIs and supported host ICU/CLDR data. Preserve accepted
Windows-to-IANA canonicalization and IANA alias behavior, while making missing
platform data an explicit failure instead of silently substituting a host
identifier. The slice starts from merged TY-03 commit
`9218d2d6e7e5e8d1e5b8376b2215815dd2409e72`.

## Normative inputs

Current live requirements were fetched from SpecAttest project `incursa-types`
before implementation. The governing draft candidates are:

- `REQ-TYPES-COMPAT-0012`: replace TimeZoneConverter with supported platform
  APIs without weakening the established resolution contract.
- `REQ-TYPES-COMPAT-0013`: cover representative Windows IDs, canonical IANA
  IDs, aliases, and both Windows and Linux hosts.
- `REQ-TYPES-COMPAT-0014`: report unsupported platform/data conditions and
  never substitute a host-local fallback.
- `REQ-TYPES-PKG-0016` and `REQ-TYPES-PKG-0018`: keep rich boundaries in the
  full package and target .NET 8.

The live records remain draft. This work records implementation evidence; it
does not promote or otherwise mutate the requirements.

## Implementation

- Resolve identifiers with `TimeZoneInfo.FindSystemTimeZoneById`.
- Canonicalize Windows identifiers through
  `TimeZoneInfo.TryConvertWindowsIdToIanaId`; preserve accepted IANA identifiers
  and aliases exactly as supplied.
- If a host omits an accepted IANA alias link file, resolve its platform CLDR
  mapping through IANA-to-Windows-to-canonical-IANA conversion while retaining
  the caller's alias as `CanonicalId`.
- Report unavailable ICU/CLDR data with `PlatformNotSupportedException`,
  invalid input with `ArgumentException`, corrupt data with
  `InvalidTimeZoneException`, and inaccessible host data with an explicit
  platform failure.
- Remove the TimeZoneConverter package reference.
- Add Windows/Linux CI coverage plus a separate invariant-globalization
  process proving the missing-data contract.
- Document the host-data compatibility boundary and migration impact.

## Requirement disposition

| Requirement(s) | TY-04 evidence or disposition |
| --- | --- |
| `REQ-TYPES-COMPAT-0012` | `TimeZoneId` uses only .NET 8 platform APIs; package assets no longer reference TimeZoneConverter. |
| `REQ-TYPES-COMPAT-0013` | Focused tests cover Pacific/Eastern/UTC Windows IDs, canonical IANA IDs, `US/Pacific`, invalid input, and conversion APIs in a Windows/Linux CI matrix. |
| `REQ-TYPES-COMPAT-0014` | Invalid IDs never fall back; an isolated invariant-globalization run requires an explicit `PlatformNotSupportedException` mentioning ICU/CLDR. |
| `REQ-TYPES-PKG-0016` | `TimeZoneId` remains a rich-package-only boundary. |
| `REQ-TYPES-PKG-0018` | Implementation and platform conversion APIs compile against the repository's .NET 8 target. |

## Verification

- Release restore/build: pass, zero warnings/errors.
- Focused `TimeZoneId` test run on Windows: 15 passed, 0 failed.
- Isolated invariant-globalization test process: 1 passed, 0 failed.
- Full Release solution: 279 passed, 0 failed, 0 skipped.
- Coverage: 73.67% line and 60.13% branch overall, above the 65%/50%
  gates.
- Rich Types mutation gate (`JsonContext`, `MonthOnly`, `TimeZoneId`): 62.16%
  (69 killed, 5 survived, 0 timeouts/errors), above the 35% break threshold
  after the Linux alias-resolution correction.
- The packed full-package nuspec and restore assets contain no
  TimeZoneConverter dependency.
- PR #7 connected validation: Workbench Verify passed; Windows and Ubuntu
  platform/invariant jobs passed; full verify, coverage, deterministic
  packaging, package-only consumer, both mutation scopes, and the aggregate
  Quality Report passed.

## Acceptance criteria

- [x] Windows and Linux CI prove representative Windows IDs, canonical IANA
  IDs, and aliases.
- [x] TimeZoneConverter is absent from restored and packed dependency assets.
- [x] Requirements, implementation, tests, compatibility decision, and
  migration guidance are traceable in the delivery report.
