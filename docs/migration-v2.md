---
workbench:
  type: doc
  workItems: []
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
| `MonthOnly` namespace | `Incursa.Types` | `Incursa` | Update imports/usings |

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
