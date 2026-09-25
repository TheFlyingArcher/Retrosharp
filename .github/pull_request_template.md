## Summary

<!-- What does this PR change, and why? One or two sentences is fine. -->

## Related issues

<!-- e.g. Closes #12. Delete this section if there isn't one. -->

## Type of change

- [ ] Bug fix
- [ ] New feature
- [ ] Refactor / cleanup (no behavior change)
- [ ] Data / schema change
- [ ] Build, CI/CD, or infrastructure
- [ ] Documentation

## Areas affected

- [ ] Engine (`src/engine/Retrosharp.Engine.Console`)
- [ ] Core / format parsing (`src/lib/Retrosharp`)
- [ ] Data / migrations (`src/lib/Retrosharp.Data`, `Retrosharp.Data.Migration`)
- [ ] Services (`src/lib/Retrosharp.Service`, `Retrosharp.Service.Interface`)
- [ ] API (`src/ui/Retrosharp.UI.Api`)
- [ ] Web UI (`src/ui/Retrosharp.UI.Web`)
- [ ] Docker / deployment (`docker-compose*.yml`, `.github/workflows`)

## How was this tested?

<!-- Describe what you ran and what you checked. Include screenshots for UI changes. -->

## Data / import impact

<!--
Delete this section if the PR doesn't touch parsing, import, or the schema.
Otherwise note:
- New migrations, and whether they are reversible
- Whether a re-import is needed
- Row-count or reconciliation-warning changes compared with the 2025 import baseline
-->

## Checklist

- [ ] `dotnet build` succeeds
- [ ] `dotnet test` passes
- [ ] `dotnet format --verify-no-changes` is clean
- [ ] Web UI builds (`ng build`), if touched
- [ ] No secrets or per-developer config committed (`appsettings.Development.json`, `.env`, `*.user`)
- [ ] Docs / README updated, if behavior or setup changed
