---
workbench:
  type: spec
  workItems:
    - TASK-0003
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/Money.md"
  path: /docs/spec/types/Money.md
---

# Money Behavioral Specification

- Type: `Money`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-11-27`
- Defining package: `Incursa.Types.Primitives`

## Domain Purpose
Represents monetary amounts as normalized decimal values with stable formatting and deterministic arithmetic.

## Canonical Value Model
- Backing representation: `decimal` normalized to 2 fractional digits.
- Canonical string representation: invariant numeric string (for example `123.45`, `-10`, `0.5`).
- Equality/comparison basis: normalized decimal value.

## Input Contract
### Accepted
- `decimal` constructor inputs across the `decimal` range.
- Invariant numeric strings accepted by `decimal.Parse(..., NumberStyles.Number, InvariantCulture)`.
- JSON number tokens and JSON string tokens representing valid invariant numeric values.

### Rejected
- `null`, empty, or whitespace strings.
- Non-numeric values (`"abc"`, `"$1.00"`, locale-specific separators not valid in invariant parsing).
- Values outside `decimal` range.

## Normalization Rules
- **MONEY-NORM-001**: Normalize to exactly 2 fractional digits with banker's rounding (`MidpointRounding.ToEven`).
- **MONEY-NORM-002**: Normalization is applied at construction and on operation results (because operators construct new `Money`).
- **MONEY-NORM-003**: `Value`, `ToDecimal`, and `ToString` expose normalized values.

Examples:

| Input | Normalized |
| --- | --- |
| `123.4567m` | `123.46m` |
| `1.005m` | `1.00m` |
| `1.015m` | `1.02m` |
| `-2.225m` | `-2.22m` |

## Public API Behavior
### Construction
- **MONEY-CONSTRUCT-001**: `new Money(decimal)` always returns normalized value.
- **MONEY-CONSTRUCT-002**: `From(decimal?)` returns `null` for `null`, otherwise normalized `Money`.

### Parse/TryParse
- **MONEY-PARSE-001**: `Parse(string)` uses invariant numeric parsing and throws on invalid input.
- **MONEY-PARSE-002**: `TryParse(string, out Money)` returns `false` for invalid input and never throws.
- **MONEY-PARSE-003**: `Parse/TryParse` with provider overloads use provided provider or invariant when provider is null.

### Formatting/ToString
- **MONEY-FORMAT-001**: `ToString()` returns invariant canonical numeric form.
- **MONEY-FORMAT-002**: `ToCurrency`, `ToAccounting`, `ToNumberString`, `ToPlainNumberString` use provided static format profiles.

### English Money Words
- **MONEY-WORDS-001**: `ToWords()` formats the normalized value directly from
  `decimal`; it does not convert the full amount to `long`, `double`, or another
  precision-losing representation.
- **MONEY-WORDS-002**: Negative non-zero values use the `minus` prefix. Zero,
  including a normalized negative zero, is `zero dollars`.
- **MONEY-WORDS-003**: The dollar phrase is always present. It uses `dollar`
  only for exactly one dollar and `dollars` otherwise. A cents clause is
  appended only when normalized cents are non-zero, using `cent` only for one.
- **MONEY-WORDS-004**: Values below one dollar retain the dollar phrase, for
  example `0.01` is `zero dollars and one cent` and `-0.01` is
  `minus zero dollars and one cent`.
- **MONEY-WORDS-005**: Integers use deterministic US English short-scale names,
  British-style `and` within a non-zero hundreds group, and hyphens for compound
  tens. Supported scales are thousand through octillion, covering the full
  normalized `decimal` range.
- **MONEY-WORDS-006**: General localization and currency selection are outside
  this API. Currency labels are the English `dollar(s)` and `cent(s)`.

### Arithmetic/Comparison
- **MONEY-ARITH-001**: All arithmetic operators return normalized `Money` results.
- **MONEY-ARITH-002**: Comparison operators compare normalized backing value.
- **MONEY-ARITH-003**: Relative percentage helpers handle zero-denominator paths without throwing.

### Converters/Serialization
- **MONEY-CONVERT-001**: `MoneyJsonConverter` reads both numeric and string tokens when valid.
- **MONEY-CONVERT-002**: `MoneyJsonConverter` writes invariant numeric string.
- **MONEY-CONVERT-003**: `MoneyTypeConverter` supports conversion to/from `string` and `decimal` and fails on unsupported invalid conversions.

## Error Contracts
- `Parse` throws `FormatException`/`OverflowException` per decimal parsing behavior.
- `TryParse` returns `false` and `default` result for invalid input.
- JSON read throws `JsonException` for invalid tokens/value text.

## Compatibility Notes
- DEC-001 approved change: normalization switched from truncation to banker's rounding.
- TY-03 removes Humanizer without hiding observable fixes. Conventional output
  such as `123.45` remains `one hundred and twenty-three dollars and forty-five
  cents`. Singular amounts now use `one dollar`/`one cent` instead of the prior
  hard-coded plurals. Negative sub-dollar amounts now retain their sign and
  cents instead of collapsing to `zero dollars`. Amounts outside the `long`
  range now format through octillion instead of failing during integer
  conversion.

## Test Requirements
- Required scenario IDs for traceability:
  - `MONEY-NORM-001`
  - `MONEY-PARSE-001`
  - `MONEY-PARSE-002`
  - `MONEY-FORMAT-001`
  - `MONEY-WORDS-001`
  - `MONEY-WORDS-002`
  - `MONEY-WORDS-003`
  - `MONEY-WORDS-004`
  - `MONEY-WORDS-005`
  - `MONEY-ARITH-001`
  - `MONEY-CONVERT-001`
