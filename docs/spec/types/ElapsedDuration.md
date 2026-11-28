---
workbench:
  type: spec
  workItems:
    - TASK-0008
  codeRefs:
    - Incursa.Types.Primitives/ElapsedDuration.cs
  path: /docs/spec/types/ElapsedDuration.md
  pathHistory:
    - "C:/docs/spec/types/ElapsedDuration.md"
---

# ElapsedDuration Behavioral Specification

- Type: `ElapsedDuration`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-11-27`

## Domain purpose

Represents an exact signed elapsed amount. It is not a calendar-relative
duration and never interprets years or months as fixed elapsed units.

## Canonical value model

- Backing representation: signed `Int64` count of 100-nanosecond units.
- Zero: unit count zero.
- Equality, hashing, and order: exact signed unit count.
- Native boundary: exact one-to-one conversion with .NET `TimeSpan` ticks.

## Lexical profile

- Canonical zero is `PT0S`.
- Nonzero text is optional leading minus, `PT`, nonnegative integral seconds,
  optional fractional seconds with at most seven digits, then `S`.
- Fractional trailing zeroes are omitted. Examples: `PT1S`, `PT0.0000001S`,
  `-PT12.5S`.
- Parsing accepts the same invariant seconds-only structure plus redundant
  leading/fractional zeroes and normalizes them.
- More than seven fractional digits is rejected rather than rounded.
- Calendar components, other time components, whitespace, plus signs,
  exponents, non-finite labels, and out-of-Int64 magnitudes are rejected.

This is a documented seconds-only subset inspired by ISO 8601-1:2019 duration
syntax. It does not claim full ISO duration conformance: calendar/date
components and component combinations are deliberately outside the exact
elapsed profile.

## Public API behavior

- Construction and `FromUnits` accept the complete signed Int64 domain.
- `FromTimeSpanExact` / `ToTimeSpanExact` preserve every 100ns unit.
- Addition, subtraction, and unary negation use checked Int64 arithmetic and
  throw `OverflowException` outside the domain.
- `Parse` throws `FormatException`; `TryParse` returns false/default.
- General span/string formatting and JSON use canonical invariant text.

## Compatibility and Tabular profile

- `ElapsedDuration` is distinct from the existing calendar-relative
  `Duration`; TY-08 does not change `Duration` wire or arithmetic behavior.
- The current Tabular closed scalar contract does not natively admit duration
  objects. Its exact application mapping is signed `Int64` 100ns units
  (`Units` / `FromUnits`) or canonical string. TY-10 owns qualification of any
  native admission; no millisecond truncation or floating conversion is allowed.

## Test scenarios

- `ELAPSED-MODEL-001`: full signed unit domain, zero, equality, and ordering.
- `ELAPSED-PARSE-001`: canonical seconds text, exact fractional resolution,
  negative values, malformed/calendar rejection, and bounds.
- `ELAPSED-ARITH-001`: checked add/subtract/negation overflow.
- `ELAPSED-CONVERT-001`: exact TimeSpan and JSON round-trips.
