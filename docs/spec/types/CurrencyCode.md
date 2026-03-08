---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "/C:/docs/spec/types/CurrencyCode.md"
  path: /docs/spec/types/CurrencyCode.md
---

# CurrencyCode Behavioral Specification

- Type: `CurrencyCode`
- Namespace: `Incursa`
- Status: `Draft`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a valid ISO-4217 currency code and metadata.

## Canonical Value Model
- Backing representation: Uppercase ISO-4217 code with metadata lookup.
- Canonical string representation: Uppercase 3-letter code (for example `USD`).
- Equality/comparison basis: Normalized currency code.

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
  - CURRENCYCODE-CONSTRUCT-001
  - CURRENCYCODE-PARSE-001
  - CURRENCYCODE-TRYPARSE-001
  - CURRENCYCODE-FORMAT-001
  - CURRENCYCODE-CONVERT-001
