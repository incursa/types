---
id: TASK-0001
type: task
status: done
priority: high
owner: platform
created: 2026-11-28
updated: 2026-11-28
tags: []
related:
  specs:
    - /docs/spec/README.md
    - /docs/spec/compat-decisions.md
    - /docs/spec/test-traceability.md
  adrs: []
  files: []
  prs:
    - "https://github.com/incursa/types/pull/4"
  issues: []
  branches:
    - users/samuelmcaravey/ty-01-types-baseline
title: "TY-01 public API, dependency closure, and compatibility baseline"
---

# TASK-0001 - TY-01 public API, dependency closure, and compatibility baseline

## Summary

Read-only baseline for ClickUp TY-01. This work inventories the package and its
compatibility boundaries; it intentionally makes no production-code, publishing,
signing, secret, or branch-protection changes.

The baseline is `d6ce1c8392aa76149374b08d9cdb97c66d5f2084`
(`feat: filter unsafe generated short codes`) from `origin/main` on 2026-11-27.

## Normative inputs

The current SpecAttest project is `incursa-types` (active, authority mode
`manager`). TY-01 used these live draft candidates rather than treating the
ClickUp summary as normative:

- `SPEC-TYPES-PKG-0001`, record version 2: `REQ-TYPES-PKG-0001..0020`,
  requirement revision 1.
- `SPEC-TYPES-COMPAT-0001`, record version 1:
  `REQ-TYPES-COMPAT-0001` and `REQ-TYPES-COMPAT-0019`, requirement revision 1.

The artifacts are still draft. This report records evidence and gaps; it does
not promote requirements or claim that deferred requirements are implemented.

## Build, package, API, and test baseline

- Project/package: `Incursa.Types`, `net8.0`, package version `1.0.0` under the
  current unversioned local build, with package generation enabled on build.
- Assembly surface: 101 exported types: 59 top-level exported types, including
  24 concrete domain value types and 33 public support interfaces. The 24 domain
  types include source-generated `AclKey`; nested public converter/helper types
  account for the remainder.
- Generator input: `Incursa.Types/Types/AclKey.string.json`; generated build
  output is not source-controlled. Other partial implementation seams include
  `SourceGenerationContext.cs`, `JsonContext.cs`, and the `.pw.cs` files.
- Tests: 16 source test files, 165 `[Fact]`/`[Theory]` declarations, and 252
  discovered Release test cases.
- `dotnet test Incursa.Core.slnx -c Release` restored and built both projects.
  The build produced zero compiler errors and emitted the package, but the test
  host could not start because this machine lacks `Microsoft.NETCore.App 8.0`.
  Re-running the built test assembly with `DOTNET_ROLL_FORWARD=Major` passed
  252/252 tests (0 failed, 0 skipped) on the installed newer runtime. A separate
  isolated TY-01 worker run attached to ClickUp passed the same 252 tests on
  .NET 8.0.34 at this exact commit. The local restore also reported NU1902 for
  `MimeKit` 4.14.0; dependency remediation is outside this inventory task.

The generated `Incursa.Types.1.0.0.nupkg` contains the net8.0 assembly, README,
and icon. Its consumer runtime dependency group is:

| Package | Version | Role in current source |
| --- | ---: | --- |
| CommunityToolkit.Diagnostics | 8.4.0 | Guard calls in file, temporal, contact, encrypted-string, and state types |
| Cronos | 0.11.1 | `RecurringPeriod` public API and parsing |
| Humanizer.Core | 3.0.1 | `Money.ToWords` |
| MimeKit | 4.14.0 | MIME inference and email parsing |
| System.Memory.Data | 10.0.0 | `BinaryData` in `BvFile`/`IFile` |
| TimeZoneConverter | 7.0.0 | `TimeZoneId` Windows/IANA conversion |
| libphonenumber-csharp | 9.0.21 | `PhoneNumber` parsing and E.164 formatting |

Build-only analyzer/generator dependencies remain private and are not listed in
the package runtime group. The resolved repository build graph also includes
transitive packages, notably BouncyCastle via MimeKit and FluentValidation via
the generator package; package-only runtime verification belongs to TY-10.

## Type ownership and behavioral contracts

| Type/group | Current contract and serialization boundary | Direct closure / split disposition |
| --- | --- | --- |
| `AclKey` | Generated string-backed value, strict parse/format, JSON/type converters | BCL plus private generator; Primitives candidate only after generated API evidence |
| `FastId`, `ShortCode` | Stable encoded text, parse/format, comparison, JSON/type converters; generation policy must not reject legacy valid values | BCL crypto/text; move in TY-02 |
| `Maybe<T>` | Some/None, default/null, match/select/value-access semantics | BCL only; move in TY-02 |
| Backed-type interfaces | Public generic contracts, including numeric/string/guid/multi-backed variants | BCL plus converter-facing contracts; move only after closure/API checks in TY-02 |
| `Percentage` | Decimal normalization, scaled parsing, formatting, equality/comparison, JSON/type converters | BCL only; move in TY-02 |
| `Money` | Decimal normalization, two-decimal behavior, invariant parse/format, converters and English words | Humanizer today; move after dependency-free wording in TY-03 |
| `Duration`, `Period` | Existing textual/JSON and arithmetic/interval behavior | BCL plus guard dependency; move without semantic rewrite in TY-02, strict versioned profile later in TY-09 |
| `RecurringPeriod` | Cron expression is public API; deterministic generation from supplied start | Cronos; retain in rich Types package |
| `TimeZoneId` | Windows/IANA lookup, canonical identifier, converters | TimeZoneConverter; retain in rich package through TY-04 compatibility work |
| `BvFile`, `IFile` | File bytes, name, MIME metadata, base64/path factories | BinaryData, MimeKit, guards; retain in rich package |
| `EmailAddress`, `PhoneNumber` | Normalized contact parsing, persisted text and converters | MimeKit/libphonenumber/guards; retain in rich package |
| `EncryptedString`, `UsaState` | Opaque base64 or state-code text plus parse/converter contracts | BCL plus guards; not required in initial Primitives closure |
| `CidrRange`, `IpAddress`, `CountryCode`, `CurrencyCode`, `JsonContext`, `Locale`, `MonthOnly`, `Url`, `VirtualPath` | Repository specs define canonical text, parse, comparison, and converter behavior | Mostly BCL; not in the required initial ownership set and remain in rich package until separately justified |

`BigDecimal`, `ElapsedDuration`, and `LocalDateTime` do not exist in the baseline.
Every existing public type is currently defined exactly once in `Incursa.Types`;
no type-forwarding or split-package graph exists yet.

## Migration and compatibility order

1. TY-02 creates `Incursa.Types.Primitives`, preserves `net8.0` and namespaces,
   moves the dependency-free interfaces/value types and the existing temporal
   types without changing temporal semantics, and makes rich Types reference it.
2. TY-03 replaces Humanizer money wording, verifies output compatibility, and
   moves `Money` into Primitives.
3. TY-04 replaces TimeZoneConverter only after Windows/Linux compatibility
   evidence; `TimeZoneId` remains rich-package functionality unless its closure
   is separately approved.
4. TY-05 records the owned BigDecimal design, limits, licenses, reference
   algorithms, vectors, and benchmark plan before implementation.
5. TY-06 and TY-07 implement and verify BigDecimal value and arithmetic layers.
6. TY-08 adds exact `ElapsedDuration` and `LocalDateTime` profiles.
7. TY-09 introduces versioned strict calendar/period behavior while preserving
   existing wire/arithmetic behavior through an explicit legacy path.
8. TY-10 performs package-only restore/runtime graph, old compiled-consumer,
   type-forwarding, public API, build/test/pack, and release-document evidence.

Each implementation slice must preserve namespace and serialization contracts,
use one defining assembly, and add package/binary checks before claiming
compatibility. Existing `docs/spec/types/*`, `docs/spec/test-traceability.md`,
and `docs/spec/compat-decisions.md` are the local baseline.

## Requirement-to-evidence map

| Requirement(s) | TY-01 disposition |
| --- | --- |
| `REQ-TYPES-PKG-0001..0003` | Deferred: no Primitives project/package exists; current Types has external runtime dependencies. |
| `REQ-TYPES-PKG-0004` | Partially evidenced: analyzer/generator references are private/build-only; future package graph must prove consumer closure. |
| `REQ-TYPES-PKG-0005` | Baseline evidenced: all current work is in this repository/solution; split not yet implemented. |
| `REQ-TYPES-PKG-0006..0009` | Deferred: candidates are still owned by Types; BigDecimal, ElapsedDuration, and LocalDateTime are absent. |
| `REQ-TYPES-PKG-0010` | Deferred: one defining assembly exists today, but the combined split graph does not. |
| `REQ-TYPES-PKG-0011` | Baseline namespaces inventoried; must be rechecked after every move. |
| `REQ-TYPES-PKG-0012..0015` | Deferred to TY-02/TY-10: no type-forwarding, old compiled consumer, package-only consumer, or Primitives runtime-graph test exists. |
| `REQ-TYPES-PKG-0016` | Baseline evidenced: contact parsing, recurring scheduling, and file functionality remain in rich Types. |
| `REQ-TYPES-PKG-0017` | Explicit gap: foundational candidates still use CommunityToolkit guards. |
| `REQ-TYPES-PKG-0018` | Baseline evidenced: both projects target net8.0; the split must preserve it. |
| `REQ-TYPES-PKG-0019..0020` | Deferred: no Primitives version constraint or split-release compatibility notes exist. |
| `REQ-TYPES-COMPAT-0001` | Partially evidenced by repository specs/decisions and 252 passing tests; every move or intentional change still needs targeted source/binary/wire evidence. |
| `REQ-TYPES-COMPAT-0019` | Partially evidenced for the current full package; moved primitive and package-only consumer regressions do not yet exist. |

## Acceptance criteria

- [x] Record the exact commit and baseline build/test results.
- [x] Inventory the public API, package graph, source-generator inputs, type
  dependency closure, and existing behavioral/serialization boundaries.
- [x] Produce the scoped migration/API compatibility order with explicit gaps.
- [x] Map all 22 applicable live SpecAttest requirements to current, deferred,
  or blocked evidence without marking unimplemented requirements complete.
