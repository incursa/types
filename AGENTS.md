# Repository Agent Guide

This repository uses Workbench as the source of truth for work items and docs navigation.

## Required Workbench Tooling

- Use the local pinned tool only.
- Restore tools before Workbench commands:
  - `dotnet tool restore`
- Run Workbench through the manifest:
  - `dotnet tool run workbench <command>`
- Do not rely on a globally installed `workbench` binary.

## Work Item and Docs Workflow

- Active work items live in `docs/70-work/items`.
- Closed work items live in `docs/70-work/done`.
- Work item templates live in `docs/70-work/templates`.
- Docs index and workboard sections are generated and must be synced with Workbench.

For regular local upkeep:

```bash
dotnet tool run workbench item normalize --include-done
dotnet tool run workbench nav sync --include-done --force
dotnet tool run workbench validate --strict
```

For full coherence sync:

```bash
dotnet tool run workbench sync --items --docs --nav --include-done --force
dotnet tool run workbench validate --strict
```

## Migration Safety Rule

Always dry-run migration before applying it:

```bash
dotnet tool run workbench migrate coherent-v1 --dry-run
dotnet tool run workbench migrate coherent-v1
```

## CI Expectations

- Verification workflows must stay read-only (`validate`, dry-run normalization/nav checks).
- Mutating Workbench operations run in maintenance automation and must open a PR instead of pushing directly to `main`.
