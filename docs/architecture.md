# Retrosharp Architecture Documentation

## System Overview

Retrosharp is an n-tiered web application for exploring historical baseball data from the [Retrosheet](https://www.retrosheet.org) project. It has two halves:

- An **import engine** that downloads Retrosheet's source archives, parses them, and loads them into PostgreSQL. It runs as long-running, message-driven workflows (sagas).
- A **web application**, made up of a REST API and an Angular front end, for browsing players, teams, seasons, standings, and games.

The authoritative requirements live in [spec/project.md](../spec/project.md) and the per-feature specs under [spec/](../spec). [spec/phase-1-build-plan.md](../spec/phase-1-build-plan.md) records what was built and in what order. This document describes how the pieces fit together as they stand today.

## Architecture Diagram

```
                         ┌──────────────────────────────┐
                         │      www.retrosheet.org      │
                         │ biofile · gl{yr}.zip · eve   │
                         └───────────────┬──────────────┘
                                         │ HTTPS download
┌───────────────────┐   ┌────────────────▼──────────────────────────────┐
│ Retrosharp.UI.Web │   │          Retrosharp.Engine.Console            │
│   Angular 22 SPA  │   │           (NServiceBus endpoint)              │
└─────────┬─────────┘   │  PersonSaga   GameLogSaga                     │
          │ HTTP/JSON   │  BulkGameEventImportSaga ──▶ GameEventSaga×N  │
┌─────────▼─────────┐   │  EngineRecoverabilityPolicy · GET /health     │
│ Retrosharp.UI.Api │   └───────▲───────────────────────┬───────────────┘
│  ASP.NET Core 10  │           │ consume               │
│  (send-only NSB)  ├───────────┤                       │
└─────────┬─────────┘  ┌────────┴────────┐              │
          │            │    RabbitMQ     │              │
          │            │ Retrosharp.     │              │
          │            │ Engine(.Errors/ │              │
          │            │ .Audit)         │              │
          │            └─────────────────┘              │
          │   Retrosharp.Service ── Retrosharp.Data     │
          │   (business logic,      (EF Core 10,        │
          │    statistics)           repositories)      │
          │                                             │
┌─────────▼─────────────────────────────────────────────▼───────┐
│                        PostgreSQL 16                          │
│   application schema (EF Core)  +  NServiceBus saga/outbox    │
└───────────────────────────────────────────────────────────────┘
          ▲
          │ one-shot at startup
┌─────────┴───────────────────┐
│ Retrosharp.Data.Migration   │  EF migrations · seed data · NSB schema
└─────────────────────────────┘
```

Both the API and the engine use the same service and data libraries. The API sends import commands over RabbitMQ; it never runs an import itself.

## Projects

| Project | Kind | Role |
|---|---|---|
| `Retrosharp` | Class library | Shared domain: contracts (entities), NServiceBus messages, Retrosheet file formats and parsers, configuration classes, DI auto-registration |
| `Retrosharp.Data` | Class library | EF Core `RetrosharpContext`, database models, repositories |
| `Retrosharp.Data.Migration` | Console app (one-shot container) | Applies EF Core migrations, loads seed data, installs the NServiceBus SQL persistence schema, then exits |
| `Retrosharp.Service.Interface` | Class library | Service contracts and result types |
| `Retrosharp.Service` | Class library | Business logic, import services, statistics, Retrosheet archive download |
| `Retrosharp.Engine.Console` | Console app (container) | NServiceBus endpoint hosting the import sagas |
| `Retrosharp.UI.Api` | ASP.NET Core Web API (container) | REST API for the front end, and a send-only NServiceBus endpoint for starting imports |
| `Retrosharp.UI.Web` | Angular SPA | Front end |
| `*.Tests` | xUnit | `Retrosharp.Format.Tests`, `Retrosharp.Service.Tests`, `Retrosharp.Engine.Console.Tests` |

## Layer Descriptions

### 1. Front End (`Retrosharp.UI.Web`)

**Technology**: Angular 22 (standalone components, signals, lazily loaded routes), Angular Material, Bootstrap 5, TypeScript 6

- Routes: home, player browse/search, player detail, and franchises. The home page is still being designed.
- Typed models under `src/model` and HTTP services under `src/service` talk to the API.
- Light and dark themes: follows the OS setting by default, with a manual toggle (`ThemeService`) saved to `localStorage`.
- Design history: [spec/frontend-prototype.md](../spec/frontend-prototype.md) and [spec/frontend-ux-improvements.md](../spec/frontend-ux-improvements.md).

### 2. Web API (`Retrosharp.UI.Api`)

**Technology**: ASP.NET Core 10 MVC controllers, OpenAPI

**Data viewing** (all `GET`, specified in [spec/api.md](../spec/api.md)):

| Controller | Routes |
|---|---|
| `PlayersController` | `/api/players`, `/search`, `/{id}`, `/{id}/batting`, `/{id}/pitching`, `/{id}/fielding`, `/{id}/games` |
| `TeamsController` | `/api/teams`, `/search`, `/{id}`, `/{id}/roster`, `/{id}/stats`, `/{id}/managers` |
| `SeasonsController` | `/api/seasons/{year}/standings`, `/{year}/teams/stats` |
| `GamesController` | `/api/games/search`, `/{id}`, `/{id}/events` |

**Import and maintenance** (these send NServiceBus commands or trigger computation):

| Controller | Route | Effect |
|---|---|---|
| `PersonController` | `POST /api/person/import` | Sends `PersonStart` |
| `GameLogController` | `POST /api/gamelog/import` | Sends `GameLogStart` for a season |
| `GameEventController` | `POST /api/gameevent/bulkimport`, `GET /api/gameevent/bulkimport/{trackingId}` | Sends `BulkGameEventImportStart` for a season; reports per-file progress |
| `StandingsController` | `POST /api/standings/compute` | Recomputes the precomputed `FranchiseSeasonStandings` |
| `DiagnosticsController` | `POST /api/diagnostics/ping`, `/ping-failing` | Round-trip and error-queue checks against the engine |

Other responsibilities:

- Responses are DTOs mapped from contract entities; contract entities never cross the API boundary directly.
- `GET /health` includes a PostgreSQL connectivity check.
- JWT bearer authentication (Microsoft.Identity.Web) is wired up, but no endpoint requires it yet. Phase 1 is anonymous by design (see [spec/api.md](../spec/api.md#no-authentication-in-phase-1)). Access control, including for the import endpoints, comes with Phase 2's authentication and single sign-on work.

### 3. Service Layer (`Retrosharp.Service`)

**Technology**: .NET 10 class library, CsvHelper, Mapster

- **Viewing and statistics**: `PersonService`, `PlayerStatisticsService`, `PlayerGameLogService`, `BattingService`, `TeamService`, `TeamStatisticsService`, `GameService`, `GameSummaryService`, `GamePlayByPlayService`, `StandingsService`. Rate statistics (AVG, OBP, SLG, OPS, BABIP, ERA, WHIP, FIP, K/9, BB/9, HR/9, FP, and others) are calculated from stored counting statistics. The FIP constant is derived from Retrosharp's own data by `FipConstantResolver`. Player game logs are derived when requested, not stored.
- **Import**: `PersonImportService`, `GameLogImportService`, `GameEventImportService`, `BulkImportService`, and `SeedDataService`, plus the file readers in `ETL/` (`BioFileService`, `GameLogFileService`, `RetrosheetFileService`).
- **Download**: `RetrosheetArchiveClient` fetches the biofile, season game log, and season event archives from Retrosheet (see [spec/retrosheet-auto-download.md](../spec/retrosheet-auto-download.md)).

### 4. Data Layer (`Retrosharp.Data`)

**Technology**: Entity Framework Core 10 with the Npgsql provider, code-first

- `RetrosharpContext` defines the schema, relationships, and unique indexes. The indexes on natural keys (for example `Person.RetroSheetId`, and a game's date, number, and teams) are what enforce import idempotency at the database level.
- Repositories derive from `BaseRepository<TM, TC>`, which maps between database models (`*Model`) and contract entities with Mapster. Queries project to the needed shape with `ProjectToType<T>()`.
- High-volume imports use batched bulk inserts that skip existing records, so rerunning an import doesn't create duplicates.
- `DateTime` values are stored as `timestamp without time zone`, because Retrosheet dates carry no time zone (see [deployment.md](./deployment.md)).

**Tables**:

| Group | Tables |
|---|---|
| Reference | `League`, `Franchise`, `Ballpark` |
| People | `Person` (players, managers, coaches, umpires) |
| Games (from game logs) | `Game`, `GameLineup`, `GameBattingStatistics`, `GamePitchingStatistics`, `GameFieldingStatistics` |
| Play-by-play (from event files) | `GameEvent`, `GameEventRunner`, `GameEventFieldingCredit`, `GameSubstitution`, `GameAdjustment`, `GameComment`, `GameEventContext`, `GameEventGameStatus` |
| Season statistics (derived from play-by-play) | `Batting`, `Pitching`, `Fielding` |
| Precomputed | `FranchiseSeasonStandings` |
| Import tracking | `BulkImport`, `BulkImportFile` |

`GameEventGameStatus` is a per-game marker. It is inserted in the same transaction as that game's statistics, and its primary key guarantees that a game's statistics are applied only once, even if a file is reprocessed or two files ever carry the same game. Modern Retrosheet team files contain only home games, so in practice this is a safety net, not a routine collision (see [spec/game-event.md](../spec/game-event.md)). `GameEventContext` holds per-game details from event file `info` records, such as local start time.

### 5. Domain Library (`Retrosharp`)

- **Contracts**: domain entities such as `Person`, `Game`, `GameSummary`, `GameEvent`, `Batting`, `Pitching`, and `Fielding`.
- **Messages**: `PersonStart`/`Complete`/`Cancel`, `GameLogStart`/`Complete`/`Cancel`, `GameEventStart`/`Complete`/`Cancel`, `GameEventImportFailed`, and `BulkGameEventImportStart`.
- **Formats**: CsvHelper mappings for the biofile, game logs, and the franchise and ballpark seed files. Also a parser for Retrosheet's event file format (`Format/EventFile`), play-by-play state and play code interpretation (`Format/PlayByPlay`), and standings calculation (`Format/Standings`).
- **Configuration**: strongly typed options such as `RetrosheetSourceConfiguration`.
- **Dependency injection**: each project has an `IocRegistrations : IRegister` class, and `ContainerRegistration.RegisterContainer()` discovers and runs them all.

### 6. Import Engine (`Retrosharp.Engine.Console`)

**Technology**: .NET 10 generic host, NServiceBus 10, RabbitMQ transport, SQL persistence on PostgreSQL

**Sagas**:

| Saga | Started by | Does |
|---|---|---|
| `PersonSaga` | `PersonStart` | Downloads Retrosheet's biodata archive, extracts `biofile0.csv`, and adds or updates `Person` rows |
| `GameLogSaga` | `GameLogStart` (season) | Downloads `gl{season}.zip` and imports `Game` and the per-game team statistics |
| `BulkGameEventImportSaga` | `BulkGameEventImportStart` (season) | Downloads `{season}eve.zip`, records one `BulkImportFile` row per team file, then sends a `GameEventStart` for each file, in configurable batches. Collects `GameEventComplete`/`GameEventImportFailed` replies. A watchdog timeout stops it from hanging forever. Refuses to run until that season's game log has been imported. |
| `GameEventSaga` | `GameEventStart` (one file) | Parses a team-season event file into `GameEvent` and the related play-by-play tables, then derives `Batting`, `Pitching`, and `Fielding` |

Working directories for downloaded archives are temporary and deleted when each run finishes. See [spec/bulk-import.md](../spec/bulk-import.md), [spec/game-log.md](../spec/game-log.md), [spec/game-event.md](../spec/game-event.md), and [spec/person.md](../spec/person.md).

**Recoverability** (`EngineRecoverabilityPolicy`):

- **Unrecoverable failures**, sorted by `ImportFailureClassifier`, go straight to the error queue with no retries. Examples: a missing file, a franchise, game, or person that can't be matched, or a play code that can't be parsed.
- **Transient failures** get immediate retries, then delayed retries with exponential backoff (a `2^n` base delay plus up to 20% jitter), then the error queue.
- When a bulk import's `GameEventStart` lands on the error queue, `BulkImportFailureNotifier` sends `GameEventImportFailed` to `BulkGameEventImportSaga`, which marks that file as failed and keeps the run moving.

**Health**: a small web host serves `GET /health` on port 8081 for the container health check.

### 7. Message Bus (RabbitMQ)

| Queue | Purpose |
|---|---|
| `Retrosharp.Engine` | The engine's input queue |
| `Retrosharp.Engine.Errors` | Failed messages, with full exception details and headers, waiting for an operator to retry them |
| `Retrosharp.Engine.Audit` | Copies of successfully processed messages |

The API is a send-only endpoint that routes `PersonStart`, `GameLogStart`, `BulkGameEventImportStart`, and the diagnostic ping messages to `Retrosharp.Engine`. The engine sends `GameEventStart` and the saga replies to itself (`SendLocal`).

### 8. Database (PostgreSQL 16)

PostgreSQL holds the application schema (managed by EF Core migrations) alongside NServiceBus SQL persistence's saga and outbox tables. The project moved from SQL Server to PostgreSQL because SQL Server has no ARM64 build, and ARM64 matters for self-hosting on a Raspberry Pi. See [spec/phase-1-build-plan.md](../spec/phase-1-build-plan.md) Step 8.

## Import Pipeline

Imports must run in dependency order, because each stage looks up records created by the one before it. Files within the same stage can be processed at the same time.

```
1. Seed data       League, Franchise, Ballpark        (migration container, at startup)
2. People          PersonSaga                          POST /api/person/import
3. Game logs       GameLogSaga, per season             POST /api/gamelog/import
4. Play-by-play    BulkGameEventImportSaga, per season POST /api/gameevent/bulkimport
                     └─ GameEventSaga, per team file
5. Standings       StandingsService                    POST /api/standings/compute
```

If a message arrives before its prerequisite exists (for example, an event file that references a game that hasn't been imported yet), that counts as a transient condition and is retried with backoff. Writes that could collide, such as a file being reprocessed while it is still running, are resolved by atomic, database-enforced checks rather than by processing files one at a time.

## Cross-Cutting Concerns

### Configuration

Configuration comes from `appsettings.json`, with per-developer overrides in `appsettings.Development.json` (git-ignored). In Docker, environment variables are loaded from `.env`, using `__` as the section separator. The main sections are:

| Section | Contents |
|---|---|
| `ConnectionStrings` | `DefaultConnection` (PostgreSQL), `RabbitMQ` |
| `Messaging` | Endpoint and queue names, SQL persistence schema and prefix, retry counts, initial retry delay |
| `BulkImport` | Default batch size, watchdog timeout |
| `RetrosheetSource` | Base URL, archive path templates, working root, HTTP timeout |

### Logging

Logging uses Microsoft.Extensions.Logging with the console provider. Entity Framework Core logs at `Warning`; NServiceBus and the application log at `Information`.

### Testing

Tests use xUnit, with NServiceBus.Testing for the sagas. Parser tests run against real Retrosheet files. Validation against live data is recorded in [spec/bulk-insert-qa-results.md](../spec/bulk-insert-qa-results.md) and [spec/stress-testing-report.md](../spec/stress-testing-report.md).

## Deployment

Retrosharp runs as five Docker Compose services: `postgres`, `rabbitmq`, `retrosharp-migration`, `retrosharp-engine-console`, and `retrosharp-ui-api`. The images target both x64 and ARM64. The migration container runs to completion before the engine and API start. An overlay, `docker-compose.pi.yml`, applies Raspberry Pi–sized resource limits (4 GB of memory, 4 cores) so memory pressure shows up in testing before the stack goes onto real hardware. The Angular front end runs separately (`npm start` during development).

See [deployment.md](./deployment.md) for setup and operation.

## Technology Stack Summary

| Layer | Technology | Version |
|---|---|---|
| Runtime and language | .NET, C# | 10.0 |
| Web API | ASP.NET Core | 10.0 |
| ORM | Entity Framework Core (Npgsql provider) | 10.0 |
| Database | PostgreSQL | 16 |
| Messaging framework | NServiceBus (RabbitMQ transport, SQL persistence) | 10.2 |
| Message broker | RabbitMQ (management plugin) | 3.x |
| CSV parsing | CsvHelper | 33.1 |
| Object mapping | Mapster | 10.0 |
| Authentication (wired up, not enforced) | Microsoft.Identity.Web, JWT bearer | 3.14 |
| Front end | Angular, Angular Material | 22 |
| Front-end styling | Bootstrap | 5.3 |
| Front-end language | TypeScript | 6.0 |
| Testing | xUnit, NServiceBus.Testing | 2.9, 10.1 |
| Containers | Docker Compose (x64 and ARM64) | v2 |

## Future Enhancements

Phase 2 is tracked in [spec/project.md](../spec/project.md#second-phase). It includes authentication and single sign-on, administrative features, an ETL activity feed, a Negro Leagues section, postseason data, advanced statistics (wRC+, WAR), player export, multi-season team analysis, ejection tracking, and team logos.
