# Incursa.Types

Incursa.Types is a .NET library of reusable value objects and strongly typed primitives for domain-driven systems.

It helps teams replace loosely typed strings and primitives with explicit domain types that provide predictable parsing, formatting, validation, and serialization behavior.

## Upgrade note

Recent hardening changes include breaking behavior updates for safety (strict parsing, converter failure behavior, and stronger invariants).
Review migration guidance before upgrading:

- https://github.com/incursa/types/blob/main/docs/migration-v2.md
- https://github.com/incursa/types/blob/main/CHANGELOG.md

## Install

```bash
dotnet add package Incursa.Types
```

## Highlights

- Time-sortable identifiers (`FastId`)
- Money and percentage value objects
- Duration, period, and recurring period primitives
- Strictly validated short codes and encrypted payload wrappers
- Path, URL, IP, and CIDR types
- Locale, country, currency, and timezone wrappers
- JSON-friendly types designed for reliable API and persistence boundaries

## Target Framework

- `net8.0`

## Documentation

- Repository: https://github.com/incursa/types
- Type catalog and usage guidance: https://github.com/incursa/types/blob/main/README.md
- Behavioral specifications: https://github.com/incursa/types/tree/main/docs/spec/types
- Serialization and EF converter examples: https://github.com/incursa/types/blob/main/docs/serialization-and-ef-samples.md

## License

Apache License 2.0
