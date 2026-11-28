---
workbench:
  type: doc
  workItems:
    - TASK-0007
  codeRefs:
    - benchmarks/Incursa.Types.BigDecimal.Benchmarks/Program.cs
  path: /docs/60-tracking/bigdecimal-baseline-benchmark-2026-11-27.md
  pathHistory:
    - "C:/docs/60-tracking/bigdecimal-baseline-benchmark-2026-11-27.md"
---

# BigDecimal baseline benchmark 2026-11-27

## Purpose

Record the pre-optimization BigInteger implementation baseline required by
`REQ-TYPES-NUM-0032`. These figures qualify direction and regression risk;
they are not a statistically rigorous hardware comparison and do not justify
a fixed-width storage path by themselves.

## Method

- Command: `dotnet run --project benchmarks/Incursa.Types.BigDecimal.Benchmarks/Incursa.Types.BigDecimal.Benchmarks.csproj -c Release`
- Runtime roll-forward: `DOTNET_ROLL_FORWARD=Major` because this workstation
  has .NET 9+ but not the repository's pinned .NET 8 runtime.
- Each case: 10,000 warm-up calls, forced collection, then 200,000 measured
  calls using `Stopwatch` and thread allocation counters.
- Operand: `12345678901234567890.125`; secondary operands are representative
  bounded exact values. Results are retained through a sink.
- Local workstation result captured on 2026-11-27; CI supplies correctness and
  portability evidence, not timing comparability.

## Baseline

| Case | ns/op | B/op |
| --- | ---: | ---: |
| parse | 1,288.8 | 192.0 |
| format | 337.6 | 112.0 |
| compare | 565.2 | 136.0 |
| hash | 56.7 | 24.0 |
| add | 1,315.4 | 416.0 |
| multiply | 600.9 | 424.0 |
| divide exact | 586.5 | 312.0 |
| divide rounded | 1,008.1 | 488.0 |
| quantize | 573.4 | 336.0 |
| coefficient/exponent access | 27.5 | 40.0 |

## Decision

Keep the owned normalized BigInteger representation. No fixed-width fast path
is selected in TY-07: there is no competing implementation or end-to-end
Tabular column benchmark demonstrating a material benefit with identical
failure semantics. TY-10 can use this harness plus typed-column measurements
when evaluating integration performance.
