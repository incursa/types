---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/CountryCode.md"
  path: /docs/spec/types/CountryCode.md
---

# CountryCode Behavioral Specification

- Type: `CountryCode`
- Namespace: `Incursa`
- Status: `Draft`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a valid ISO-3166 country code.

## Canonical Value Model
- Backing representation: Normalized alpha-2 and alpha-3 ISO country identifiers.
- Canonical string representation: Uppercase alpha-2 code for `ToString()`.
- Equality/comparison basis: Normalized country identity.

## Input Contract
### Accepted
- Valid inputs for constructor/factory/parse APIs that can be normalized into the canonical model.

### Rejected
- `null`, empty, or malformed values that violate type invariants.

## Normalization Rules
- Normalize input to canonical representation on construction and parse paths.

## Public API Behavior
### Construction
- `Parse` throws for invalid values.
- Constructors/factories enforce invariants and normalize canonical form.

### Parse/TryParse
- `TryParse` never throws and returns `false`/`null` on invalid input.
- `Parse` delegates to validated parsing and throws type-appropriate exceptions on invalid input.

### Formatting/ToString
- `ToString()` returns canonical representation.

### Converters/Serialization
- JSON and type converters round-trip canonical values.
- Invalid converter inputs fail fast with explicit exceptions.

## Error Contracts
- Invalid input raises parse/format exceptions based on API contract.

## Compatibility Notes
- Behavior changes from previous runtime semantics require an entry in `docs/spec/compat-decisions.md`.

## Test Requirements
- Required scenario IDs for traceability:
  - COUNTRYCODE-CONSTRUCT-001
  - COUNTRYCODE-PARSE-001
  - COUNTRYCODE-TRYPARSE-001
  - COUNTRYCODE-FORMAT-001
  - COUNTRYCODE-CONVERT-001
