---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "C:/docs/future-types.md"
  path: /docs/future-types.md
---

# Value object design checklist

Use this checklist when evaluating an additional `Incursa.Types` value object. Candidate tracking belongs in Workbench work items or issues, not in this durable design guide.

## Contract questions

- What invalid states does the type prevent that a primitive cannot prevent reliably?
- Are parsing, formatting, comparison, equality, and normalization rules deterministic and culture-safe?
- Does the type preserve meaningful distinctions such as null versus empty, case sensitivity, precision, time zone, or leading zeros?
- Are boundary values and failure modes explicit?
- Can the type evolve without silently changing serialized or database representations?

## Integration expectations

- Provide consistent `Parse`, `TryParse`, formatting, JSON, `TypeConverter`, and EF Core guidance when those integrations apply.
- Document canonical and display forms separately.
- Avoid network lookups or mutable external catalogs in core parsing behavior.
- Keep sensitive values redacted by default in logs and diagnostic output.
- Add migration guidance before changing an existing representation or validation rule.

## Evidence expectations

- Cover valid, invalid, edge, round-trip, and culture-sensitive cases.
- Add serialization and persistence tests for public converters.
- Use property, mutation, or fuzz testing for parser-heavy or boundary-heavy types when practical.
- Record performance requirements before introducing caching, pooling, or custom allocation strategies.
