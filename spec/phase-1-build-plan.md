# Retrosharp Phase 1 Build Plan

## Overview

This document breaks Phase 1 into discrete, dependency-ordered build steps, each small enough to build and verify on its own before moving to the next. It exists so that Phase 1 is not built as a single large effort, but as a sequence of independently correct, independently testable pieces — each step should be complete and verified against its governing spec's Acceptance Criteria before the next step begins.

Each step lists the spec(s) that govern it. Refer back to those specs for the authoritative detail — this document only sequences the work and states what "done" means for each step; it does not restate requirements already written elsewhere.

Steps within the same numbered stage that don't depend on each other (for example, Step 2 and Step 3) may be built in either order, or in parallel. Sub-steps (6a, 6b, ...) are meant to be built in order, since each depends on the previous.

Update the **Status** line under each step as work progresses (`Not Started`, `In Progress`, `Complete`) so this document stays a reliable picture of where Phase 1 actually stands.

## Dependency Graph

```
Step 1: Schema Alignment
        │
        ├──► Step 2: Seed Data Automation ──┐
        │                                     │
        └──► Step 3: Person Parser ──────────┤
                                              │
                                              ▼
                                   Step 4: ETL Messaging Infrastructure
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

**Unrelated discrepancy noticed (not fixed, out of scope for this step)**:
- `PitchingModel` is mapped to a database table literally named `PitchingModel` (`[Table("PitchingModel")]`), inconsistent with every other table's naming convention (`Person`, `Batting`, `Fielding`, `Game`, etc. are all named after the entity, not the model class). Predates this step's changes. Flagging for a future cleanup pass rather than renaming unilaterally, since it wasn't part of today's spec decisions and a table rename has broader ripple effects worth a deliberate decision.

**Verification performed**:
- `dotnet build` on the migration project (which transitively builds `Retrosharp` and `Retrosharp.Data`): 0 errors.
- `dotnet ef migrations add` generated cleanly (with an expected data-loss warning from splitting `GameStatistics`, harmless since no real data exists yet).
- `dotnet ef database update` applied both migrations to a fresh LocalDB database (`(localdb)\mssqllocaldb`, database `Retrosharp`) without error.
- Queried the resulting database directly via `sqlcmd`: all 10 new tables exist with the expected column counts, `Fielding` has 11 columns (10 original + `Position`), and `GameEventGameStatus` has exactly 2 columns (`GameId`, `ProcessedUtc`) confirming the shared-primary-key design was applied correctly rather than EF Core adding a redundant identity column.
- The LocalDB database was left in place (not dropped) — it matches the connection string already checked into `appsettings.json` and is ready to be used as the working database for Step 2 onward.

---

## Step 2: Seed Data Automation

**Status**: Not Started

**Governing spec**: [seed-data.md](./seed-data.md)

**Depends on**: Step 1.

**Objective**: Implement the two seeding mechanisms decided on — `HasData` for `League`, and a CSV-driven idempotent upsert routine for `Franchise` and `Ballpark` — wired into `Retrosharp.Data.Migration`'s existing startup sequence.

**Deliverables**:
- `League` rows baked into a migration via `HasData`.
- An idempotent upsert routine reading `franchises.csv` and `ballparks.csv`, matched by natural key, added to `Retrosharp.Data.Migration`'s startup sequence alongside its existing `Database.MigrateAsync()` call.
- Verification that the same command works unmodified as a manual invocation, a GitHub Actions step, and a Docker container initialization step.

**Definition of done**: matches [seed-data.md](./seed-data.md)'s Acceptance Criteria — a fresh database is fully seeded by one run, and repeated runs produce no duplicates or unwanted changes.

---

## Step 3: Person Parser

**Status**: Not Started

**Governing spec**: [person.md](./person.md)

**Depends on**: Step 1 only. Does not depend on Step 2 — `Person` and seed data are independent of each other and can be built in either order.

**Objective**: Parse Retrosheet's biofile and populate `Person`.

**Deliverables**:
- Biofile CSV parser, using the newer file format per the spec.
- Date-field normalization (day `00` → `01`, month `00` → `01`, missing year → `NULL`).
- `HOF` boolean derived from the "HOF" field value.
- Idempotent insert/update logic keyed on Retrosheet ID.
- Atomicity: an unrecoverable error leaves no partial data committed.
- End-of-parse logging of records added vs. updated.

**Definition of done**: matches [person.md](./person.md)'s Acceptance Criteria in full. This can be validated in isolation — it does not require the messaging infrastructure from Step 4 to be tested, since it can run as a plain console invocation before being wired into a saga.

---

## Step 4: ETL Messaging Infrastructure

**Status**: Not Started

**Governing spec**: [project.md](./project.md) (ETL requirements), [game-log.md](./game-log.md) and [game-event.md](./game-event.md) (saga requirements)

**Depends on**: Step 1. Logically independent of Steps 2 and 3, but there's little reason to build it before at least one of them exists to test against.

**Objective**: Stand up the shared NServiceBus/RabbitMQ plumbing that both the Game Log Parser and Game Event Parser sagas will run on top of. This is infrastructure shared by Steps 5 and 6, not specific to either parser.

**Deliverables**:
- `Retrosharp.Engine.Console` configured as an NServiceBus endpoint over RabbitMQ, with saga persistence.
- Error queue and audit queue configured.
- Recoverability policy: exponential backoff with jitter, configurable retry count and initial wait, eventual dead-letter on exhaustion.
- `Retrosharp.UI.Api` configured as a send-only endpoint capable of initiating ETL jobs.
- Structured logging for all NServiceBus operations.

**Definition of done**: a trivial test message can be sent from the API endpoint, received by the Engine, and successfully processed end-to-end, with a deliberately-failing test message demonstrating the retry/backoff/dead-letter path.

---

## Step 5: Game Log Parser

**Status**: Not Started

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
