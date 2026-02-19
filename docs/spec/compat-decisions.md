# Compatibility Decisions

Case-by-case decisions when desired behavior differs from current behavior.

## Decision Log

### DEC-001: Money precision normalization
- Date: 2026-02-19
- Type: `Money`
- Prior behavior: values normalized by truncation to 2 decimal places.
- New behavior: values normalized using banker's rounding (`MidpointRounding.ToEven`) to 2 decimal places.
- Decision: **Approved**
- Rationale:
  - avoids systematic rounding bias in aggregate financial calculations
  - preserves non-failing behavior for high-precision inputs
  - provides deterministic canonicalization
- Migration impact:
  - midpoint values may differ from previous truncation behavior (for example `123.455` now normalizes to `123.46`)
- Required actions:
  - update `Money` runtime normalization
  - update money tests and docs
