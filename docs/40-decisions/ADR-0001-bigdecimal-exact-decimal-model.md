---
workbench:
  type: adr
  workItems:
    - TASK-0005
  codeRefs: []
  path: /docs/40-decisions/ADR-0001-bigdecimal-exact-decimal-model.md
  pathHistory:
    - "C:/docs/40-decisions/ADR-0001-bigdecimal-exact-decimal-model.md"
---

# BigDecimal exact decimal model and arithmetic policy

## Status

Accepted for TY-06 and TY-07 implementation on 2026-11-27. The governing
SpecAttest requirements remain draft revision 1; implementation evidence must
not promote their lifecycle status.

## Context

Incursa.Types needs one dependency-free decimal value that is exact across the
SQL Server `DECIMAL(38,s)` domain, can extend to Arrow Decimal256's 76-digit
domain, and remains safe when callers request larger values. The current
requirements prohibit binary-floating intermediaries, ambient precision,
implicit rounding, special values, and unbounded exponent-driven allocation.

The value will be used both as a normal shared primitive and at Tabular typed
column boundaries. A fixed-width transport representation is therefore useful,
but it must not define different equality or arithmetic semantics.

All applicable `REQ-TYPES-NUM-0001..0015`, `0019`, and `0022..0033` records
were fetched from SpecAttest project `incursa-types` at revision 1 / record
version 1 before this decision was written.

### Reference source review

No source code was copied. The following versions and exact commits were read
for algorithms, failure modes, API tradeoffs, and license constraints:

| Reference | Pinned source | License | Useful evidence | Reason not to adopt as the runtime model |
| --- | --- | --- | --- | --- |
| [PeterO.Numbers `EDecimal`](https://github.com/peteroupc/Numbers/tree/26b7571292aca5037430227a223dac1702b43b80) | 1.8.2, `26b7571292aca5037430227a223dac1702b43b80` | CC0-1.0/public-domain dedication | Arbitrary coefficient/exponent, explicit `EContext`, exponent ranges, traps, and a broad rounding catalog | Includes NaN/infinities; intrinsic equality is representation-sensitive; contexts can expose mutable flags; large surface and no Incursa allocation policy |
| [.NET `SqlDecimal`](https://github.com/dotnet/runtime/blob/b455eab3a099cbec2bbd8e5b1b4703177c325284/src/libraries/System.Data.Common/src/System/Data/SQLTypes/SQLDecimal.cs) | runtime v8.0.34, `b455eab3a099cbec2bbd8e5b1b4703177c325284` | MIT | Four-word fixed storage, the complete 38-digit SQL domain, SQL precision/scale formulas, and overflow behavior | SQL-specific public type, nonnegative scale, and operations that intentionally propagate scale and round to SQL rules |
| [Singulink.Numerics.BigDecimal](https://github.com/Singulink/Singulink.Numerics.BigDecimal/tree/564d9a73c591e0e8c570427094e8c62d6fd65bc1) | source 3.6.0, `564d9a73c591e0e8c570427094e8c62d6fd65bc1` (latest published package observed: 3.5.1) | MIT | Normalized `BigInteger` + `int` exponent, exact division, explicit rounded division, conversion and generic-math tests | Runtime dependencies; `/` falls back to rounded precision derived partly from operands; no Incursa-wide hostile-exponent/allocation policy |
| [ExtendedNumerics.BigDecimal](https://github.com/AdamWhiteHat/BigDecimal/tree/36a013873008597457bc5a288f8257d627b92673) | source 3004.0.1, `36a013873008597457bc5a288f8257d627b92673` | MIT | Broad arithmetic and formatting implementation using `BigInteger` mantissa/exponent | Mutable global `Precision`, `AlwaysTruncate`, `AlwaysNormalize`, and default rounding settings violate deterministic no-ambient-context requirements |
| [Apache Arrow .NET](https://github.com/apache/arrow-dotnet/tree/79e3844aca811abb3f9c463c0c27d407723e13be) | v23.0.0, `79e3844aca811abb3f9c463c0c27d407723e13be` | Apache-2.0 | Decimal128/256 fixed-width scaled-integer layouts and 38/76-digit interchange profiles | A columnar physical representation, not a general exact value/arithmetic contract; scale belongs to the field schema |

License disposition: algorithmic behavior was studied, but the implementation
will be independently authored. Any later copied or adapted code requires a
separate review and the applicable license/notice attribution before merge.

## Decision

### 1. Intrinsic value model

`BigDecimal` is an immutable finite value:

`Coefficient × 10^Exponent`

- `Coefficient` is `System.Numerics.BigInteger`.
- `Exponent` is a signed `int` and is the power of ten, not an inherited
  precision or schema scale.
- Zero is uniquely `(0, 0)`.
- Every nonzero coefficient has no trailing base-ten zero; removed zeroes are
  added to the exponent with checked arithmetic.
- NaN, positive infinity, and negative infinity are outside the domain.
- The value does not store a precision, rounding mode, profile, or mutable
  context. Profiles validate values and operations; they do not change value
  identity.

This makes equality and hash identity structural after normalization and hence
mathematical. Comparison uses sign and adjusted decimal magnitude before any
bounded alignment; it never converts through `decimal`, `double`, or `float`.

### 2. Canonical lexical form

- Zero is `0`.
- Nonzero is the normalized invariant signed coefficient, lowercase `e`, and
  the invariant exponent.
- Positive exponents have no plus sign; negative exponents contain one minus.
- Examples: `123e0`, `123e4`, `-123e-4`.
- Parsers may accept a separately documented invariant decimal/scientific
  grammar, but formatting always emits this single canonical form.
- Input length, coefficient digits, and exponent text are preflighted before
  constructing a `BigInteger` or computing a power of ten.

### 3. Profiles and resource limits

The implementation exposes immutable named profiles and an immutable operation
context. No mutable global context exists.

| Profile | Coefficient digits | Exponent range | Purpose |
| --- | ---: | ---: | --- |
| `Decimal38` | 38 | -38 through 38 | Mandatory SQL/Arrow Decimal128-compatible value envelope |
| `Decimal76` | 76 | -76 through 76 | Optional Arrow Decimal256-compatible value envelope |
| `VariableSafe` | 4,096 | -4,096 through 4,096 | Default general-purpose bounded profile |

`VariableSafe` additionally limits accepted text to 8,192 characters, an
operation's estimated working coefficient to 8,192 decimal digits, and a
single operation's owned temporary allocation budget to 64 KiB. These are
conservative API-safety limits, not claims about `BigInteger` capacity. Callers
may construct a stricter or larger immutable context explicitly, subject to
absolute `int` and allocation preflight checks.

Fixed-schema adapters validate the schema independently. For `(precision,
scale)`, `value × 10^scale` must be an integer whose magnitude fits `precision`
digits. Admission never rounds.

### 4. Exact arithmetic and result policy

- `+`, `-`, `*`, exact `/`, and remainder are checked exact operations under
  `BigDecimalContext.Default` (`VariableSafe`).
- Operand spellings, digit counts, and exponents do not become result
  precision. Each result is normalized and checked against the selected
  context.
- Addition/subtraction preflight exponent alignment and working digits before
  computing `10^difference`.
- Multiplication preflights checked exponent addition and the coefficient-digit
  upper bound before multiplying.
- Exact division reduces coefficient factors. After cancellation, the
  denominator may contain only factors of 2 and 5. A remaining factor causes a
  nonterminating-result exception; zero divisor and context overflow have
  distinct failures.
- Exponent/scale math uses checked arithmetic before allocation.
- A fixed-width fast path may be added only after the verification plan shows
  identical results/failures and a material benchmark benefit.

### 5. Rounded operations

Rounding is never selected implicitly. Rounded division or rescaling requires:

1. an explicit target scale or significant precision; and
2. one explicit rounding mode: toward zero, away from zero, toward positive
   infinity, toward negative infinity, nearest ties-to-even, or nearest
   ties-away-from-zero.

Quantization returns a mathematical value and verifies the requested scale,
but scale is not retained as intrinsic identity. Callers that need presentation
scale retain it in their schema or format instruction.

### 6. Conversion policy

- Exact APIs are named `To...Exact` / `TryTo...Exact` and reject fractional,
  range, or precision loss.
- Rounded, truncating, and saturating APIs are separately named and require
  their policy arguments.
- Floating-point construction must explicitly choose exact binary-value
  conversion or invariant round-trip-text conversion; no implicit float/double
  operator is planned for TY-06.
- `SqlDecimal` is a test oracle and boundary conversion only, never backing
  storage or a public dependency.
- Arrow Decimal128/256 adapters operate on scaled two's-complement integers and
  field precision/scale; they share the same intrinsic BigDecimal semantics.

### 7. Generic math

TY-07 will evaluate generic-math interfaces member by member. An interface is
implemented only when its operator and conversion contracts can retain exact
semantics and explicit rounding. Convenience is not sufficient justification
for an operator that silently rounds (especially division).

## Consequences

- TY-06 can implement parsing, canonicalization, equality, comparison, hash,
  and exact native conversions without choosing arithmetic rounding behavior.
- TY-07 can implement checked arithmetic and explicit contexts against a
  stable value identity.
- Decimal38 and Decimal76 transport optimizations remain possible but are not
  part of the logical value contract.
- Very large or hostile inputs fail before exponent-dependent allocation. A
  caller that truly needs larger values must opt into a larger immutable
  context.
- Canonical text does not preserve an input's cosmetic scale. Schema scale and
  display formatting are separate concerns.
- The owned implementation avoids third-party runtime dependencies and avoids
  importing SQL-specific types into the public API.
- The golden-vector and benchmark plan in
  `/docs/30-contracts/bigdecimal-verification-plan.md` is required evidence for
  implementation and any later optimized storage path.
