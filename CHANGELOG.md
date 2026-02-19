# Changelog

All notable changes to this project are documented in this file.

## [Unreleased]

### Added
- Regression hardening test suite covering strict parsing, boundary semantics, deterministic recurring windows, default-value safety, converter failure behavior, and invariant numeric serialization.
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

### Fixed
- Conflicting/non-project license header in `Duration`.
- Multiple silent-invalid-state paths caused by default coercion and permissive converters.
