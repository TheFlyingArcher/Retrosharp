# Retrosharp Seed Data

## Overview

Retrosharp requires certain static data to be present. This data is virtually unchanging, so a saga-based ETL parser is unnecessary. Instead, seed data is populated through the code-first tooling Retrosharp already uses — see [Automation](#automation).

## Automation

Seed data is populated using two mechanisms, chosen per table based on how large and how frequently-changing the underlying data is:

- **`League`** is seeded via Entity Framework Core's `HasData` model configuration, baking its seven rows directly into an EF Core migration. Since `Retrosharp.Data.Migration` already applies pending migrations automatically on startup, this data is populated as a natural side effect of running that project — no additional step is needed. This fits `League` well because the list is small and effectively permanent.
- **`Franchise`** and **`Ballpark`** are seeded by a dedicated, idempotent upsert routine that reads `franchises.csv` and `ballparks.csv` directly (using CsvHelper, already a project dependency) and inserts or updates rows matched by their natural key — `SiteCode` for `Ballpark`, and the combination of a franchise's stable identifier and its effective date range for `Franchise`. This fits these tables better than `HasData`, since the CSVs remain the actual source of truth: updating a CSV (a franchise relocation, a new ballpark) doesn't require hand-generating a new migration, only re-running the seeding routine.

Both mechanisms run as part of the `Retrosharp.Data.Migration` console project's startup sequence, alongside its existing `Database.MigrateAsync()` call, so that "run this project" performs schema migration and reference data seeding as a single step. Because the CSV-driven routine is idempotent, this same command can be safely invoked from any of the following contexts without coordination between them:

- Manually, by a developer or administrator.
- As a step in a GitHub Actions workflow.
- As part of a Docker container's initialization/entrypoint step.

No saga, message bus, or admin-triggered ETL workflow is required for seed data, since it is not derived from a Retrosheet datafile requiring parsing — it is either hardcoded (`League`) or a direct, structurally-simple CSV import (`Franchise`, `Ballpark`).

## Leagues

This data represent professional baseball leagues that have existed in the United States. The data is sourced from Retrosheet's [team IDs](https://https://www.retrosheet.org/TeamIDs.htm) file.

NOTE: This does not include amateur leagues, semi-professional leagues, or leagues that were not considered professional baseball leagues. This also does not include leagues for minor leagues such as Pacific Coast League.

```text
LeagueCode,LeagueName
AA,American Association
AL,American League
FL,Federal League
NA,National Association
NL,National League
PL,Players League
UA,Union Association
```

## Ballparks

This data represents ballparks used to play professional baseball within the United States. The data is sourced from Retrosheet's [ballpark IDs](https://www.retrosheet.org/ballparks.zip) file. If the end date is empty, that means the ballpark is still in use by a franchise. If the end date is populated, that means the ballpark is no longer in use by a franchise.

NOTE: It comes in a ZIP file and contains a CSV file with the ballpark data.

Please reference the [ballparks.csv](../docs/csv/ballparks.csv) present within this repository.

## Franchises

This data represents current and historical franchises that have existed within the United States. The data is sourced from Retrosheet's [Team Nicknames History](https://www.retrosheet.org/Nickname.htm) page containing the format of the text file. The actual file is called [CurrentNames.csv](https://www.retrosheet.org/CurrentNames.csv) and is a CSV file.

A copy of the franchise data can be found in the [franchises.csv](../docs/csv/franchises.csv) file present within this repository. It is the same data as the Retrosheet's CurrentNames.csv file but with a more representative filename.

## Requirements

1. `League` is seeded using EF Core `HasData`, populated automatically whenever `Retrosharp.Data.Migration` applies pending migrations.
1. `Franchise` and `Ballpark` are seeded using an idempotent upsert routine reading `franchises.csv` and `ballparks.csv`, matched by natural key, run as part of `Retrosharp.Data.Migration`'s startup sequence.
1. The seeding routine must be safely re-runnable without producing duplicate rows, so that it can be invoked manually, from a GitHub Actions workflow, or as part of a Docker container's initialization step without any coordination between those contexts.
1. No `*.sql` scripts are used for seed data; the code-first EF Core model and the CSV-driven upsert routine are the sole source of truth.

## Acceptance Criteria

1. Running `Retrosharp.Data.Migration` against a fresh database fully populates `League`, `Franchise`, and `Ballpark`.
1. Running `Retrosharp.Data.Migration` repeatedly against an already-seeded database does not create duplicate rows or unnecessarily alter existing rows.
1. The same `Retrosharp.Data.Migration` invocation works unmodified as a manual command, a GitHub Actions step, and a Docker container initialization step.
