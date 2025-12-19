# Bravellian.Types

A set of reusable value objects and helper abstractions that can be shared across Bravellian services and libraries. The package focuses on predictable formatting/parsing, deterministic identifiers, and strong typing for common domain concepts.

## Installation

```bash
# using dotnet CLI
dotnet add package Bravellian.Types
```

The library currently targets **.NET 10.0**. If your solution uses an earlier target, enable multi-targeting in your project file or add a `TargetFramework` compatible with .NET 10 when consuming the package locally.

## Quick start

```csharp
using Bravellian;

// Money and Percentage
var subtotal = new Money(12.34m);
var taxRate = new Percentage(8.25m);
var total = subtotal + (subtotal * taxRate);
Console.WriteLine(total.ToAccounting()); // $13.35

// FastId deterministic identifier
typedrecord Order(FastId Id);
var order = new Order(FastId.FromGuidWithinTimestampRange(Guid.NewGuid(), DateTimeOffset.UtcNow));
Console.WriteLine(order.Id); // Crockford base32, time-ordered

// Time windows
var duration = Duration.Parse("P1DT2H30M");
var period = new Period(DateTimeOffset.UtcNow, duration);
Console.WriteLine(period.Contains(DateTimeOffset.UtcNow.AddHours(1))); // True

// Paths and optional values
var path = new VirtualPath("assets/images/logo.png");
Console.WriteLine(path.GetExtension()); // .png
Maybe<string> maybeEmail = Maybe<string>.None;
Console.WriteLine(maybeEmail.GetValueOrDefault("no-email@example.com"));
```

## Building and testing

```bash
# restore and run analyzers/tests
dotnet test

# build the library only
dotnet build Bravellian.Types/Bravellian.Types.csproj

# verify formatting and StyleCop rules before committing
dotnet format --verify-no-changes
```

## Tooling and style

* The repository ships with **StyleCop** rules (`stylecop.json`) and Roslyn analyzers; keep warnings at zero. The CI pipeline runs `dotnet format --verify-no-changes` to enforce the `.editorconfig` conventions (four-space indentation, `var` for implicit local types where clear, and UTF-8 encoding).
* Enable XML documentation on public APIs. When adding new types, include `<summary>` and parameter remarks so reference docs stay complete.
* Use `dotnet pack Bravellian.Types/Bravellian.Types.csproj -p:ContinuousIntegrationBuild=true -o ./artifacts` to validate the NuGet package layout locally. Packages are generated on build because `GeneratePackageOnBuild` is enabled.

## Type catalog

* **FastId** – Snowflake-like, time-sortable identifier with deterministic GUID/string conversion helpers.
* **Money** – Truncated two-decimal monetary amounts with accounting/number formatting helpers.
* **Percentage** – Percentage value object that truncates to two decimal places and supports arithmetic.
* **Duration** – ISO 8601 duration parser/formatter; supports fractional units and date arithmetic.
* **Period** – ISO 8601 time interval composed of a start and `Duration`; overlap and containment helpers.
* **RecurringPeriod** – Cron expression wrapper that materializes upcoming `Period` windows.
* **ShortCode** – Fixed-length, uppercase alpha-numeric code generator/parser.
* **VirtualPath** – Directory separator-aware virtual path with JSON converter and System.IO helpers.
* **BvFile** – In-memory file payload with MIME inference and base64/path factories.
* **Maybe<T>** – Lightweight option monad with LINQ-like combinators.
* **Backed type interfaces** – `IStringBackedType`, `INumberBackedType`, and multi-backed variants for strongly typed IDs and values.
* **Contact & locale** – `EmailAddress`, `PhoneNumber`, `CountryCode`, `CurrencyCode`, `Locale`, and `TimeZoneId` wrap platform data with parsing/normalization.
* **Networking** – `Url`, `IpAddress`, and `CidrRange` provide safe parsing and containment helpers.

See [`docs/types.md`](docs/types.md) and [`docs/fastid.md`](docs/fastid.md) for deeper notes and guidance. JSON/EF Core conversion snippets live in [`docs/serialization-and-ef-samples.md`](docs/serialization-and-ef-samples.md). Remaining candidate primitives are cataloged in [`docs/future-types.md`](docs/future-types.md).

## Contributing

1. Create a feature branch from `main`.
2. Add/adjust XML documentation on new or modified types.
3. Ensure all unit tests pass (`dotnet test`) and include new tests for parsing/formatting changes.
4. Open a pull request with a summary of the change and any migration considerations.
