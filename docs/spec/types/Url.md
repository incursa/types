---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/Url.md"
  path: /docs/spec/types/Url.md
---

# Url Behavioral Specification

- Type: `Url`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a valid normalized URL value.

## Canonical Value Model
- Backing representation: `Uri` plus absolute/relative indicator.
- Canonical string representation: `Uri.ToString()` after normalization.
- Equality/comparison basis: Normalized URI value.

## Input Contract
### Accepted
- Valid absolute or relative URI text accepted by `Uri.TryCreate(..., RelativeOrAbsolute)`.

### Rejected
- Null/whitespace input.
- Text not parseable as a URI.

## Normalization Rules
- Absolute URLs normalize scheme and host to lowercase.
- Default ports are removed for `http` (`80`) and `https` (`443`).
- Empty absolute paths normalize to `/`.
- Relative references are preserved without absolute normalization.

## Public API Behavior
### Construction
- Constructor/`Parse` validate and normalize to canonical URI.

### Parse/TryParse
- `TryParse` returns false/default on invalid input.
- `Parse` throws `ArgumentException` for invalid input.

### Formatting/ToString
- `ToString()` returns canonical representation.

### Converters/Serialization
- JSON and type converters round-trip canonical URL text.
- Invalid JSON converter input throws `JsonException`.

## Error Contracts
- Invalid URL parse throws `ArgumentException`.
- Invalid JSON conversion throws `JsonException`.

## Compatibility Notes
- Behavior changes from previous runtime semantics require an entry in `docs/spec/compat-decisions.md`.

## Test Requirements
- Required scenario IDs for traceability:
  - URL-CONSTRUCT-001
  - URL-PARSE-001
  - URL-TRYPARSE-001
  - URL-FORMAT-001
  - URL-CONVERT-001
