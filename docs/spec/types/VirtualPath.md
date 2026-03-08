---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "/C:/docs/spec/types/VirtualPath.md"
  path: /docs/spec/types/VirtualPath.md
---

# VirtualPath Behavioral Specification

- Type: `VirtualPath`
- Namespace: `Incursa`
- Status: `Draft`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a logical virtual path split into normalized path segments.

## Canonical Value Model
- Backing representation: Ordered segment list with separator metadata.
- Canonical string representation: Joined segment string using the configured separator.
- Equality/comparison basis: Segment sequence plus separator.

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
  - VIRTUALPATH-CONSTRUCT-001
  - VIRTUALPATH-PARSE-001
  - VIRTUALPATH-TRYPARSE-001
  - VIRTUALPATH-FORMAT-001
  - VIRTUALPATH-CONVERT-001
