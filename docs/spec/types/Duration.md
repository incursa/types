---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/Duration.md"
  path: /docs/spec/types/Duration.md
---

# Duration Behavioral Specification

- Type: `Duration`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
ISO-8601 duration value object with parsing, canonical string formatting, and date arithmetic.

## Canonical Value Model
- Backing representation: nullable component values for years/months/weeks/days/hours/minutes/seconds.
- Canonical string representation: ISO-8601 with ordered components; zero is `P0D`.
- Equality/comparison basis: canonical string ordering.

## Input Contract
### Accepted
- ISO-8601 duration strings accepted by parser regex, including fractional and negative units.

### Rejected
- Null/empty/whitespace and malformed duration tokens.

## Normalization Rules
- **DURATION-NORM-001**: output ordering is date components then optional time components.
- **DURATION-NORM-002**: zero-equivalent duration outputs `P0D`.
- **DURATION-NORM-003**: zero components are omitted from canonical output.

## Public API Behavior
### Parse/TryParse
- `Parse` throws `FormatException` when input is invalid.
- `TryParse` returns null/false for invalid input.

### Formatting
- `ToString` returns canonical ISO-8601 representation.

### Calculate
- Applies integral and fractional component parts in unit order from years to milliseconds.

### Converters/Serialization
- JSON converter accepts valid duration string and writes canonical string.

## Error Contracts
- Invalid parse input throws `FormatException` via `Parse`.
- JSON invalid value throws `JsonException`.

## Compatibility Notes
- Negative components are preserved in canonical output.

## Test Requirements
- Required scenario IDs for traceability:
  - `DURATION-RT-001`
  - `DURATION-NORM-001`
  - `DURATION-CALC-001`
  - `DURATION-PARSE-INVALID-001`
  - `DURATION-CONVERT-001`
