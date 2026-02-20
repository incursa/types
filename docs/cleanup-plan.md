---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/cleanup-plan.md"
  path: /docs/cleanup-plan.md
---

# Incursa.Types hardening plan

## Documentation and discoverability
- The public README is effectively empty, leaving consumers without installation, usage, or contribution guidance. Expand it with package purpose, quick-start samples, build/test commands, and type catalog links.【F:README.md†L1-L1】
- Add API-level docs per type (XML docs and rendered reference) so consumers know when to use Money, Percentage, FastId, Duration, Period, etc. Tie this to automated doc generation in CI.

## Testing and quality coverage
- A placeholder `UnitTest1` exists with no assertions, signaling incomplete coverage scaffolding.【F:Incursa.Types.Tests/UnitTest1.cs†L17-L22】
- Existing tests cover only a subset of types (FastId deterministic conversions and boundary checks; Money arithmetic/formatting; Percentage truncation/formatting). Broaden coverage to other value objects (Duration, Period, RecurringPeriod, BvFile, VirtualPath, ShortCode, Maybe, backed-type interfaces, USA state parsing) and add negative/edge cases for existing suites.【F:Incursa.Types.Tests/FastIdGenerationTests.cs†L23-L200】【F:Incursa.Types.Tests/MoneyTests.cs†L18-L110】【F:Incursa.Types.Tests/PercentageTests.cs†L18-L77】【F:Incursa.Types/Duration.cs†L9-L70】
- Introduce deterministic fuzz/property tests for parsing/formatting-heavy types (Duration regex parsing, Period arithmetic, FastId round-trips) and serialization compatibility tests for all custom converters.

## API design and organization
- FastId currently encodes a 34-bit timestamp plus 30 bits of randomness with a custom 2025 epoch, similar to Snowflake-like identifiers; document guarantees (ordering, collision risk, time overflow) and provide migration guidance from GUIDs/strings.【F:Incursa.Types/FastId.cs†L25-L185】
- The root namespace contains many narrowly-differentiated interface variants (`IMultiBackedType`3–17, backed-type abstractions). Consider grouping them into sub-namespaces or consolidating generics to simplify discovery and package surface area.【F:Incursa.Types/IMultiBackedType`3.cs†L15-L20】
- Review target framework choice (`net8.0`) and decide if multi-targeting (e.g., net8.0/netstandard2.1) is needed to match consumer ecosystems.【F:Incursa.Types/Incursa.Types.csproj†L3-L19】

## Tooling and developer experience
- Add build/test instructions and editorconfig/style guidance; StyleCop configuration exists but lacks contributor-facing notes.【F:README.md†L1-L1】
- Establish CI for tests, analyzers, and package publish validation. Include reproducible build verification and NuGet packaging checks aligned with `GeneratePackageOnBuild` settings.【F:Incursa.Types/Incursa.Types.csproj†L4-L19】

## Coverage gaps and new type ideas
- Evaluate additional common primitives (Email, PhoneNumber, Country, CurrencyCode, TimeZone, Url, IPAddress/CIDR) to complement existing value objects.
- Ensure enum-like types such as `UsaState` have exhaustive casing/parsing tests and consider adding other locales. Explore source-generation to avoid large hand-written switch statements.【F:Incursa.Types/Enums/UsaState.cs†L1-L200】
- Add serialization and EF Core value converter samples for each backed type to validate ORM usability.
