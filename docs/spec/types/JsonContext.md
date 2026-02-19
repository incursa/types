# JsonContext Behavioral Specification

- Type: `JsonContext`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents mutable JSON object context data used for typed and untyped access.

## Canonical Value Model
- Backing representation: `JsonObject`.
- Canonical string representation: `RawData.ToJsonString()`.
- Equality/comparison basis: record-struct equality over backing field state.

## Input Contract
### Accepted
- `JsonObject` constructor values that are non-null.
- Dictionary constructors with non-null keys and values compatible with `JsonValue`.
- `Parse`/`TryParse` text containing a JSON object payload.

### Rejected
- Null `JsonObject` constructor input.
- `Parse` text that is null/whitespace or not a JSON object.
- Default (`default(JsonContext)`) mutation calls via `SetData`.

## Normalization Rules
- `TryParse` only accepts JSON object text and returns null/false for malformed content.
- `RawData` on a default instance resolves to an empty `JsonObject` for reads.

## Public API Behavior
### Construction
- `new JsonContext(JsonObject)` throws `ArgumentNullException` when null.
- `Empty()` returns a writable empty object context.
- `FromObject<T>(null)` returns `Empty()`.

### Parse/TryParse
- `TryParse(string)` returns `null` for invalid/malformed content.
- `TryParse(string, out JsonContext)` returns `false` and `default` on invalid content.
- `Parse(string)` throws `FormatException` for invalid content.

### Formatting/ToString
- `ToString()` returns canonical serialized JSON object text.
- Default instance string form is `{}`.

### Converters/Serialization
- JSON converter reads and writes object payloads.
- Type converter round-trips JSON object strings.
- Invalid type-converter string input throws `FormatException`.

## Error Contracts
- Invalid parse input throws `FormatException`.
- Invalid default mutation throws `InvalidOperationException`.
- Invalid JSON converter input throws `JsonException`.

## Compatibility Notes
- Behavior changes from previous runtime semantics require an entry in `docs/spec/compat-decisions.md`.

## Test Requirements
- Required scenario IDs for traceability:
  - JSONCONTEXT-CONSTRUCT-001
  - JSONCONTEXT-PARSE-001
  - JSONCONTEXT-TRYPARSE-001
  - JSONCONTEXT-FORMAT-001
  - JSONCONTEXT-CONVERT-001
