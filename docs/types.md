# Type reference

This document provides quick guidance for the most commonly used value objects. Each type includes XML documentation in-source and participates in JSON serialization through built-in converters.

## Money
* Truncates to two decimal places on construction.
* Supports arithmetic against `decimal`, `int`, and `Percentage`.
* `ToAccounting` renders parentheses for negatives; `ToNumeric` omits group separators.

## Percentage
* Truncates to two decimal places.
* Commonly used with `Money` calculations: `subtotal * taxRate`.
* Formats to percent strings through `ToString`, honoring the invariant culture.

## FastId
* 64-bit identifier: 34 bits of timestamp (seconds since custom epoch **2025-01-01**) + 30 bits of randomness.
* Encoded with Crockford Base32 for URL-safe, case-insensitive strings that sort chronologically.
* Deterministic helpers: `FromGuidWithinTimestampRange` and `FromStringWithinTimestampRange` hash inputs to stable IDs.
* Provides JSON and type converters; implements `IParsable` for seamless model binding.

## Duration
* Parses ISO 8601 duration strings (e.g., `P3Y6M4DT12H30M5S`) including fractional units.
* `Calculate(start)` adds the duration to a `DateTimeOffset`, distributing fractional components down the unit chain.
* `TryParse` returns `null` on invalid input without throwing.

## Period
* Represents an ISO 8601 time interval of `{StartDateTime}/{Duration}`.
* Validates that the end is not before the start and exposes inclusive/exclusive end points.
* Helpers: `Contains`, `Overlaps`, `ClampDate` to map intervals to `DateOnly`.

## RecurringPeriod
* Wraps a `CronExpression` (via Cronos) and materializes time windows through `GetPeriod(startUtc)`.
* JSON converter serializes the cron expression as a string.
* `TryParse` gracefully handles invalid cron expressions.

## VirtualPath
* Separator-aware string wrapper with JSON converter.
* Provides equality and comparison semantics using a canonical separator (`¦`) to avoid culture issues.
* Extensions for `ChangeExtension`, `GetFileName`, `Combine`, and conversion to system paths.

## ShortCode
* Short, uppercase alphanumeric code with parsing and equality semantics.
* Useful for human-friendly invite codes, confirmation tokens, and feature flags.

## BvFile
* In-memory file payload storing `BinaryData`, file name, and MIME type.
* Factory helpers for base64 strings and filesystem paths (MIME inferred by `MimeKit`).

## Maybe<T>
* Option type with `Match`, `Select`, and `SelectMany` helpers for LINQ-style composition.
* `None` singleton indicates absence; `Value` throws if accessed when empty.

## Contact and identity
* `EmailAddress`: RFC 5322 parsing via MimeKit with IDN/punycode normalization and lower-cased domains.
* `PhoneNumber`: libphonenumber-backed parsing that emits E.164 strings and rejects ambiguous short codes; exposes region codes.

## Locale and geography
* `CountryCode`: ISO 3166-1 alpha-2/alpha-3 handling via `RegionInfo`, case-insensitive parsing, and accessible English names.
* `CurrencyCode`: ISO 4217 catalog with numeric code and minor-unit metadata for Money rounding.
* `TimeZoneId`: IANA/Windows identifier bridge powered by `TimeZoneConverter` for cross-platform scheduling.
* `Locale`: BCP-47 language tag normalization with `CultureInfo` fallback handling for private subtags.

## Networking
* `Url`: Absolute/relative URL parsing with IDN host normalization, default port stripping, and lowercased schemes/hosts.
* `IpAddress`: IPv4/IPv6 wrapper with typed checks and JSON/type converter support.
* `CidrRange`: CIDR prefix validation plus `Contains` helpers for network rules.

## Backed type abstractions
* `IStringBackedType`, `INumberBackedType`, and multi-backed interfaces enable strongly typed IDs with explicit backing primitives.
* Combine with JSON/type converters for consistent serialization and model binding.
