---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/Period.md"
  path: /docs/spec/types/Period.md
---

# Period Behavioral Specification

- Type: `Period`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a time interval with explicit inclusive start and exclusive end semantics.

## Canonical Value Model
- Backing representation: `StartInclusive`, `EndExclusive`, `Duration`.
- Canonical string representation: `{StartInclusive:O}/{Duration}`.
- Equality/comparison basis: record value equality.

## Input Contract
### Accepted
- Constructor `(start, end)` where `end >= start`.
- Constructor `(start, duration)` where calculated end is not before start.
- Parse format: `start/duration`.

### Rejected
- Ranges where end is before start.
- malformed interval text.

## Normalization Rules
- **PERIOD-SEM-001**: containment and overlap use half-open semantics `[start, end)`.
- **PERIOD-SEM-002**: touching boundaries are not overlaps.

## Public API Behavior
### Parse/TryParse
- `Parse` throws `FormatException` when invalid.
- `TryParse` returns null/false on invalid.

### Interval logic
- `Contains(date)` is true iff `date >= StartInclusive && date < EndExclusive`.
- `Overlaps(other)` is true iff intervals intersect with positive width.

### Converters/Serialization
- JSON converter round-trips canonical string form.

## Error Contracts
- Invalid constructors throw `ArgumentException`.
- Invalid parse throws `FormatException`.

## Compatibility Notes
- End-exclusive semantics are authoritative.

## Test Requirements
- Required scenario IDs for traceability:
  - `PERIOD-BOUNDARY-001`
  - `PERIOD-OVERLAP-001`
  - `PERIOD-PARSE-001`
  - `PERIOD-PARSE-INVALID-001`
  - `PERIOD-CONVERT-001`
