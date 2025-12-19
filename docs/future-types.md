# Future value objects

These candidates complement the existing Bravellian.Types primitives. Each item includes suggested behaviors to validate before adding to the package surface.

## Contact and identity
- **EmailAddress**: RFC 5322-compliant parser with normalization (lowercasing domain, Unicode punycode) and MX/hostname validation hooks.
- **PhoneNumber**: E.164 formatting, country-aware parsing via libphonenumber-style rules, and short-code protection to avoid misinterpretation.
- **ShortName/DisplayName**: Unicode-safe display names with length and script validation for user-facing labels.

## Locale and geography
- **Country**/**CountryCode**: ISO 3166-1 alpha-2/alpha-3 codes with casing-insensitive parsing and localized display names.
- **TimeZoneId**: IANA zone identifier with daylight-offset helpers and Windows time zone bridging.
- **CurrencyCode**: ISO 4217 code with numeric code and exponent metadata for Money-style arithmetic.
- **Locale**: BCP47 language tag value object with subtags and fallback helpers.

## Networking
- **Url**: Absolute/relative URL parsing with normalization and safe redirection validation.
- **IpAddress / CidrRange**: IPv4/IPv6 parsing, containment checks, and serialization helpers for firewall rule modeling.

## Platform integration
- **ConnectionString**: Structured representation that can redact secrets in logging and merge environment overrides.
- **FeatureFlagKey**: Strongly-typed keys for configuration providers with slug/segment validation.

Implementation guidance: model these with consistent parsing/formatting semantics, JSON and TypeConverter support, and EF Core `ValueConverter` samples similar to existing types.
