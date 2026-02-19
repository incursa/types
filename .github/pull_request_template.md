## Summary

Describe what changed and why.

## Spec-First Checklist

- [ ] I added or updated specs in `docs/spec/types/*.md` for each affected public concrete type.
- [ ] If behavior changed from prior semantics, I added a decision entry in `docs/spec/compat-decisions.md`.
- [ ] I updated `docs/spec/test-traceability.md` to map spec clauses to test methods.
- [ ] New/updated tests verify spec behavior (not implicit implementation details).

## Validation

- [ ] `dotnet format --verify-no-changes Incursa.Core.slnx`
- [ ] `dotnet build Incursa.Core.slnx -c Release`
- [ ] `dotnet test Incursa.Core.slnx -c Release`

## Migration/Compatibility Notes

Document any runtime behavior changes for consumers.
