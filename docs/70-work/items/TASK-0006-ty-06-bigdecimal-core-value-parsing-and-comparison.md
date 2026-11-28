---
id: TASK-0006
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
  files: []
  prs:
    - "https://github.com/incursa/types/pull/9"
  issues: []
  branches:
    - users/samuelmcaravey/ty-06-bigdecimal-core
title: TY-06 BigDecimal core value parsing and comparison
---

# TASK-0006 - TY-06 BigDecimal core value parsing and comparison

## Summary

Implement the dependency-free immutable BigDecimal value core selected by
ADR-0001: bounded invariant parsing, normalization, canonical formatting,
mathematical identity/comparison/hash, immutable profiles, and exact native
conversions. The branch starts from merged TY-05 commit
`18fd1e96b231044adc9b8c40383f77a68c1b0010`.

Current SpecAttest candidates `REQ-TYPES-NUM-0001..0015`, `0025..0026`,
`0029`, `0031`, and `0033` were fetched before implementation. All remain
draft revision 1 / record version 1; this work records evidence without
promoting requirement lifecycle state.

## Requirement disposition

| Requirement(s) | TY-06 evidence |
| --- | --- |
| NUM-0001..0003 | `BigDecimal` stores owned `BigInteger` coefficient and signed exponent; parsing/conversion never use binary floating point. |
| NUM-0004..0006 | Immutable Decimal38, Decimal76, and VariableSafe profiles expose finite coefficient, exponent, input, working, and allocation bounds. |
| NUM-0007..0012 | Constructor/parser normalization gives unique zero, factor-free coefficient, mathematical equality/hash/comparison, and canonical text. |
| NUM-0013..0015 | Explicit invariant grammar, preflighted input/digits/buffer, checked exponent parsing/effective exponent/normalization. |
| NUM-0025..0026 | Separately named exact integer/decimal conversions reject all loss; no lossy conversion is exposed in TY-06. |
| NUM-0029 | Grammar rejects NaN and infinities. |
| NUM-0031 | Python Decimal-authored JSONL vectors and `SqlDecimal` boundary tests provide independent evidence. |
| NUM-0033 | Shared primitive defines canonical semantics for Tabular to consume; adapter integration is outside TY-06. |

## Verification

- `BigDecimalCoreTests` covers vectors, Decimal38/SqlDecimal boundaries,
  normalization, equality/hash/comparison, hostile syntax and bounds, checked
  exponent overflow, canonical formatting, and exact conversion failures.
- `scripts/verify-bigdecimal-core-vectors.py` independently validates fixture
  coefficient/exponent/canonical results with Python `decimal.Decimal`; Python
  is not installed on this workstation, so the executable cross-check is
  deferred to a Python-capable environment while the same fixture is exercised
  by .NET tests.
- `dotnet test Incursa.Core.slnx -c Release`: 341 passed, 0 failed, 0
  skipped using `DOTNET_ROLL_FORWARD=Major` on this workstation; connected CI
  supplies the pinned .NET 8 runtime.
- Local coverage gate passed at 75.12% line and 62.00% branch overall
  (thresholds 65% / 50%); BigDecimal measured 86.97% line and 86.78% branch.
- Package-only restore/build/run passed from the generated
  `Incursa.Types.Primitives` nupkg with `BigDecimal`, with zero runtime package
  dependencies.
- Workbench strict validation, type-spec structural verification, formatting,
  analyzers, and diff checks pass. Connected CI remains to be recorded.

## Acceptance criteria

- [x] Mandatory SQL precision, normalized zero, negative scales, malformed and
  oversized input, equality/hash, comparison, and exact conversions pass.
- [x] Dependency-free package API and independently authored vectors are
  verified, with no runtime dependency added.
- [ ] Requirements map to code/tests or explicit deferral and Release solution
  tests plus connected CI are recorded.
