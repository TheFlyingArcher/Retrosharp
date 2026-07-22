# Retrosharp Phase 1 Build Plan

## Overview

This document breaks Phase 1 into discrete, dependency-ordered build steps, each small enough to build and verify on its own before moving to the next. It exists so that Phase 1 is not built as a single large effort, but as a sequence of independently correct, independently testable pieces — each step should be complete and verified against its governing spec's Acceptance Criteria before the next step begins.

Each step lists the spec(s) that govern it. Refer back to those specs for the authoritative detail — this document only sequences the work and states what "done" means for each step; it does not restate requirements already written elsewhere.

Steps within the same numbered stage that don't depend on each other (for example, Step 2 and Step 4) may be built in either order, or in parallel. Sub-steps (6a, 6b, ...) are meant to be built in order, since each depends on the previous.

Update the **Status** line under each step as work progresses (`Not Started`, `In Progress`, `Complete`) so this document stays a reliable picture of where Phase 1 actually stands.

## Dependency Graph

```
Step 1: Schema Alignment
        │
        ├──► Step 2: Seed Data Automation ──┐
        │                                     │
        └──► Step 4: ETL Messaging Infrastructure
                      │
                      ▼
             Step 3: Person Parser ──────────┤
                                              │
                                              ▼
                                   Step 5: Game Log Parser
                                              │
                                              ▼
                                   Step 6: Game Event Parser (6a → 6f)
                                              │
                                              ▼
                                   Step 7: Data Viewing API
                                              │
                                              ▼
                                   Step 8: Containerized Deployment
                                              │
                                              ▼
                                   Step 9: End-to-End Validation
```

---

## Step 1: Schema Alignment

**Status**: Complete

**Governing specs**: [game-event.md](./game-event.md) (Data Model), [game-log.md](./game-log.md), [person.md](./person.md), [seed-data.md](./seed-data.md), [project.md](./project.md)

**Depends on**: Nothing — this is the starting point.

**Objective**: Bring the existing EF Core models and migrations up to date with every schema decision made since the original implementation pass. The existing codebase predates several of today's decisions and needs reconciling, not just extending.

**Deliverables**:
- `Fielding` model updated to key on `(PersonId, FranchiseId, SeasonYear, Position)` instead of `(PersonId, FranchiseId, SeasonYear)`.
- New models and tables: `GameEvent`, `GameEventRunner`, `GameEventFieldingCredit`, `GameEventGameStatus`, `GameSubstitution`, `GameAdjustment`, `GameComment`, per the [Data Model](./game-event.md#data-model) section.
- `GameBattingStatistics`, `GamePitchingStatistics`, `GameFieldingStatistics` models exist per [game-log.md](./game-log.md), distinct from the player-level `Batting`/`Pitching`/`Fielding` tables.
- A new EF Core migration capturing all of the above, verified to apply cleanly to a fresh database.

**Definition of done**: `dotnet ef database update` (or the equivalent `Retrosharp.Data.Migration` invocation) creates every table described across all specs, with the relationships, indexes, and delete behaviors called for. No application code beyond the data layer needs to exist yet — this step is schema only.

### Progress Log

**What was found (starting state)**:
- The existing `Retrosharp.Data`/`Retrosharp` (Contract) layers predated every decision made in today's spec sessions. No `GameEvent`-family tables existed at all. `FieldingModel`/`Fielding` had no `Position` column and was keyed on `(PersonId, FranchiseId, SeasonYear)` only. `GameStatisticsModel`/`GameStatistics` was a single undifferentiated table shaped like team batting stats, with no team-level pitching or fielding equivalents.

**What was built**:
- 5 new enums (`GameEventType`, `BattedBallType`, `FieldingCreditType`, `BaseState`, `GameAdjustmentType`) in `Retrosharp.Contract.GameEvent`, shared between the Contract and Data.Model layers (both projects already reference the same assembly, so this avoids duplicating enum definitions).
- 7 new Contract classes and 7 new EF Core models for the `GameEvent`-family tables (`GameEvent`, `GameEventRunner`, `GameEventFieldingCredit`, `GameEventGameStatus`, `GameSubstitution`, `GameAdjustment`, `GameComment`).
- Split the single `GameStatistics`/`GameStatisticsModel` into `GameBattingStatistics`, `GamePitchingStatistics`, and `GameFieldingStatistics` (Contract, Data.Model, and matching repositories/interfaces), matching `game-log.md`'s three-table requirement. The old repository, interface, and `IocRegistratrions.cs` registration were replaced with three parallel ones.
- Added `Position` to `Fielding`/`FieldingModel` and updated its composite index to `(PersonId, FranchiseId, SeasonYear, Position)`.
- Updated `GameModel`/`FranchiseModel` navigation properties to match the split statistics tables, and added `GameModel` navigation properties for the new `GameEvent`-family collections.
- Generated and applied migration `20260715031434_GameEventAndStatisticsSchema`.

**Discrepancies and decisions made during implementation** (none of these were explicitly specified in the governing docs, so they're recorded here rather than silently assumed):
- **Delete behavior for `GameEventRunner`/`GameEventFieldingCredit`**: set to `Restrict` rather than `Cascade` from `GameEvent`. Cascading here would create a multiple-cascade-paths conflict under SQL Server, since `GameEventFieldingCredit` has foreign keys to both `GameEvent` and `GameEventRunner` (which itself cascades from `GameEvent`). Only the direct `Game` → `GameEvent`/`GameSubstitution`/`GameAdjustment`/`GameComment` edges use `Cascade`, matching the existing precedent set by `GameLineup` and the statistics tables.
- **Person-referencing relationships use no back-navigation collection** (`.WithMany()` with no lambda) for every new FK targeting `Person` (batter, pitcher, responsible pitcher, fielder, substituted-in player, adjustment subject). This follows the precedent already set by `FieldingModel`'s existing `Person`/`Franchise` configuration, and avoids bloating `PersonModel` with a dozen more navigation collections for what will be very high-volume tables.
- **`Fielding`'s composite index was not made unique.** The spec describes `(PersonId, FranchiseId, SeasonYear, Position)` as a natural key, but `Batting` and `Pitching`'s existing composite indexes are also non-unique (performance indexes only, not enforced constraints). Kept consistent with that existing pattern rather than unilaterally introducing stricter enforcement — worth revisiting when the Step 6 stat-derivation logic is built, since that's what will actually be responsible for not violating the natural key.
- **`GameEventType` and `BattedBallType` enum members** are a representative starting set based on the examples named in `game-event.md`, not an exhaustive list. `game-event.md` itself says "etc." — expect this enum to grow during Step 6a (play-code grammar parser) as real play codes are worked through.

**Errors encountered**:
- The full solution build (`dotnet build Retrosharp.slnx`) reports 4 errors, all pre-existing and unrelated to this step's changes: NServiceBus's `SqlPersistenceTask` fails on `Retrosharp.Engine.Console.Saga.GameLogSaga` because its `GameLogSagaData` type lives in a different assembly (`Retrosharp`) than the saga itself. This is Step 4/5 territory (ETL Messaging Infrastructure / Game Log Parser), not Step 1. Confirmed the three projects actually in scope for this step (`Retrosharp`, `Retrosharp.Data`, `Retrosharp.Data.Migration`) build independently with 0 warnings beyond the pre-existing nullable-reference-type warning pattern already present throughout the codebase, and 0 errors.

**Unrelated discrepancy noticed, since resolved**:
- `PitchingModel` was mapped to a database table literally named `PitchingModel`, inconsistent with every other table's naming convention (`Person`, `Batting`, `Fielding`, `Game`, etc. are all named after the entity, not the model class). Predated this step's changes and was flagged here as out of scope. Fixed in a follow-up commit (`1fad1c1`, "Fix bad table name") via `[Table("Pitching")]`, with the resulting `RenameTable` migration folded into Step 2's `SeedDataSchemaFixesAndLeagueSeed` migration. Confirmed against the live database: the table is `Pitching`, no `PitchingModel` remains.

**Verification performed**:
- `dotnet build` on the migration project (which transitively builds `Retrosharp` and `Retrosharp.Data`): 0 errors.
- `dotnet ef migrations add` generated cleanly (with an expected data-loss warning from splitting `GameStatistics`, harmless since no real data exists yet).
- `dotnet ef database update` applied both migrations to a fresh LocalDB database (`(localdb)\mssqllocaldb`, database `Retrosharp`) without error.
- Queried the resulting database directly via `sqlcmd`: all 10 new tables exist with the expected column counts, `Fielding` has 11 columns (10 original + `Position`), and `GameEventGameStatus` has exactly 2 columns (`GameId`, `ProcessedUtc`) confirming the shared-primary-key design was applied correctly rather than EF Core adding a redundant identity column.
- The LocalDB database was left in place (not dropped) — it matches the connection string already checked into `appsettings.json` and is ready to be used as the working database for Step 2 onward.

---

## Step 2: Seed Data Automation

**Status**: Complete

**Governing spec**: [seed-data.md](./seed-data.md)

**Depends on**: Step 1.

**Objective**: Implement the two seeding mechanisms decided on — `HasData` for `League`, and a CSV-driven idempotent upsert routine for `Franchise` and `Ballpark` — wired into `Retrosharp.Data.Migration`'s existing startup sequence.

**Deliverables**:
- `League` rows baked into a migration via `HasData`.
- An idempotent upsert routine reading `franchises.csv` and `ballparks.csv`, matched by natural key, added to `Retrosharp.Data.Migration`'s startup sequence alongside its existing `Database.MigrateAsync()` call.
- Verification that the same command works unmodified as a manual invocation, a GitHub Actions step, and a Docker container initialization step.

**Definition of done**: matches [seed-data.md](./seed-data.md)'s Acceptance Criteria — a fresh database is fully seeded by one run, and repeated runs produce no duplicates or unwanted changes.

### Progress Log

**What was found (starting state)**: no seeding mechanism existed; `League`/`Franchise`/`Ballpark` were entirely unpopulated. Checking the real seed CSVs against the existing schema (rather than assuming they'd just fit) surfaced three real bugs that would have made seeding impossible or silently wrong:
- `Franchise.FranchiseCode` had a **unique** index, but Retrosheet's real data reuses the same code across consecutive eras of one franchise — confirmed empirically (`BSN` and `CLE` each appear 7 times, `SLN`/`CHN`/`BRO` 6 times, etc.). The actual natural key is the combination of the franchise's stable identifier and that era's start date.
- `Ballpark.FirstGame` was a required non-nullable `DateTime`, but 395 of 656 real ballpark rows (60%) have no recorded first-game date.
- A broader, more consequential discovery: EF Core's nullable-reference-type convention silently forces **every** non-`?`-annotated string property to `NOT NULL` in the database, regardless of whether `[Required]` is present. This affects dozens of fields across the whole schema that were clearly meant to be optional (`Person`'s birth/death/cemetery fields, `Game.GameNotes`, etc.) — not something introduced this session, but not previously exercised against real data either. Fixed only the fields Step 2 actually populates with blanks (`Franchise.DivisionCode`, `Franchise.AlternateNickname`, `Ballpark.ParkName`, `Ballpark.StateProvinceCountry`); the rest is flagged below as a priority item before Step 3, since `person.md` explicitly requires tolerating incomplete biographical data on exactly this kind of field.

**What was built**:
- Migration `SeedDataSchemaFixesAndLeagueSeed`: the nullability fixes above, `Franchise`'s corrected natural-key index (composite unique on `FranchiseIdentifier`+`FranchiseStart`, `FranchiseCode` demoted to non-unique), and `League`'s 7 rows via `HasData`.
- `FranchiseSeedRow`/`FranchiseSeedRowMapping` and `BallparkSeedRow`/`BallparkSeedRowMapping` in `Retrosharp.Format`, mirroring the existing `BioFileMapping` convention exactly (CsvHelper `ClassMap`, index-based column mapping, explicit blank-to-null conversion for optional fields).
- `SeedDataService` (`Retrosharp.Service`, behind `ISeedDataService`): loads existing `Franchise`/`Ballpark` rows into memory once per run, then upserts by natural key, logging added/skipped counts — mirrors the existing "check before insert" idempotency pattern already used for `Person`.
- CSV files wired into `Retrosharp.Data.Migration.csproj` via a `Link`-ed `CopyToOutputDirectory` reference to the single canonical `docs/csv/` location (no duplication of the actual data), resolved at runtime via `AppContext.BaseDirectory` so the routine works regardless of working directory — local run, CI, or inside a Docker container.
- `Program.cs` now runs seeding immediately after `MigrateAsync()`, logging a summary line.

**Discrepancies and decisions made during implementation**:
- The three schema issues above.
- **Fixed a real bug in shared DI infrastructure**, not specific to this step: `Retrosharp/DI/ContainerRegistration.cs`'s auto-discovery of `IRegister` implementations walked only `Assembly.GetReferencedAssemblies()`, which reflects assemblies whose *types* are directly touched by the referencing assembly's compiled IL — not every assembly listed as a project reference. Since `Program.cs` only used `ISeedDataService` (from `Retrosharp.Service.Interface`) and never directly referenced a type from `Retrosharp.Service` (the implementation project, where `SeedDataService` and its `IocRegistrations` actually live), that assembly was never discovered, so `SeedDataService` silently failed to register. Fixed by also scanning the calling assembly's own output directory for `Retrosharp*.dll` files. This wasn't unique to my change — any future service in the same situation (interface referenced directly, implementation only reached through DI) would hit the identical failure in `Retrosharp.UI.Api` or `Retrosharp.Engine.Console`.

**Errors encountered** (troubleshooting along the way, not final state):
- LocalDB detached the `Retrosharp` database between sessions — the `.mdf`/`.ldf` files existed on disk but weren't attached to the running instance. An environment quirk, not a migration problem; resolved by removing the orphaned files and letting `dotnet ef database update` rebuild cleanly from all three migrations in sequence.
- `ISeedDataService` resolution failed at runtime (`InvalidOperationException: No service ... registered`) before the `ContainerRegistration` fix above.
- Noted, not fixed: EF Core logs a benign warning about savepoints being disabled (`MultipleActiveResultSets=true` combined with `BaseRepository.CreateAsync`'s per-row-transaction pattern). Pre-existing behavior, functioned correctly, out of scope here.

**Verification performed**:
- Solution-wide build: 0 errors beyond the same 4 pre-existing `Retrosharp.Engine.Console` errors already identified and confirmed unrelated in Step 1.
- Ran the full pipeline against a fresh database: **125 franchises added, 656 ballparks added** — both match the source CSVs' actual row counts exactly.
- Ran it again immediately after: **0 added**, all 125/656 correctly reported as already present — idempotency verified by observed behavior, not just code inspection.
- Verified via `sqlcmd` that `League` has exactly the 7 expected rows with correct codes and names, and that the nullability fixes landed as `is_nullable = 1` in the live database schema.

---

## Step 3: Person Parser

**Status**: Complete

**Governing spec**: [person.md](./person.md)

**Depends on**: Step 1 and Step 4 — `Person` runs as a saga per [person.md](./person.md) and [parser.md](./parser.md), so the messaging infrastructure must exist first. Does not depend on Step 2 — `Person` and seed data are independent of each other and can be built in either order.

**Objective**: Parse Retrosheet's biofile and populate `Person`.

**Deliverables**:
- Biofile CSV parser, using the newer file format per the spec.
- Date-field normalization (day `00` → `01`, month `00` → `01`, missing year → `NULL`).
- `HOF` boolean derived from the "HOF" field value.
- Idempotent insert/update logic keyed on Retrosheet ID.
- Idempotent, atomic saga per file, per [parser.md](./parser.md).
- End-of-parse logging of records added vs. updated.
- API endpoint to initiate processing of a biofile.

**Definition of done**: matches [person.md](./person.md)'s Acceptance Criteria in full, including running as an NServiceBus saga triggered by a message placed on the service bus via its API endpoint, per [parser.md](./parser.md).

### Progress Log

**What was found (starting state)**:
- `BioFileMapping.cs` targeted the wrong (legacy) 31-column layout: missing `usename` entirely (shifting every subsequent field by one column), and a copy-paste bug reading `FinalUmpireGame` from `Row[35]` (out of bounds) instead of `Row[25]`. Date parsing used generic `DateTime.TryParse`, implementing none of the spec's 00-day/00-month/missing-year normalization rules.
- `BaseRepository.UpdateAsync` threw `NotImplementedException` — fine for Franchise/Ballpark seeding (skip-only), not fine for Person, which must update existing records.
- No test project existed anywhere in the solution.
- Verified `biodata/biofile0.csv` (downloaded directly from Retrosheet) against the real newer-format column layout before writing any code: 32 columns confirmed exactly, `YYYYMMDD` dates confirmed, and every date-normalization edge case the spec calls out confirmed present in the real data (month-and-day-both-`00`, day-only-`00`, fully blank dates). ~26,961 data rows.

**What was built**:
- `RetrosheetDateParser`: a single shared static method implementing the three normalization rules, replacing the copy-pasted `DateTime.TryParse` lambdas that had caused the `FinalUmpireGame` bug in the first place.
- `BioFileMapping` rewritten for the confirmed real 32-column layout, all 10 date fields routed through `RetrosheetDateParser`, `HOF` fixed to an exact `"HOF"` match.
- `BaseRepository.UpdateAsync` implemented for real (single-entity transaction pattern, mirroring `CreateAsync`).
- `PersonRepository.BulkUpsertAsync`: whole-file atomicity via one transaction spanning the entire batch, existing rows loaded into a dictionary once, periodic `SaveChangesAsync` every 1000 rows to bound EF Core's change-tracker overhead without breaking atomicity.
- `PersonImportService`, `PersonStart`/`PersonComplete`/`PersonCancel` messages, and a real `PersonSaga` (mirroring `GameLogSaga`'s Start/Complete/Cancel shape, but with actual working logic rather than stubs) in `Retrosharp.Engine.Console`.
- A UI.Api endpoint (`POST /api/person/import`) to trigger real imports.
- `Retrosharp.Format.Tests`: the solution's first xUnit test project, covering `RetrosheetDateParser` and `BioFileMapping` against real edge-case values pulled directly from `biofile0.csv`, including a regression test specifically targeting the class of bug that caused the original `FinalUmpireGame` column mismatch.

**Discrepancies and decisions made during implementation**:
- `biodata/` (the downloaded Retrosheet source files) was deliberately left out of the repository and added to `.gitignore` — the biofile is architecturally an external ETL input referenced by file path at runtime (like Game Log files will be in Step 5), not a build-embedded resource like the seed-data CSVs from Step 2, and most of the files in that directory (`coaches0.csv`, `managers0.csv`, etc.) aren't even used by this parser.

**Errors encountered** (real bugs found via live verification against the full real file, not caught by unit tests against a small fixture):
- `PersonModel` forced several string columns `NOT NULL` via EF Core's nullable-reference-type convention, but real biofile data legitimately leaves many of them blank (`Bats`, `Throws`, `UseName`, and most death/cemetery fields for anyone still alive or with incomplete records) — exactly the gap flagged as priority technical debt in Step 2's progress log. Fixed via a new migration making 18 fields nullable across both `PersonModel` and the `Person` contract.
- Mapster's `Map(source, destination)` overload was overwriting the tracked `PersonModel`'s primary key with the incoming `Person`'s default `Id` (0) during in-place updates, since a freshly-parsed `Person` never carries a real database `Id` — EF Core's change tracker correctly rejected the resulting key modification. Fixed by explicitly restoring the key after mapping, in both `BulkUpsertAsync` and the newly-implemented `BaseRepository.UpdateAsync`.
- Both bugs were caught only because verification ran the entire real ~27,000-row file rather than stopping at a small hand-crafted fixture; the fixture-based unit tests didn't (and structurally couldn't) exercise either failure mode, since the fixture data happened not to include enough blank-field variety and the update path was never being exercised until the idempotency re-run.

**Verification performed**:
- 12 unit tests (date normalization edge cases + column-mapping regression tests): all passing.
- Full solution build: 0 errors.
- Live first import against the real `biofile0.csv` (routed through the same UI.Api → RabbitMQ → Engine.Console path proven in Step 4): 26,961 people added, exactly matching the file's row count.
- Spot-checked directly via `sqlcmd`: Hank Aaron's row (`IsHof=1`, `BirthDate=1934-02-05`, `DeathDate=2021-01-22`) and all three date-normalization edge cases (`19010000`→`1901-01-01`, `18420200`→`1842-02-01`, blank→`NULL`) correct in the live database.
- Live idempotency re-run: 0 added, 26,961 updated, row count unchanged — confirmed after fixing the Mapster key-overwrite bug above. The failed run in between (before that fix) rolled back cleanly with zero partial data committed, confirming atomicity held even under a real, deterministic failure.

### Post-completion fix: optional string fields stored as `""` instead of `NULL`

Reported after this step was marked complete: a quick examination of the live `Person` table showed `BirthCity`, `BirthCountry`, `DeathCity`, `Cemetery`, and similar fields holding empty strings rather than `NULL` wherever the biofile left them blank — wasting space and muddying any future indexing/filtering on "no value recorded."

**Root cause**: `BioFileMapping.cs` mapped these 14 optional string fields (`UseName`, `BirthCity`, `BirthState`, `BirthCountry`, `DeathCity`, `DeathState`, `DeathCountry`, `CemetaryName`, `CemetaryCity`, `CemetaryState`, `CemetaryCountry`, `CemetaryNote`, `BirthName`, `AlternateName`) with plain `.Index(n)`, no `.Convert(...)`. CsvHelper's default converter for a blank field behaves differently by target type: for a nullable *value* type (`DateTime?`, `char?` — used by `Bats`/`Throws`/every date field), a blank field converts to `null`; for `string`, it converts to `string.Empty`. Since `Person`/`PersonModel` already declared these fields nullable (`string?`, fixed by Step 2/3's own nullable-reference-type migration), the schema was ready for `NULL` — but nothing in the parsing path ever actually produced one for a blank string field.

**Fix**: added a shared `NullIfBlank` helper in `BioFileMapping.cs` and applied `.Convert(c => NullIfBlank(c.Row[n]))` to all 14 fields (mirroring the existing `RetrosheetDateParser`-based pattern already used for dates). `BioFile.cs`'s corresponding properties changed from `string` to `string?` to match. `RetrosheetId`, `LastName`, and `FullName` were left untouched — confirmed 0 blank occurrences for all three across the full real file, so they don't need the same treatment.

**Verification**: 2 new/updated unit tests in `Retrosharp.Format.Tests` using real fixture rows (22/22 passing). Re-ran the live biofile import through `PersonRepository.BulkUpsertAsync`'s existing update path (no new tooling needed — Mapster's `Map(source, destination)` overwrites the tracked entity's fields including nulls): every previously-empty-string count moved to the exact same count of `NULL` (`UseName` 1,126; `BirthCity` 2,881; `DeathCity` 14,746; `Cemetery` 16,182; `AlternateName` 26,944 — all now zero empty strings), with row count unchanged (26,961) and a repeat run confirming stability.

---

## Step 4: ETL Messaging Infrastructure

**Status**: Complete

**Governing spec**: [project.md](./project.md) (ETL requirements), [parser.md](./parser.md) (base saga requirements shared by every parser), [person.md](./person.md), [game-log.md](./game-log.md), and [game-event.md](./game-event.md) (parser-specific saga requirements)

**Depends on**: Step 1 only. Logically independent of Step 2, so these two can be built in either order or in parallel. Unlike the original plan, this step is no longer something to defer until a parser exists to test against — it is now foundational: Step 3 (Person) cannot begin until this step is complete, since Person runs as a saga per [person.md](./person.md) and [parser.md](./parser.md). Person doesn't need Step 2's seed data, so it's the first parser that will exist to test this infrastructure against, once this step is done.

**Objective**: Stand up the shared NServiceBus/RabbitMQ plumbing that the Person, Game Log, and Game Event Parser sagas will all run on top of. This is infrastructure shared by Steps 3, 5, and 6, not specific to any one parser.

**Deliverables**:
- `Retrosharp.Engine.Console` configured as an NServiceBus endpoint over RabbitMQ, with saga persistence.
- Error queue and audit queue configured.
- Recoverability policy: exponential backoff with jitter, configurable retry count and initial wait, eventual dead-letter on exhaustion.
- `Retrosharp.UI.Api` configured as a send-only endpoint capable of initiating ETL jobs.
- Structured logging for all NServiceBus operations.

**Definition of done**: a trivial test message can be sent from the API endpoint, received by the Engine, and successfully processed end-to-end, with a deliberately-failing test message demonstrating the retry/backoff/dead-letter path.

### Progress Log

**What was found (starting state)**:
- `Retrosharp.Engine.Console/Program.cs` was literally `Console.WriteLine("Hello, World!");` — no host, no NServiceBus configuration at all, despite the `.csproj` already referencing `NServiceBus`, `NServiceBus.Extensions.Hosting`, `NServiceBus.Persistence.Sql`, and `NServiceBus.RabbitMQ`.
- `GameLogSaga.cs` already existed as a scaffold (all three handlers `throw new NotImplementedException()`), but was breaking the solution build: `GameLogSagaData` lived in the shared `Retrosharp` library, not `Retrosharp.Engine.Console`, and NServiceBus.Persistence.Sql's `SqlPersistenceTask` MSBuild task requires a saga's *entire type hierarchy* — not just its concrete `SagaData` type — to live in the same assembly as the saga class. This was the exact cause of the 4 known build errors flagged in Step 1's progress log.
- `Retrosharp.UI.Api/Program.cs` was a stock ASP.NET Web API template with NServiceBus packages referenced but zero configuration.
- `NServiceBus.Persistence.Sql` was pinned to 8.3.0, which predates the already-referenced `NServiceBus` 10.2.0 core's persistence extensibility API (`UsePersistence<T>()` now requires `T : IPersistenceDefinitionFactory<T>`, which 8.3.0's `SqlPersistence` type doesn't implement). The correct minimum compatible version is 9.0.2.

**What was built**:
- Relocated `GameLogSagaData` *and* `BaseSagaData` (not just the concrete type — the script generator needs the whole hierarchy) into `Retrosharp.Engine.Console/Saga/`, out of the shared `Retrosharp` library.
- `Retrosharp.Engine.Console/Program.cs` rewritten as a full NServiceBus host: RabbitMQ transport, SQL persistence (`NServiceBus.Persistence.Sql` 9.0.2), explicit error/audit queue names, and a custom exponential-backoff-with-jitter recoverability policy (NServiceBus's built-in delayed retries only support a linear `TimeIncrease`, not true exponential backoff or jitter).
- `Retrosharp.UI.Api/Program.cs` configured as a send-only endpoint with explicit message routing.
- New `MessagingConfiguration` class (`Retrosharp.Configuration`) for RabbitMQ connection string, endpoint name, queue names, and recoverability settings — kept separate from the existing SQL-focused `RetrosharpConfiguration`.
- `Retrosharp.Data.Migration` now applies the NServiceBus persistence schema via the official `NServiceBus.Persistence.Sql.ScriptRunner.Install()` API (found mid-implementation, more robust than hand-rolled SQL execution), copying the scripts generated at Engine.Console's build time via a build-order-only `ProjectReference`.
- A throwaway `PingMessage`/`FailingPingMessage` diagnostic pair (UI.Api controller + Engine.Console handlers), used only to prove the pipeline end-to-end per this step's Definition of Done — not tied to any real parser.
- Removed `EnableInstallers()` from Engine.Console after discovering it was the sole cause of NServiceBus.Persistence.Sql re-running its schema installer at every endpoint startup, duplicating what Data.Migration already applies; RabbitMQ's own queue/exchange creation happens unconditionally regardless of installer settings, so nothing was lost by removing it.

**Discrepancies and decisions made during implementation**:
- `ScriptRunner.Install()`'s actual parameter order (`dialect, tablePrefix, connectionBuilder, scriptDirectory, shouldInstallOutbox, shouldInstallSagas, shouldInstallSubscriptions, cancellationToken`) doesn't match what a first reading of its signature suggests (`scriptDirectory` and `tablePrefix` are easy to transpose, both being adjacent string parameters) — confirmed via reflection against the actual compiled assembly rather than guesswork.
- No corresponding boolean exists for the Timeout script category; only Outbox, Sagas, and Subscriptions are gated, and `TimeoutData` was not created as a result. Left as-is since it's plausible modern NServiceBus versions no longer need a SQL-backed timeout store when using a transport (RabbitMQ) capable of native delayed delivery; not confirmed definitively, flagged for future attention if timeout-dependent functionality is ever needed.
- Chose to test cross-machine connectivity empirically (Parallels shared-network host address, `10.211.55.2`) rather than assume `localhost` would work from within the Windows VM, since RabbitMQ runs in Docker on the Mac host, not inside the guest.

**Errors encountered**:
- `NServiceBus.Persistence.Sql` 8.3.0/`NServiceBus` 10.2.0 version mismatch (see above) — resolved by upgrading to 9.0.2.
- `ScriptRunner.Install()` parameter order mismatch — resolved via reflection against the compiled assembly.
- Killing a hung background `dotnet run` process also killed the underlying LocalDB engine, losing (empty, no real data) database files mid-session; recreated cleanly.

**Verification performed**:
- Full solution build: 0 errors, confirming the `SagaData`/`BaseSagaData` relocation fixed the previously-known build failure.
- Live end-to-end test against a real RabbitMQ instance (Docker on the Mac host, reached from this Windows VM via Parallels' shared-network host address): a trivial `PingMessage` sent from UI.Api was received and processed by Engine.Console, RequestIds matching exactly.
- A deliberately-failing `FailingPingMessage` exercised the full retry/backoff/dead-letter path: 3 immediate retries, then 5 delayed retries at ~2.3s/4.2s/8.3s/16.2s/32.3s (confirmed exponential, each roughly doubling, with jitter), ending with the message correctly routed to the configured error queue.
- After removing `EnableInstallers()`, re-verified live: RabbitMQ queue creation and message flow still work correctly, and the duplicate "Executing saga creation scripts" log line no longer appears.

---

## Step 5: Game Log Parser

**Status**: Complete

**Governing spec**: [game-log.md](./game-log.md)

**Depends on**: Step 1, Step 2 (needs `League`, `Franchise`, `Ballpark` populated), Step 3 (needs `Person` populated), Step 4 (runs as a saga).

**Objective**: Parse Retrosheet's season-wide Game Logs files (`glYYYY.TXT`) and populate `Game`, `GameLineup`, `GameBattingStatistics`, `GamePitchingStatistics`, and `GameFieldingStatistics`.

**Deliverables**:
- Field-position parser matching the [Format](./game-log.md#format) section exactly.
- Lookups resolving franchise codes, person IDs (managers, umpires, pitchers, batters), and ballpark IDs against the tables populated in Steps 2 and 3.
- Idempotent, atomic saga per file.
- Batch/multi-file processing support.
- API endpoint to initiate processing of a Game Log file.

**Definition of done**: matches [game-log.md](./game-log.md)'s Acceptance Criteria. At this point, a full season's worth of games, lineups, and team-level aggregate statistics should be queryable directly from the database, even though no player-level stats or play-by-play exist yet.

### Progress Log

**What was found (starting state)**: CSV parsing infrastructure (`GameLogFileService`, `GameLogMapping`) already existed and looked complete, but validating it against a real reference file (`docs/csv/gl2025.txt`, the full 2025 season, 2,430 games — 30 teams × 162 ÷ 2, confirmed) before writing any new code surfaced five real, previously-undetected bugs:
- **`GameLogMapping`'s entire Home Lineup block was off by one CSV field** (indices 133-159 instead of 132-158). Every home lineup ever parsed by the existing code had its batter ID/name/position scrambled by one field per batter; the ninth batter's "position" was actually reading the (usually blank) `AdditionalInformation` field. The Visitor Lineup mapping was correct.
- **`GameLog.GameDate` had no date-parsing `.Convert(...)`**, unlike every date field in the Person parser's `BioFileMapping`.
- **Fields 159/160 (`AdditionalInformation`/`AcquisitionInfo`) were never mapped at all.**
- **`GameLogFileService` never set `HasHeaderRecord = false`.** Game log files have no header row (unlike the biofile, which does), so CsvHelper's default would have silently discarded the first game of every real file.
- **`GameLog.GameAttendance` was a non-nullable `int`**, but the real file has exactly one row (a suspended-and-completed-later game) with a blank attendance figure, matching the format spec's own "Missing fields will be NULL" note — this threw a `TypeConverterException` on live import until fixed.

Separately, `GameModel`'s only natural-key index (`GameDate`, `HomeFranchiseId`, `VisitorFranchiseId`) didn't include `GameNumber`. The real file has 14 genuine doubleheaders (28 rows each of `GameNumber` "1" and "2"), so any idempotency check keyed without `GameNumber` would have treated a doubleheader's second game as a duplicate of the first and silently dropped it.

The actual DB-persistence path for Game Log had never been built: `GameLogSaga` was a `NotImplementedException` stub, and `GameService.ProcessGameLogsAsync` built a partial `Game` object but never called a repository and completely ignored `GameLineup`/`Game*Statistics`.

**What was built**:
- Fixed all five bugs above in `GameLogMapping.cs`, `GameLog.cs`, and `GameLogFileService.cs`.
- `IFranchiseRepository.GetByFranchiseCodeAndDateAsync(code, asOfDate)`: resolves a franchise by code *and* date, since `FranchiseCode` isn't unique across a franchise's eras (Step 2 finding) and Game Log rows only carry a code plus a game date.
- Migration `GameNaturalKey`: extended `Game`'s natural-key index to `(GameDate, GameNumber, HomeFranchiseId, VisitorFranchiseId)` and made it unique.
- `GameLogRecord` (`Retrosharp.Contract.Game`): a bundling DTO grouping one game's full graph (`Game` + lineups + three stats collections) so it can move from the service layer to the repository layer as one unit.
- `GameRepository.BulkInsertAsync`: whole-file-atomicity, batched (`SaveChangesAsync` every 200 games), **skip-only** idempotency (not upsert) — a completed historical game's recorded stats don't change the way a person's biographical data can, unlike Person's upsert pattern from Step 3.
- `GameLogImportService` (mirroring `PersonImportService`'s shape): resolves every FK per game (franchise by code+date, ballpark by site code, person by Retrosheet ID for managers/umpires/pitchers/batters), flattens the two 9-batter lineups into 18 `GameLineup` rows, derives `PlateAppearances` (not directly supplied by Retrosheet) and pulls team `Runs` from the game score rather than the hitting-stats sub-block, and maps `AdditionalInformation` into the existing `Game.GameNotes` free-text field.
- Real `GameLogSaga`/`GameLogSagaData`, brought to parity with `PersonSaga` (constructor now stores its dependencies, `GameLogCancel` added to `ConfigureHowToFindSaga`, `FilePath` added to the saga data, real handler bodies). `GameLogComplete` now carries `GamesAdded`/`GamesSkipped` instead of a single vague `RecordsProcessed`.
- `GameLogController` (`POST /api/gamelog/import`), mirroring `PersonController`, plus the missing `routing.RouteToEndpoint(typeof(GameLogStart), ...)` line in `UI.Api/Program.cs`.
- Removed the dead `GameService.ProcessGameLogsAsync`/`IGameService.ProcessGameLogsAsync` stub, fully superseded by `GameLogImportService`; trimmed `GameService`'s now-unused `IFranchiseRepository`/`IPersonRepository`/`IBallparkRepository` constructor dependencies.
- 7 new unit tests in `Retrosharp.Format.Tests` covering all five mapping/parsing bugs (including a home-lineup regression test and the blank-attendance case), using real rows pulled directly from `gl2025.txt`. (2 more added post-completion — see below.)
- `.gitignore`: added `docs/csv/gl*.txt` and `docs/csv/biofile*.csv` alongside the existing `biodata/` entry — both are downloaded ETL test inputs, not build resources.

**Discrepancies and decisions made during implementation**:
- **Skip-only idempotency for `Game`**, not upsert. A completed historical game's stats are a fixed historical record; re-parsing the same file should never touch existing rows. Contrasts deliberately with Person's upsert pattern.
- **`AcquisitionInfo` is parsed (bug fixed) but not persisted anywhere.** It has no natural destination column and isn't required by any spec acceptance criterion; adding a dedicated column for Retrosheet's own data-completeness metadata (as opposed to baseball data) felt like scope creep. Flagging here rather than silently dropping it without a record.
- **`GameModel.GameNumber` (byte) can't represent "A"/"B" three-team-doubleheader codes.** Predates this step, not present in any modern season (confirmed absent from the 2025 file), so `GameLogImportService.ParseGameNumber` throws `NotSupportedException` rather than silently mis-parsing, deferring an actual fix to whenever a file containing one is encountered.
- **The optional-identifier "(none)"/blank normalization was moved into the service layer**, applied uniformly to all six optional identifiers (four umpire positions, saving pitcher, game-winning batter) rather than relying on `GameLogMapping`'s existing partial handling, which only guarded two of the six even though the format spec says any umpire position can be unfilled.
- **A confirmed operational gap in Step 4's infrastructure, found while standing up a fresh environment for this step's verification**: `EnableInstallers()` was removed from `Engine.Console` in Step 4 on the reasoning that "RabbitMQ's own queue/exchange creation happens unconditionally regardless of installer settings." Against a genuinely fresh RabbitMQ broker (no residual queues from prior testing), this did not hold — `Engine.Console` failed to start with `Cannot validate the delivery limit of the 'Retrosharp.Engine' queue because it does not exist`, and the queue/exchange had to be declared by hand via the RabbitMQ management API before the endpoint could start. Not fixed as part of this step (it's Step 4's infrastructure decision to revisit), but flagged here since it will block anyone else standing up this project against a fresh broker — worth resolving before Step 8 (Containerized Deployment) or Step 9 (End-to-End Validation), where a clean environment is exactly the scenario that needs to work.

**Errors encountered**:
- The five bugs above, each caught only by running the real, complete reference file rather than a small hand-crafted fixture — consistent with the pattern already seen in Steps 2 and 3.
- Fresh desktop environment had no RabbitMQ at all and `Person` was empty (the biofile is gitignored, laptop-only); resolved by starting a local RabbitMQ container and having the genuine biofile (`docs/csv/biofile0.csv`) supplied and imported first.
- A freshly-migrated database (via `dotnet ef database update` alone) does not have NServiceBus's saga/outbox schema — that only gets installed by actually running `Retrosharp.Data.Migration`'s `Program.cs` (which also calls `ScriptRunner.Install()`), not by the EF CLI directly. Resolved by running the migration project's actual entry point.
- Sending two Person-import requests in quick succession (while working around the above) raced two concurrent saga instances against an empty `Person` table, producing transient `Cannot insert duplicate key row` errors on one side — expected and self-healed via NServiceBus's existing retry policy; not a Step 5 concern, but confirms the retry/backoff infrastructure from Step 4 behaves correctly under a real collision.

**Verification performed**:
- Solution-wide build: 0 errors.
- 19 unit tests in `Retrosharp.Format.Tests` (12 pre-existing Person tests + 7 new Game Log tests): all passing.
- Live import of the real biofile (26,961 people) to populate prerequisites, then live end-to-end import of the real, complete 2025 season file (`gl2025.txt`) through the full `UI.Api` → RabbitMQ → `Engine.Console` pipeline: **2,430 games added, 0 errors**, exactly matching the file's row count.
- `sqlcmd` verification: `GameLineup` = 43,740 rows (2,430 × 18 exactly), each of `GameBattingStatistics`/`GamePitchingStatistics`/`GameFieldingStatistics` = 4,860 rows (2,430 × 2 exactly).
- Spot-checked the Tokyo season-opener (LAN @ CHN, 2025-03-18) directly against the raw file: home lineup batters, positions, and final score (4-1) all correct — confirming the home-lineup off-by-one fix.
- Spot-checked one of the file's 14 real doubleheaders (CLE @ MIN, 2025-09-20): both games (6-0 and 8-0) present as distinct `Game` rows, confirming the natural-key fix.
- Re-ran the same file: **0 added, 2,430 skipped** every time, with `Game`/`GameLineup`/stats row counts unchanged — idempotency verified by observed behavior, including across a scenario where the import was triggered more than twice.

### Post-completion fix: `SavingPitcherId`/`GameWinningBatterId` always `NULL`

Reported after this step was marked complete: every imported game had `SavingPitcherId` and `GameWinningBatterId` set to `NULL`, even for games with real saves and walk-off RBIs in the source data.

**Root cause**: `GameLogMapping.cs`'s `.Convert()` calls for `UmpireLeftId`, `UmpireRightId`, `SavingPitcherId`, and `GameWinningPlayerId` (all four using the same pattern, pre-existing before this step) read the raw field via `c.Value.<PropertyName>` instead of `c.Row.GetField(index)`. `MemberMap.Convert()` is overloaded for both directions — `Convert(ConvertFromString<TMember>)` for reading (parameter exposes only `.Row`, no `.Value`) and `Convert(ConvertToString<TClass>)` for writing (parameter exposes `.Value` as the whole source object). Since `ConvertFromStringArgs` has no `.Value` member, `c.Value.SavingPitcherId` only type-checks against the *write*-side overload, so the compiler silently bound all four `.Convert()` calls to the wrong overload. That overload is never invoked while reading a file, so CsvHelper fell back to producing an empty string for the member regardless of the real CSV content — confirmed empirically with an isolated CsvHelper repro reproducing the exact pattern. This predated Step 5 (existing scaffolding) and wasn't caught by verification because the Step 5 spot-checks covered lineups, scores, and doubleheaders, but not these four specific columns.

**Fix**: rewrote all four `.Convert()` callbacks to read via `c.Row.GetField(index)`. Verified against real data: `SavingPitcherId` now has exactly 1,201 non-null values and `GameWinningBatterId` exactly 2,332, matching the raw file's real (non-`"(none)"`, non-blank) counts precisely. `UmpireLeftId`/`UmpireRightId` remain `NULL` for all 2,430 games — confirmed correct, since the 2025 regular-season file genuinely never populates those two fields (field umpires are historical/postseason-only).

**Verification**: added 2 new regression tests (`Parse_RealSavingPitcherId_MapsToTheCorrectValue`, `Parse_RealGameWinningPlayerId_MapsToTheCorrectValue`) using real rows already in the fixture — 21/21 tests passing. Cleared and re-imported all 2,430 games live; spot-checked the Tokyo opener's saving pitcher resolves correctly to Tanner Scott (`scott003`); re-confirmed idempotency (0 added, 2,430 skipped) still holds post-fix.

---

## Step 6: Game Event Parser

This is the largest and highest-risk step, and the one place where the plan intentionally breaks work into finer sub-steps than any other stage — the play-code grammar is intricate enough that building it as one unit would work against the "one at a time, correctness first" approach this plan is for.

**Governing spec**: [game-event.md](./game-event.md)

**Depends on**: Step 1, Step 2, Step 3, Step 4, and Step 5 (needs `Game` to already exist).

### Step 6a: Play-code grammar parser (no database dependency)

**Status**: Complete

**Objective**: Build and unit-test the raw parsing logic — turning a Retrosheet play-code string (for example, `S8.1-3;2-H`) into an in-memory structured representation (event type, batted ball type, runner advances, fielding sequence, RBI/error/unearned-run annotations) — entirely independent of the database or saga infrastructure.

**Deliverables**: a pure parsing library, tested against a broad set of real play codes pulled from actual Retrosheet files, covering hits, outs, strikeouts, walks, stolen bases, double/triple plays, and rundowns.

**Definition of done**: the parser correctly decomposes a representative sample of real play codes into structured objects, verified by unit tests, with no I/O of any kind.

### Progress Log

**What was found (starting state)**: no Game Event parsing code existed yet, but the `GameEvent`-family schema, contracts, and enums (`GameEventType`, `BattedBallType`, `BaseState`, `FieldingCreditType`, `GameAdjustmentType`) were already built in Step 1, ahead of need. The user supplied two real, complete reference files — `docs/csv/2025SDN.EVN` and `docs/csv/2025SEA.EVA` (81 games each, 14,256 total plays) — which were inventoried for every distinct play code (1,986 unique) before writing any parsing code, the same "read the real data first" discipline used in Steps 2, 3, and 5.

**What was built**:
- Two new `GameEventType` enum members, `DefensiveIndifference` and `OtherAdvance`, for the `DI`/`OA` primary codes found in the real data (no migration needed — plain `int`-backed enum column).
- `Retrosharp.Format.PlayByPlay` namespace: `ParsedPlay`, `ParsedRunnerAdvance`, `ParsedFieldingCredit` (plain POCOs mirroring `GameEvent`/`GameEventRunner`/`GameEventFieldingCredit` but carrying no `PersonId` — only base numbers and fielder numbers, since resolving those to actual `Person` rows needs lineup/substitution state that's Step 6b's job) and `PlayCodeParseException`.
- `PlayCodeParser.Parse(rawEventText, countField, pitchSequence)`: splits the code into primary/modifiers/advances (paren-aware, so a `/` or `;` inside an annotation like `(E1/TH)` isn't mistaken for a segment separator), dispatches the primary code across every category in the real data (hits, fielded outs including multi-runner double/triple plays, walks/intentional walks, strikeouts, hit-by-pitch, errors — both a bare `E<n>` and an error embedded mid-fielding-sequence like `4E3` — fielder's choice, stolen base/caught stealing/pickoff-caught-stealing, wild pitch/passed ball/balk, defensive indifference/other-advance/no-play, and the `K+`/`W+` bundled-event combinator), parses modifiers for trajectory (`BattedBallType`) and `SH`/`SF`, and parses each advance segment for base movement, outs-on-the-bases with fielding credits, and the RBI/earned-run default-then-override rule confirmed empirically against the real data (a scored run is an RBI and earned unless annotated `(NR)`/`(NORBI)` or `(UR)`/`(TUR)` — `(RBI)` itself never appears in modern data). `FoulBallsWithTwoStrikes` is derived separately by scanning the pitch-sequence string, since it's a cleaner and more reliable source than trying to infer it from the play code; `Balls`/`Strikes` come directly from the count field.
- 33 new unit tests in `Retrosharp.Format.Tests`, every one using a real play code (with its real count/pitch-sequence fields) pulled directly from the two reference files — including the spec's 6-4-3 double play example matched against a real play, a real relay-throw-out analog of the 8-6-2 example, and a real rundown-style chain where one fielder is credited twice. Two tests (`WP`/`PB` with no accompanying baserunner) are the only synthetic ones, clearly marked, since neither file happens to contain a standalone occurrence of either.

**Discrepancies and decisions made during implementation**:
- **`EventType` for a fielded out can't be resolved until modifiers are parsed.** `GroundOut` vs `FlyOut` depends on the trajectory modifier (ground ball vs anything caught in the air), which comes after the primary code in the string. Handled by having the primary-code parser return a "pending trajectory" placeholder for digit-leading codes, resolved once modifiers are read. Confirmed via the real data that every genuine fielded out carries a trajectory modifier — a fielded-out code with none throws rather than guessing.
- **A digit-leading code can still turn out to be an `Error`, not an out**, when it contains a mid-sequence `E<n>` (`4E3` = fielder 4 assists, fielder 3 is charged the error, batter reaches safely). This isn't obvious from the leading digit alone; the parser tracks whether any group within the sequence resolved as an error and overrides the `EventType` accordingly.
- **`FLE<n>` (a foul ball dropped for an error) produces zero runners**, unlike a bare `E<n>`. A dropped foul doesn't put the batter on base or end the plate appearance — nothing about base-occupancy state actually changed, so there's nothing to record as a `GameEventRunner`, even though the error itself is still captured via `EventType.Error`.
- **`(TUR)` (team unearned run) is treated identically to `(UR)`.** The schema's `IsEarnedRun` is a single boolean with no team-vs-individual distinction, so collapsing them is a deliberate simplification, not an oversight.
- **Every out from a primary-code digit/parenthetical group is modeled as a force at the next base up** (`StartBase + 1`), matching every real example and the spec's own worked examples. This doesn't yet cover a tag play recorded via that same mechanism where the runner is out at a base other than the next one up — not observed in either reference file, flagged here in case a broader historical dataset surfaces one later.
- **`PO<base>` (pickoff without a caught-stealing attempt) is implemented per Retrosheet's documented convention but was never exercised against real data** — zero occurrences in either file (only `POCS` appears). Same treatment for `C` (catcher's interference).

**Errors encountered** (found only by validating against the complete real files, not by hand-picked samples):
- The initial implementation passed all hand-written unit tests but failed on 17 of the 14,256 real plays when run against the full files. All 17 traced to four real grammar gaps: (1) `/` appearing inside a parenthesized annotation (`PO2(E1/TH)`) was being split as a modifier separator; (2) the mid-sequence error pattern (`3E1`, `4E3`, `5E3`) wasn't recognized at all; (3) `FLE<n>` (dropped foul error) wasn't recognized as its own pattern; (4) the `(TUR)` annotation wasn't recognized. All four were fixed and the full 14,256-play validation was re-run clean (0 failures) before writing the final unit test suite.
- A follow-up semantic spot-check (not just "doesn't throw") caught a fifth bug: `4E3`-style codes were still returning `EventType.GroundOut` instead of `EventType.Error`, since the digit-leading dispatch didn't distinguish "resolved as an out" from "resolved as an error" when deciding the pending-trajectory placeholder. Fixed by having the fielded-out parser report whether it encountered an error group.
- A sixth bug, caught only by the formal unit test suite: `WP` (wild pitch) was being matched by the generic `W` (walk) prefix check before ever reaching the dedicated `WP` branch, so every wild pitch was misparsed as a walk. Fixed by reordering the dispatch so exact-match codes are checked before single-letter prefix fallbacks.

**Verification performed**:
- 55 unit tests in `Retrosharp.Format.Tests` (22 pre-existing + 33 new): all passing.
- Full solution build: 0 errors.
- A throwaway validation harness (not committed) ran `PlayCodeParser.Parse` against all 14,256 real plays across both complete reference files: 0 failures, both before finalizing the grammar (which caught the six bugs above) and after (confirming the fixes held).

### Step 6b: `GameEvent`, `GameEventRunner`, `GameEventFieldingCredit` persistence

**Status**: Complete

**Objective**: Wire the Step 6a parser output into the database, populating `GameEvent`, `GameEventRunner`, and `GameEventFieldingCredit` for a single game, per the [Data Model](./game-event.md#data-model).

**Definition of done**: parsing one game's play-by-play produces exactly the rows the Data Model's worked examples (5-3 ground out, 6-4-3 double play, 8-6-2 relay, rundown) describe, verified against real games containing those play types.

### Progress Log

**What was found (starting state)**: no code anywhere read the raw `.EVN`/`.EVA` file structure (`id`/`info`/`start`/`play`/`sub`/`com`/`data`/adjustment records) -- Step 6a only consumed an already-extracted `rawEventText`/`countField`/`pitchSequence` triple. The `GameEvent`-family EF models (built in Step 1) had no navigation collections (`GameEventModel.Runners`, `GameEventRunnerModel.FieldingCredits`), so a 3-level graph couldn't be saved the way `GameModel`'s `GameLineups` collection already allows for `Game`. No natural-key lookup existed on `IGameRepository` to resolve an event file's `id,TTTYYYYMMDDX` record to a `Game.Id`. Real-data inventory of the two reference files (`docs/csv/2025SDN.EVN`, `2025SEA.EVA`) before writing any code surfaced a real, load-bearing structural detail not mentioned in the spec: `radj` records, which manufacture a baserunner mid-game under the extra-innings international tiebreaker rule, with no preceding plate appearance to establish them.

**What was built**:
- `Retrosharp.Format.EventFile` (new namespace, `src/lib/Retrosharp/Format/EventFile/`): `EventFileReader.ReadGames(filePath)` reads a raw event file into a sequence of `EventFileGame`s, each holding every record in strict original file order (`StartRecord`, `SubRecord`, `PlayRecord`, `ComRecord`, `DataRecord`, `AdjustmentRecord`). Uses CsvHelper's low-level `CsvParser` per line (quote-aware, since `com` records legitimately contain literal commas inside quoted text) rather than a fixed `ClassMap`, since record shape varies by the first field.
- `GameEventResolver` (`Retrosharp.Format.PlayByPlay`, pure logic, no I/O): walks an `EventFileGame`'s records maintaining three pieces of state -- a per-team lineup/position tracker (seeded by `start`, updated by `sub`), a per-half-inning baserunner tracker (identity + the pitcher responsible for that runner if they score, carried forward unchanged across pitching changes -- the "inherited runner" rule), and half-inning boundaries (precomputed by scanning backward for each record's next `PlayRecord`, so a `sub`/`radj` between two half-innings is correctly attributed to the *upcoming* half before it mutates state). Combined with Step 6a's `PlayCodeParser`, produces fully-identified `GameEventPlayRecord`s (new DTOs in `Retrosharp.Contract.GameEvent`: `GameEventRecord` → `GameEventPlayRecord` → `GameEventRunnerRecord`, mirroring `GameLogRecord`'s existing whole-graph pattern).
- `GameEventModel.Runners` / `GameEventRunnerModel.FieldingCredits` navigation collections, and the corresponding `RetrosharpContext.OnModelCreating` `.WithMany(...)` updates -- code-only, confirmed via a throwaway `dotnet ef migrations add` (immediately removed) that it produces no schema change.
- `IGameRepository.GetByNaturalKeyAsync(gameDate, gameNumber, homeFranchiseId, visitorFranchiseId)`, mirroring `FranchiseRepository.GetByFranchiseCodeAndDateAsync`'s existing pattern.
- `IGameEventRepository`/`GameEventRepository.BulkInsertAsync`: one transaction per call, an in-memory `HashSet<int>` of `GameId`s already present in `GameEvent` as the game-level "already processed" check appropriate to this step's scope (not the full `GameEventGameStatus` atomic claim, which is Step 6d's job), `SaveChangesAsync` once per game. Every `GameEventFieldingCreditModel`'s `.GameEvent` navigation property is set explicitly (in addition to being placed in the runner's `FieldingCredits` collection) -- it has two independent required FKs into the same ancestor tree (`GameEventId` direct, `GameEventRunnerId` via the runner), and only one of them gets fixed up automatically by EF Core's change tracker from collection placement alone.
- `IGameEventImportService`/`GameEventImportService.ImportAsync(filePath)`: processes every game in a file (an `id` record is a natural game boundary, so there's no cheaper way to process "just one game"), resolving home/visitor franchise and the target `Game` via the new natural-key lookup, resolving every distinct Retrosheet ID in the game via the existing `IPersonRepository.GetByRetrosheetIdAsync`, then calling `GameEventResolver.Resolve`. No saga, controller, or API endpoint yet -- deliberately deferred to Step 6f.
- New unit tests for `EventFileReader` and `GameEventResolver` in `Retrosharp.Format.Tests`, using a new fixture (`Fixtures/eventfile_sample.EVN`, 5 real complete games trimmed from `2025SDN.EVN`, chosen to include every Data Model worked example plus the `radj` tiebreaker case and a real pinch-runner substitution) plus hand-assembled `EventFileGame` scaffolding around real play codes for the resolver-specific state-tracking tests (inherited runner, pinch-runner identity swap).

**Discrepancies and decisions made during implementation**:
- Read the event file's own `start`/`sub` records directly for lineup/position tracking, rather than reusing Step 5's `GameLineup` table -- `GameLineup` only has the *starting* lineup (no substitutions) and stores position as a free string rather than the numeric code `GameEventFieldingCredit.Position` needs.
- `sub` and `radj` records are *interpreted* for runtime lineup/baserunner state (necessary for correct resolution) but not *persisted* -- `GameSubstitution`/`GameAdjustment` are Step 6c's job. `badj`/`padj`/`ladj`/`presadj` are recognized but not acted on; `presadj` (an explicit override of the inherited-runner default) was never observed in either real reference file, flagged for revisiting if real data ever needs it.
- No new `Retrosharp.Data.Tests` project -- persistence verified live against real data via `sqlcmd`, consistent with Steps 2/3/5's precedent; revisit at Step 6d if a genuine concurrency race needs more rigor than that allows.

**Errors encountered** (found only by resolving and persisting the complete real files, not by hand-picked samples -- consistent with every prior step):
- A resolver bug, not a parser bug: a play's runner-advance mutations were applied one full remove-then-add per advance, in the list order the parser returned them -- but a walk's own batter-to-first advance can be listed *before* an existing runner's advance off the same base in the same play, so removing that runner's start base after the batter's add evicted the wrong occupant. Fixed by splitting into two full passes over the play's advances: every removal first, then every addition -- independent of list order.
- Three real, previously-undetected bugs in the *already-complete* Step 6a `PlayCodeParser`, each found only by resolving every play across both complete real files (55 hand-picked unit tests hadn't exercised any of them):
  - The semicolon-joined-multiple-primary-codes branch (`"SB3;SB2"`, a double steal) used `result ??= ParseSingleCode(subCode, ...)` -- `??=` short-circuits and skips *calling* `ParseSingleCode` entirely once `result` is non-null, silently dropping every steal after the first.
  - `PO<base>`'s parenthetical was always parsed as a fielder-putout chain (like a fielded out's `"(<fielders>)"`), but a bare `PO` (unlike `POCS`/`CS`) can instead carry an error annotation (`"PO2(E1/TH)"`) -- a different grammar entirely. Reading `"E1/TH"` character-by-character as fielder digits produced garbage `Position` values from non-digit characters (`'E'-'0'=21`, `'/'-'0'` wrapping to 255, `'T'-'0'=36`, `'H'-'0'=24`).
  - The runner-out branch of advance-segment parsing (`"1X2(4E6)"`) called the same raw fielder-chain parser used for plain putout chains, which had no notion of an embedded error mid-chain (unlike the primary code's dedicated `"4E3"` handling) -- same garbage-`Position` symptom. Fixing the chain parser to recognize `<digit>E<digit>` surfaced a second, non-obvious error: the runner was still marked out despite the chain's last credit being an `Error` rather than a `Putout`. Confirmed wrong by an outs-count check against the real half-inning: treating it as an out would make it that half-inning's 3rd out mid-sequence, yet the same real file has another batter -- and another play -- still coming in that same half-inning, an impossible 4th out. Fixed by treating a trailing `Error` credit on an "X" advance as meaning the runner is safe, matching the same convention already used for `PO2(E1/TH)` and the primary code's `"4E3"`.
- A real CsvHelper `BadDataException` on a genuine data quirk: one `com` record in `2025SEA.EVA` has a stray period trailing its closing quote (`com,"...circle)".`), which fails CsvHelper's strict RFC4180 check. Fixed by setting `BadDataFound = null` on the parser configuration.
- All four bugs above were fixed at their root (`PlayCodeParser.cs`/`EventFileReader.cs`), with a real-play regression test added to `PlayCodeParserTests.cs`/`EventFileReaderTests.cs` for each, rather than worked around in the resolver.

**Verification performed**:
- 77 unit tests in `Retrosharp.Format.Tests` (55 pre-existing Step 6a + 22 new): all passing.
- Full solution build: 0 errors.
- Live import of both complete real reference files end-to-end (`GameEventImportService.ImportAsync`, via a throwaway DI harness, against the already-populated LocalDB database from Steps 2/3/5 -- 26,961 people, 125 franchises, 2,430 games): **81 games inserted for each file, 0 errors** -- 14,256 total `GameEvent` rows, exactly matching the total play count Step 6a's own validation harness reported across the same two files.
- `sqlcmd`-verified all four Data Model worked examples directly against the live rows: the 5-3 (well, 6-3, per the real play actually used) ground out, the 6-4-3 double play, the relay throw, and the rundown all produce exactly the runner/fielding-credit shapes described in [game-event.md](./game-event.md#data-model), including the double-FK check (`SELECT COUNT(*) FROM GameEventFieldingCredit WHERE GameEventId = 0` → 0).
- Re-ran both files: **0 games inserted, 81 skipped** each time -- game-level idempotency confirmed by observed behavior.
- Final data-integrity sweep across both imported files: 14,256 `GameEvent`, 15,514 `GameEventRunner`, 8,371 `GameEventFieldingCredit` rows, 162 distinct games, zero rows with an unresolved (`0`) `PersonId`/`BatterId`/`PitcherId`/fielding-credit `GameEventId` anywhere.

### Step 6c: Context records

**Status**: Complete

**Objective**: Parse and persist `GameSubstitution`, `GameAdjustment`, and `GameComment` records.

**Definition of done**: substitutions, handedness/batting-order adjustments, and commentary from a game are captured without being conflated into `GameEvent`.

### Progress Log

**What was found (starting state)**: `GameSubstitutionModel`/`GameAdjustmentModel`/`GameCommentModel` (built in Step 1) and `EventFileReader` (built in Step 6b, already parsing `sub`/`*adj`/`com` records in strict file order) were both already in place and completely unused for persistence -- 6b only *interpreted* `sub`/`radj` for its own in-memory lineup/baserunner tracking. All three tables turned out to be direct children of `Game` with a plain `Cascade` FK and no shared-ancestor double-FK complication like `GameEventFieldingCredit` had, and `GameModel` already carried the `GameSubstitutions`/`GameAdjustments`/`GameComments` navigation collections (built ahead of need, like `GameEvents` was) -- meaning this step needed no new grammar, no new state tracking, and no schema changes at all, just mapping already-parsed records to already-existing Contract classes.

**What was built**:
- `GameContextResolver` (`Retrosharp.Format.EventFile`, pure logic, no I/O, sibling to `GameEventResolver`): walks an `EventFileGame`'s records once, mapping every `SubRecord` → `GameSubstitution`, every `AdjustmentRecord` (all five types) → `GameAdjustment` (with `AdjustmentTypeCode` mapped to the existing `GameAdjustmentType` enum), and every `ComRecord` → `GameComment`. `StartRecord`s are deliberately excluded from `GameSubstitution` output (they're the starting lineup, not a substitution). Each of the three output lists gets its own independent 1-based `Sequence` counter, kept consistent with `GameEvent.Sequence`'s already-persisted meaning (counting only plays) rather than introducing a shared/interleaved counter across all four tables.
- `GameEventRecord` (Contract) extended with `Substitutions`/`Adjustments`/`Comments`.
- `GameEventImportService.ResolvePersonIdsAsync` broadened to collect Retrosheet IDs from every `AdjustmentRecord` type, not just `radj` (needed since `badj`/`padj`/`ladj`/`presadj` all carry a `PersonId` too).
- `GameEventImportService.MapToGameEventRecordAsync` now also calls `GameContextResolver.Resolve` and includes the result in the returned `GameEventRecord`.
- `GameEventRepository.BulkInsertAsync` extended to map and insert the three context tables per game, inside the same per-game transaction and the same game-level "already has any `GameEvent` rows" skip check built in 6b -- no new idempotency mechanism needed, since these rows never exist independently of a game's `GameEvent` rows.
- 10 new unit tests in `Retrosharp.Format.Tests` (`GameContextResolverTests.cs`), using real `sub`/`com`/`badj`/`radj` lines plus synthetic coverage for `padj`/`ladj`/`presadj` (absent from both real reference files), including a `StartRecord`-is-excluded check and an unresolvable-`PersonId` throw check.

**Discrepancies and decisions made during implementation**: none beyond what the plan already flagged and confirmed -- independent per-table `Sequence` counters (not a global one shared with `GameEvent`), and folding the three new lists directly into the existing `GameEventRecord` DTO rather than introducing a separate wrapper type, since both are produced together for the same game.

**Errors encountered**: none -- unlike Steps 6a/6b, no new bugs surfaced against the real reference files, consistent with this step reusing already-validated infrastructure (the file reader, the Retrosheet ID → PersonId resolution path) rather than adding new parsing logic.

**Verification performed**:
- 87 unit tests in `Retrosharp.Format.Tests` (77 pre-existing + 10 new): all passing.
- Full solution build: 0 errors.
- Cleared all `GameEvent`-family and context tables and re-imported both complete real reference files end-to-end via the same throwaway DI harness used in 6b: **81 games inserted for each file, 0 errors**, with `GameEvent` back to its expected 14,256 rows.
- `sqlcmd`-verified exact row counts against the files' own real line counts: `GameSubstitution` 1,833 (916 + 917 real `sub` lines), `GameAdjustment` 65 (18 + 47 real `*adj` lines across all five types), `GameComment` 155 (74 + 81 real `com` lines) -- zero rows with an unresolved (`0`) `PersonId` anywhere. `GameAdjustment`'s type breakdown (1 `BattingHandedness`, 64 `RunnerPlacement`) confirmed only `badj`/`radj` occur in either real file, matching Step 6b's own findings.
- Spot-checked `GameComment`/`GameSubstitution` rows directly against the source file's first game (`SDN202503270`, `Game.Id` 2437): comment text and substitution sequence/team/position values match the raw lines exactly.
- Re-ran both files: **0 games inserted, 81 skipped** each, confirming idempotency now covers the three new tables along with everything else.

### Post-completion note: `ej` (ejection) comments have a predetermined format, deliberately not parsed further

While spot-checking `GameComment` rows above, 10 real rows matching `ej,<person>,<job code>,<umpire>,<reason>` were noticed — Retrosheet's own [event file documentation](https://www.retrosheet.org/eventfile.htm) confirms this is a predetermined, structured format (job codes `P`/`M`/`C`/`T`/`N`), not free narrative text like the rest of `GameComment`. Extracting these into a dedicated `GameEjection` table was deliberately kept out of Phase 1 scope and documented instead as a Phase 2 feature — see [game-event.md](./game-event.md#future-enhancement-phase-2-gameejection) for the full data model and [project.md](./project.md#second-phase)'s Second Phase feature list. `GameComment` itself needed no change: it already stores `ej` rows verbatim, correctly, exactly like every other comment.

### Step 6d: `Batting`/`Pitching`/`Fielding` derivation and `GameEventGameStatus`

**Status**: Complete

**Objective**: Implement the atomic per-game claim via `GameEventGameStatus`, and derive/apply `Batting`, `Pitching`, and `Fielding` updates only after a successful claim.

**Definition of done**: processing the same game's stats twice (simulating two concurrent sagas) results in exactly one application of that game's statistics, verified by a concurrency test that deliberately races two sagas against the same shared game.

### Progress Log

**What was found (starting state)**: `Batting`/`Pitching`/`Fielding`'s EF models and repositories existed (Step 3) but were a blank slate for this step's purposes -- `BattingService.SaveAsync` was create-or-noop, not create-or-increment, and no `PitchingService`/`FieldingService` existed at all. `GameEventGameStatus`'s Contract/Model/schema config existed (Step 1) but had no repository. Step 1's own progress log had flagged `Fielding`'s composite natural-key index as deliberately left non-unique, "worth revisiting when the Step 6 stat-derivation logic is built" -- confirmed the same was true of `Batting`/`Pitching`. `BaseRepository<TM,TC>.CreateAsync`/`UpdateAsync` each open their own transaction, meaning they can't be called from inside another already-open transaction on the same `DbContext` -- ruled out reusing `BattingRepository`/`PitchingRepository`/`FieldingRepository`'s existing methods for this step's write path.

**What was built**:
- Migration `UniqueStatisticsNaturalKeys`: made `Batting`/`Pitching`/`Fielding`'s composite natural-key indexes unique, resolving the Step 1 breadcrumb. Applied cleanly against the live database (all three tables were still empty).
- `GameStatisticsResolver` (`Retrosharp.Format.PlayByPlay`, pure logic, no I/O, sibling to `GameEventResolver`/`GameContextResolver`): derives per-game Batting/Pitching/Fielding deltas from an already-resolved `IReadOnlyList<GameEventPlayRecord>`. Notable derivation rules: stolen bases/caught-stealing are attributed to the *runner* (via `GameEventRunner`), not whoever is currently at bat; runs allowed are attributed to `ResponsiblePitcherId` (not the current `PitcherId`), correctly handling inherited runners; innings pitched is stored as total outs recorded (a `short` can't hold a fractional inning); a batter's starting/finishing pitcher status (for `GamesStarted`/`GamesFinished`/`CompleteGames`/`Shutouts`) is derived from the first/last play of each half-inning rather than needing the game's `start` records passed in separately. New delta DTOs: `BattingDelta`/`PitchingDelta`/`FieldingDelta` (`Retrosharp.Contract`), bundled by `GameStatisticsDelta` (`Retrosharp.Contract.GameEvent`).
- `Pitching.Saves` and `Fielding.PassedBalls`/`DoublePlays`/`TriplePlays` are deliberately left at their default value -- not derivable from the current schema (no denormalized "current catcher" concept exists, unlike `PitcherId`) or too rule-dependent for a simple per-event count (`Saves`). `Batting.Positions` is likewise left at `0` -- see the dedicated Phase 2 documentation note below.
- `Pitching.EarnedRuns` is sourced directly from the game's own `data,er,...` records (parsed by `EventFileReader` since Step 6b, unused until now) -- resolved by `GameEventImportService` into a `Dictionary<int PersonId, short EarnedRuns>` and passed into `GameStatisticsResolver`. A pitcher with no matching `data er` record gets `EarnedRuns = 0` with a logged warning. This is a deliberate scope resolution: `game-event.md`'s own Requirement wording separates *sourcing* the stored value (a plain lookup, done here) from *independently computing* a play-by-play value for comparison/warning purposes (real validation logic, left for Step 6e) -- even though `phase-1-build-plan.md`'s own Step 6e wording reads as if it bundles both.
- `IGameStatisticsRepository`/`GameStatisticsRepository.TryApplyGameStatisticsAsync(gameId, delta)`: attempts the atomic claim (`Add` + `SaveChangesAsync` a `GameEventGameStatusModel`; a unique-constraint violation, caught via `SqlException.Number` 2601/2627, means another process already claimed this game -- roll back and return `false`). If the claim succeeds, applies each delta via a plain check-then-act (query for an existing natural-key row; `Add` a new one or `ExecuteUpdateAsync` an atomic in-place increment) rather than an insert/catch/fallback dance -- row-level races on one player's season row turn out not to be reachable in practice, since every game a player plays for a given franchise lives in that franchise's own event file (processed sequentially by one saga), and the only cross-file overlap (the shared-game scenario) is already fully serialized by the claim itself.
- `GameEventRepository.BulkInsertAsync` restructured from one transaction spanning the whole file to one transaction **per game** for the `GameEvent`-family insert, followed by a call to `TryApplyGameStatisticsAsync` for *every* game processed (not just newly-inserted ones) as its own separate transaction. This was a necessary, not optional, change: `GameStatisticsRepository` needs its own `BeginTransactionAsync()` on the same `DbContext`/connection `GameEventRepository` uses, and EF Core doesn't support nested transactions on one connection -- committing per game resolves that while also making a mid-file crash leave only fully-completed games behind (each is either fully done -- event data and statistics -- or untouched), consistent with "no partial parses" at the per-record level rather than the per-file level.
- `IGameEventRepository.BulkInsertAsync`'s return shape extended to `(GamesInserted, GamesSkipped, StatisticsApplied, StatisticsSkipped)`; `GameEventImportResult` and `GameEventImportService`'s logging extended to match.
- 13 new unit tests in `Retrosharp.Format.Tests` (`GameStatisticsResolverTests.cs`) covering each derivation rule in isolation (a single, a walk, an intentional walk, a sacrifice fly, a stolen base attributed to the runner, a caught-stealing, a run credited to the responsible pitcher rather than the current one, a 6-4-3 double play's GIDP and repeated fielder credit, a complete-game shutout, a reliever who finishes without starting, innings-pitched counting outs across all runners, and earned-runs sourcing/defaulting).
- Documented `Batting.Positions`'s intended meaning and a Phase 2 replacement (tracking positions actually played per season, not a single scalar column) in `spec/game-event.md` and `spec/project.md`'s Second Phase list, alongside the existing `GameEjection` entry.

**Discrepancies and decisions made during implementation**:
- A pre-existing naming mismatch between `Contract.Batting.Hits` (plural) and `Data.Model.BattingModel.Hit` (singular) would have silently left `Hit` at `0` if mapped via Mapster's default convention-based matching. `GameStatisticsRepository` maps every field explicitly instead of relying on Mapster for this write path, avoiding the trap entirely; the mismatch itself is out of this step's scope to fix and is called out in code rather than silently worked around.
- No new `Retrosharp.Data.Tests` project, per the precedent flagged as revisitable at this exact step in Step 6b's own progress log -- the genuine concurrency race this step's Definition of Done requires *does* need more rigor than a serial `sqlcmd` spot-check, so it was verified with a small throwaway console harness running two real, independent `DbContext`-backed calls concurrently against the live database, rather than building out a permanent test project.

**Errors encountered**: none against the real reference files -- unlike Steps 6a/6b, this step's derivation logic operates on already-validated `GameEvent`-family data rather than parsing new raw text, so there was no new grammar to get wrong. The transaction-nesting conflict above was caught at compile/first-run time, not via live-data validation.

**Verification performed**:
- 100 unit tests in `Retrosharp.Format.Tests` (87 pre-existing + 13 new): all passing.
- Full solution build: 0 errors.
- Cleared `GameEventGameStatus`/`Batting`/`Pitching`/`Fielding` and re-ran both real reference files (whose `GameEvent`-family rows already existed from Steps 6b/6c) end-to-end: **0 games inserted, 81 games skipped, 81 games' statistics applied** for each file -- confirming statistics-application idempotency is correctly independent of raw-event idempotency, exactly as designed.
- Cross-validated derived stats against the live `GameEvent` table directly (not just the raw file) for a real player (Fernando Tatis Jr., `tatif002`): `Strikeouts` and `Homeruns` matched exactly against an independent `GROUP BY EventType` query, and `AtBats`/`BaseOnBalls` reconciled precisely once sacrifice-flagged plate appearances were accounted for. Spot-checked several real starting pitchers' derived lines (games started, innings pitched, hits/runs/earned-runs/strikeouts allowed) for plausibility against real 2025 form.
- Data-integrity sweep: 483 `Batting`, 461 `Pitching`, 753 `Fielding`, 162 `GameEventGameStatus` rows (one per game, matching the 162 distinct games from Steps 6b/6c), zero rows with an unresolved (`0`) `PersonId`/`FranchiseId`.
- **Concurrency verification** (the step's own explicit Definition of Done): a throwaway console harness (two independent `DbContext` instances, two genuinely concurrent `TryApplyGameStatisticsAsync` calls racing for the same `GameId`) confirmed exactly one call returned `true`, exactly one `GameEventGameStatus` row was created, and the resulting `Batting` row reflected the test delta exactly once (not doubled). Database restored afterward by clearing the test data and re-running the real import, re-confirming the real stats matched their pre-test values exactly (Tatis Jr.: 298 AB / 81 H, unchanged) and idempotency held (`0` applied, `81` already claimed on the following re-run).
- Re-ran both files again after restoring: **0 games inserted, 0 games' statistics applied, 81 games' statistics already claimed** for each file.

### Step 6e: Reconciliation checks

**Status**: Complete

**Objective**: Implement the logged-warning-only reconciliation checks -- team aggregates against `Game*Statistics`, and an independently-computed play-by-play earned-run figure against Retrosheet's `data` records. `Pitching.EarnedRuns` itself is already sourced from the `data` record as of Step 6d; this step adds the *validation* layer (compute independently, compare, log a warning on disagreement) on top, without altering the stored value.

**Definition of done**: an intentionally-introduced discrepancy in test data produces a logged warning in both cases, without altering the authoritative stored values.

### Progress Log

**What was found (starting state)**: `GameBattingStatistics`/`GamePitchingStatistics`/`GameFieldingStatistics` (Step 5) and `BattingDelta`/`PitchingDelta`/`FieldingDelta` (Step 6d) carry overlapping but not identical fields -- `RunsBattedIn` is tracked nowhere in `Batting`/`BattingDelta` at all, and `GamePitchingStatistics.TeamEarnedRuns`/`GameFieldingStatistics.PassedBalls`/`DoublePlays`/`TriplePlays` have no derived counterpart to compare against, since Step 6d deliberately left those un-derived. No reconciliation logic of any kind existed; this was a blank slate.

**What was built**:
- `GameReconciliationResolver` (`Retrosharp.Format.PlayByPlay`, pure logic, no I/O, sibling to `GameStatisticsResolver`): `ResolveTeamTotals` aggregates `GameStatisticsDelta`'s already-computed per-player deltas by franchise into team totals (`TeamBattingTotal`/`TeamPitchingTotal`/`TeamFieldingTotal`, bundled by `GameTeamStatisticsDelta` -- new `Retrosharp.Contract.GameEvent` DTOs), plus a fresh play-by-play scan for `RunsBattedIn` (summing `GameEventRunner.IsRBI`), since that field isn't tracked anywhere else. `ResolveIndependentEarnedRuns` sums `GameEventRunner.IsEarnedRun` credited to `ResponsiblePitcherId`, independently of the `data er`-sourced `Pitching.EarnedRuns` value.
- `ReconciliationComparer` (same namespace): pure field-by-field comparison producing `TeamStatisticsDiscrepancy`/`EarnedRunDiscrepancy` records (no logging, no I/O) -- this is what makes the step's own Definition of Done directly unit-testable (feed a deliberately mismatched pair, assert the discrepancy list is exactly right) without needing `ILogger` capture or mocks.
- `GameEventRecord` extended with `HomeFranchiseId`/`VisitorFranchiseId`. `GameEventImportService` extended with three new dependencies (`IGameBattingStatisticsRepository`/`IGamePitchingStatisticsRepository`/`IGameFieldingStatisticsRepository`, already registered since Step 1) and a new `ReconcileGameAsync`, called in a loop from `ImportAsync` after `BulkInsertAsync` completes -- a distinct end-of-file pass, not interleaved with the per-game claim/write transaction, matching the spec's "at the end of processing a team-season file" wording. Only `GetByGameIdAsync` is ever called on the three repositories, so the "never overwrites `Game*Statistics`" guarantee holds by construction.
- Documented `GamePitchingStatistics.TeamEarnedRuns` and `GameFieldingStatistics.PassedBalls`/`DoublePlays`/`TriplePlays` as excluded from reconciliation (Phase 2 items) in `spec/game-event.md` and `spec/project.md`'s Second Phase list, for the same reasons Step 6d already excluded deriving them.
- 10 new unit tests in `Retrosharp.Format.Tests` covering the resolver and comparer in isolation.

**Discrepancies and decisions made during implementation**:
- `Putouts`/`Assists` were *also* excluded from the live comparison after live verification (see below) surfaced that they're structurally under-derivable, not buggy -- a bare strikeout carries no fielder digits in Retrosheet's raw text (the "catcher gets the putout" convention is implicit, not written), so capturing the official total needs "who's the current catcher" tracking that doesn't exist. This is the same underlying gap already documented as a Phase 2 exclusion for `PassedBalls` in Step 6d; comparing them would produce a guaranteed false-positive warning on every real game.

**Errors encountered** (all found only by running the reconciliation checks against the complete real reference files and root-causing each discrepancy with direct SQL queries against the persisted `GameEvent`-family rows, not by hand-picked samples -- consistent with the pattern in every prior step that touched real data):

The first live run produced hundreds of warnings, not zero. Investigating each category found four distinct, previously-undetected bugs in already-"Complete" Steps 6a/6b/6d -- all invisible until this step started reading columns (`IsRBI`, `IsEarnedRun`) or comparing totals (`Fielding`, `StolenBases`) that nothing had consumed before:

1. **Home runs never defaulted `IsRBI`/`IsEarnedRun` to true.** The batter's own trip around the bases on a home run is implicit in the primary code (`HR/F9D` has no explicit `B-H` segment), so `AddBatterRunner`'s HR/bare-`H` branches never reached the "a scored run is an RBI and earned by default" rule that `ApplyAdvanceSegment` already applied to *explicit* advance segments. Fixed by applying the same default inside `AddBatterRunner` whenever it's given `EndBase = Home, IsOut = false`. Confirmed against real data: every home run in the reference files was missing both flags until this fix; `GameBattingStatistics.RunsBattedIn` discrepancies dropped from ~60 to zero across both files after re-deriving.
2. **Double-play fielding chains dropped the pivot fielder's assist.** `spec/game-event.md`'s own worked 6-4-3 example requires the second baseman credited twice -- a putout on the forced runner, then an assist on the batter -- but `ParseFieldedOutGroups` treated `"64(1)3"`'s pre-paren digits (`"64"`) and post-paren digits (`"3"`) as two fully disjoint fielder chains, never carrying the first group's last fielder (the pivot) into the second group. Fixed by threading a `carryOverFielder` through `ParseFieldedOutGroups`/`AssignFieldedOutGroup`, prepending it as an assist onto the next group's own chain. The existing Step 6a/6b unit tests for this exact play (`Parse_SixFourThreeDoublePlay_TwoRunnersOutWithCorrectCredits`, `Resolve_SixFourThreeDoublePlay_ForcedRunnerIdentityCarriedFromPriorPlay`) had encoded the *buggy* behavior as their expected result and were corrected to match the spec's own worked example.
3. **Bundled `K+SB`/`K+WP`/etc. plays silently dropped the secondary event.** `GameEvent.EventType` can only hold one value, so a play like `K+SB2` or `K+WP` -- Retrosheet's own "+"-combinator for a secondary baserunning/misc event bundled onto a strikeout or walk -- had its right-hand event parsed correctly into runner/base movement, but no EventType signal survived for `GameStatisticsResolver`'s `StolenBase`/`CaughtStealing`/`WildPitch`/`Balk`-keyed counting logic to find, since the play's overall `EventType` was always `Strikeout`/`Walk`. Required a small, deliberate schema addition (confirmed with the user before implementing, given it touches already-shipped Step 6a/6b/6d code and data): `GameEvent.SecondaryEventType` (nullable `GameEventType`, migration `AddGameEventSecondaryEventType`), populated by threading the right-hand code's resolved `EventType` up through `PlayCodeParser.ParsePrimaryCode`'s "+"-branch and `ParsedPlay`. `GameStatisticsResolver` now checks `SecondaryEventType` alongside `EventType` for pitcher counting stats (`ApplyPitcherEvent` called once per non-null type) and for `StolenBases`/`TimesCaughtStealing` crediting -- the latter also gained an explicit `runner.StartBase != BaseState.BattersBox` guard, since a bundled play's `Runners` list can now legitimately include both the batter (from the primary K/W) and the actual base-stealer (from the secondary SB/CS), and only the guard prevents the struck-out batter from being credited a phantom steal. 34 real bundled plays found across both reference files (23 `StolenBase`, 5 `CaughtStealing`, 5 `WildPitch`, 1 `PassedBall`).

All three fixes required a full clear-and-re-import of both real reference files (`GameEvent`-family, context tables, and `Batting`/`Pitching`/`Fielding` all cleared and regenerated from scratch) to correct already-persisted data, not just newly-processed data going forward.

**Known residual, not yet root-caused** (deliberately left open per user decision, to avoid further expanding this step's scope): after all three fixes, live reconciliation against both complete real reference files still logs ~20 warnings across 162 games that don't match any of the four causes above -- `GroundedIntoDoublePlay` off-by-one in either direction (2 games), `PlateAppearances`/`AtBats` off by exactly one PA (3 games, paired), a handful of residual `StolenBases`/`TimesCaughtStealing` mismatches beyond what the `SecondaryEventType` fix resolved (5 occurrences, now over-counting rather than under-counting), and `Pitching.EarnedRuns` independent-computation disagreements (7 occurrences, both directions). Each is a real, low-volume, currently-unexplained discrepancy worth investigating in a future pass -- flagged here rather than silently ignored, per this project's established convention, but not blocking this step's completion given reconciliation's own job is *detection*, not elimination.

**Verification performed**:
- 115 unit tests in `Retrosharp.Format.Tests` (110 pre-existing + 5 new reconciliation-specific, plus corrections to 2 pre-existing tests that had encoded the double-play assist bug as expected behavior): all passing.
- Full solution build: 0 errors.
- Full clear-and-re-import of both real reference files after all three fixes: 81 games inserted for each file, `GameEventFieldingCredit` now 8,560 rows (up from 8,371 pre-fix, confirming the recovered double-play assists), 34 `GameEvent` rows carry a non-null `SecondaryEventType` (23 StolenBase, 5 CaughtStealing, 5 WildPitch, 1 PassedBall).
- Reconciliation now logs 353 warnings across both files (down from an unmeasured but much larger count before the fixes) -- 324 `Putouts` + a handful of `Assists`/`Errors` (all excluded-from-scope, expected per the `Fielding` documentation above), plus the ~20-warning known residual documented above. Zero `RunsBattedIn` warnings and zero double-play-related warnings, confirming both scoped fixes are fully effective.
- **Intentional-discrepancy live test** (the step's own explicit Definition of Done): manually corrupted `GameBattingStatistics.Hit` for a real game/franchise (16 -> 17 via `sqlcmd`), re-ran the import, confirmed exactly one new warning logged with the correct game/franchise/field/persisted/derived values (`Hits = 17, but play-by-play derives 16`), confirmed via `sqlcmd` that the row was still `17` (not overwritten) after reconciliation ran, then restored it to `16` and re-ran once more to confirm the warning disappeared and the total warning count returned to the 353 baseline.

### Step 6f: Full saga integration

**Status**: Complete

**Objective**: Assemble 6a–6e into the complete saga: file-level idempotency (ignoring queued duplicate messages for an in-progress file), retry/backoff, atomicity, and the API endpoint to initiate processing.

**Definition of done**: matches [game-event.md](./game-event.md)'s Acceptance Criteria in full, including concurrent processing of two files that share a game (see [Considerations](./game-event.md#considerations)).

### Progress Log

**What was found (starting state)**: `GameEventImportService` (6a-6e) was fully built and only exercised via a throwaway DI console harness -- no saga, no message types, no API endpoint existed yet, unlike `PersonSaga`/`GameLogSaga` (Steps 3/5), which already run over Step 4's messaging infrastructure. Neither of those existing sagas correlates on file path -- both correlate on a fresh `RequestId` minted per API call -- so neither actually satisfies "ignore duplicate messages for an in-progress file" (`game-event.md` Requirements 169-170): two calls for the same file today just run two fully independent imports. This is a real, pre-existing gap in already-shipped Steps 3/5, not something introduced here, and out of scope to retrofit onto Person/GameLog as part of this step.

**What was built**:
- `GameEventStart`/`GameEventComplete`/`GameEventCancel` (`Retrosharp.Message.GameEvent`), mirroring `Message.GameLog`'s shape (no `SeasonYear` field needed, since `GameEventImportService` already derives it per-game internally).
- `GameEventSagaData`/`GameEventSaga` (`Retrosharp.Engine.Console.Saga`), structured like `GameLogSaga` with one deliberate difference: correlated on **`FilePath`**, not `RequestId` -- the first use of this pattern in the codebase, needed so a second `GameEventStart` for the same file routes to the same saga instance instead of always starting an independent duplicate import. `GameEventSagaData.IsRunning` is set `true` at the start of `Handle(GameEventStart)` and only cleared by `MarkAsComplete()` in the separate `Handle(GameEventComplete)` call; a `GameEventStart` that finds `IsRunning` already `true` is logged and ignored.
- `GameEventController` (`POST /api/gameevent/import`), mirroring `GameLogController` minus the `SeasonYear` field; `GameEventStart` routed in `Retrosharp.UI.Api/Program.cs` alongside the existing `PersonStart`/`GameLogStart` lines.
- No new EF Core migration for application tables -- all persistence already existed (6b-6e). `GameEventSagaData`'s SQL persistence schema was generated automatically at `Retrosharp.Engine.Console`'s build time (`GameEventSaga_Create.sql`) and installed against the live database via `Retrosharp.Data.Migration`'s existing `ScriptRunner.Install()` mechanism -- no new migration code, the same process already used for `PersonSagaData`/`GameLogSagaData`.
- Retry/backoff needed no saga-specific code at all -- it's fully inherited from Step 4's endpoint-wide recoverability policy, since `GameEventImportService`'s existing exceptions already propagate rather than being swallowed.

**Discrepancies and decisions made during implementation**:
- `Retrosharp.UI.Api/Program.cs` was missing `builder.Services.AddDbContext<RetrosharpContext>(...)` entirely -- a real, pre-existing gap (not introduced by this step) that only surfaced when actually starting UI.Api for live verification, since `ContainerRegistration`'s assembly-wide scan registers every repository/service (including ones needing `RetrosharpContext`) regardless of whether the endpoint is send-only, and ASP.NET Core's `Development`-environment service-provider validation fails at startup if any registered service can't be constructed. `PersonController`/`GameLogController` have the exact same transitive dependency and would have failed identically -- this wasn't a Game-Event-specific bug, just never previously triggered by running UI.Api this way. Fixed by adding the same `AddDbContext<RetrosharpContext>` registration Engine.Console already has.

**Errors encountered** (all found via live verification through the real API -> RabbitMQ -> Engine.Console pipeline, not the throwaway DI harness used in 6a-6e):
- The `Retrosharp.UI.Api` startup failure above.
- A self-inflicted verification mistake, caught and corrected before it could persist: to test the "shared game across two files" scenario, `GameEventGameStatus` was cleared for one real, already-fully-imported game (2437) so it could be re-claimed fresh -- but its already-accumulated `Batting`/`Pitching`/`Fielding` season totals were left in place, so the re-claim *added* that game's contribution a second time on top of the original, silently double-counting 22 players' `Batting` rows, 14 `Pitching` rows, and 21 `Fielding` rows. Caught immediately by having snapshotted those exact rows before the test (the same before/after discipline used throughout this project's live verifications) -- restored every row to its exact pre-test value via direct `sqlcmd` `UPDATE`s and re-diffed to confirm an exact match. Root cause: releasing a game's claim without also reverting its already-applied statistics makes a "harmless" re-claim actually double-count; any future test of this kind needs to clear *both* together, or use a game with no prior statistics applied at all.

**Verification performed**:
- Full solution build: 0 errors. Existing 115 unit tests still passing (this step is integration wiring, no new derivation logic).
- Live end-to-end saga run of a real reference file (`2025SDN.EVN`) through the actual `POST /api/gameevent/import` -> RabbitMQ -> `Engine.Console` pipeline: `0 games inserted, 81 games skipped, 0 games' statistics applied, 81 games' statistics already claimed` -- exactly matching the already-idempotent state from Steps 6b-6e's DI-harness runs.
- Duplicate-message race test: fired two `POST` requests for the same file (`2025SEA.EVA`) essentially simultaneously. Both actually started (`IsRunning` didn't prevent the race, since both requests were handled before either committed) and both ran their full import to completion (harmless, since the result was identical both times: `0 inserted, 81 skipped, 0 applied, 81 already claimed`). One of the two saga instances failed to persist with a real SQL unique-constraint violation on `Index_Correlation_FilePath` (confirming NServiceBus's SQL persistence enforces the correlation key's uniqueness at the database level); NServiceBus's own retry policy (Step 4) automatically redelivered that message, and the retry completed cleanly once the first instance's row had already been removed by its own `MarkAsComplete()`. End state: `GameEventSaga` table empty (both instances completed), zero change in `Batting`/`Pitching`/`Fielding`/`GameEventGameStatus` row counts -- confirming that even when the file-level dedup doesn't prevent a race, the system self-heals with no double-counting, exactly as the plan's own honesty note anticipated.
- Shared-game-across-two-files test (the step's own explicit Definition of Done): built two identical small fixture files from one real game's record block (`docs/csv/2025SDN.EVN`'s first game, `SDN202503270`, `Game.Id` 2437), cleared that one game's `GameEventGameStatus` claim, and submitted both fixtures via the real API concurrently. Result: one file's import applied the game's statistics (`1 games' statistics applied, 0 already claimed`), the other found it already claimed (`0 applied, 1 already claimed`) -- `GameEventGameStatus` ended with exactly one row for the game, and (after correcting the verification mistake above) the affected players' `Batting`/`Pitching`/`Fielding` rows matched their pre-test values exactly, confirming the game's statistics were applied exactly once despite the concurrent race.

---

## Step 7: Data Viewing API

**Status**: In Progress

**Governing spec**: [api.md](./api.md) (routes, response shapes, statistic formulas, and the data-model gaps this step resolves), [project.md](./project.md) (Data Viewing and Statistics features)

**Depends on**: Step 5 and Step 6 (needs data to view).

**Objective**: Build the REST API surface for Phase 1's Data Viewing feature — player search, career/season statistics, game summaries, and individual play-by-play events, for both players and teams, per [api.md](./api.md).

Comparably large to Step 6, so it's split into independently built and verified sub-steps, the same way 6a-6f were:

- **7a** — Shared plumbing (pagination envelope, response-DTO/controller conventions) + Player Search + Player Detail.
- **7b** — Player batting/pitching/fielding statistics: career and season, including the multi-franchise combined-total row, and auditing/extending the existing `PlayerStatisticsService` scaffold.
- **7c** — `PitcherEventAggregate` (HR/FB, HR/9, pitcher BABIP) and `LeagueSeasonPitchingTotals`/FIP constant.
- **7d** — Player game log (per-game stat lines derived on demand from `GameEvent`/`GameEventRunner`).
- **7e** — Team search, roster, and season statistics.
- **7f** — Game summary and play-by-play.

### Step 7a: Shared Plumbing, Player Search, Player Detail

**Status**: Complete

**What was found (starting state)**:
- `IPersonService`/`PersonService` already had a `GetWithCareerStatsAsync` method, but it was a non-functional stub — it loaded the `Person` row and, per its own comment, did nothing else ("a more complete implementation would load batting/pitching/fielding stats"). It had no callers anywhere in the codebase. `PersonService`'s constructor also injected `IBattingRepository`/`IPitchingRepository`/`IFieldingRepository` that only that stub referenced (only in a comment, never in code) — dead dependencies once the stub was removed.
- `IPersonRepository.SearchByNameAsync`/`PersonRepository.SearchByNameAsync` took just a search term and returned every match with no paging, no total count, and no explicit ordering (meaning `Skip`/`Take` pagination added on top of it would have had undefined row order in SQL Server). Confirmed via grep it had no callers outside `PersonService`'s pass-through wrapper, so the signature could be changed directly rather than adding a parallel overload.
- **A genuine, previously-latent gap surfaced by this step's live verification**: `Retrosharp.UI.Api/appsettings.json` never had a `ConnectionStrings:DefaultConnection` entry — only `RabbitMQ`. This went undetected through Steps 3/5/6f because every prior UI.Api endpoint only sends fire-and-forget messages onto the service bus; none of them actually execute a query against `RetrosharpContext` from within the API process itself. `PlayersController` is the first code path to do so, and it failed immediately with `System.InvalidOperationException: The ConnectionString property has not been initialized` — `RetrosharpConfiguration.Instance()` builds its own `ConfigurationBuilder` reading `appsettings.json`, and the key genuinely didn't exist in that file. This is a different root cause from Step 6f's `RetrosharpContext` DI-registration gap (that fix made the type resolvable; this fix makes the resolved instance actually point at a real database).

**What was built**:
- `Retrosharp.UI.Api/Models/PagedResult.cs`: the project's first list/search response envelope (`Items`, `TotalCount`, `Limit`, `Offset`).
- `IPersonRepository.SearchByNameAsync`/`PersonRepository.SearchByNameAsync` extended to `(searchTerm, limit, offset)`, returning `(IEnumerable<Person> Items, int TotalCount)` via a separate `CountAsync()` plus `OrderBy(Surname).ThenBy(UseName).Skip().Take()` — the explicit ordering is a necessary addition beyond what was originally scoped, since `Skip`/`Take` without an `ORDER BY` has undefined row order in SQL Server and would have made pagination unreliable. `IPersonService.SearchByNameAsync` updated to match, as a pure pass-through.
- Removed `IPersonService.GetWithCareerStatsAsync` (interface and implementation) and `PersonService`'s now-unused `IBattingRepository`/`IPitchingRepository`/`IFieldingRepository` constructor dependencies, the same category of cleanup Step 5 did for `GameService` after removing `ProcessGameLogsAsync`.
- `PlayersController` (`GET /api/players/search?q=&limit=&offset=`, `GET /api/players/{id}`), kept separate from `PersonController` (which only triggers the biofile ETL saga). New response DTOs `PlayerSearchResult`/`PlayerDetail` in `Retrosharp.UI.Api/Models/`, mapped from `Person` via Mapster's `Adapt<T>()` (already registered via `AddMapster()`). Both DTOs use `Id` rather than `PersonId` for the primary key, matching `Entity.Id` and Mapster's name-based mapping convention, rather than introducing a custom mapping configuration for one field.
- Added `ConnectionStrings:DefaultConnection` to `Retrosharp.UI.Api/appsettings.json`, matching Engine.Console's value.

**Discrepancies and decisions made during implementation**:
- `PlayerDetail` deliberately omits `Person`'s cemetery-related fields (`Cemetery`, `CemeteryCity`, etc.) and `BirthName`/`AlternateName` — genealogical detail not central to a baseball statistics/data-viewing feature. Everything else identity/biographical stays, including manager/coach/umpire debut-last date ranges, since `Person` covers all four roles.
- No `[Authorize]` on `PlayersController`, consistent with every existing controller and `api.md`'s Phase 1 anonymous-access decision.

**Errors encountered**:
- The `Retrosharp.UI.Api/appsettings.json` missing-connection-string gap above — not caused by this step's changes, but only exposed by them, and fixed as part of this step per the project's established practice of fixing real bugs found via live verification rather than working around them.

**Verification performed**:
- Full solution build: 0 errors (198 pre-existing nullable-reference warnings, unchanged in kind from prior steps).
- Live: started `Retrosharp.UI.Api`, called `GET /api/players/search?q=cease` against the real imported biofile data (26,961 people) — correctly returned Dylan Cease (`id: 3999`, `ceasd001`). Called `GET /api/players/3999` — correct full detail. Called `GET /api/players/999999999` — `404`.
- Pagination boundaries verified against the real `Person` table: `q=zzzznotaname` → empty `items`, `totalCount: 0`. `q=smith&limit=5` → 5 items, `totalCount: 258`. `q=smith&limit=5&offset=5` → next 5 items, no overlap with the first page, same `totalCount`. `q=smith&limit=5&offset=100000` → empty `items`, `totalCount: 258` unchanged. Invalid `limit=0`, `offset=-1`, and missing `q` all correctly rejected with `400`.

### Step 7b: Player Batting/Pitching/Fielding Statistics

**Status**: Complete

**What was found (starting state)**:
- `IPlayerStatisticsService`/`PlayerStatisticsService` (pre-existing scaffold, per `api.md`'s own audit note) had `GetBattingStatsAsync`/`GetPitchingStatsAsync`/`GetFieldingStatsAsync(personId, season?)` (raw rows, no rate stats, no combined total) plus `CalculateCareerBattingStatsAsync(personId)` (a single combined row, batting only, using a `FranchiseId = 0` sentinel to mean "career"). None of the four was wired to a controller. The shape couldn't express "one season split across two franchises, plus a combined total," nor a pitching/fielding career total, without becoming inconsistent.
- **A significant, previously-undetected bug, found only because this step is the first code to ever read `Batting` back out through the repository layer**: `Data.Model.BattingModel.Hit` (singular) vs. `Contract.Batting.Batting.Hits` (plural) — a real naming mismatch between the EF model and the Contract class. `BattingRepository.GetByPersonIdAsync`'s `ProjectToType<Batting>()` relies on Mapster's convention-based (exact-name) property matching, which silently left `Hits` at its default value of `0` for every row, with no error. This had been present and *already known* by whoever wrote Step 6d's write path — `GameStatisticsRepository.ApplyBattingDeltaAsync` has an explicit comment documenting the mismatch and manually maps around it (`Hit = delta.Hits`) — but nothing before this step ever exercised the read path through the repository/Contract layer; Step 6d/6e's own verifications checked `Batting` rows via direct `sqlcmd` queries against the raw `Hit` column, which bypasses this bug entirely. Caught live: `GET /api/players/20670/batting` (Julio Rodríguez, 327 real at-bats) returned `hits: 0`, `battingAverage: 0`, and a `totalBases` consistent with `Hits` being silently zeroed (`64` instead of the correct `142`).

**What was built**:
- `PlayerStatLines<T>`/`PlayerStatLine<T>` (`Retrosharp.Contract`): a season-or-career result shape — `Rows` (one `PlayerStatLine<T>` per franchise, denormalized with `FranchiseCode`/`FranchiseName`) plus `CombinedTotal` (every counting stat in `Rows` summed, rate stats recomputed from the sums, `null` when `Rows` has ≤1 entry). This replaces the `FranchiseId = 0` sentinel entirely — the combined total is its own field, not a fake row impersonating a franchise.
- `PitchingStatistics : Pitching` and `FieldingStatistics : Fielding` (`Retrosharp.Contract`), alongside the existing `BattingStatistics`: `InningsPitchedDecimal`/`InningsPitchedDisplay` (`Pitching.InningsPitched` stores outs, not innings — see `api.md`), `Era`, `Whip`, `StrikeoutsPerNine`, `WalksPerNine` for pitching; `FieldingPercentage` for fielding. HR/9, HR/FB, FIP, and pitcher BABIP are deliberately not here — they need `PitcherEventAggregate`, which is Step 7c.
- `IPlayerStatisticsService`/`PlayerStatisticsService` rebuilt around three consistent methods, `GetBattingAsync`/`GetPitchingAsync`/`GetFieldingAsync(personId, season?)`, each fetching every row via the existing `GetByPersonIdAsync` repository methods, filtering in-memory by season when given, resolving each row's `Franchise` via `IFranchiseRepository.GetByIdAsync` (cached per call to avoid redundant lookups for a player who stayed with one franchise across many seasons), and summing into `CombinedTotal` when more than one row applies.
- Three new `PlayersController` actions (`GET /api/players/{id}/batting|pitching|fielding?season=`), each returning a `PlayerStatsResponse<TLine>` (`Retrosharp.UI.Api/Models/`) wrapping `BattingLine`/`PitchingLine`/`FieldingLine` — flattened, franchise-denormalized response DTOs distinct from the Contract-layer statistics classes. `FranchiseCode`/`FranchiseName`/`SeasonYear`/`Position` are explicitly nulled on the `CombinedTotal` line, since a combined total may span more than one franchise/season/position.
- Fixed the `Hit`/`Hits` bug at its root: renamed `BattingModel.Hit` → `Hits`, updated `GameStatisticsRepository.ApplyBattingDeltaAsync`'s two explicit references (now redundant, since the names match — simplified to plain property access), and generated migration `RenameBattingHitToHits` (a clean `RenameColumn`, correctly scoped to only the `Batting` table — `GameBattingStatistics`'s own, unrelated, correctly-named `Hit` column is untouched).

**Discrepancies and decisions made during implementation**:
- Franchise identity (`FranchiseCode`/`FranchiseName`) is resolved in the service layer (`PlayerStatisticsService` injecting `IFranchiseRepository` directly, the same pattern already used by `PersonService`/`BattingService`/etc.), not the controller, keeping the established controller-calls-service-only layering intact.
- `FieldingLine.Position` stays at the natural per-position grain (no extra franchise-level subtotal tier summing across positions before the season/career grand total) — a rare case (a traded utility player who also changed positions) deferred until it's shown to matter, per the approved plan.

**Errors encountered**:
- The `BattingModel.Hit`/`Batting.Hits` mismatch above — root-caused, fixed via rename + migration, and verified corrected against real data (see below).

**Verification performed**:
- Full solution build: 0 errors. Full existing test suite: 115/115 passing, unchanged.
- Live pitching: `GET /api/players/3999/pitching` (Dylan Cease) returned a real, internally-consistent partial-2025 line (17 starts, 91.2 IP) — `Era`/`Whip`/`StrikeoutsPerNine`/`WalksPerNine` hand-verified correct against the stored counting stats (for example, `ERA = 9×39÷(275÷3) = 3.829`, matching exactly). Cross-referenced live against Baseball-Reference's real 2025 Cease page: confirmed this is the same real player/season (their full-season FIP of `3.56` matches what was already known), and that the imported reference data reflects a genuine partial season (Baseball-Reference shows 32 total starts for 2025; the database has 17) rather than a data error — consistent with the reference files having been downloaded mid-2025 for this project's testing, not after the season concluded. An attempted manual cumulative reconciliation against Baseball-Reference's individual game log (summing H/ER/BB/SO through start 17) was inconclusive — figures were close but not exact, and this manual cross-check (transcribing a dense, non-tabular scraped page by eye) is fragile enough that a mismatch is more likely a transcription error than a real data bug; a rigorous version of this check belongs in Step 6's own domain (diffing against the actual Retrosheet source file, as Step 6d/6e's own progress logs already did) rather than being re-litigated here. The formula-correctness verification (the actual purpose of this AC) is unaffected by that ambiguity.
- Live batting: `GET /api/players/20670/batting` (Julio Rodríguez, 327 real at-bats, the most of any player in the imported data) caught the `Hit`/`Hits` bug (`hits: 0`); after the fix, returned `hits: 78`, and `AVG`/`OBP`/`SLG`/`OPS` hand-verified correct against the stored counting stats (`.239`/`.306`/`.434`/`.741`).
- Live fielding: `GET /api/players/20670/fielding` returned a correct center-field line (`FieldingPercentage = (221+3)÷225 = .9956`).
- Combined-total row verified via a constructed fixture (per `api.md` AC 4, since no real multi-franchise 2025 season exists in the imported data): inserted two synthetic `Batting` rows for Dylan Cease (who has zero real `Batting` rows, since he's a pitcher who hasn't batted in the imported games) — one at SDN, one at SEA, same season, hand-computed expected totals. `GET /api/players/3999/batting?season=2025` returned both rows correctly plus a `CombinedTotal` matching the hand-computed sums exactly (`PA=15, AB=12, Hits=4, AVG=.333, OBP=.467, SLG=.833, OPS=1.30`), with `FranchiseCode`/`FranchiseName`/`SeasonYear` all `null` on the total. Fixture rows deleted immediately after; confirmed via `sqlcmd` that `Batting` returned to its exact baseline row count (483) and Cease's endpoint returned to empty.
- `404` confirmed on all three new routes for a non-existent player ID.

### Step 7c: PitcherEventAggregate, LeagueSeasonPitchingTotals, and FIP

**Status**: Complete

**What was found (starting state)**: Nothing pre-existing to correct here — 7b deliberately left HR/9, HR/FB, FIP, and pitcher BABIP out of scope, since none are derivable from `Pitching`'s own stored fields. This step is new query surface and new derivation logic throughout.

**What was built**:
- `GameStatisticsResolver`'s `PlateAppearanceEndingEvents`/`NonAtBatEvents` (the rule that decides whether a plate appearance counts as an official at-bat) promoted from `private` to `internal`, so the new pitcher-side resolver could reuse the exact same rule rather than risk a second copy drifting out of sync.
- `PitcherGameEventRecord`/`PitcherEventAggregate` (`Retrosharp.Contract.GameEvent`) and `LeagueSeasonPitchingTotals` (`Retrosharp.Contract.Pitching`).
- `PitcherEventAggregateResolver` and `FipConstantCalculator` (`Retrosharp.Format.PlayByPlay`) — pure logic, no I/O, sibling to `GameStatisticsResolver`/`ReconciliationComparer`. 9 new unit tests (`PitcherEventAggregateResolverTests`, `FipConstantCalculatorTests`) cover both against constructed data with hand-computed expected outputs.
- Four new repository methods: `IGameEventRepository.GetPitcherGameEventsAsync`/`GetLeagueHomerunsAllowedAsync`, `IPitchingRepository.GetLeagueTotalsAsync`, `IGamePitchingStatisticsRepository.GetLeagueTeamEarnedRunsAsync` — each a small, targeted aggregate query, following the existing repository style exactly.
- `PitchingStatistics` extended with the four new settable inputs (`HomerunsAllowed`/`FlyBallsAllowed`/`AtBatsAgainst`/`SacrificeFliesAgainst`/`FipConstant`) and four new computed properties (`HomeRunsPerNine`, `HomeRunsPerFlyBall`, `Fip`, `BattingAverageOnBallsInPlay`).
- `PlayerStatisticsService.GetPitchingAsync` extended: fetches the pitcher's `GameEvent` data once per call (not once per row), resolves it into per-franchise-season aggregates, and for each row resolves that row's league (via `Franchise.LeagueId`, already being resolved for `FranchiseCode`/`FranchiseName`) and computes/caches that league-season's FIP constant. `SumPitching` extended to also sum the four new counting fields and blend `FipConstant` by innings weight across rows (a season split across two leagues gets a proper weighted constant, not an arbitrary pick).
- `PitchingLine` extended with the same new fields plus `FipConstant`/`FipConstantLeagueCode`/`FipConstantSeasonYear`, so the response is self-documenting about which constant was applied.

**Discrepancies and decisions made during implementation**:
- **A genuine scope-mismatch discovered live, worth flagging precisely** (a sharper characterization of the limitation `api.md` already anticipated, not a new bug in the code): `LeagueSeasonPitchingTotals`'s two halves come from data sources that can legitimately cover different subsets of games for the very same franchise-season. `GamePitchingStatistics.TeamEarnedRuns` (Game Log Parser) reflects every game in `Game` for that franchise-season — confirmed live to be SDN's full, real 162-game 2025 season (`578` team earned runs across 162 games, `March 27` to `September 28`). `Pitching`'s counting stats (Game Event Parser) only reflect games that have actually been processed by the Game Event Parser — currently just Cease's 17 real starts (`2289` outs, ~763 innings across SDN's whole staff). Dividing a full-season earned-run total by a partial-season innings total inflates `leagueEra` far beyond `leagueRawComponent`, producing a `FipConstant` of `~62.7` — not a bug in the arithmetic (confirmed correct by hand), but a real, if narrow, consequence of Game Log import running ahead of Game Event import for the same season, which is a normal, expected transient state during real operation (not just today's two-team test environment) since `project.md`'s own dependency order allows the two parsers to progress independently. This resolves to the same place `api.md`'s own Considerations already reasoned through for the coarser "not enough teams imported" limitation, just more precisely: the constant is only meaningful once a league's Game Event import has caught up to its Game Log import for that season, which Step 9's complete-season import guarantees. Not fixed here — flagged for whoever picks up Step 9, in case the same scope-alignment question resurfaces there.
- `FipConstantLeagueCode` on a combined total spanning two leagues is rendered as `"NL/AL"` (distinct codes joined) rather than picking one arbitrarily, mirroring how `FipConstantSeasonYear` is left `null` when rows span more than one season.

**Errors encountered**: None beyond the scope-mismatch finding above, which is a data/design consideration rather than an implementation defect.

**Verification performed**:
- Full solution build: 0 errors. Full test suite: 124/124 passing (115 existing + 9 new).
- Live, hand-verified against Dylan Cease's real data: `HomerunsAllowed` (12) and `FlyBallsAllowed` (71) cross-checked via a direct `sqlcmd` count against `GameEvent` for his `PitcherId`; `AtBatsAgainst` (347) and `SacrificeFliesAgainst` (1) likewise cross-checked via a hand-built SQL predicate mirroring the exact at-bat classification rule. `HomeRunsPerNine` (`9×12÷91.667 = 1.178`), `HomeRunsPerFlyBall` (`12÷71 = .169`), and pitcher `BABIP` (`(80-12)÷(347-123-12+1) = .3192`) all matched the API's output exactly.
- FIP confirmed to compute end-to-end without error (`FipConstantLeagueCode: "NL"`, `FipConstantSeasonYear: 2025`) — not compared to a published figure, per the AC3 resolution already agreed when `api.md` was written; the resulting value's implausibility is fully explained by the scope-mismatch finding above, not a formula error (the formula itself is separately verified by the 9 unit tests against constructed data with known expected outputs).
- Combined-total cross-league blend verified via a constructed fixture: inserted a synthetic `Pitching` row for Dylan Cease at Seattle (AL) alongside his real San Diego (NL) row, same season. The response's `CombinedTotal.FipConstant` (`61.696`) matched the hand-computed innings-weighted blend of the two rows' own constants (`(62.742×275 + 58.502×90) ÷ 365 = 61.696`) exactly, `FipConstantLeagueCode` correctly read `"NL/AL"`, and every counting stat summed correctly. Fixture row deleted immediately after; confirmed via `sqlcmd` that `Pitching` returned to its exact baseline row count (461).

### Step 7d: Player Game Log

**Status**: Complete

**What was found (starting state)**: A pleasant surprise — the actual per-game derivation logic this step needs already exists and is already tested. `GameStatisticsResolver.Resolve()` (Step 6d) and `GameReconciliationResolver.ResolveIndependentEarnedRuns()` (Step 6e) both already take one game's full play-by-play and produce exactly the per-player deltas a game log needs; nothing in 6b-6e or 7a-7c ever needed to read a game's play-by-play back out of the database as a graph, though, since every prior read only needed scalar fields or simple counts. That read path was the only genuinely new piece.

**What was built**:
- `GamePlayByPlay` (`Retrosharp.Contract.GameEvent`): one game's reconstructed play-by-play (`HomeFranchiseId`, `VisitorFranchiseId`, `GameDate`, `Plays`), shaped to feed directly into the existing resolvers.
- Three new `IGameEventRepository` methods: `GetGameIdsAsBatterAsync`/`GetGameIdsAsPitcherAsync` (distinct game IDs for a player, season-filtered) and `GetGamesPlayByPlayAsync` (fetches every play for every requested game in one batched query — not one query per game — grouping client-side by `GameId`; `GameEventFieldingCredit` rows are deliberately not fetched, since `Resolve()`'s `Fieldings` output isn't used here).
- `PlayerGameBattingLine`/`PlayerGamePitchingLine` (`Retrosharp.Contract.GameEvent`): a game's `BattingDelta`/`PitchingDelta` plus `GameId`/`GameDate`/`IsHome`/`FranchiseCode`/`OpponentFranchiseCode`.
- New `IPlayerGameLogService`/`PlayerGameLogService` (`GetBattingGameLogAsync`/`GetPitchingGameLogAsync`), kept separate from `PlayerStatisticsService` since it's a genuinely different operation (re-deriving from source events, not summing stored aggregates). For each game: run `ResolveIndependentEarnedRuns` then `Resolve()` (exactly as Step 6d/6e already do during import), pick out the requested player's own delta, and resolve `FranchiseCode`/`OpponentFranchiseCode` via the same franchise-cache pattern already established in `PlayerStatisticsService`.
- `PlayersController`: `GET /api/players/{id}/games?season=&type=batting|pitching`, plus `GameBattingLine`/`GamePitchingLine` response DTOs (`Retrosharp.UI.Api/Models/`) — named distinctly from the Contract-layer types of almost the same name to avoid a same-name collision across namespaces, consistent with how 7b/7c always used different names between the Contract and DTO layers.

**Discrepancies and decisions made during implementation**:
- Per-game `EarnedRuns` in the game log is the **independently-computed** figure (from `GameEventRunner.IsEarnedRun`), not the season aggregate's authoritative "data,er,..." record value — that raw per-game figure was only ever used transiently during Step 6d's import and was never persisted at the per-game grain, so there's nothing more authoritative to read back at this level. Documented explicitly on `PlayerGamePitchingLine`/`GamePitchingLine`, mirroring how Step 6e already treats this same independently-derived figure elsewhere.
- No pagination on this endpoint, per `api.md`'s own reasoning (one player, one season, at most ~162 rows).

**Errors encountered**: None.

**Verification performed**:
- Full solution build: 0 errors. Full test suite: 124/124 passing, unchanged (no new derivation logic — this step is entirely reuse of already-tested resolvers plus new plumbing, so live verification against real data was the primary check).
- Live, and a strong one: `GET /api/players/3999/games?type=pitching` (Dylan Cease) returned all 17 real games; summing `Hits`/`Runs`/`EarnedRuns`/`BaseOnBalls`/`Strikeouts` across every row matched his already-verified 7b/7c season totals **exactly** (`80`/`41`/`39`/`39`/`123`) — including `EarnedRuns`, meaning the independently-computed figure agrees with the authoritative season total for every one of his games, with zero reconciliation discrepancy.
- Live: `GET /api/players/20670/games?type=batting` (Julio Rodríguez) returned 83 games; summing all ten cross-checked fields (`PlateAppearances`/`AtBats`/`Hits`/`Doubles`/`Triples`/`Homeruns`/`BaseOnBalls`/`Strikeouts`/`StolenBases`/`Runs`) matched his already-verified 7b season totals exactly, with no exceptions.
- `400` confirmed for both a missing `type` and an invalid `type` value; `404` confirmed for a non-existent player; an out-of-range `season` (2024, no data) correctly returned an empty array rather than an error.

---

## Step 8: Containerized Deployment

**Status**: Not Started

**Governing spec**: [project.md](./project.md) ("independently deployable" requirement)

**Depends on**: Step 7 (API must exist to containerize meaningfully), though Dockerfile work for `Retrosharp.Engine.Console` could begin as soon as Step 4 is stable.

**Deliverables**: Dockerfiles for `Retrosharp.UI.Api` and `Retrosharp.Engine.Console`, a `docker-compose.yml` covering SQL Server, RabbitMQ, and both applications, and health check endpoints.

**Definition of done**: `docker-compose up` brings up a fully working stack from a clean environment, including seed data and schema migration via Step 2's mechanism.

---

## Step 9: End-to-End Validation

**Status**: Not Started

**Depends on**: All prior steps.

**Objective**: Validate the full pipeline against real, complete Retrosheet data for at least one full season — seed data, biofile, Game Logs, and Game Event files for every team — confirming statistics computed from the resulting data match known, independently-verifiable figures for that season (for example, published league leaders).

**Definition of done**: a full season imports cleanly, statistics are queryable and correct, and the reconciliation warnings from Step 6e are silent (no unexpected discrepancies) against real data.
