---
id: TASK-0005
type: task
status: done
priority: critical
owner: platform
created: 2026-11-27
updated: 2026-11-27
tags: []
related:
  specs:
    - /docs/30-contracts/bigdecimal-verification-plan.md
  adrs:
    - /docs/40-decisions/ADR-0001-bigdecimal-exact-decimal-model.md
  files: []
  prs:
    - "https://github.com/incursa/types/pull/8"
  issues: []
  branches:
    - users/samuelmcaravey/ty-05-bigdecimal-design
title: TY-05 BigDecimal exact precision design and reference review
---

# TASK-0005 - TY-05 BigDecimal exact precision design and reference review

## Summary

Define the exact decimal model and the executable evidence plan that unblock
TY-06/TY-07. Review pinned source and licenses for PeterO.Numbers EDecimal,
.NET SqlDecimal, Singulink, ExtendedNumerics, and Apache Arrow; select an owned
dependency-free representation and explicit resource/rounding policy. This is
a design-only slice from merged TY-04 commit
`9390b64329acf5a1769bf6e01e1337c2b6852d32`.

## Normative inputs

SpecAttest `SPEC-TYPES-NUM-0001` requirements `REQ-TYPES-NUM-0001..0015`,
`0019`, and `0022..0033` were fetched before the review. Every record is draft,
revision 1, record version 1. TY-05 records a design disposition and does not
claim implementation or mutate requirement lifecycle.

## Deliverables

- ADR-0001 selects normalized `BigInteger` coefficient plus signed `int`
  exponent, canonical text, mathematical equality/hash, immutable contexts,
  Decimal38/Decimal76/VariableSafe profiles, explicit rounding, exact
  conversion naming, and no special values or ambient precision.
- Default VariableSafe limits are 4,096 coefficient digits, exponents
  `-4096..4096`, 8,192 input/working digits, and 64 KiB temporary allocation;
  callers must opt into larger immutable limits.
- Exact division accepts only terminating base-ten results; rounded operations
  require an explicit target and one of six required modes.
- The verification plan defines independent Python/Fraction, SqlDecimal, and
  Arrow vectors, hostile-input cases, fixture provenance, and a full benchmark
  matrix before any fixed-width optimization.
- Reference versions, commits, licenses, useful algorithms, incompatibilities,
  and no-copy disposition are recorded in the ADR.

## Requirement disposition

| Requirement(s) | TY-05 design evidence |
| --- | --- |
| NUM-0001..0003 | Owned finite `BigInteger × 10^int` model; no binary intermediary or runtime dependency. |
| NUM-0004..0006 | Decimal38 mandatory, Decimal76 optional, VariableSafe bounded profile and explicit larger contexts. |
| NUM-0007..0012 | Unique zero, trailing-zero normalization, mathematical identity/hash/ordering, canonical `coefficient e exponent` text. |
| NUM-0013..0015 | Invariant grammar plus input/digit/exponent/allocation preflight and checked exponent math. |
| NUM-0019, 0022..0024 | Terminating exact division; explicit target and six-mode rounding catalog. |
| NUM-0025..0027 | Exact conversion names, separately named lossy policy, immutable non-ambient contexts. |
| NUM-0028..0030 | Fixed-width equivalence gate, no special values, member-by-member generic-math qualification. |
| NUM-0031..0033 | Independent vector provenance, benchmark matrix, and shared Types/Tabular corpus. |

## Verification

- Pinned upstream source and license files inspected; no source copied.
- Workbench navigation/link sync and strict validation: pass.
- Type-spec structural verification: pass.
- `dotnet test Incursa.Core.slnx -c Release`: 279 passed, 0 failed, 0
  skipped using `DOTNET_ROLL_FORWARD=Major` on this workstation; connected CI
  supplies the pinned .NET 8 runtime.
- No production code or package dependency changed in this design-only slice.
- PR #8 connected validation: Workbench Verify, Windows/Ubuntu timezone
  regression matrix, full verify/coverage/packaging, package-only consumer,
  both mutation scopes, and aggregate Quality Report all passed.

## Acceptance criteria

- [x] ADR defines exact arithmetic, limits, precision propagation, canonical
  form, conversion policy, and pinned reference/license review.
- [x] Golden-vector and benchmark plan covers exact/inexact, SQL/Arrow, and
  hostile exponent cases with SqlDecimal only as an oracle.
- [x] Applicable requirements map to design/evidence or explicit TY-06/TY-07
  implementation deferral, and Release solution tests are recorded.
