# FastId reference

`FastId` is a 64-bit, time-sortable identifier intended as a lightweight alternative to GUIDs. It uses a custom epoch and Crockford Base32 encoding to keep the encoded string short, URL-safe, and lexicographically ordered by time.

## Bit layout
* **34-bit timestamp** — seconds since custom epoch `2025-01-01T00:00:00Z`.
* **30-bit randomness** — uniform random payload used to reduce collision probability.

A monotonic timestamp produces time-ordered IDs while still allowing ~1 billion unique values per second within the same timestamp bucket.

## Guarantees and limits
* **Ordering**: IDs generated in the same process with monotonic time will sort chronologically by their encoded value.
* **Collision risk**: 30 bits of randomness yields a 1 in ~1 billion collision probability per timestamp slice; use deterministic factories when idempotency is required.
* **Overflow horizon**: 34 timestamp bits cover ~544 years from the custom epoch, expiring around year 2569.

## Migration guidance
* **From GUIDs**: use `FastId.FromGuidWithinTimestampRange(guid, cutoff)` to hash existing identifiers into the FastId space while constraining the timestamp to your desired maximum.
* **From strings**: use `FastId.FromStringWithinTimestampRange(value, cutoff)` for deterministic conversions from arbitrary string sources.
* **Parsing/serialization**: `FastId.TryParse`, `FastIdJsonConverter`, and `FastIdTypeConverter` enable seamless model binding and JSON (de)serialization.

## Usage tips
* For APIs requiring stable pagination or ordering, prefer FastId over GUID to keep sort order aligned with creation time.
* When persistence compatibility matters, store both the 64-bit `Value` and the encoded string to simplify debugging.
* Consider multi-targeting consumers if they do not yet build on .NET 10.0.
