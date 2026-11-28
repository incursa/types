---
id: TASK-0002
type: task
status: in-progress
priority: critical
owner: platform
created: 2026-11-27
updated: 2026-11-27
tags: []
related:
  specs:
    - /docs/spec/README.md
    - /docs/spec/compat-decisions.md
    - /docs/spec/test-traceability.md
  adrs: []
  files: []
  prs: []
  issues: []
  branches:
    - users/samuelmcaravey/ty-02-primitives-extraction
title: TY-02 extract dependency-free primitives package
---

# TASK-0002 - TY-02 extract dependency-free primitives package

## Summary

Create `Incursa.Types.Primitives` inside the existing repository and solution,
move the verified dependency-free types into that assembly without namespace or
behavior changes, and keep `Incursa.Types` as the compatible rich-package entry
point. This slice starts from merged TY-01 commit
`0b5458a66c8ae1329aea2725362824fb5de8f9f5`.

The implementation intentionally does not move `Money`, introduce
`BigDecimal`, add `ElapsedDuration`/`LocalDateTime`, or rewrite temporal
semantics. Those changes remain ordered behind TY-03 and TY-06 through TY-09.

## Normative inputs

The live `incursa-types` SpecAttest export was fetched before implementation.
The governing draft artifacts and current revisions were:

- `SPEC-TYPES-PKG-0001`, record version 2;
  `REQ-TYPES-PKG-0001..0019`, requirement revision 1.
- `SPEC-TYPES-COMPAT-0001`, record version 1;
  `REQ-TYPES-COMPAT-0001,0009..0011,0015..0018`, requirement revision 1.

No requirement status or SpecAttest record was changed. Requirements assigned
to later TY tasks are recorded as partial/deferred rather than claimed complete.

## Package and ownership changes

- Added `Incursa.Types.Primitives` targeting `net8.0`, with no direct runtime
  `PackageReference` entries and an empty NuGet dependency group.
- Moved `FastId`, `ShortCode`, `Maybe<T>`, `Percentage`, `Duration`, and
  `Period`, preserving the `Incursa` namespace and source behavior.
- Moved the BCL-only backed contracts: numeric specializations,
  `IFastIdBackedType<T>`, `IGenericBackedType<T>`, `IGuidBackedType<T>`,
  `IStringBackedType<T>`, numeric and multi-backed variants, plus
  `IBvParsable<T>` required by `ShortCode`.
- Replaced the two moved CommunityToolkit guard calls with equivalent BCL
  argument validation.
- Added a rich-to-primitives project/package dependency and type forwarders for
  every exported Primitives type. The combined graph therefore has one defining
  assembly while previously compiled full-package consumers can resolve the
  moved identities through `Incursa.Types`.
- Retained Money/Humanizer, contact parsing, file/BinaryData, Cronos scheduling,
  time-zone conversion, and all other specialized functionality in the rich
  package.

## Compatibility and package evidence

- `PrimitivePackageBoundaryTests` proves assembly ownership, rich-package
  reference direction, forwarding of every exported Primitives type, and a
  BCL-only runtime assembly-reference graph.
- The existing parsing, formatting, JSON converter, deterministic identifier,
  Percentage, Duration, Period, and ShortCode suites continue to exercise the
  moved implementations transitively through `Incursa.Types`.
- `scripts/verify-primitives-package.ps1` opens the built nupkg, rejects any
  runtime NuGet dependency, restores a temporary consumer from the package
  alone, and builds/runs representative `FastId`, `Percentage`, and `Duration`
  usage. The Quality Gate now executes this check against the first
  reproducibility package set.
- The generated full-package nuspec declares `Incursa.Types.Primitives` at the
  built package version. Complete old-binary and assembly-qualified payload
  fixtures remain explicitly deferred to TY-10; this slice supplies and tests
  the type-forwarding mechanism they will qualify.

## Requirement disposition

| Requirement(s) | TY-02 evidence or disposition |
| --- | --- |
| `REQ-TYPES-PKG-0001..0005` | Implemented by package/project identity, dependency direction, empty runtime dependency group, private build-only tooling, and same-repository solution membership. |
| `REQ-TYPES-PKG-0006` | Partial: Percentage moved; Money remains TY-03 and BigDecimal remains TY-06/TY-07. |
| `REQ-TYPES-PKG-0007` | Partial: existing Duration and Period moved without semantic changes; absent ElapsedDuration and LocalDateTime remain TY-08. |
| `REQ-TYPES-PKG-0008..0011` | Implemented for this slice: identifiers, Maybe/backed contracts, one defining assembly, and namespace continuity. |
| `REQ-TYPES-PKG-0012..0013` | Type forwarders and existing wire/converter tests added/preserved; exhaustive old compiled-consumer and assembly-qualified serialization fixtures remain TY-10. |
| `REQ-TYPES-PKG-0014..0015` | Implemented by package-only restore/build/run plus nuspec and runtime assembly-graph checks. |
| `REQ-TYPES-PKG-0016..0018` | Implemented: rich boundaries retained, moved guards removed, and `net8.0` preserved. |
| `REQ-TYPES-PKG-0019` | Implemented by the full-package project reference, which packs as a dependency on the matching Primitives build version. |
| `REQ-TYPES-COMPAT-0001,0009..0011` | Existing behavior retained and covered by the 255-case suite, including focused Percentage/FastId/ShortCode tests. |
| `REQ-TYPES-COMPAT-0015..0016,0018` | Contact parsing, file/BinaryData, and Cronos functionality remain in rich Types. |
| `REQ-TYPES-COMPAT-0017` | Boundary preserved: Primitives introduces no file/BinaryData scalar; Tabular integration qualification remains outside this repository. |

## Verification

- Deterministic Release build with warnings as errors: pass, zero warnings and
  zero errors.
- `dotnet test Incursa.Core.slnx -c Release`: 255 passed, 0 failed, 0 skipped.
  This workstation lacks the .NET 8 runtime, so the built net8 test assembly
  was executed with `DOTNET_ROLL_FORWARD=Major`; GitHub CI supplies the pinned
  SDK/runtime and remains the authoritative connected gate.
- Local package-only restore/build/run: pass for
  `Incursa.Types.Primitives 1.0.0`; zero runtime package dependencies.
- Reproducible pack: both `Incursa.Types` and `Incursa.Types.Primitives` produce
  normalized-identical nupkg contents across warmed repeated CI-property packs.
- Coverage: 73.16% line and 59.83% branch overall; rich Types is 70.18%/56.02%
  and Primitives is 78.08%/70.30%, above the 65%/50% gates.
- Focused mutation gate: 56.31% (116 killed, 4 survived, 0 timeouts/errors),
  above the configured 35% break threshold.
- `dotnet format Incursa.Core.slnx --no-restore`: pass.

## Acceptance criteria

- [x] Primitives builds with no external runtime package dependencies; full
  Types references Primitives; no type is defined in both assemblies.
- [x] Existing tests and package/serialization/API compatibility checks pass,
  with the remaining exhaustive TY-10 migration fixtures called out explicitly.
- [x] Applicable live requirement IDs map to code/tests or explicit deferred
  evidence, and the required Release solution test run is recorded.
