# Coverage Ratchet Plan

This repo uses a ratcheting strategy to move coverage gates to the long-term target without breaking delivery on an existing low baseline.

## Current baseline (2026-02-19)
- Line: `60%`
- Branch: `45%`

Configured in `.github/workflows/quality.yml` as:
- `COVERAGE_THRESHOLD_LINE`
- `COVERAGE_THRESHOLD_BRANCH`

## Long-term target
- Line: `85%`
- Branch: `85%`

## Ratchet policy
1. Never lower thresholds.
2. Raise line/branch thresholds when a PR lifts sustained total coverage above the current gate by at least 2 points.
3. Prefer raising branch threshold first for parser/validator-heavy types.
4. For each threshold increase, update:
   - `.github/workflows/quality.yml`
   - this document
   - `CHANGELOG.md` (Unreleased)

## Immediate focus areas
- `BvFile`
- `JsonContext`
- `EmailAddress` / `EncryptedString`
- `MoneyExtensions`
- `Percentage`
- `MonthOnly`
- `TimeZoneId` / `Url`
