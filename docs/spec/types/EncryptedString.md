---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "/C:/docs/spec/types/EncryptedString.md"
  path: /docs/spec/types/EncryptedString.md
---

# EncryptedString Behavioral Specification

- Type: `EncryptedString`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents ciphertext-like opaque text with validation constraints.

## Canonical Value Model
- Backing representation: validated ciphertext-like base64 string.
- Canonical string representation: exact input payload (no re-encoding normalization).
- Equality/comparison basis: Exact encoded payload.

## Input Contract
### Accepted
- Non-empty base64 text with length multiple of 4 and decoded byte length >= 12.

### Rejected
- Null/whitespace input.
- Non-base64 strings.
- Base64 strings shorter than ciphertext minimum expectations.

## Normalization Rules
- Value is preserved as provided once validated.

## Public API Behavior
### Construction
- `Parse` and `From` validate payload format/shape before storing.
- `GenerateRandom` returns parseable base64 ciphertext-like payload.

### Parse/TryParse
- `TryParse` returns false/default (or null for nullable overload) on invalid input.
- `Parse` throws on invalid input.

### Formatting/ToString
- `ToString()` returns canonical representation.

### Converters/Serialization
- JSON and type converters round-trip payload strings.
- Invalid converter string input throws `FormatException`/`JsonException`.

## Error Contracts
- Empty payload -> `ArgumentException`.
- Invalid base64 shape/content -> `FormatException`.

## Compatibility Notes
- Behavior changes from previous runtime semantics require an entry in `docs/spec/compat-decisions.md`.

## Test Requirements
- Required scenario IDs for traceability:
  - ENCRYPTEDSTRING-CONSTRUCT-001
  - ENCRYPTEDSTRING-PARSE-001
  - ENCRYPTEDSTRING-TRYPARSE-001
  - ENCRYPTEDSTRING-FORMAT-001
  - ENCRYPTEDSTRING-CONVERT-001
