# MonthOnly Behavioral Specification

- Type: `MonthOnly`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a year-month value independent of day and time.

## Canonical Value Model
- Backing representation: month number offset (`0..119987`).
- Canonical string representation: `yyyy-MM`.
- Equality/comparison basis: Month number.

## Input Contract
### Accepted
- `new MonthOnly(year, month)` for Gregorian-valid `year`/`month`.
- `Parse/TryParse` values matching `yy-M`, `yy-MM`, `yyyy-M`, `yyyy-MM`.
- `Parse/TryParse` values parseable via invariant `DateTime` parsing fallback.

### Rejected
- Null/whitespace or malformed parse text.
- `FromMonthNumber` outside `0..119987`.
- `AddMonths` overflow beyond representable range.

## Normalization Rules
- All string formatting normalizes to `yyyy-MM`.
- Internal representation normalizes to month-number offset.

## Public API Behavior
### Construction
- Constructor validates Gregorian date parts through `DateOnly`.
- `FromMonthNumber` validates range and throws on overflow.

### Parse/TryParse
- `TryParse` returns false/default for invalid input.
- `Parse` throws `FormatException` for invalid input.

### Formatting/ToString
- `ToString()` returns canonical `yyyy-MM`.

### Converters/Serialization
- JSON converter accepts only JSON string tokens.
- JSON and type converters round-trip canonical `yyyy-MM`.
- Invalid converter input throws format/JSON exceptions.

## Error Contracts
- `Parse` invalid input -> `FormatException`.
- `FromMonthNumber` and `AddMonths` overflow -> `ArgumentOutOfRangeException`.
- `CompareTo(object)` non-`MonthOnly` argument -> `ArgumentException`.

## Compatibility Notes
- Behavior changes from previous runtime semantics require an entry in `docs/spec/compat-decisions.md`.

## Test Requirements
- Required scenario IDs for traceability:
  - MONTHONLY-CONSTRUCT-001
  - MONTHONLY-PARSE-001
  - MONTHONLY-TRYPARSE-001
  - MONTHONLY-FORMAT-001
  - MONTHONLY-CONVERT-001
