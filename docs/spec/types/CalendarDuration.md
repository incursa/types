---
workbench:
  type: spec
  workItems:
    - TASK-0009
  codeRefs:
    - Incursa.Types.Primitives/CalendarDuration.cs
  path: /docs/spec/types/CalendarDuration.md
  pathHistory:
    - "C:/docs/spec/types/CalendarDuration.md"
---

# CalendarDuration Behavioral Specification

## Summary

`CalendarDuration` is the strict calendar-relative duration profile. Its value
is a signed integral month count, signed integral day count, and signed exact
100-nanosecond elapsed count. It is separate from the compatibility `Duration`
type.

## Goals

- Preserve calendar intent without approximating years or months as days.
- Make elapsed precision and DST choices explicit.
- Provide one invariant canonical wire representation.

## Non-goals

- Full ISO 8601 duration grammar or fractional calendar units.
- A context-free total order: month/day durations require a start and zone to
  become instants.
- Behavioral changes to legacy `Duration`.

## Requirements

### Value and lexical profile

- Components are `Int32 Months`, `Int32 Days`, and `ElapsedDuration Elapsed`.
- Every nonzero component has one shared sign. Mixed-sign construction and
  component-local signs are rejected.
- Input accepts a single leading minus, `P`, ordered integral `Y/M/W/D`
  components, and optional ordered fractional `H/M/S` components after `T`.
- Years normalize to 12 months and weeks to 7 days. Fractional calendar units
  are rejected. Fractional time is accepted only when it is exactly
  representable in 100-nanosecond units; there is no rounding.
- Canonical output uses only normalized months, days, and exact seconds:
  `[-]P[nM][nD][Tn[.fffffff]S]`. Zero is `P0D`; zero fields and trailing
  fractional zeros are omitted.
- Equality and hashing are structural. No comparison operators or
  `IComparable` contract are provided.

This is a documented, ISO-inspired subset and does not claim full ISO 8601
conformance.

### Application

- Zone-free application performs months, days, then exact elapsed ticks.
  Gregorian month arithmetic clamps the day to the target month's last day.
- Named-zone application performs months and days on local fields, requires an
  explicit `LocalTimeResolution`, resolves the local result, then applies the
  elapsed component on the instant timeline.
- `Reject` rejects ambiguous and nonexistent times. `EarlierInstant` and
  `LaterInstant` are valid only for ambiguity; `ShiftForward` and
  `ShiftBackward` are valid only for gaps and move fields by the transition
  gap. Unsupported policy/outcome combinations throw.
- Range and arithmetic overflow fail instead of wrapping.

### Serialization and compatibility

- JSON is a canonical string and invalid input throws `JsonException`.
- The type lives in dependency-free `Incursa.Types.Primitives` and is forwarded
  by `Incursa.Types`.
- Legacy `Duration` remains source, binary, behavioral, and wire compatible;
  adoption is opt-in by type name.

## Test scenarios

- `CALDUR-PARSE-001`: normalization, exact fractional time, invalid order,
  fractional calendar units, excess precision, and range limits.
- `CALDUR-SIGN-001`: one-sign invariant and canonical zero/negative forms.
- `CALDUR-APPLY-001`: month-end clamp, leap dates, negative values, and 100ns.
- `CALDUR-ZONE-001`: both ambiguity choices, both gap shifts, rejection, and
  calendar-before-elapsed DST semantics.
- `CALDUR-JSON-001`: canonical JSON round-trip and invalid input.
