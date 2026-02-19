# Changelog

All notable changes to this project are documented in this file.

## [Unreleased]

### Added
- Regression hardening test suite covering strict parsing, boundary semantics, deterministic recurring windows, default-value safety, converter failure behavior, and invariant numeric serialization.
- Spec-driven low-coverage test suite for `CountryCode`, `CurrencyCode`, `IpAddress`, `PhoneNumber`, `ShortCode`, and `UsaState` covering parse/tryparse, formatting, converter, and serialization contracts.
- Wave-1 hardening test suite for `JsonContext`, `MonthOnly`, `BvFile`, `EmailAddress`, `EncryptedString`, `TimeZoneId`, `Url`, `Percentage`, and `MoneyExtensions`.
- Wave-2 hardening test suite for `JsonContext`, `Maybe<T>`, `CidrRange`, `Money`, and `UsaState` edge-case API behavior.
- Additional mutation-targeted assertions for `Percentage` and `TimeZoneId` comparison/converter/parse edge behavior.
- `docs/migration-v2.md` migration guide for breaking hardening changes.
- Spec-first behavior system under `docs/spec/` (per-type specs, compatibility decisions, and test traceability map).
- `docs/coverage-ratchet-plan.md` for staged coverage-gate increases to the long-term target.
- CI quality gates for:
  - coverage threshold baseline (ratcheting from line `52%` / branch `29%` toward `85%` / `85%`)
  - mutation testing threshold (Stryker `break-at` 70%)
- `EncryptedString` invariant enforcement partial implementation for Base64 ciphertext validation.

### Changed
- `ShortCode` converted to strict value semantics (`readonly record struct`) with validated parsing.
- `Duration` canonical formatting/comparison corrected, including negative component round-tripping and zero canonical form (`P0D`).
- `Period.Contains` and `Period.Overlaps` corrected to strict half-open interval semantics (`[start, end)`).
- `RecurringPeriod.GetPeriod` now derives windows deterministically from provided `startUtc`.
- `FastId` default state hardened and equality corrected to compare underlying value.
- `JsonContext` default access made safe; mutation of uninitialized default now fails with explicit error.
- `Money` and `Percentage` parse/serialize behavior standardized to invariant culture.
- `MonthOnly` namespace normalized to `Incursa`.
- Type converters for several public types now fail fast on invalid input instead of returning default values.
- Test infrastructure normalized to stable xUnit v2/VSTest package alignment to restore reliable Stryker mutant-to-test mapping.
- CI quality gates ratcheted to line `65%` / branch `50%`, and mutation gate expanded to wave-B scope (`Money`, `JsonContext`, `MonthOnly`) with non-zero threshold.
- Wave-1 specifications promoted to `Approved` status for `JsonContext`, `MonthOnly`, `BvFile`, `EmailAddress`, `EncryptedString`, `TimeZoneId`, and `Url`.
- Repository text normalization policy now enforces LF-only line endings via `.gitattributes` + `.editorconfig`.
- Mutation hardening for wave-B types increased to `58.25%` score (`Money`, `JsonContext`, `MonthOnly`) and CI mutation thresholds were ratcheted to `break-at 35`, `threshold-low 35`, `threshold-high 55`.
- CI mutation scope expanded to include `Percentage` and `TimeZoneId` after pilot validation (expanded five-file score `54.91%`).

### Fixed
- Conflicting/non-project license header in `Duration`.
- Multiple silent-invalid-state paths caused by default coercion and permissive converters.
- `PhoneNumber.GenerateRandom()` now returns parseable E.164 values instead of constructing invalid GUID-based values.
- `EmailAddress.GenerateRandom()` now returns parseable canonical email values instead of constructing invalid GUID-only payloads.
- `JsonContext.GetData(name, ...)` now correctly deserializes object/array nodes instead of requiring scalar-only `JsonValue`.
- `MonthOnly.TryParse` now rejects out-of-range year/month values (for example `0000-01`, `2025-13`) without throwing.
