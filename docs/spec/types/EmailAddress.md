---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/EmailAddress.md"
  path: /docs/spec/types/EmailAddress.md
---

# EmailAddress Behavioral Specification

- Type: `EmailAddress`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-02-19`

## Domain Purpose
Represents a normalized email address and parsed mailbox components.

## Canonical Value Model
- Backing representation: normalized address string plus `MailAddress`.
- Canonical string representation: lowercase local + `@` + lowercase ASCII (IDN mapped) domain.
- Equality/comparison basis: Normalized email value.

## Input Contract
### Accepted
- Standard mailbox syntax accepted by `MimeKit.MailboxAddress.Parse`.
- Optional display-name forms (for example `Jane Doe <jane@example.com>`).

### Rejected
- Null/whitespace input.
- Inputs without exactly one `@` in parsed mailbox address.
- Malformed mailbox text rejected by parser.

## Normalization Rules
- Local part is trimmed and lowercased.
- Domain part is trimmed, IDN-mapped to ASCII, and lowercased.
- `ToString()` returns normalized address text.

## Public API Behavior
### Construction
- `Parse` and `From` enforce normalization and validation.

### Parse/TryParse
- `TryParse` returns false/default (or null for nullable overload) on invalid input.
- `Parse` throws on invalid input.

### Formatting/ToString
- `ToString()` returns normalized canonical address string.

### Converters/Serialization
- JSON converter round-trips canonical string.
- Type converter round-trips canonical string and throws `FormatException` for invalid text.

## Error Contracts
- Parse failures propagate parser exceptions or argument exceptions.
- JSON converter invalid input throws `JsonException`.

## Compatibility Notes
- Behavior changes from previous runtime semantics require an entry in `docs/spec/compat-decisions.md`.

## Test Requirements
- Required scenario IDs for traceability:
  - EMAILADDRESS-CONSTRUCT-001
  - EMAILADDRESS-PARSE-001
  - EMAILADDRESS-TRYPARSE-001
  - EMAILADDRESS-FORMAT-001
  - EMAILADDRESS-CONVERT-001
