# Deployment (Docker Compose, PostgreSQL, ARM64/Raspberry Pi)

## Overview

The stack is five containers, defined in `docker-compose.yml` at the repo root:

| Service | Role |
|---|---|
| `postgres` | PostgreSQL 16, the application database |
| `rabbitmq` | RabbitMQ (management plugin included), the message bus |
| `retrosharp-migration` | One-shot: applies EF Core migrations, seeds `Franchise`/`Ballpark`, installs the NServiceBus.Persistence.Sql saga/outbox schema, then exits |
| `retrosharp-engine-console` | The ETL/saga processor -- receives `PersonStart`/`GameLogStart`/`GameEventStart` messages and does the actual parsing/import work |
| `retrosharp-ui-api` | The REST API (`Retrosharp.UI.Api`) -- everything under `/api`, plus `GET /health` |

`retrosharp-engine-console` and `retrosharp-ui-api` both wait for `retrosharp-migration` to exit successfully before starting (`depends_on: condition: service_completed_successfully`), so a fresh `docker compose up` always has a ready schema before either app tries to use it.

The project moved from SQL Server to PostgreSQL specifically for this step, since SQL Server has no ARM64 build and the one Microsoft product that did (Azure SQL Edge) was retired September 30, 2025. See `spec/phase-1-build-plan.md` Step 8 for the full rationale and migration details.

## Prerequisites

- Docker Engine with the Compose plugin (`docker compose`, not the standalone `docker-compose`). On Raspberry Pi OS: `curl -fsSL https://get.docker.com | sh`, then add your user to the `docker` group.
- On a Raspberry Pi, use the 64-bit ("arm64") build of Raspberry Pi OS -- the 32-bit build cannot run these images.

## First-time setup

```bash
cp .env.example .env
```

Edit `.env` and set real values for `POSTGRES_PASSWORD` and `RABBITMQ_DEFAULT_PASS` (the placeholders are `change-me`). `.env` is git-ignored; never commit real credentials.

```bash
docker compose up -d --build
```

This builds all three application images locally and starts the full stack. First build takes a few minutes (compiling the whole .NET solution inside the SDK image); subsequent builds are fast thanks to Docker layer caching.

## Verifying it worked

```bash
curl http://localhost:5197/health          # UI.Api -- checks real Postgres connectivity
curl http://localhost:5197/api/teams/search?code=SDN   # only returns data once you've run the ETL imports below
docker compose ps                          # all 5 services should show "healthy" (migration shows "Exited (0)")
```

`retrosharp-migration` seeds `Franchise`/`Ballpark` reference data automatically, but **does not** import any Retrosheet play-by-play/biofile data -- that still requires triggering the existing ETL endpoints (`POST /api/Person/import`, `POST /api/GameLog/import`, `POST /api/GameEvent/import`) with a file path the `retrosharp-engine-console` container can actually read.

`docker-compose.yml` bind-mounts `./data/retrosheet` on the host to `/data/retrosheet` in `retrosharp-engine-console` for exactly this: drop your Retrosheet source files there (biofiles, game logs, and -- for bulk import -- a season's event-file zip) and pass container paths like `/data/retrosheet/2024eve.zip` to the endpoints. Only the engine container mounts it; the API never reads the files, it just forwards the path on the bus.

### Bulk Game Event import

`POST /api/gameevent/bulkimport` with `{ "zipPath": "/data/retrosheet/2024eve.zip" }` imports a whole season of team-season event files in one call (batched, resumable, per-file status). It returns `202` with a `trackingId`; poll `GET /api/gameevent/bulkimport/{trackingId}` for progress. The season's Game Log must already be imported or the run is rejected (visible as `status: "Failed"` on the status endpoint). The engine extracts into an `_bulk-import/<id>/` subfolder of the mount and deletes each file once it imports successfully; failed files are left for inspection. Tuning (`BulkImport__DefaultBatchSize`, `BulkImport__WatchdogTimeoutHours`, `BulkImport__ExtractionRoot`) is in `.env.example`. See [spec/bulk-import.md](../spec/bulk-import.md).

## Build contexts -- not uniform, and that's intentional

Two of the three Dockerfiles build from `src/` as context; the third builds from the repo root. This isn't an oversight -- `Retrosharp.Data.Migration` needs `docs/csv/*.csv` (the seed-data source files, outside `src/`) at *build* time, since its `.csproj` copies them into the published output via `<None Include>`. `docker compose build`/`up` already passes the right context for each service per `docker-compose.yml`; this only matters if you ever build one of the images manually:

```bash
# UI.Api and Engine.Console: context is src/
docker build -f src/ui/Retrosharp.UI.Api/Dockerfile -t retrosharp-ui-api src
docker build -f src/engine/Retrosharp.Engine.Console/Dockerfile -t retrosharp-engine-console src

# Data.Migration: context is the REPO ROOT
docker build -f src/lib/Retrosharp.Data.Migration/Dockerfile -t retrosharp-migration .
```

## ARM64 builds

Building on the Pi's own hardware is the simplest path for a hobbyist single-device setup -- no cross-compilation, no registry needed:

```bash
# on the Raspberry Pi itself, Docker's own architecture is already arm64
docker compose up -d --build
```

To cross-build ARM64 images from an x64 dev machine instead (useful for testing before you have Pi hardware, or if you'd rather not compile on the Pi):

```bash
docker buildx build --platform linux/arm64 -f src/ui/Retrosharp.UI.Api/Dockerfile -t retrosharp-ui-api:arm64 src
docker buildx build --platform linux/arm64 -f src/engine/Retrosharp.Engine.Console/Dockerfile -t retrosharp-engine-console:arm64 src
docker buildx build --platform linux/arm64 -f src/lib/Retrosharp.Data.Migration/Dockerfile -t retrosharp-migration:arm64 .
```

Cross-building runs the whole .NET compile under QEMU emulation, so expect it to take noticeably longer than a native build (the migration image, which transitively compiles two projects, took several minutes even on a reasonably fast dev machine). Load or push the resulting images to run them on the Pi.

**Caveat**: these builds were verified to complete successfully via `buildx`, but were not run on physical Raspberry Pi hardware -- that verification is on you. If something behaves differently on real ARM64 hardware, it's most likely worth checking first.

## Known benign log lines

- `Cannot load library libgssapi_krb5.so.2` -- an Npgsql warning when probing for optional Kerberos/GSSAPI auth support, which this project doesn't use (plain username/password). Already silenced in all three Dockerfiles by installing `libgssapi-krb5-2`; if you see it, that package didn't get installed for some reason.
- `fail: Microsoft.EntityFrameworkCore.Database.Command[20102] ... SELECT "MigrationId", "ProductVersion" FROM "__EFMigrationsHistory"` immediately followed by `Database migrations applied successfully.` in the migration container's very first run against a brand-new database -- EF Core's own internal "does the history table exist yet" probe, logged at `fail` level even though it's an expected, handled condition. Harmless.

## Two real bugs found and fixed while building this (documented in code comments, summarized here for context)

1. **`RetrosharpContext` DateTime columns**: Npgsql's EF Core provider defaults `DateTime` to Postgres's `timestamp with time zone`, which requires `Kind=Utc` and throws on `Kind=Unspecified` -- exactly what this project's CSV parsers produce for birthdates, game dates, etc. (plain calendar dates with no real timezone meaning, matching what SQL Server's old `datetime2` stored). Fixed via a single `ConfigureConventions` override mapping every `DateTime`/`DateTime?` to `timestamp without time zone`, rather than annotating ~20 properties individually.
2. **`Retrosharp.Engine.Console`'s RabbitMQ startup on a truly fresh broker**: `EnableInstallers()` turned out to be required after all -- the previous assumption ("queues form correctly without it") had only ever been tested against an already-warm broker with queues already created by earlier sessions. A genuinely fresh broker (no persisted queues) fails NServiceBus's own startup broker-verification check, which expects the input queue to already exist. Relatedly, the sidecar `/health` listener's original `Task.WhenAll(host.RunAsync(), healthApp.RunAsync())` masked this failure entirely -- since the health listener runs forever on its own, `WhenAll` never completed even after the NServiceBus host had already faulted, so the container kept reporting healthy while never actually finishing startup. Fixed with `Task.WhenAny` + rethrowing the first-completed task's result, so a real startup failure now correctly exits the process and lets Docker's restart policy recover it.

## Resource expectations

Not benchmarked on real Pi hardware. As a rough guide for sizing: five containers (Postgres, RabbitMQ/Erlang, and three .NET processes) comfortably fit on a Raspberry Pi 4 or 5 with 4GB+ RAM for a hobbyist-scale single-league dataset; a 1-2GB Pi (Zero 2 W, older Pi 3 models) is likely too tight, particularly for RabbitMQ's Erlang VM and Postgres's default shared-buffer sizing. If memory is constrained, reducing Postgres's `shared_buffers` and RabbitMQ's memory high watermark are the first things worth tuning.
