# BvFile Behavioral Specification

- Type: `BvFile`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents binary file content with optional file name and content type metadata.

## Canonical Value Model
- Backing representation: `BinaryData` plus immutable `FileName` and `ContentType`.
- Canonical string representation: not applicable (binary domain object, no custom `ToString` contract).
- Equality/comparison basis: Record equality over payload and metadata.

## Input Contract
### Accepted
- Byte-array, span, stream, `BinaryData`, base64, path, and `FileInfo` constructor/factory inputs.
- Base64 factory overloads with explicit or default metadata.

### Rejected
- Invalid base64 payloads for `FromBase64`.
- Null/empty path for `FromPath`.
- Null `FileInfo` for `FromFileInfo`.

## Normalization Rules
- `FromBase64(base64)` defaults `FileName` to `file` and `ContentType` to `application/octet-stream`.
- `FromPath` and `FromFileInfo` infer MIME type from file name extension.

## Public API Behavior
### Construction
- Constructors preserve provided metadata and wrap bytes as `BinaryData`.
- Factory methods delegate to constructors with validated inputs.

### Parse/TryParse
- Not applicable: `BvFile` does not expose `Parse`/`TryParse`.

### Formatting/ToString
- Not applicable: no dedicated formatting API.

### Converters/Serialization
- JSON constructor supports serialization with `BinaryData`, file name, and content type.

## Error Contracts
- Invalid base64 in `FromBase64` -> `FormatException`.
- Invalid path/file metadata arguments propagate framework argument exceptions.

## Compatibility Notes
- Behavior changes from previous runtime semantics require an entry in `docs/spec/compat-decisions.md`.

## Test Requirements
- Required scenario IDs for traceability:
  - BVFILE-CONSTRUCT-001
  - BVFILE-PARSE-001
  - BVFILE-TRYPARSE-001
  - BVFILE-FORMAT-001
  - BVFILE-CONVERT-001
