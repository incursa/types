---
workbench:
  type: spec
  workItems:
    - TASK-0009
  codeRefs:
    - Incursa.Types.Primitives/ExactPeriod.cs
  path: /docs/spec/types/ExactPeriod.md
  pathHistory:
    - "C:/docs/spec/types/ExactPeriod.md"
---

# ExactPeriod Behavioral Specification

## Summary

`ExactPeriod` is a half-open interval `[StartInclusive, EndExclusive)` over
exact `DateTimeOffset` instants. The declared endpoint offsets are retained as
part of value identity and wire representation.

## Goals

- Exact 100-nanosecond interval boundaries and duration.
- Unambiguous empty, containment, touching, and overlap semantics.
- Offset-preserving canonical text and equality.

## Non-goals

- Replacing or changing legacy `Period`.
- Implicit timezone or DST resolution.
- Ordering intervals as values.

## Requirements

### Value and interval semantics

- End must be the same instant as or later than start. Equal instants form an
  empty interval.
- Containment is inclusive at start and exclusive at end.
- Empty intervals overlap nothing. Nonempty intervals that only touch do not
  overlap.
- `Elapsed` is the exact checked difference of endpoint UTC ticks.
- Equality and hashing include both local endpoint ticks and declared offsets.
  `IsInstantEquivalentTo` is the explicit offset-insensitive alternative.

### Construction and serialization

- `FromElapsed` accepts only nonnegative exact elapsed durations.
- `FromCalendarDuration` resolves the start and calendar result using an
  explicit zone and `LocalTimeResolution`; a result before start is rejected.
- Canonical text is two fixed seven-fraction offset endpoints separated by
  `/`: `yyyy-MM-ddTHH:mm:ss.fffffffzzz/yyyy-MM-ddTHH:mm:ss.fffffffzzz`.
- Parsing accepts only that profile, retains both offsets, and rejects reversed
  instants. JSON uses the same canonical string.
- The type lives in dependency-free `Incursa.Types.Primitives` and is forwarded
  by `Incursa.Types`.

### Compatibility

Legacy `Period` remains unchanged. `ExactPeriod` is an opt-in type with a
different two-endpoint wire format and offset-sensitive identity, so consumers
must migrate explicitly rather than silently interpreting existing payloads.

## Test scenarios

- `EXACTPERIOD-BOUNDARY-001`: empty, containment, touching, and overlap cases.
- `EXACTPERIOD-IDENTITY-001`: declared offsets affect equality while instant
  equivalence ignores them.
- `EXACTPERIOD-PARSE-001`: canonical endpoint/offset/100ns and JSON round-trip.
- `EXACTPERIOD-DURATION-001`: exact elapsed and calendar/zone construction.
