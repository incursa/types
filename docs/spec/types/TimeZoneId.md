---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/TimeZoneId.md"
  path: /docs/spec/types/TimeZoneId.md
---

# TimeZoneId Behavioral Specification

- Type: `TimeZoneId`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a valid time zone identifier, supporting Windows/IANA forms.

## Canonical Value Model
- Backing representation: resolved `TimeZoneInfo` and canonical identifier.
- Canonical string representation: canonical ID returned by `ToString()`.
- Equality/comparison basis: Canonical time zone identity.

## Input Contract
### Accepted
- Valid IANA or Windows time-zone IDs resolvable by `TZConvert`.

### Rejected
- Null/whitespace values.
- Unknown or invalid zone identifiers.

## Normalization Rules
- Windows IDs normalize to IANA when available.
- IANA inputs preserve caller-provided identifier text.
- Fallback canonical ID uses resolved `TimeZoneInfo` identity when necessary.

## Public API Behavior
### Construction
- Constructor and `Parse` resolve/validate ID and populate `TimeZoneInfo`.

### Parse/TryParse
- `TryParse` returns false/default for invalid inputs.
- `Parse` throws `ArgumentException` for invalid IDs.

### Formatting/ToString
- `ToString()` returns canonical representation.

### Converters/Serialization
- JSON and type converters round-trip canonical ID values.
- Invalid converter input throws via base converter/JSON exceptions.

## Error Contracts
- Invalid zone ID resolution throws `ArgumentException` wrapping timezone resolution exceptions.

## Compatibility Notes
- Behavior changes from previous runtime semantics require an entry in `docs/spec/compat-decisions.md`.

## Test Requirements
- Required scenario IDs for traceability:
  - TIMEZONEID-CONSTRUCT-001
  - TIMEZONEID-PARSE-001
  - TIMEZONEID-TRYPARSE-001
  - TIMEZONEID-FORMAT-001
  - TIMEZONEID-CONVERT-001
