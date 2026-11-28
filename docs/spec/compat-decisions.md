---
workbench:
  type: spec
  workItems:
    - TASK-0001
    - TASK-0002
    - TASK-0004
    - TASK-0010
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/compat-decisions.md"
  path: /docs/spec/compat-decisions.md
---

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

### DEC-002: Random generator outputs must be parseable canonical values
- Date: 2026-02-19
- Type: `EmailAddress`
- Prior behavior: `GenerateRandom()` constructed GUID-only text and failed type invariants.
- New behavior: `GenerateRandom()` emits a canonical parseable email (`<random>@example.test`).
- Decision: **Approved**
- Rationale:
  - generator APIs must produce values valid for their own parse/constructor contracts
  - prevents flaky downstream failures in tests and seed-data flows
- Migration impact:
  - callers relying on previous invalid payload shape must update expectations
- Required actions:
  - update runtime generator implementation
  - add generator contract test coverage

### DEC-003: `MonthOnly.TryParse` must not throw on out-of-range components
- Date: 2026-02-19
- Type: `MonthOnly`
- Prior behavior: `TryParse("2025-13")` and other out-of-range month/year inputs could throw due constructor validation.
- New behavior: `TryParse` returns `null` / `false` for out-of-range values without throwing.
- Decision: **Approved**
- Rationale:
  - `TryParse` APIs must be exception-free for invalid user input by contract
  - avoids runtime exceptions in validation-heavy call paths
- Migration impact:
  - callers that previously observed exceptions from `TryParse` now receive non-throwing failure results
- Required actions:
  - guard year/month ranges before construction
  - add boundary and invalid-component tests

### DEC-004: `ShortCode` content safety applies only to generation
- Date: 2026-11-21
- Type: `ShortCode`
- Prior behavior: random generation accepted every structurally valid candidate.
- New behavior: random generation discards candidates containing curated unwanted fragments—including profanity, slurs, hostile slang, multilingual terms, and supported digit substitutions—and retries.
- Decision: **Approved**
- Rationale:
  - prevents accidentally issuing user-visible codes with objectionable content across common languages
  - limits policy cost to code generation rather than every parse or use
  - preserves all previously issued structurally valid codes
- Migration impact:
  - the distribution of newly generated codes excludes disallowed candidates
  - construction and parsing behavior are unchanged
- Required actions:
  - add a deterministic generation-policy helper and tests
  - document the generation-only compatibility boundary

### DEC-005: Time-zone mapping follows supported platform ICU/CLDR data
- Date: 2026-11-27
- Type: `TimeZoneId`
- Prior behavior: TimeZoneConverter supplied a bundled Windows/IANA mapping
  snapshot and could fall back to a resolved platform identifier when an IANA
  canonical mapping was unavailable.
- New behavior: .NET `TimeZoneInfo` performs lookup and Windows/IANA conversion;
  missing conversion data fails with `PlatformNotSupportedException`, corrupt
  host data remains `InvalidTimeZoneException`, and no host-local or unmapped
  identifier is silently substituted.
- Decision: **Approved for TY-04 implementation; SpecAttest requirements remain draft**
- Rationale:
  - removes the third-party runtime dependency using supported .NET 8 APIs
  - preserves common Windows, IANA, UTC, and alias behavior on supported hosts
  - makes invariant/NLS-only/globalization limitations visible to callers
  - lets platform servicing update ICU/CLDR mapping data
- Migration impact:
  - a mapping changed by the host ICU/CLDR version can produce the platform's
    current canonical IANA identifier rather than the old bundled snapshot
  - unsupported hosts now receive an actionable exception instead of a fallback
    identifier
- Required actions:
  - run Windows and Linux ID/alias/invalid-ID tests
  - run an invariant-globalization failure-contract test
  - retain the rich-package boundary and .NET 8 target

### DEC-006: Preserve pre-split identities through package type forwarding

- Date: 2026-11-27
- Area: `Incursa.Types` / `Incursa.Types.Primitives`
- Prior behavior: all public types were defined by the single rich assembly.
- New behavior: dependency-free foundational types are defined once by
  Primitives and forwarded by Types; the public namespace remains `Incursa`.
- Decision: **Approved for package qualification; publication remains separate**
- Rationale:
  - lets low-level libraries consume the foundation without rich runtime
    dependencies
  - preserves source names and old compiled assembly-qualified references
  - prevents duplicate type identity in combined package graphs
- Migration impact:
  - deployments must restore and deploy the new transitive Primitives assembly
  - consumers referencing Types require no package-reference change
  - direct foundational consumers may switch to Primitives and recompile
- Required actions:
  - run both package-only consumers and assert the resolved runtime graph
  - run the pre-split compiled-consumer fixture against current forwarders
  - keep source/binary/behavior/wire release notes separate
