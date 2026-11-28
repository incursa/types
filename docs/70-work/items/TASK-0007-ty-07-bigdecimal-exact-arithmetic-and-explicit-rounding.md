---
id: TASK-0007
type: task
status: in-progress
priority: critical
owner: platform
created: 2026-11-27
updated: 2026-11-27
tags: []
related:
  specs:
    - /docs/spec/types/BigDecimal.md
    - /docs/30-contracts/bigdecimal-verification-plan.md
  adrs:
    - /docs/40-decisions/ADR-0001-bigdecimal-exact-decimal-model.md
  files:
    - /docs/60-tracking/bigdecimal-baseline-benchmark-2026-11-27.md
  prs:
    - "https://github.com/incursa/types/pull/10"
  issues: []
  branches:
    - users/samuelmcaravey/ty-07-bigdecimal-arithmetic
title: TY-07 BigDecimal exact arithmetic and explicit rounding
---

# TASK-0007 - TY-07 BigDecimal exact arithmetic and explicit rounding

## Summary

Implement checked exact arithmetic, terminating exact division, truncated
remainder, explicit quantization/rounded division, all six required rounding
modes, immutable operation contexts, and classified resource failures on the
TY-06 BigDecimal identity model. This branch starts from merged TY-06 commit
`bdde30c32bbd77c8fdcb1c7c0ebd36d77f764184`.

Current SpecAttest candidates `REQ-TYPES-NUM-0016..0033` were fetched before
implementation. All remain draft revision 1 / record version 1; TY-07 records
evidence without promoting requirement lifecycle state.

## Requirement disposition

| Requirement(s) | TY-07 evidence |
| --- | --- |
| NUM-0016..0018 | Exact add/subtract/multiply plus default and explicit immutable contexts; classified profile/working/allocation failures. |
| NUM-0019..0021 | Reduced terminating division, nonterminating exception, zero-divisor checks, and exact truncated-toward-zero remainder. |
| NUM-0022..0024 | Rounded division and quantization require target scale plus one of six explicit modes; tie and directed-sign tests. |
| NUM-0025..0026 | TY-06 exact conversions remain unchanged; no implicit lossy conversion is introduced. |
| NUM-0027 | `BigDecimalContext` and profiles are immutable; operators use one stable default without mutable ambient state. |
| NUM-0028 | No fixed-width optimization selected; equivalence gate remains applicable to later work. |
| NUM-0029 | Finite-only TY-06 domain remains unchanged. |
| NUM-0030 | Exact compatible operators/interfaces implemented; aggregate `INumber<BigDecimal>` deliberately not claimed pending full conversion/function conformance. |
| NUM-0031 | Independent Fraction JSONL vectors, SqlDecimal-compatible arithmetic, ties, and resource/failure fixtures. |
| NUM-0032 | Reproducible local harness records parse, format, compare, hash, arithmetic, rounding, and access baseline; no optimization selected. |
| NUM-0033 | Shared BigDecimal remains the canonical implementation for Tabular consumption; TY-10 owns adapter qualification. |

## Verification

- `BigDecimalArithmeticTests` covers independent vectors, SqlDecimal oracle
  comparisons, terminating/nonterminating division, remainder signs, zero,
  six rounding modes, positive/negative ties, invalid scale/mode, resource
  limits, immutable contexts, and cross-representation equality.
- `scripts/verify-bigdecimal-arithmetic-vectors.py` verifies JSONL results with
  Python `decimal.Decimal` plus `fractions.Fraction`; Python is unavailable on
  this workstation, so executable cross-check remains for a Python-capable
  environment while .NET tests consume the fixture.
- The dependency-free benchmark harness and captured figures are recorded in
  the linked 2026-11-27 tracking document. No fixed-width path was selected.
- `dotnet test Incursa.Core.slnx -c Release`: 394 passed, 0 failed, 0
  skipped using `DOTNET_ROLL_FORWARD=Major` on this workstation; connected CI
  supplies the pinned .NET 8 runtime.
- Coverage gate passed at 76.03% line / 62.67% branch overall (65% / 50%
  thresholds); arithmetic implementation measured 90.43% line / 80.72%
  branch.
- Warn-as-error deterministic build, formatting, benchmark build/run,
  Workbench strict validation, type-spec verification, and diff checks pass.
- Package-only nupkg restore/build/run exercises explicit rounded division and
  passes with zero runtime package dependencies. Connected CI remains to be
  recorded.

## Acceptance criteria

- [x] Arithmetic/rounding modes, ties, zero divisor, overflow, limits, and
  cross-representation equality tests pass.
- [x] Generic-math compatibility and baseline benchmark evidence are recorded;
  no implicit rounding or third-party runtime dependency is introduced.
- [ ] Applicable requirements map to code/tests or explicit deferral and
  Release solution plus connected CI results are recorded.
