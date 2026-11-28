---
id: TASK-0010
type: task
status: in-progress
priority: critical
owner: platform
created: 2026-11-27
updated: 2026-11-27
tags: []
related:
  specs:
    - /docs/spec/compat-decisions.md
    - /docs/migration-v2.md
  adrs: []
  files:
    - /docs/60-tracking/package-qualification-tabular-handoff-2026-11-27.md
  prs:
    - "https://github.com/incursa/types/pull/13"
  issues: []
  branches:
    - users/samuelmcaravey/ty-10-package-qualification
title: TY-10 package qualification and Tabular handoff
---

# TASK-0010 - TY-10 package qualification and Tabular handoff

## Summary

Qualify the split `Incursa.Types.Primitives` and `Incursa.Types` packages from
merged TY-01..TY-09 work for independent consumption and the downstream
Tabular handoff. This task adds package graph and old compiled-consumer gates;
it does not publish packages or modify release controls.

The current SpecAttest candidates `REQ-TYPES-PKG-0001..0020`,
`REQ-TYPES-NUM-0031..0033`, `REQ-TYPES-TIME-0044..0047`, and
`REQ-TYPES-COMPAT-0019` were fetched before implementation. All remain draft
revision 1 / record version 1; this work records evidence without promoting
their lifecycle state.

## Verification

- `dotnet test Incursa.Core.slnx -c Release`: 482 passed, 0 failed, 0
  skipped using `DOTNET_ROLL_FORWARD=Major` on this workstation; connected CI
  supplies the pinned .NET 8 runtime.
- Deterministic warn-as-error Release build and Release pack complete with zero
  warnings/errors for current source.
- Primitives-only and Types-only nupkg consumers restore/build/run successfully;
  the resolved Primitives graph has zero other runtime packages.
- A consumer compiled at pre-split commit
  `d6ce1c8392aa76149374b08d9cdb97c66d5f2084` runs unchanged against current
  assemblies and resolves moved types/assembly-qualified identity through
  forwarders. Its isolated baseline build reports only the historical MimeKit
  4.14 advisory already removed from current source.
- Formatting, type-spec verification, Workbench strict validation, and diff
  checks pass. Connected deterministic package comparison, mutation, and
  aggregate Quality Report remain to be recorded from the pull request.

## Acceptance criteria

- [x] Independent package consumers and runtime dependency graph checks cover
  both packages, including old compiled-consumer type forwarding.
- [x] Source, binary, behavior, wire, target-framework, version-constraint,
  generated-artifact, and downstream Tabular boundaries are documented.
- [ ] Full Release test/build/pack and connected CI evidence are recorded;
  publication remains a separate approval.
- [x] Current draft requirements map to evidence or explicit downstream
  deferral without claiming unimplemented native Tabular admission.
