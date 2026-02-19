# JsonContext Behavioral Specification

- Type: `JsonContext`
- Namespace: `Incursa`
- Status: `Draft`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents mutable JSON object context data used for typed and untyped access.

## Canonical Value Model
- Backing representation: `JsonObject` payload.
- Canonical string representation: Serialized JSON object text.
- Equality/comparison basis: Record struct equality over backing state.

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
  - JSONCONTEXT-CONSTRUCT-001
  - JSONCONTEXT-PARSE-001
  - JSONCONTEXT-TRYPARSE-001
  - JSONCONTEXT-FORMAT-001
  - JSONCONTEXT-CONVERT-001
