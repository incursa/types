---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "/C:/docs/spec/types/FastId.md"
  path: /docs/spec/types/FastId.md
---

# FastId Behavioral Specification

- Type: `FastId`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Compact, time-sortable identifier with deterministic conversion helpers and URL-safe string encoding.

## Canonical Value Model
- Backing representation: signed 64-bit integer containing timestamp/random bits.
- Canonical string representation: uppercase Crockford Base32 (`Encoded`).
- Equality/comparison basis: underlying `Value`.

## Input Contract
### Accepted
- Valid Crockford Base32 strings and raw numeric string values.
- Deterministic source values from guid/string helpers with timestamp bounds.

### Rejected
- Null/empty/whitespace parse inputs.
- Invalid Base32 characters or oversized encoded values.

## Normalization Rules
- **FASTID-NORM-001**: encoded representation is uppercase canonical form.
- **FASTID-NORM-002**: `default(FastId)` is valid empty id (`Value=0`, `Encoded="0"`).

## Public API Behavior
### Generation
- `New()` uses UTC seconds from custom epoch plus random bits.

### Parse/TryParse
- `Parse` throws on invalid values.
- `TryParse` returns false/default and never throws.

### Deterministic factories
- `FromGuidWithinTimestampRange` and `FromStringWithinTimestampRange` throw when max timestamp is before epoch/out of capacity.

### Converters/Serialization
- JSON/type converter round-trips through encoded string.

## Error Contracts
- `Parse` throws `ArgumentException` for invalid values.
- Deterministic helpers throw `ArgumentOutOfRangeException` for invalid max timestamp.

## Compatibility Notes
- Empty/default semantics are explicitly supported.

## Test Requirements
- Required scenario IDs for traceability:
  - `FASTID-EMPTY-001`
  - `FASTID-PARSE-001`
  - `FASTID-DETERMINISTIC-001`
  - `FASTID-BOUNDS-001`
  - `FASTID-CONVERT-001`
