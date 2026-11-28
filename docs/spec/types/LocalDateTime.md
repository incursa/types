---
workbench:
  type: spec
  workItems:
    - TASK-0008
  codeRefs:
    - Incursa.Types.Primitives/LocalDateTime.cs
  path: /docs/spec/types/LocalDateTime.md
  pathHistory:
    - "C:/docs/spec/types/LocalDateTime.md"
---

# LocalDateTime Behavioral Specification

- Type: `LocalDateTime`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-11-27`

## Domain purpose

Represents Gregorian date and wall-clock fields without a timezone, UTC
offset, or implied host-local interpretation.

## Canonical value model

- Backing representation: `DateTime` with `DateTimeKind.Unspecified`.
- Range: Gregorian years 0001 through 9999 inclusive.
- Resolution: one 100-nanosecond tick.
- Equality, hashing, and ordering: local Gregorian field ticks only.
- Canonical text: `YYYY-MM-DDThh:mm:ss.fffffff`, always exactly seven
  fractional digits and never a zone/offset suffix.

## Input and validation

- Parsing accepts canonical text only.
- Invalid Gregorian dates, year zero/out-of-range years, `24:00`, leap-second
  labels, missing/extra fractional digits, whitespace, and offset/`Z` suffixes
  are rejected.
- A `DateTime` constructor requires `DateTimeKind.Unspecified`; callers must
  remove or resolve existing zone semantics explicitly.
- Component construction validates Gregorian fields and subsecond ticks
  `0..9,999,999`.

This is an extended calendar-date/time subset inspired by ISO 8601-1:2019,
with a deliberately fixed seven-digit fractional field. It does not claim full
ISO date/time conformance: reduced precision, `24:00`, leap seconds, expanded
years, offsets, and zone designators are outside the profile.

## Arithmetic and conversion

- Adding/subtracting `ElapsedDuration` applies exact checked ticks to the local
  fields. Crossing the year range throws `OverflowException`.
- Subtracting two values returns exact `ElapsedDuration` field ticks.
- `Ticks` / `FromTicks` provide an exact Gregorian local-field transport using
  ticks since 0001-01-01; range validation is retained.
- `WithOffset` requires an explicit valid .NET offset and returns the defined
  `DateTimeOffset` instant.
- `InTimeZone` requires an explicit `TimeZoneInfo`; it rejects ambiguous and
  nonexistent local times instead of choosing an offset. Unambiguous values
  use that named zone's offset.
- No API consults `TimeZoneInfo.Local` or `DateTimeKind.Local` implicitly.

## Formatting and serialization

- General span/string formatting and JSON use canonical invariant text.
- `Parse` throws `FormatException`; `TryParse` returns false/default.

## Compatibility and Tabular profile

- This type is separate from `DateTimeOffset`, `MonthOnly`, calendar-relative
  `Duration`, and `Period`; TY-08 changes none of those types.
- The current Tabular closed scalar contract does not natively admit zone-free
  timestamps. Its exact application mapping is `Int64` Gregorian local ticks
  (`Ticks` / `FromTicks`) or canonical string, with no zone inference. TY-10
  owns qualification of any native admission and must preserve this contract.

## Test scenarios

- `LOCALDT-MODEL-001`: Gregorian bounds, leap days, fields, resolution, and order.
- `LOCALDT-PARSE-001`: canonical round-trip and invalid date/time/offset forms.
- `LOCALDT-ARITH-001`: exact elapsed arithmetic and range overflow.
- `LOCALDT-OFFSET-001`: explicit valid/invalid offset resolution.
- `LOCALDT-ZONE-001`: unambiguous zone resolution and default rejection of DST
  ambiguity/nonexistence.
- `LOCALDT-CONVERT-001`: unspecified DateTime and JSON round-trips.
