---
workbench:
  type: spec
  workItems:
    - TASK-0004
  codeRefs: []
  pathHistory:
    - "C:/docs/spec/types/TimeZoneId.md"
  path: /docs/spec/types/TimeZoneId.md
---

# TimeZoneId Behavioral Specification

- Type: `TimeZoneId`
- Namespace: `Incursa`
- Status: `Approved`
- Last Updated: `2026-11-27`

## Domain Purpose
Represents a valid time zone identifier, supporting Windows/IANA forms.

## Canonical Value Model
- Backing representation: resolved `TimeZoneInfo` and canonical identifier.
- Canonical string representation: canonical ID returned by `ToString()`.
- Equality/comparison basis: Canonical time zone identity.

## Input Contract
### Accepted
- Valid IANA or Windows time-zone IDs resolvable by the host .NET
  `TimeZoneInfo` implementation and its installed ICU/CLDR or Windows registry
  data.
- IANA link aliases accepted by the host, such as `US/Pacific`.

### Rejected
- Null/whitespace values.
- Unknown or invalid zone identifiers.
- Cross-platform identifiers when the host is missing the ICU conversion data
  required to resolve or canonicalize them.

## Normalization Rules
- Windows IDs normalize to IANA when available.
- IANA inputs preserve caller-provided identifier text.
- An IANA alias recognized by platform CLDR data can resolve through its
  canonical IANA target when the host omits the alias link file; the supplied
  alias text is still preserved.
- `UTC` follows the Windows-to-IANA mapping and canonicalizes to `Etc/UTC`.
- No host-local fallback is allowed. If a resolved Windows identity cannot be
  mapped to IANA, construction fails explicitly.

## Public API Behavior
### Construction
- Constructor and `Parse` use `TimeZoneInfo.FindSystemTimeZoneById`, resolve and
  validate the supplied ID, and populate `TimeZoneInfo`.
- Windows-to-IANA canonicalization uses
  `TimeZoneInfo.TryConvertWindowsIdToIanaId`.

### Parse/TryParse
- `TryParse` returns false/default for invalid inputs.
- `Parse` throws `ArgumentException` for invalid IDs.

### Formatting/ToString
- `ToString()` returns canonical representation.

### Converters/Serialization
- JSON and type converters round-trip canonical ID values.
- Invalid converter input throws via base converter/JSON exceptions.

## Error Contracts
- Invalid zone ID resolution throws `ArgumentException` wrapping timezone resolution exceptions.
- Missing ICU/CLDR conversion capability, inaccessible system data, invariant
  globalization, or Windows NLS-only behavior that prevents the required
  cross-platform mapping throws `PlatformNotSupportedException` with an
  actionable message.
- Corrupt system time-zone data remains `InvalidTimeZoneException` and is not
  misreported as an invalid caller identifier.
- `TryParse` converts all of these construction failures to `false`/`default`;
  it never substitutes `TimeZoneInfo.Local`.

## Compatibility Notes
- Common Windows IDs, IANA IDs, and aliases retain their established canonical
  output. Mapping data now follows the supported host .NET/ICU/CLDR version
  instead of the TimeZoneConverter package snapshot, so newly changed CLDR
  mappings can follow platform servicing.
- Hosts without required conversion data now fail explicitly rather than
  returning a non-IANA fallback identifier. See `DEC-005`.

## Test Requirements
- Required scenario IDs for traceability:
  - TIMEZONEID-CONSTRUCT-001
  - TIMEZONEID-PARSE-001
  - TIMEZONEID-TRYPARSE-001
  - TIMEZONEID-FORMAT-001
  - TIMEZONEID-CONVERT-001
  - TIMEZONEID-PLATFORM-001
  - TIMEZONEID-PLATFORM-002
  - TIMEZONEID-PLATFORM-003
