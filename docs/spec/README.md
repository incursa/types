---
workbench:
  type: doc
  workItems: []
  codeRefs: []
  pathHistory:
    - "/C:/docs/spec/README.md"
  path: /docs/spec/README.md
---

# Behavioral Specification Guide

This directory is the source of truth for expected behavior of public concrete types in `Incursa.Types`.

## Purpose

Specifications define:
- valid and invalid inputs
- normalization and canonical representation
- parse and formatting contracts
- error behavior
- converter and serialization behavior
- compatibility decisions when behavior changes are proposed

Tests must validate these specs. Tests are not the source of truth.

## Required files

- `docs/spec/types/<TypeName>.md`: one spec per public concrete type
- `docs/spec/compat-decisions.md`: case-by-case compatibility decisions
- `docs/spec/test-traceability.md`: mapping from spec clauses to test methods

## Status model

Each type spec should declare status:
- `Draft`: initial behavior capture
- `Approved`: reviewed and accepted as authoritative

## Change workflow

1. Update or add spec first.
2. Record compatibility decision if behavior changes from existing runtime behavior.
3. Update tests and traceability mapping.
4. Merge only when spec + tests + traceability are all present.
