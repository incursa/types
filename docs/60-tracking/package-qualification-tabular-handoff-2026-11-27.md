---
workbench:
  type: doc
  workItems:
    - TASK-0010
  codeRefs:
    - scripts/verify-package-compatibility.ps1
  path: /docs/60-tracking/package-qualification-tabular-handoff-2026-11-27.md
  pathHistory:
    - "C:/docs/60-tracking/package-qualification-tabular-handoff-2026-11-27.md"
---

# Package qualification and Tabular handoff 2026-11-27

## Notes

## Scope and authority

This report qualifies the package outputs after TY-01 through TY-09. The base
commit is `d25602c8bbf2afaa8a0a8441adddefdab36b79ff`. Current SpecAttest candidates
were fetched on 2026-11-27 and remain draft revision 1 / record version 1.
They are used as the behavioral authority but are not promoted by this task.

No package was published. No signing, secret, release, or branch-protection
setting was changed.

## Package graph

| Contract | Qualified result |
| --- | --- |
| Package identities | `Incursa.Types.Primitives` and `Incursa.Types` |
| Target framework | Both packages expose exactly `lib/net8.0`; this preserves the existing supported TFM. |
| Dependency direction | `Incursa.Types` depends on `Incursa.Types.Primitives`; the reverse reference does not exist. |
| Primitives runtime closure | The nupkg has an empty runtime dependency group, and an isolated consumer's resolved package graph contains only Primitives. |
| Rich package closure | A package-only consumer resolves Types, the synchronized Primitives version, and the documented rich dependencies. |
| Version constraint | The Types nuspec declares the same Primitives package version produced by the build. |
| Build-only tooling | SourceLink, analyzers, reproducible-build tooling, and the generator remain private/build-only and are absent from consumer runtime groups. |
| Defining assemblies | Moved types are defined once by Primitives and forwarded by Types; rich contact, recurrence, file, network, locale, and timezone functionality remains defined by Types. |
| Generated artifacts | Release packages contain the expected net8.0 assemblies, README, icon, repository metadata, and the rich symbol package; deterministic normalized package comparison remains a CI gate. |

## Automated consumer qualification

`scripts/verify-package-compatibility.ps1` performs three isolated checks from
built nupkgs rather than project references:

1. A Primitives-only consumer restores, builds, and runs BigDecimal and exact
   temporal canonical/JSON paths, then asserts that no additional runtime
   package was resolved.
2. A Types-only consumer restores transitively with Primitives, builds, and
   runs both rich (`EmailAddress`) and moved (`Money`, `Duration`) APIs.
3. An old consumer is compiled against baseline commit
   `d6ce1c8392aa76149374b08d9cdb97c66d5f2084`, before the package split. The
   unchanged consumer binary is then loaded against the current Types and
   Primitives assemblies. Moved Money, Duration, and Percentage references plus
   the `Incursa.Money, Incursa.Types` assembly-qualified identity resolve
   through type forwarding.

The repository regression suite separately covers converters and JSON payloads
for moved types, while `PrimitivePackageBoundaryTests` checks one defining
assembly, complete forwarder coverage, assembly-qualified identities, and zero
external Primitives runtime references.

## Compatibility disposition

| Dimension | Result and consumer action |
| --- | --- |
| Source | Moved types retain namespace `Incursa`; consumers may keep existing usings. Direct Primitives consumers change only the package reference. New strict temporal types are opt-in. |
| Binary | The rich assembly forwards every exported Primitives type. The pre-split compiled-consumer check passes without recompiling its assembly. Deployments must restore the new transitive Primitives dependency rather than replacing one DLL in isolation. |
| Behavior | Legacy Duration/Period behavior remains; approved Money wording, TimeZoneId platform mapping, and earlier hardening changes are documented in the migration guide. Exact BigDecimal and strict temporal APIs are additions. |
| Wire | Existing moved-type JSON/converter contracts remain under regression. CalendarDuration and ExactPeriod use new, separately named canonical profiles; schemas must opt in and version any field migration. |

## Tabular handoff

Read-only qualification used `incursa/tabular-dotnet` commit
`7cebcb3238ea6128508b6b7c3fb609856f89013a`. Its closed scalar contract admits
native `decimal`, strings, signed integers, and offset timestamps, but excludes
`BigInteger`, arbitrary objects, timezone-free `DateTime`, `TimeSpan`, duration,
and period objects. The qualified application mappings are therefore:

| Shared primitive | Current Tabular mapping |
| --- | --- |
| `BigDecimal` | Canonical string for the full declared profile, or exact native `decimal` only after `ToDecimalExact` succeeds. No silent rounding/narrowing. |
| `ElapsedDuration` | Signed `Int64` exact units or canonical string. |
| `LocalDateTime` | Signed `Int64` Gregorian local ticks or canonical string; no inferred zone. |
| `CalendarDuration` | Canonical string; calendar policy remains application-owned. |
| `ExactPeriod` | Canonical two-endpoint string; `DateTimeOffset` columns may be used only when endpoints are modeled separately. |
| `Money` / `Percentage` | Exact native `decimal` values at their documented normalized scale. |

This is package and explicit-mapping qualification, not native admission of new
Tabular scalar kinds. Native BigDecimal/temporal adapters and consumption of a
published package belong to downstream TB-04 and require their own approval.
`REQ-TYPES-NUM-0033` is therefore partially evidenced, not claimed complete.

## Requirement disposition

| Requirement(s) | Evidence / disposition |
| --- | --- |
| PKG-0001..0005 | Package IDs, one-way project/package graph, zero Primitives runtime dependencies, private build tooling, and shared repository/solution are mechanically verified. |
| PKG-0006..0009 | Numeric, temporal, identifier, Maybe, and backed-interface ownership is checked by assembly/package boundary tests and package consumers. |
| PKG-0010..0013 | One defining assembly, preserved namespaces, complete type forwarding, old compiled consumer, serialization/converter regressions, and assembly-qualified identities are covered. |
| PKG-0014..0015 | Both consumers restore nupkgs without source references; the Primitives resolved runtime graph is asserted empty beyond the package itself. |
| PKG-0016..0019 | Rich functionality stays in Types, foundational guards are dependency-free, net8.0 is preserved, and Types declares the synchronized Primitives version. |
| PKG-0020 | Source/binary/behavior/wire changes are separated above and in `docs/migration-v2.md`. |
| NUM-0031 | Independent Python/Fraction vectors and the mandatory SqlDecimal domain remain in the Release regression suite. |
| NUM-0032 | Parse/format/compare/hash/arithmetic baselines exist. No optimized storage path was selected because native Tabular typed-column admission and its end-to-end benchmark remain downstream work. |
| NUM-0033 | Canonical/equality/exact arithmetic semantics are qualified for explicit application mappings; native shared Tabular BigDecimal implementation is deferred to TB-04. |
| TIME-0044..0046 | Specs identify ISO-inspired subsets/deviations, version legacy changes, and cover the full temporal edge matrix on Windows and Linux. |
| TIME-0047 | Exact units/ticks and canonical application mappings align with the inspected closed scalar contract; native admission is not claimed. |
| COMPAT-0019 | Full repository regressions, Primitives-only consumer, Types-only consumer, and old compiled-consumer forwarding run in the quality gate. |

## Release boundary

This evidence makes the package outputs release-candidate ready for review. It
does not authorize publication. A release owner must separately approve the
version, changelog/release notes, signing/provenance, registry destination, and
downstream rollout.

## Local verification result

- Release build: success, zero current-source warnings/errors.
- Release tests: 482 passed, 0 failed, 0 skipped.
- Release pack: both nupkgs plus the rich symbol package produced.
- Package gates: Primitives consumer, Types consumer, runtime graph checks,
  synchronized version dependency, and old compiled-consumer forwarding pass.
- Workbench strict validation, formatting, type-spec structure, and diff checks
  pass.
