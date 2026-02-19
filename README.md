# Incursa.Types

A set of reusable value objects and helper abstractions that can be shared across Incursa services and libraries. The package focuses on predictable formatting/parsing, deterministic identifiers, and strong typing for common domain concepts.

## Release status

The current hardening release introduces **breaking behavioral changes** to improve safety. Review migration notes before upgrading:

- [`docs/migration-v2.md`](docs/migration-v2.md)
- [`CHANGELOG.md`](CHANGELOG.md)
- [`docs/spec/README.md`](docs/spec/README.md)

## Installation

```bash
# using dotnet CLI
dotnet add package Incursa.Types
```

The library currently targets **.NET 8.0**. If your solution uses an earlier target, enable multi-targeting in your project file or add a `TargetFramework` compatible with .NET 8 when consuming the package locally.

## Quick start

```csharp
using Incursa;

// Money and Percentage
var subtotal = new Money(12.34m);
var taxRate = Percentage.ParseScaled("8.25");
var total = subtotal + (subtotal * taxRate);
Console.WriteLine(total.ToAccounting()); // $13.35

// FastId deterministic identifier
record Order(FastId Id);
var order = new Order(FastId.FromGuidWithinTimestampRange(Guid.NewGuid(), DateTimeOffset.UtcNow));
Console.WriteLine(order.Id); // Crockford base32, time-ordered

// Time windows
var duration = Duration.Parse("P1DT2H30M");
var period = new Period(DateTimeOffset.UtcNow, duration);
Console.WriteLine(period.Contains(DateTimeOffset.UtcNow.AddHours(1))); // True (end is exclusive)

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

# enforce coverage thresholds locally (line+branch >= 85%)
dotnet test Incursa.Core.slnx -c Release /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=85 /p:ThresholdType=line,branch /p:ThresholdStat=total

# build the library only
dotnet build Incursa.Types/Incursa.Types.csproj

# verify formatting and StyleCop rules before committing
dotnet format --verify-no-changes

# run mutation tests (same gate strategy used in CI)
dotnet tool install --tool-path .tools dotnet-stryker --version 4.9.0
./.tools/dotnet-stryker -p Incursa.Types/Incursa.Types.csproj -tp Incursa.Types.Tests/Incursa.Types.Tests.csproj -l Basic -b 70 --threshold-low 70 --threshold-high 85
```

## Tooling and style

* The repository ships with **StyleCop** rules (`stylecop.json`) and Roslyn analyzers; keep warnings at zero.
* CI quality gate enforces:
  * formatting/analyzers
  * deterministic build/pack reproducibility checks
  * unit tests
  * coverage threshold (`line` + `branch` >= 85%)
  * mutation threshold (Stryker `break-at` 70%)
* Enable XML documentation on public APIs. When adding new types, include `<summary>` and parameter remarks so reference docs stay complete.
* Use `dotnet pack Incursa.Types/Incursa.Types.csproj -p:ContinuousIntegrationBuild=true -o ./artifacts` to validate the NuGet package layout locally. Packages are generated on build because `GeneratePackageOnBuild` is enabled.

## Type catalog

* **FastId** – Snowflake-like, time-sortable identifier with deterministic GUID/string conversion helpers.
* **Money** – Banker-rounded two-decimal monetary amounts with accounting/number formatting helpers.
* **Percentage** – Percentage value object that truncates to two decimal places and supports arithmetic.
* **Duration** – ISO 8601 duration parser/formatter; supports fractional units and date arithmetic.
* **Period** – ISO 8601 time interval composed of a start and `Duration`; uses half-open semantics `[start, end)`.
* **RecurringPeriod** – Cron expression wrapper that materializes deterministic `Period` windows from the provided reference time.
* **ShortCode** – validated uppercase Crockford-style code value object with strict parse behavior.
* **VirtualPath** – Directory separator-aware virtual path with JSON converter and System.IO helpers.
* **BvFile** – In-memory file payload with MIME inference and base64/path factories.
* **Maybe<T>** – Lightweight option monad with LINQ-like combinators.
* **Backed type interfaces** – `IStringBackedType`, `INumberBackedType`, and multi-backed variants for strongly typed IDs and values.
* **Contact & locale** – `EmailAddress`, `PhoneNumber`, `CountryCode`, `CurrencyCode`, `Locale`, and `TimeZoneId` wrap platform data with parsing/normalization.
* **EncryptedString** – validated Base64 ciphertext wrapper (rejects plain text input).
* **Networking** – `Url`, `IpAddress`, and `CidrRange` provide safe parsing and containment helpers.

See [`docs/types.md`](docs/types.md) and [`docs/fastid.md`](docs/fastid.md) for deeper notes and guidance. JSON/EF Core conversion snippets live in [`docs/serialization-and-ef-samples.md`](docs/serialization-and-ef-samples.md). Behavioral specifications are versioned in [`docs/spec/types`](docs/spec/types). Remaining candidate primitives are cataloged in [`docs/future-types.md`](docs/future-types.md).

## Contributing

1. Create a feature branch from `main`.
2. Add/adjust XML documentation on new or modified types.
3. Ensure all unit tests pass (`dotnet test`) and include new tests for parsing/formatting changes.
4. Open a pull request with a summary of the change and any migration considerations.
