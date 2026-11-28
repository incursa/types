---
workbench:
  type: spec
  workItems:
    - TASK-0006
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
