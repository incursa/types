---
workbench:
  type: spec
  workItems:
    - TASK-0006
    - TASK-0007
  codeRefs:
    - Incursa.Types.Primitives/BigDecimal.cs
    - Incursa.Types.Primitives/BigDecimalProfile.cs
  path: /docs/spec/types/BigDecimal.md
  pathHistory:
    - "C:/docs/spec/types/BigDecimal.md"
---

# BigDecimal Behavioral Specification

- Type: `BigDecimal`
- Namespace: `Incursa`
- Status: `Approved for TY-06 core behavior`
- Last Updated: `2026-11-27`

## Domain purpose

Represents one finite exact decimal value without binary-floating conversion,
SQL-specific backing storage, third-party runtime dependencies, or retained
cosmetic scale.

## Canonical value model

- Value: normalized signed `BigInteger Coefficient × 10^int Exponent`.
- Zero is uniquely coefficient zero and exponent zero.
- A nonzero coefficient has no trailing base-ten zero.
- Canonical text is `0` or normalized coefficient + lowercase `e` + signed
  exponent. Positive exponents have no plus sign.
- Equality and hashing use the normalized pair. Comparison uses sign, adjusted
  decimal magnitude, and decimal significand digits; it does not convert to a
  native number or allocate exponent-sized powers of ten.

## Input contract

### Accepted

- Invariant grammar: `[+-]?digits[.digits]?[eE[+-]?digits]?`.
- At least one integer digit is required; when present, the decimal point must
  be followed by at least one fractional digit.
- Leading coefficient zeroes, trailing fractional zeroes, uppercase `E`, and
  exponent plus signs are accepted and normalized.
- `Parse` and `TryParse` overloads accept an immutable `BigDecimalProfile`.

### Rejected

- Whitespace, grouping separators, underscores, missing integer/fractional or
  exponent digits, and culture-specific number syntax.
- NaN and positive/negative infinity.
- Input length, normalized coefficient digits, normalized exponent, or parse
  buffer estimates outside the selected profile.
- Exponent text or normalization arithmetic that overflows `int`.

## Profiles and bounds

| Profile | Coefficient digits | Exponent | Input | Working digits | Temporary allocation |
| --- | ---: | ---: | ---: | ---: | ---: |
| `Decimal38` | 38 | -38..38 | 256 chars | 128 | 4 KiB |
| `Decimal76` | 76 | -76..76 | 512 chars | 256 | 8 KiB |
| `VariableSafe` | 4,096 | -4,096..4,096 | 8,192 chars | 8,192 | 64 KiB |

`VariableSafe` is the default. Profiles are immutable values; no mutable or
ambient precision setting exists.

## Public API behavior

### Construction and parsing

- Constructors normalize and validate a supplied coefficient and exponent.
- `Parse` throws `FormatException` for malformed or out-of-policy text.
- `TryParse` returns false/default for all malformed or out-of-policy text.
- Parsing preflights length, significant digit count, exponent text, checked
  effective exponent, and buffer size before `BigInteger` construction.

### Formatting

- `ToString()` and general (`G`) span formatting return canonical invariant
  text only.
- Other format strings are unsupported.

### Exact native conversions

- `FromDecimalExact` reads the native 96-bit coefficient and scale directly.
- `ToBigIntegerExact`, `ToInt32Exact`, `ToInt64Exact`, and `ToDecimalExact`
  reject fractional, range, or precision loss with `OverflowException`.
- Corresponding `TryTo...Exact` APIs return false/default for loss.
- TY-06 provides no implicit binary floating, rounded, truncating, or
  saturating conversion API.

### Exact arithmetic and remainder

- `+`, `-`, `*`, `/`, and `%` use the stable immutable
  `BigDecimalContext.Default`; named methods accept an explicit context.
- Addition, subtraction, and multiplication return the exact mathematical
  result or `BigDecimalLimitException` when the selected profile cannot admit
  an operand, working value, allocation estimate, exponent, or result.
- Exact division reduces the coefficient fraction. A denominator containing
  factors other than 2 and 5 throws
  `BigDecimalNonTerminatingDivisionException`; a zero divisor throws
  `DivideByZeroException`.
- Remainder uses a quotient truncated toward zero and returns the exact
  residual. Its sign follows the dividend when nonzero.

### Explicit rounding

- `DivideRounded` requires a target scale and `BigDecimalRoundingMode`.
- `Quantize` requires the same explicit target and policy.
- Modes are toward zero, away from zero, toward positive infinity, toward
  negative infinity, nearest ties-to-even, and nearest ties-away-from-zero.
- Scale belongs to the operation/schema, not intrinsic identity; a rounded
  result is normalized after the requested scale has been enforced.

### Generic math compatibility

- Exact arithmetic, comparison, parsing, and formatting members use the
  standard operators/interfaces where their contracts preserve these
  semantics.
- TY-07 deliberately does not implement `INumber<BigDecimal>`. That aggregate
  interface requires a much broader generic conversion and numeric-function
  surface, including truncating/saturating creation policies, whose complete
  conformance has not been qualified. Implementing it partially would create
  misleading generic compatibility.
- No operator or conversion silently substitutes rounded arithmetic. Lossy
  behavior remains available only through separately named APIs with explicit
  policy.

## Compatibility and dependency boundary

- `BigDecimal` and `BigDecimalProfile` live in the dependency-free
  `Incursa.Types.Primitives` package and retain the `Incursa` namespace.
- The `Incursa.Types` assembly forwards both public types for compatibility.
- `SqlDecimal` is used only by tests as a mandatory-domain oracle.
- Arithmetic and explicit rounding are deferred to TY-07.

## Requirement and test scenarios

- `BIGDECIMAL-MODEL-001`: finite BigInteger/exponent model and normalization.
- `BIGDECIMAL-PARSE-001`: invariant exact grammar and canonical round-trip.
- `BIGDECIMAL-LIMIT-001`: malformed, oversized, exponent, and allocation bounds.
- `BIGDECIMAL-IDENTITY-001`: mathematical equality, hash, and comparison.
- `BIGDECIMAL-CONVERT-001`: exact native conversions reject loss.
- `BIGDECIMAL-SQL-001`: Decimal38 boundaries agree with `SqlDecimal`.
- `BIGDECIMAL-EVIDENCE-001`: independently generated JSONL vectors agree with
  the implementation.
- `BIGDECIMAL-ARITHMETIC-001`: exact add/subtract/multiply and classified limits.
- `BIGDECIMAL-DIVIDE-001`: terminating exact division, nontermination, and zero.
- `BIGDECIMAL-REMAINDER-001`: truncated-toward-zero exact residual.
- `BIGDECIMAL-ROUND-001`: scale plus six explicit rounding modes and ties.
- `BIGDECIMAL-CONTEXT-001`: immutable non-ambient operation context.
