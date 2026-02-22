---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "/C:/docs/spec/types/CidrRange.md"
  path: /docs/spec/types/CidrRange.md
---

# CidrRange Behavioral Specification

- Type: `CidrRange`
- Namespace: `Incursa`
- Status: `Draft`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents an IPv4 or IPv6 CIDR network range for containment checks.

## Canonical Value Model
- Backing representation: Network address and prefix length.
- Canonical string representation: Normalized CIDR text (for example `192.168.0.0/16`).
- Equality/comparison basis: Network and prefix length.

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
  - CIDRRANGE-CONSTRUCT-001
  - CIDRRANGE-PARSE-001
  - CIDRRANGE-TRYPARSE-001
  - CIDRRANGE-FORMAT-001
  - CIDRRANGE-CONVERT-001
