---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/ShortCode.md"
  path: /docs/spec/types/ShortCode.md
---

# ShortCode Behavioral Specification

- Type: `ShortCode`
- Namespace: `Incursa`
- Status: `Draft`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a validated short alphanumeric code with canonical normalized formatting.

## Canonical Value Model
- Backing representation: Normalized uppercase raw code.
- Canonical string representation: Uppercase canonical code with formatting helpers.
- Equality/comparison basis: Canonical raw code.

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

### Generation
- Random generation rejects candidates containing a curated set of unwanted fragments and retries until an acceptable candidate is produced.
- The generation policy also recognizes digit substitutions supported by the short-code alphabet.
- The policy applies only to newly generated values. Construction and parsing continue to accept all structurally valid codes for backward compatibility.
- The policy covers high-confidence profanity, slurs, and hostile slang in English, Spanish, French, German, Dutch, Italian, Portuguese, and Latin-script Arabic where the terms can be represented by the short-code alphabet.
- Inclusion means a fragment is unsuitable for an uncontextualized, randomly generated public code; it does not assert that the term is universally offensive in every identity, cultural, or reclaimed use.
- Vocabulary is curated from the MIT-licensed [`cuss`](https://github.com/words/cuss) dataset and the public-domain [`censor-text/profanity-list`](https://github.com/censor-text/profanity-list), then reviewed rather than imported wholesale. No external corpus is loaded at runtime.

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
  - SHORTCODE-CONSTRUCT-001
  - SHORTCODE-PARSE-001
  - SHORTCODE-TRYPARSE-001
  - SHORTCODE-FORMAT-001
  - SHORTCODE-CONVERT-001
  - SHORTCODE-GENERATE-SAFETY-001
