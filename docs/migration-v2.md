---
workbench:
  type: spec
  workItems:
    - TASK-0003
    - TASK-0004
    - TASK-0009
    - TASK-0010
  codeRefs: []
  pathHistory:
    - "C:/docs/migration-v2.md"
  path: /docs/migration-v2.md
---

# Migration guide: hardened v2 behavior

This release hardens type invariants and removes several silent-failure paths.
If you are upgrading from earlier versions, review the changes below and update consumers before rollout.

## Breaking changes summary

| Area | Previous behavior | New behavior | Action |
| --- | --- | --- | --- |
| `ShortCode` | Reference type with permissive parsing (`TryParse` effectively always succeeded) | `readonly record struct` with strict alphabet/length validation and real parse failure | Recompile callers, handle parse failures, avoid null assumptions |
| `Period.Contains` | End boundary could be treated as included | Half-open interval semantics `[start, end)` | Recheck boundary logic in scheduling/reporting code |
| `Period.Overlaps` | Touching boundaries could be treated as overlap | Strict interval overlap logic | Update any tests expecting boundary-touch overlap |
| `RecurringPeriod.GetPeriod` | Used ambient `UtcNow` for end | Uses provided `startUtc` for deterministic windows | Remove tests that depended on wall-clock behavior |
| `TypeConverter` invalid input | Could coerce to default in several types | Throws `FormatException` on invalid input | Add exception handling in model-binding/config pipelines |
| `EncryptedString` | No meaningful invariant enforcement | Requires Base64 ciphertext-like payload | Ensure callers pass encrypted/base64 data, not plain text |
| `Money` normalization | Fractional normalization truncated excess precision | Values normalize with banker's rounding (`MidpointRounding.ToEven`) to 2 decimals | Rebaseline midpoint-sensitive financial assertions (for example `1.005`, `1.015`) |
| `Money.ToWords()` | Humanizer with hard-coded plural labels; negative sub-dollar values lost their sign/cents; values beyond `long` failed | Dependency-free deterministic English grammar with singular/plural labels, exact negative cents, and full `decimal` range through octillion | Rebaseline word snapshots for singular, negative sub-dollar, or very large amounts |
| `MonthOnly` namespace | `Incursa.Types` | `Incursa` | Update imports/usings |
| `TimeZoneId` mapping source | TimeZoneConverter bundled Windows/IANA mapping data | .NET 8 `TimeZoneInfo` with host ICU/CLDR or Windows data | Ensure supported hosts provide ICU conversion data; handle `PlatformNotSupportedException` explicitly |

## Behavioral hardening

## Default value safety
`default(T)` for core value structs is now explicitly handled to avoid null-backed runtime failures.

Examples:

```csharp
FastId empty = default;
Console.WriteLine(empty.Encoded); // "0"

JsonContext ctx = default;
Console.WriteLine(ctx.ToString()); // {}
```

## Numeric determinism
`Money` and `Percentage` parse/JSON output now use invariant culture for consistent API and persistence boundaries.

```csharp
CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
JsonSerializer.Serialize(new Money(12.34m)); // "12.34"
```

`Money.ToWords()` remains English-only. It always emits a dollar phrase and
emits a cents phrase only for non-zero normalized cents:

```csharp
new Money(1.01m).ToWords();  // "one dollar and one cent"
new Money(-0.01m).ToWords(); // "minus zero dollars and one cent"
```

`TimeZoneId` no longer brings in TimeZoneConverter. Common Windows IDs continue
to canonicalize to IANA, while IANA aliases preserve the accepted input text.
Invariant globalization or Windows NLS-only environments that cannot perform
the required conversion now fail explicitly instead of substituting a local or
unmapped identifier.

## Strict calendar duration and exact period adoption

The existing `Duration` and `Period` types remain available without TY-09
semantic or wire changes. New code that needs exact arithmetic and explicit DST
behavior can opt into `CalendarDuration` and `ExactPeriod`.

| Compatibility dimension | Legacy types | Strict types / migration action |
| --- | --- | --- |
| Source | Existing `Duration` and `Period` names and members remain. | Change the declared type and construction/parsing calls explicitly. |
| Binary | Existing types remain defined in `Incursa.Types.Primitives` and forwarded through `Incursa.Types`. | New strict types are also forwarded; recompile to consume their APIs. |
| Behavior | Legacy floating calendar arithmetic and `Period` behavior are unchanged by TY-09. | Use integral months/days, exact elapsed units, and explicit `LocalTimeResolution`. |
| Wire | Existing duration and start/duration period strings remain unchanged. | `CalendarDuration` has one-sign normalized text; `ExactPeriod` uses two fixed offset endpoints. Version schemas/messages before changing stored or exchanged fields. |

Do not deserialize a legacy field directly as a strict type. Parse the old type,
apply an application-owned conversion policy, then write the new canonical
field under a versioned schema. Fractional years/months/days require an explicit
business decision because the strict profile does not approximate them.

## Package split adoption

`Incursa.Types` now depends on `Incursa.Types.Primitives`. Existing consumers
may keep referencing the rich package; package restore brings Primitives
transitively. Libraries that need only the dependency-free foundation can
reference Primitives directly:

```bash
dotnet add package Incursa.Types.Primitives
```

| Compatibility dimension | Split-package contract |
| --- | --- |
| Source | Moved types retain the `Incursa` namespace. Existing usings remain valid. |
| Binary | `Incursa.Types` forwards moved public types. An old pre-split consumer binary is exercised against current assemblies in CI. Deploy the complete restored dependency graph; copying only a replacement Types DLL is unsupported. |
| Behavior | Moving a type does not itself change behavior. Approved behavioral changes are listed separately in this guide. |
| Wire | Existing moved-type JSON/type-converter payloads remain under regression. New BigDecimal and strict temporal profiles are additions and require explicit schema adoption. |

Both packages currently target net8.0 and are built at one synchronized package
version. Types declares that produced Primitives version as a runtime dependency.
Package publication, version selection, and registry rollout require separate
release approval.

## Upgrade checklist

1. Update imports/usings for `MonthOnly` to `Incursa`.
2. Audit all `TryParse` and `TypeConverter` call sites for explicit failure handling.
3. Re-run boundary tests around period containment/overlap.
4. Validate encrypted payload flow for `EncryptedString`.
5. Rebaseline any snapshot tests that include serializer output.

## Recommended rollout

1. Deploy as a major-version upgrade.
2. Roll out behind a compatibility test stage using production-like payloads.
3. Monitor deserialization/model-binding failures during early adoption and fix caller inputs.
