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
- 7 new unit tests in `Retrosharp.Format.Tests` covering all five mapping/parsing bugs (including a home-lineup regression test and the blank-attendance case), using real rows pulled directly from `gl2025.txt`.
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

---

## Step 6: Game Event Parser

This is the largest and highest-risk step, and the one place where the plan intentionally breaks work into finer sub-steps than any other stage — the play-code grammar is intricate enough that building it as one unit would work against the "one at a time, correctness first" approach this plan is for.

**Governing spec**: [game-event.md](./game-event.md)

**Depends on**: Step 1, Step 2, Step 3, Step 4, and Step 5 (needs `Game` to already exist).

### Step 6a: Play-code grammar parser (no database dependency)

**Status**: Not Started

**Objective**: Build and unit-test the raw parsing logic — turning a Retrosheet play-code string (for example, `S8.1-3;2-H`) into an in-memory structured representation (event type, batted ball type, runner advances, fielding sequence, RBI/error/unearned-run annotations) — entirely independent of the database or saga infrastructure.

**Deliverables**: a pure parsing library, tested against a broad set of real play codes pulled from actual Retrosheet files, covering hits, outs, strikeouts, walks, stolen bases, double/triple plays, and rundowns.

**Definition of done**: the parser correctly decomposes a representative sample of real play codes into structured objects, verified by unit tests, with no I/O of any kind.

### Step 6b: `GameEvent`, `GameEventRunner`, `GameEventFieldingCredit` persistence

**Status**: Not Started

**Objective**: Wire the Step 6a parser output into the database, populating `GameEvent`, `GameEventRunner`, and `GameEventFieldingCredit` for a single game, per the [Data Model](./game-event.md#data-model).

**Definition of done**: parsing one game's play-by-play produces exactly the rows the Data Model's worked examples (5-3 ground out, 6-4-3 double play, 8-6-2 relay, rundown) describe, verified against real games containing those play types.

### Step 6c: Context records

**Status**: Not Started

**Objective**: Parse and persist `GameSubstitution`, `GameAdjustment`, and `GameComment` records.

**Definition of done**: substitutions, handedness/batting-order adjustments, and commentary from a game are captured without being conflated into `GameEvent`.

### Step 6d: `Batting`/`Pitching`/`Fielding` derivation and `GameEventGameStatus`

**Status**: Not Started

**Objective**: Implement the atomic per-game claim via `GameEventGameStatus`, and derive/apply `Batting`, `Pitching`, and `Fielding` updates only after a successful claim.

**Definition of done**: processing the same game's stats twice (simulating two concurrent sagas) results in exactly one application of that game's statistics, verified by a concurrency test that deliberately races two sagas against the same shared game.

### Step 6e: Reconciliation checks

**Status**: Not Started

**Objective**: Implement the two logged-warning-only reconciliation checks — team aggregates against `Game*Statistics`, and computed earned runs against Retrosheet's `data` records — with `Pitching.EarnedRuns` sourced from the `data` record.

**Definition of done**: an intentionally-introduced discrepancy in test data produces a logged warning in both cases, without altering the authoritative stored values.

### Step 6f: Full saga integration

**Status**: Not Started

**Objective**: Assemble 6a–6e into the complete saga: file-level idempotency (ignoring queued duplicate messages for an in-progress file), retry/backoff, atomicity, and the API endpoint to initiate processing.

**Definition of done**: matches [game-event.md](./game-event.md)'s Acceptance Criteria in full, including concurrent processing of two files that share a game (see [Considerations](./game-event.md#considerations)).

---

## Step 7: Data Viewing API

**Status**: Not Started

**Governing spec**: [project.md](./project.md) (Data Viewing and Statistics features)

**Depends on**: Step 5 and Step 6 (needs data to view).

**Objective**: Build the REST API surface for Phase 1's Data Viewing feature — player search, career/season statistics, game summaries, and individual play-by-play events, for both players and teams.

**Note**: unlike Steps 1–6, this step doesn't yet have a dedicated spec document defining exact routes, request/response shapes, or the statistic-calculation formulas (AVG, OBP, SLG, OPS, BABIP, WHIP, ERA, FIP, HR/FB, K/9, HR/9, BB/9, FP) in the same level of detail as the ETL specs. Consider writing a dedicated `api.md` spec before or during this step, the same way `game-event.md` was written before its implementation began.

**Deliverables**: to be defined in that future spec — at minimum, player search, player statistics (career and season), game lookup/summary, and game event (play-by-play) retrieval endpoints.

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
