# Future value objects

These candidates complement the existing Bravellian.Types primitives. Each item includes suggested behaviors to validate before adding to the package surface.

## Contact and identity
- **ShortName/DisplayName**: Unicode-safe display names with length and script validation for user-facing labels.

## Locale and geography
- **Country**/**CountryCode**: Implemented. Future work: localized display names and optional data downloads for non-ISO territories.
- **TimeZoneId**: Implemented. Future work: expose offset helpers and caching for hosted services.
- **CurrencyCode**: Implemented. Future work: surface historic/withdrawn codes and tie minor-unit metadata to Money rounding utilities.
- **Locale**: Implemented. Future work: fallback rules for private subtags and robust pluralization helpers.

## Networking
- **Url**: Implemented. Future work: safe redirect validation helpers and allow/disallow list enforcement.
- **IpAddress / CidrRange**: Implemented. Future work: IPv6 abbreviation controls and multicast/private range helpers.

## Platform integration
- **ConnectionString**: Structured representation that can redact secrets in logging and merge environment overrides.
- **FeatureFlagKey**: Strongly-typed keys for configuration providers with slug/segment validation.

Implementation guidance: model these with consistent parsing/formatting semantics, JSON and TypeConverter support, and EF Core `ValueConverter` samples similar to existing types.
