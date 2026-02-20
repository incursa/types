---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/Percentage.md"
  path: /docs/spec/types/Percentage.md
---

# Percentage Behavioral Specification

- Type: `Percentage`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents percentages as fractional decimal values (`0.125` = 12.5%) with deterministic truncation and formatting.

## Canonical Value Model
- Backing representation: `decimal` raw fraction truncated to 4 fractional digits.
- Canonical string representation: `ToString()` percent format with default 2 decimal places (`12.34%`).
- Equality/comparison basis: `Value` (raw fraction).

## Input Contract
### Accepted
- Numeric decimal inputs for constructor/`Parse`.
- Scaled parse values via `ParseScaled`/`TryParseScaled`.

### Rejected
- Null/empty/whitespace and non-numeric strings.

## Normalization Rules
- **PCT-NORM-001**: constructor truncates raw fractional values to 4 decimal digits.
- **PCT-NORM-002**: `ScaledValue` is `Value * 100`.

## Public API Behavior
### Construction
- `new Percentage(decimal|double)` normalizes by truncation.

### Parse/TryParse
- `Parse/TryParse` treat text as raw fractional form.
- `ParseScaled/TryParseScaled` treat text as scaled percentage form.

### Formatting/ToString
- `ToString()` defaults to 2 decimal places with percent sign.
- `ToString(int decimals)` truncates display value to requested precision.
- Formatting is invariant-culture.

### Converters/Serialization
- JSON converter reads string/number forms and writes invariant raw fractional string.
- TypeConverter supports `string` and `decimal` conversions.

## Error Contracts
- `Parse` throws on invalid numeric text.
- `TryParse` returns false/default on invalid input.

## Compatibility Notes
- Current behavior intentionally truncates rather than rounds raw fractional values.

## Test Requirements
- Required scenario IDs for traceability:
  - `PCT-NORM-001`
  - `PCT-PARSE-001`
  - `PCT-SCALED-001`
  - `PCT-FORMAT-001`
  - `PCT-CONVERT-001`
