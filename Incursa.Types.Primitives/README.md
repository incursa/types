# Incursa.Types.Primitives

Dependency-free foundational value types and contracts for Incursa .NET
libraries. Public types retain the `Incursa` namespace so existing source code
can migrate without namespace changes.

The package includes `BigDecimal` with bounded exact parsing, canonical value
identity, checked exact arithmetic, and explicit rounding, `Money` with deterministic dependency-free English word
formatting, exact `ElapsedDuration` and zone-free `LocalDateTime`, `Percentage`,
identifiers, optional values, existing calendar duration and period values,
and the foundational backed-type contracts.

The package targets .NET 8 and has no external runtime NuGet dependencies.
Specialized contact parsing, recurring scheduling, file handling, and other
rich functionality remain in `Incursa.Types`.
