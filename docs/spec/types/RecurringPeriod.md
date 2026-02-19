# RecurringPeriod Behavioral Specification

- Type: `RecurringPeriod`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents recurring time windows defined by cron expressions.

## Canonical Value Model
- Backing representation: `CronExpression`.
- Canonical string representation: cron expression text from `Expression.ToString()`.
- Equality/comparison basis: record value equality.

## Input Contract
### Accepted
- Valid cron expressions supported by Cronos.

### Rejected
- Invalid cron expressions.

## Normalization Rules
- **RECUR-DET-001**: `GetPeriod(startUtc)` must compute both start and next end from the provided reference time.
- **RECUR-DET-002**: no dependence on ambient wall-clock time for deterministic calculations.

## Public API Behavior
### Parse/TryParse
- `Parse` throws on invalid cron expression.
- `TryParse` returns null/false for invalid values.

### GetPeriod
- Returns default when no prior/next occurrence exists.
- Otherwise returns `Period` bounded by previous and next occurrence around reference time.

### Converters/Serialization
- JSON converter reads/writes cron expression string.

## Error Contracts
- Invalid parse throws `CronFormatException` via `Parse`.
- JSON invalid value throws `JsonException`.

## Compatibility Notes
- Determinism by explicit reference time is required.

## Test Requirements
- Required scenario IDs for traceability:
  - `RECUR-DET-001`
  - `RECUR-PARSE-001`
  - `RECUR-PARSE-INVALID-001`
  - `RECUR-WINDOW-001`
  - `RECUR-CONVERT-001`
