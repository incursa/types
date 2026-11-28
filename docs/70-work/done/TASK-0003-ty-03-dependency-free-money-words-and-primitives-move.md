---
id: TASK-0003
type: task
status: done
priority: critical
owner: platform
created: 2026-11-27
updated: 2026-11-27
tags: []
related:
  specs:
    - /docs/spec/types/Money.md
    - /docs/spec/test-traceability.md
    - /docs/migration-v2.md
  adrs: []
  files: []
  prs:
    - "https://github.com/incursa/types/pull/6"
  issues: []
  branches:
    - users/samuelmcaravey/ty-03-money-primitives
title: TY-03 dependency-free Money words and Primitives move
---

# TASK-0003 - TY-03 dependency-free Money words and Primitives move

## Summary

Replace Humanizer-backed Money wording with a deterministic decimal-safe
English formatter, move `Money` and `MoneyExtensions` into
`Incursa.Types.Primitives`, and preserve the established normalization,
arithmetic, formatting, converter, serialization, namespace, and full-package
entry-point contracts. The slice starts from merged TY-02 commit
`3f7d6423e253b4483ff4ad68f3de107c29f9809c`.

## Normative inputs

Current live requirements were fetched from SpecAttest project `incursa-types`
before implementation. The governing draft candidates are:

- `SPEC-TYPES-PKG-0001`, record version 2:
  `REQ-TYPES-PKG-0006,0010..0013`, requirement revision 1.
- `SPEC-TYPES-COMPAT-0001`, record version 1:
  `REQ-TYPES-COMPAT-0002..0008`, requirement revision 1.

The live records remain draft. This work records implementation evidence; it
does not promote or otherwise mutate the requirements.

## Implementation

- Moved `Money` and `MoneyExtensions` into the Primitives assembly while
  preserving their `Incursa` namespace and adding full-package type forwarders.
- Removed `Humanizer.Core`; neither package now restores or declares it.
- Reimplemented `ToWords()` using normalized `decimal` arithmetic and bounded
  base-1000 groups. The formatter supports the entire decimal range through
  octillion without a `long`/floating-point conversion.
- Defined exact English grammar for sign, zero, dollars/cents, singular/plural,
  hundreds-group `and`, compound-tens hyphens, and omission of zero cents.
- Preserved the established conventional output
  `one hundred and twenty-three dollars and forty-five cents` for `123.45`.
- Explicitly documented observable corrections: `one dollar`/`one cent` use
  singular labels, negative sub-dollar values retain sign and cents, and
  amounts outside the `long` range no longer overflow during word formatting.
- Extended the package-only consumer to compile and execute Money from the
  built Primitives nupkg using an isolated NuGet cache.

## Requirement disposition

| Requirement(s) | TY-03 evidence or disposition |
| --- | --- |
| `REQ-TYPES-PKG-0006` | Partial package-family completion: Money and Percentage are now owned by Primitives; BigDecimal remains TY-06/TY-07. |
| `REQ-TYPES-PKG-0010..0011` | Money/MoneyExtensions have one defining assembly and unchanged namespaces; boundary tests enumerate all exported Primitives types. |
| `REQ-TYPES-PKG-0012` | Full Types forwards both moved public types; an assembly-qualified `Incursa.Money, Incursa.Types` lookup resolves through the forwarder. Exhaustive compiled-consumer fixtures remain TY-10. |
| `REQ-TYPES-PKG-0013` | Existing JSON/type-converter round trips pass from the new defining assembly; assembly-qualified identity resolution is tested. Broader payload fixtures remain TY-10. |
| `REQ-TYPES-COMPAT-0002` | Constructor and operator paths retain midpoint-to-even two-decimal normalization; existing and focused midpoint tests pass. |
| `REQ-TYPES-COMPAT-0003..0005` | Dependency-free ToWords implementation and exact documented grammar/range are in `Money.cs` and the Money behavioral specification. |
| `REQ-TYPES-COMPAT-0006` | Tests cover zero, signs, sub-dollar values, singular/plural, cents, whole dollars, multi-scale amounts, normalization boundaries, and decimal min/max. |
| `REQ-TYPES-COMPAT-0007` | All intentional output corrections are explicit in the type spec and migration guide. |
| `REQ-TYPES-COMPAT-0008` | Humanizer reference and usage removed; package assets contain no Humanizer dependency. |

## Verification

- Release build with warnings as errors: pass, zero warnings/errors.
- `dotnet test Incursa.Core.slnx -c Release`: 269 passed, 0 failed, 0 skipped
  using `DOTNET_ROLL_FORWARD=Major` on this workstation; connected CI supplies
  the pinned .NET 8 runtime.
- Package-only Primitives restore/build/run: pass from an isolated package cache
  and local nupkg; Money returned `one dollar and one cent`.
- Two repeated CI-property packs produced normalized-identical full and
  Primitives nupkg contents.
- Coverage: 73.78% line and 60.24% branch overall, above the 65%/50% gates.
- Primitives mutation gate (`Money` + `Percentage`): 53.61% (103 killed, 4
  survived, 1 timeout), above the 35% break threshold.
- Rich Types regression mutation gate (`JsonContext`, `MonthOnly`,
  `TimeZoneId`): 64.36% (65 killed, 4 survived, 0 timeouts/errors). CI now runs
  both scopes so the assembly move does not silently drop Money from mutation
  coverage or abandon the existing rich-package regression set.

## Acceptance criteria

- [x] Money wording covers sign, zero, cents, singular/plural, large amounts,
  normalization boundaries, and the full decimal range without precision loss.
- [x] Existing Money conversion/serialization behavior passes; Money is defined
  in Primitives; neither package depends on Humanizer.
- [x] Applicable requirements map to code/tests or explicit TY-10 deferrals,
  and the required Release solution test run is recorded.
