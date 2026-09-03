# Retrosharp Stress Testing Implementation Plan

## Overview

This document plans a stress-testing pass over the Phase 1 stack. It exists because
the application is complete enough to run a full season end-to-end (see
[phase-1-build-plan.md](./phase-1-build-plan.md) Steps 1–9) but has only ever been
exercised with **serial**, single-file imports. Nothing has yet pushed the
NServiceBus/RabbitMQ ETL pipeline, the saga persistence layer, or the Data Viewing
API under concurrent load, and nothing has run the deployable artifact under the
memory and CPU ceilings of its Phase 1 deployment target.

Phase 1's deployment target is a 4 GB Raspberry Pi 4/5 running the five-container
Docker Compose stack (see [docs/deployment.md](../docs/deployment.md) and
[phase-1-build-plan.md](./phase-1-build-plan.md) Step 8). Physical Pi hardware is not
required for most of what this plan covers — the hardware-independent properties
(concurrency correctness, idempotency under race, resource-limit behaviour, failure
recovery) are validated first on the **Mac host (Apple Silicon / arm64)**, which
runs the same containers, at the same architecture, with a Pi-sizing overlay that
imposes Pi-scale resource limits. What the Mac cannot answer — absolute Pi
throughput/latency, SD-card or USB-SSD I/O behaviour, thermal throttling — is
called out explicitly and deferred to a later physical-Pi pass.

Everything in this plan runs on the Mac host. There is no separate staging or cloud
environment in Phase 1.

## Scope

**In scope:**
- Correctness of the ETL pipeline under concurrent file imports, including the
  database-enforced idempotency that [project.md](./project.md) requires *instead
  of* file-level or global serialization.
- Saga behaviour under load: correlation, completion, the error queue, and
  NServiceBus recoverability/backoff.
- Behaviour of the deployable stack under Pi-scale memory and CPU limits: GC
  behaviour under a hard cgroup ceiling, OOM-kill and restart-policy loops,
  connection-pool and message-queue backpressure.
- Data Viewing API throughput and latency under sustained read load, on a populated
  database, with the API CPU-capped.
- Recovery from mid-run infrastructure failure (Postgres restart, RabbitMQ pause).
- Competing-consumer behaviour when the engine is scaled to more than one replica.

**Out of scope:**
- Absolute performance numbers for real Raspberry Pi hardware (throughput, latency,
  time-to-import-a-season). Deferred to a physical-Pi pass.
- SD-card / USB-SSD I/O latency, which is usually the real bottleneck for the
  write-heavy ETL path on a Pi, and thermal throttling under sustained load.
- Data volume beyond the 2025 season. The dev dataset is 2025-only (see the
  `import/` contents below); concurrency and resource-limit behaviour are fully
  testable at this volume, but true volume stress is not. Adding earlier seasons is
  a prerequisite for any volume-scaling work, not for this plan.
- Front-end (`Retrosharp.UI.Web`) load. Out of scope for the same reason it is out
  of scope for the build plan.
- Authentication/authorization load. Not enforced in Phase 1.

## Environment

### Host

- Mac (Apple Silicon, arm64), Docker Desktop with Compose v2 (`docker compose`, not
  the standalone `docker-compose`). Compose v2 is required because the Pi-sizing
  overlay relies on `deploy.resources.limits` being honoured for a local `up`.
- The repository is cloned natively on the Mac. It is **not** run from a
  Parallels shared folder — shared-folder I/O makes Docker builds unreliable.
- Any pre-existing ad-hoc `postgres` / `rabbitmq` containers must be stopped first;
  they collide with the compose stack on ports 5432 / 5672 / 15672.

### Compose overlay

The stack is brought up with three compose files layered in order:

```
docker compose \
  -f docker-compose.yml \
  -f docker-compose.override.yml \
  -f docker-compose.pi.yml \
  up -d --build
```

| File | Role |
|---|---|
| `docker-compose.yml` | Base five-service stack (see [docs/deployment.md](../docs/deployment.md)). |
| `docker-compose.override.yml` | Bind-mounts the host `./import` directory into `retrosharp-engine-console` as `/import:ro`, so the ETL endpoints can resolve file paths. |
| `docker-compose.pi.yml` | Pi-sizing overlay — Pi-scale CPU/memory limits and Postgres/RabbitMQ tuning. Layered last so its limits win. |

### Pi-sizing overlay limits

`docker-compose.pi.yml` approximates a 4 GB Pi 4/5 (4 cores, ~3.3 GB usable for
containers after 64-bit Pi OS Lite):

| Service | CPU limit | Memory limit | Pi-scale tuning |
|---|---|---|---|
| `postgres` | 1.0 | 1024M | `shared_buffers=128MB`, `work_mem=4MB`, `maintenance_work_mem=64MB`, `max_connections=50`, parallel workers capped |
| `rabbitmq` | 0.75 | 768M | `RABBITMQ_VM_MEMORY_HIGH_WATERMARK` pinned at ~500 MiB absolute |
| `retrosharp-engine-console` | 1.25 | 896M | Workstation GC (`DOTNET_gcServer=0`), `DOTNET_GCConserveMemory=5`, healthcheck `start_period` raised to 90s |
| `retrosharp-ui-api` | 1.0 | 640M | Workstation GC, healthcheck `start_period` raised to 90s |
| `retrosharp-migration` | 2.0 | 640M | Workstation GC; one-shot, exits before engine/api start |

Steady-state memory budget (migration has exited): 1024 + 768 + 896 + 640 =
**3328M**, fitting a 4 GB Pi with OS headroom.

Notes carried from the overlay's own header comment:
- Memory limits are **per container**. Scaling the engine
  (`--scale retrosharp-engine-console=N`) applies the 896M limit to each replica —
  drop it to ~448M before scaling, or N replicas exceed the 4 GB budget.
- CPU limits sum to ~4.0 and make contention deterministic. A real Pi lets all four
  cores float; to test that instead, remove the `cpus:` lines and cap the Docker
  Desktop VM at 4 CPUs.
- The overlay reproduces RAM/CPU pressure only. SD-card I/O, thermal throttling,
  and the Pi's slower per-core speed are not reproduced.

### Data inputs

The host `./import` directory (mounted at `/import` in the engine container) holds
the 2025 season:

| Path (container) | Contents |
|---|---|
| `/import/biodata/biofile0.csv` | Retrosheet biofile — 26,961 people. The other files in `biodata/` are unused. |
| `/import/gamelogs/gl2025.txt` | 2025 season game log — 2,430 games. |
| `/import/gameevents/2025/*.EV{A,N}` | 30 play-by-play files, one per team, 81 games each. |

All import requests must pass **container** paths (`/import/...`), and JSON bodies
must use forward slashes — backslashes are mangled before the request is parsed.
The engine container runs as a non-root user (`$APP_UID`) against a read-only
mount, so host file permissions must allow world read/traverse.

### Load generation

The API load generator (`k6`, `bombardier`, `hey`, or equivalent) runs on the Mac
host, **outside** the compose resource limits, so the client is never the
bottleneck. Running it from a separate machine (e.g. the Parallels Windows VM
against `http://<mac-host>:5197`) is an optional isolation improvement, not a
requirement.

ETL load is generated by scripted concurrent `curl` POSTs — no dedicated tool
needed.

### Known variables to hold constant or account for

- **NServiceBus trial license is expired.** Message processing still works, and
  current NServiceBus does not throttle throughput on an expired license — but for
  a throughput measurement it is a variable. A free development licence from
  particular.net removes it. Whichever is chosen, keep it the same across all runs.
- **EF Core SQL command logging.** The engine container's `appsettings.json` keeps
  `Microsoft.EntityFrameworkCore` at `Warning`, so command-level logging should be
  quiet. Confirm log volume after each run (`docker compose logs
  retrosharp-engine-console | wc -l`); a flood indicates a misconfigured level and
  would tax a real Pi's storage disproportionately.
- **Fresh-broker startup.** `Retrosharp.Engine.Console` calls
  `EnableInstallers()`, so a clean `docker compose up` against an empty RabbitMQ
  succeeds without manual queue creation.

## Considerations

### Test the deployable artifact, not `dotnet run`

All prior runs used `dotnet run` with the apps on one machine and Postgres/RabbitMQ
on another. That topology has an extra network hop and does not containerize the
.NET processes, so its numbers do not transfer to either target. Every scenario
here runs the actual compose images.

### An arm64 host is architecture-representative, not speed-representative

Running on Apple Silicon means the images are arm64, the same as the Pi, so
"does the arm64 image even run" is answered for free. Only the *ratios* between
services under the Pi overlay are Pi-like; per-core speed on the Mac is far higher,
so wall-clock timings are a relative baseline, not a Pi prediction.

### The Pi overlay reproduces RAM/CPU pressure, not I/O or thermal

This is the central limitation. Findings about heap behaviour under a hard limit,
OOM kills, restart loops, connection-pool exhaustion, and queue backpressure
transfer to the Pi. Findings about how fast an import completes do not, and the
write-heavy ETL path in particular is expected to be I/O-bound on real Pi storage
in a way the Mac's SSD hides entirely.

### Database-enforced idempotency is the property under test

[project.md](./project.md) requires that two ETL processes which could write the
same record (e.g. two Game Event files sharing a game) resolve it with atomic,
database-enforced idempotency checks — explicitly *not* by serializing file
processing. The concurrent-import scenarios exist to exercise exactly this. A
correct result is identical final row counts and an empty error queue regardless of
concurrency; file-level locking or a single-threaded consumer would be a
regression even if row counts came out right.

### Workstation GC under constrained memory

The overlay forces `DOTNET_gcServer=0` on all .NET services. Server GC allocates a
heap per core and is too memory-hungry for several runtimes sharing a 4 GB box;
Workstation GC is the correct choice for the Pi target, so it is what gets tested.

### Reset between runs or the workload disappears

Every parser is idempotent and skip-only for already-imported records. A second run
against a populated database imports nothing and measures nothing. Each scenario
starts from `docker compose ... down -v` (which drops the `postgres-data` volume),
then a fresh `up` that re-runs migration + seed.

### The load generator must not compete with the system under test

On the same host, a saturating load generator steals CPU from the containers and
corrupts API latency numbers. It runs outside the compose limits on the Mac host,
or on a separate machine.

## Metrics

Every scenario captures the same set:

1. **Wall-clock time** per stage (person, game log, all events, standings compute)
   and end-to-end.
2. **Peak memory and CPU per container**, from a sampled `docker stats` timeline
   written to a file for the duration of the run.
3. **`OOMKilled` and `RestartCount` per container**, read after the run:
   `docker inspect --format '{{.Name}} OOM={{.State.OOMKilled}} restarts={{.RestartCount}}' $(docker compose ps -q)`.
4. **RabbitMQ state**: peak input-queue depth, error-queue count (must be 0), audit
   throughput, and whether any connection entered `blocked` state against the
   memory watermark. From the management UI at `http://localhost:15672`.
5. **Postgres**: peak connection count vs. `max_connections=50`, and any lock-wait
   or pool-exhaustion errors in the engine/API logs.
6. **Final row counts vs. the known-good baseline** (below). Any deviation is a
   correctness failure regardless of timing.

### Known-good 2025 baseline

A clean, complete 2025 import produces:

| Table | Rows |
|---|---|
| `Person` | 26,961 |
| `Game` | 2,430 |
| `GameLineup` | 43,740 |
| `GameBattingStatistics` / `GamePitchingStatistics` / `GameFieldingStatistics` | 4,860 each |
| `Batting` (player-season) | 770 |
| `Pitching` (player-season) | 1,015 |
| `Fielding` (player-season) | 2,272 |
| `GameEvent` | 216,845 |
| `GameEventRunner` | 238,502 |
| `GameEventFieldingCredit` | 172,661 |
| `GameEventGameStatus` | 2,430 |
| `FranchiseSeasonStanding` | 30 |

Standings: total wins = total losses = 2,430. Spot checks against real 2025 MLB:
HR leaders Raleigh 60 / Schwarber 56 / Ohtani 55 / Judge 53 / Caminero 45; best
records MIL 97–65, PHI 96–66.

Expected non-fatal noise: roughly **130 `GameEventImportService` reconciliation
warnings** (mostly ±1 earned run on individual relievers, a few ±1 on GIDP / RBI /
errors). These are the Step 6e integrity-check signal where play-by-play-derived
values disagree with Retrosheet's authoritative `data,er` records; the
authoritative value is stored. Their count and character should stay stable across
runs — a large increase is a finding.

## Implementation Steps

Steps are ordered. Step 1 is the control and must pass before the concurrent
scenarios mean anything.

### Step 0: Pre-flight

**Status**: Not Started

**Objective**: Confirm the environment is ready before generating any load.

**Procedure**:
1. Stop any ad-hoc `postgres` / `rabbitmq` containers; confirm only the compose
   stack holds ports 5432 / 5672 / 15672.
2. Bring up the three-file overlay. Confirm `docker compose ps` shows all services
   healthy and `retrosharp-migration` as `Exited (0)`.
3. `docker compose ... config` — confirm the merged configuration has the Pi limits
   on every service and the `/import` mount on the engine.
4. `curl http://localhost:5197/health` returns 200; `curl
   http://localhost:5197/api/teams?limit=1` returns seed data (proves migration
   seeded League/Franchise/Ballpark, not just that it exited).
5. `docker compose exec retrosharp-engine-console sh -c "ls -la /import/gamelogs
   /import/gameevents/2025 /import/biodata/biofile0.csv"` — files are present and
   readable by the container user.
6. Confirm the load-generation tool is installed and can reach the API.
7. Confirm the DB-inspection path: connect to `localhost:5432` with the `.env`
   credentials (this is a different database from any earlier ad-hoc instance).

**Pass criteria**: all of the above succeed.

### Step 1: Serial baseline run (the control)

**Status**: Not Started

**Objective**: Establish that the deployable stack completes a full, correct 2025
import **under the Pi limits** with serial imports, and capture the baseline
metrics every later scenario is compared against.

**Procedure**:
1. `down -v` and fresh `up`.
2. Start the `docker stats` sampler.
3. POST imports strictly serially, waiting for each saga to complete before the
   next:
   - `POST /api/person/import` → `/import/biodata/biofile0.csv`
   - `POST /api/gamelog/import` → `/import/gamelogs/gl2025.txt`, `seasonYear` 2025
   - `POST /api/gameevent/import` for each of the 30 files, one at a time
   - `POST /api/standings/compute?season=2025`
4. Stop the sampler. Capture all six metric groups.

**Pass criteria**:
- Final row counts exactly match the known-good baseline.
- Error queue empty; no saga left uncompleted.
- No container `OOMKilled`; `RestartCount` 0 for `postgres`, `rabbitmq`,
  `retrosharp-ui-api`, `retrosharp-engine-console`.
- Reconciliation-warning count is ~130 and consistent with the baseline character.

**What a failure means**:
- Engine `OOMKilled` during the Person upsert or event import → the bulk-upsert /
  event-graph batch sizes are too large for the 896M budget. The fix is smaller
  batches, not a larger limit, since the Pi will not have one.
- Postgres pool errors at this stage → `max_connections=50` is too low for the
  engine's per-batch transaction pattern even without concurrency; investigate the
  connection lifetime in the bulk-write path.

### Step 2: Concurrent ETL — 30 event files at once

**Status**: Not Started

**Objective**: Exercise database-enforced idempotency, saga correlation, and the
error queue when many Game Event imports run at once instead of serially.

**Procedure**:
1. `down -v`, fresh `up`, then serially import person + game log only (events
   depend on `Game` and `Person`).
2. Start the sampler.
3. Fire all 30 `POST /api/gameevent/import` requests as close to simultaneously as
   the shell allows, without waiting between them.
4. Wait for all 30 sagas to reach completion. Then `POST
   /api/standings/compute?season=2025`.
5. Capture metrics.

**Pass criteria**:
- Final row counts exactly match the known-good baseline — concurrency changes
  timing, not results.
- Error queue empty. Every `GameEventSaga` instance completed and the saga table is
  empty.
- No `OOMKilled`. Restarts, if any, are recovered and the final state is still
  correct.
- RabbitMQ input-queue depth rises and drains; no connection stuck `blocked`.

**What a failure means**:
- Duplicate-key exceptions that are *not* self-healed by NServiceBus retries →
  idempotency is not actually atomic at the database level for some write path.
- Row counts off → a shared-game write race is losing or double-counting data; this
  is the exact failure [project.md](./project.md) forbids solving with
  serialization.
- Sustained queue growth without drain, or `blocked` connections → RabbitMQ hit the
  memory watermark; note the depth at which it happened.

### Step 3: Duplicate / idempotency race

**Status**: Not Started

**Objective**: Confirm a re-submitted import for a file already in progress or
already complete is safe.

**Procedure**:
1. Continue from a Step 2 database, or rebuild to the post-game-log state.
2. For 3–5 of the event files, fire two `POST /api/gameevent/import` requests for
   the same file within milliseconds of each other.
3. Separately, re-POST several files that have **already** completed.
4. Capture metrics.

**Pass criteria**:
- Row counts unchanged from the baseline after all duplicate and repeat
  submissions.
- `GameEventSaga`'s `IsRunning` guard and/or the database idempotency checks absorb
  the duplicates; at most one transient unique-constraint failure per race, and it
  is retried to a clean completion.
- Error queue empty.

**What a failure means**: any permanent error, any row-count change, or a saga left
stuck `IsRunning` indicates the dedup path is not race-safe.

### Step 4: Sustained API read load

**Status**: Not Started

**Objective**: Measure Data Viewing API throughput and latency on a populated
database with the API CPU-capped at 1.0 and memory-capped at 640M.

**Procedure**:
1. Start from a fully populated, standings-computed 2025 database (end state of
   Step 2).
2. Run a mixed read workload with the load generator for a sustained period
   (e.g. 10–15 minutes), covering:
   - `GET /api/players/search?q=...` and `GET /api/players?letter=...`
   - `GET /api/players/{id}` and `/batting`, `/pitching`, `/fielding`, `/games`
   - `GET /api/teams`, `/api/teams/{id}/roster`, `/api/teams/{id}/stats`
   - `GET /api/games/search?season=2025`, `GET /api/games/{id}`,
     `GET /api/games/{id}/events` (the play-by-play endpoint is the heaviest)
   - `GET /api/seasons/2025/standings`, `GET /api/seasons/2025/teams/stats`
3. Ramp concurrency until latency degrades or errors appear; record the knee.
4. Capture metrics, plus load-generator output (RPS, p50/p95/p99, error rate).

**Pass criteria**:
- No 5xx responses under normal concurrency; graceful degradation (rising latency,
  not errors) as concurrency climbs.
- API container does not `OOMKilled`.
- Postgres connections stay within `max_connections=50`.

**What a failure means**:
- 5xx or connection errors well below expected concurrency → connection-pool
  sizing or a per-request query that does not scale (the play-by-play and
  season-stats endpoints are the first suspects).
- API `OOMKilled` → a response path buffering too much in memory; the full-game
  play-by-play response is the first suspect.

### Step 5: Failure injection

**Status**: Not Started

**Objective**: Confirm the stack recovers from mid-run infrastructure failure.

**Procedure**:
1. Begin a concurrent event import as in Step 2.
2. Mid-import, in separate trials:
   - `docker compose restart postgres`
   - `docker pause rabbitmq` for ~30s, then `unpause`
   - `docker compose restart retrosharp-engine-console`
3. Allow the import to run to completion afterwards (re-submitting any files whose
   sagas were lost is permitted and itself tests idempotency).
4. Capture metrics.

**Pass criteria**:
- The stack recovers without manual intervention beyond re-submitting imports.
- NServiceBus immediate/delayed retries and the exponential-backoff policy absorb
  the outage; messages are not lost to the error queue for a transient failure.
- Final row counts match the baseline once the import is allowed to finish.
- Healthchecks correctly mark a killed container unhealthy and the restart policy
  brings it back.

**What a failure means**:
- Messages in the error queue after only a transient outage → recoverability is
  misclassifying transient failures as unrecoverable (see
  [defects.md](./defects.md), "Needless Retrying").
- A container that stays down or stays `unhealthy` → healthcheck timing
  (`start_period`, `retries`) is wrong for the constrained environment.
- Row counts wrong after recovery → a partial write was committed without its
  idempotency guard.

### Step 6: Competing consumers (engine scale-out)

**Status**: Not Started

**Objective**: Confirm multiple engine replicas can share the input queue safely.

**Procedure**:
1. Lower `retrosharp-engine-console`'s memory limit to ~448M (per the overlay
   note) and `up --scale retrosharp-engine-console=2` (optionally 3 if the memory
   budget allows).
2. Run the Step 2 concurrent event import.
3. Capture metrics, including how work distributes across replicas (from their
   logs).

**Pass criteria**:
- Final row counts match the baseline; no double-processing across replicas.
- Saga persistence and the outbox correctly prevent two replicas from both
  completing the same unit of work.
- Error queue empty.

**What a failure means**: any row-count deviation or duplicated side effect
indicates saga/outbox correlation is not safe for competing consumers, which blocks
the horizontal-scaling story in [docs/architecture.md](../docs/architecture.md).

### Step 7: Analysis and tuning loop

**Status**: Not Started

**Objective**: Turn findings into concrete changes and re-verify.

**Procedure**:
- For each failure or concerning metric, record: the scenario, the observation, the
  suspected cause, the change made, and the re-run result.
- Prefer fixes that make the Pi target viable (smaller batch sizes, connection
  lifetime, response streaming) over fixes that only raise a limit.
- Re-run the affected scenario, and Step 1, after any change.

**Pass criteria**: every scenario in Steps 1–6 passes its own criteria on a clean
run, with the tuning changes in place.

## Acceptance Criteria

1. The three-file compose overlay (`docker-compose.yml` +
   `docker-compose.override.yml` + `docker-compose.pi.yml`) brings up a healthy,
   seeded stack on the Mac host, and `docker compose config` confirms the Pi
   limits and the `/import` mount are applied.
2. **Step 1** (serial baseline) completes a full 2025 import under the Pi limits
   with final row counts exactly matching the known-good baseline, an empty error
   queue, and no container OOM-killed.
3. **Step 2** (30 concurrent event imports) produces the identical baseline row
   counts and an empty error queue — proving idempotency is database-enforced, not
   dependent on serialization.
4. **Step 3** (duplicate race) leaves row counts unchanged and no saga stuck, with
   at most self-healed transient constraint violations.
5. **Step 4** (sustained API read load) shows graceful latency degradation with no
   5xx under normal concurrency, the API within its memory limit, and Postgres
   within `max_connections`; the concurrency knee is recorded.
6. **Step 5** (failure injection) recovers from Postgres restart, RabbitMQ pause,
   and engine restart without lost messages or manual intervention beyond
   re-submitting imports, and finishes with baseline row counts.
7. **Step 6** (two-plus engine replicas) produces baseline row counts with no
   double-processing.
8. Every scenario's six metric groups are captured to durable artifacts (stats
   timelines, inspect output, RabbitMQ figures, row-count dumps, load-generator
   reports).
9. Findings and any resulting tuning changes are recorded in the Progress Log, and
   affected scenarios plus Step 1 are re-run clean afterwards.
10. The limitations this pass does **not** cover — absolute Pi throughput/latency,
    SD-card/USB-SSD I/O, thermal throttling, data volume beyond 2025 — are
    restated in the Progress Log as the explicit scope of a future physical-Pi
    pass.

## Progress Log

**Status**: In Progress

_(Record here, per scenario: environment specifics — Docker Desktop CPU/RAM
allocation, image digests, licence state; the six metric groups; any failures with
suspected cause, the change made, and the re-run result. Follow the format used in
[phase-1-build-plan.md](./phase-1-build-plan.md) — what was found, what was done,
discrepancies and decisions, errors encountered, verification performed.)_

### Run 1 — serial baseline, unconstrained (2026-09-02)

Ran ahead of the Pi-sized passes as an unconstrained shakeout on the Mac host: not
`docker-compose.pi.yml`, just base + the import bind-mount override, on a Docker
Desktop VM with ~13.6 GiB. Dataset extended to **two** seasons (2024 + 2025) —
`import/gamelogs/{2024,2025}/glYYYY.txt` and 30 event files per year. NServiceBus
trial licence expired (message processing unaffected). Fresh DB (`down -v` first).

**Pipeline: pass.** 1 biofile + 2 game logs + 60 event files + 2 standings computes,
strictly serial. Zero messages to the error queue, zero stuck sagas, zero container
restarts. Person (26,961), both game logs, all 30 **2025** event files, and both
standings computations completed correctly; the 2025 half reproduced the standalone
baseline from [project.md] byte-for-byte (`GameEvent` +216,845, GES 2,430, player
stats split cleanly across the two seasons).

**Timing** (serial; my completion poller added ~13s/file overhead, so engine work is
lower): Person 19s · gl2024 32s · gl2025 33s · 2024 events 612s · 2025 events 589s ·
standings 1s · total ~21.5 min.

**Resources** (sampled `docker stats`, no limits): engine-console peak 164% CPU
(1.64 cores) / 352 MiB; postgres 58% / 183 MiB; rabbitmq 52% / 222 MiB; ui-api idle
(no read load this run). No OOM, no restarts. Note the engine's 1.64-core burst is
above `docker-compose.pi.yml`'s 1.25-core cap — serial event parsing will throttle
~25–30% under the Pi overlay, as intended.

**Log review:** 258 `warn: GameEventImportService` reconciliation lines across both
seasons (~130/season — the expected ±1 ER / GIDP / RBI noise, same rate as the 2025
baseline). No `fail:`/`crit:` lines.

**Finding 1 — parser gap (fixed).** `2024MIA.EVN`'s first play was a bare `2` (an
unassisted catcher putout on a foul pop) with no `G/L/F/P/BG/BP/BL` trajectory
modifier — valid but rare Retrosheet. `PlayCodeParser` threw `PlayCodeParseException`
rather than classifying it. Fix: `ClassifyUnannotatedFieldedOut` fallback (a lone
unassisted OF/catcher putout → FlyOut, a throw or lone infield putout → GroundOut,
`BattedBallType` left null). 6 new `PlayCodeParserTests`. Branch
`fix/playcode-bare-fielded-out`.

**Finding 2 — silent whole-file loss (fixed).** Because the exception is unrecoverable
and each saga caught-and-`MarkAsComplete()`d unrecoverable failures, all 81 Marlins
2024 games were dropped with only a `warn:` line — nothing on the error queue, saga
"successful", API 202 indistinguishable from success. Caught only by reconciling
`GameEventGameStatus` (2,348) vs the 2024 game-log count (2,429). Fix: the
unrecoverable-vs-transient decision moved into `EngineRecoverabilityPolicy.Decide`,
which `MoveToError`s unrecoverable failures on the first try (still zero retries); the
sagas no longer catch anything. Failed files now land in `Retrosharp.Engine.Errors`
with the full exception and are operator-retryable. See [defects.md] "Needless
Retrying". 9 new `EngineRecoverabilityPolicyTests`; 3 saga tests updated.

**Finding 3 — 2024 log is 2,429 games**, not 2,430: one 2024 MLB game was cancelled
and never made up. Not a defect; the 2024 reconciliation baseline is 2,429 (standings
computed correctly against it — LAN 98‑64, PHI 95‑67).

**Finding 4 — test-harness only:** the DB-poll-based completion detection in the run
driver adds ~13s/file. For the Pi-sized runs, tail the engine container log for the
real "import complete" line so timings are the engine's.

**Still open:** caller-facing import status (the API 202 gives no success/failure
channel even now that failures reach the error queue) — a status endpoint or
persisted ETL-run record is Phase 2 ("ETL activity feed/dashboard" in project.md).

**Next:** rebuild the engine image on the fixes, backfill `2024MIA.EVN`, verify GES
2,348 → 2,429 and Marlins 2024 stats appear, then start the Pi-sized passes from
Step 1.

**Backfill (2026-09-03):** engine rebuilt from `fix/playcode-bare-fielded-out`
(later merged to `main` as `20b59b1` + `f454df4`). Live verification of both fixes:
- Observability: `POST` with a nonexistent path → 1 message in
  `Retrosharp.Engine.Errors` carrying `ExceptionType`
  `System.IO.FileNotFoundException`, the message, the stack, `FailedQ`,
  `OriginatingEndpoint`, and the full original body (`FilePath` + `RequestId`) —
  operator-retryable. Engine queue stayed 0 (no retry loop); audit queue unchanged
  (not counted as success). Contrast the pre-fix behaviour where the same POST
  incremented the audit queue and left the error queue empty.
- Parser + backfill: `POST 2024MIA.EVN` completed in ~17s. 2024 `GameEventGameStatus`
  2,348 → 2,429; 2024 games missing events 81 → 0; game 6 (2024-03-28 MIA v PIT),
  previously eventless, returns 176 play-by-play entries via the API. Error queue
  stayed empty. 2024 standings unchanged (derive from `Game`, not events).
- Idempotency: re-`POST 2024MIA.EVN` → every count identical, error queue 0.

### Step 1 — serial baseline under the Pi overlay (2026-09-03)

Full three-file overlay (`+ docker-compose.pi.yml`), fresh DB (`down -v`), engine
built from `main` with both fixes. Docker Desktop VM ~13.6 GiB (the overlay's
per-container `deploy.resources.limits` are what constrain each service, not the VM).
RabbitMQ watermark confirmed applied at 477 MiB (`rabbitmq-pi.conf` via `conf.d`,
after `RABBITMQ_VM_MEMORY_HIGH_WATERMARK` as an env var was rejected by the 3.13
image — fixed in `ebff973`). Same serial sequence as Run 1, two seasons.

**Pass.** Total 1,174s. Final row counts identical to the post-backfill Run 1 state
(Person 26,961 · Game 4,859 · GES 4,859 = 2024 2,429 + 2025 2,430 · GameEvent
465,584 · Batting 1,511 · Pitching 1,991 · Fielding 4,463 · standings 30+30). Zero
`[!]` warnings, error queue empty throughout, no saga resets. `docker inspect`:
`OOMKilled=false RestartCount=0` on all four containers. `2024MIA.EVN` imported clean
(GES +81, 26s) — parser fix confirmed under the Pi build.

**Timing vs unconstrained Run 1** (post-backfill): Person 19s (=19s) · gl2024 32s
(=32s) · gl2025 32s (=33s) · 2024 events 554s (Run 1 612s, but that included the MIA
abort) · 2025 events 530s (589s) · standings 1s. **The Pi CPU caps do not affect
serial ETL throughput** — event stages were marginally *faster* than unconstrained,
within noise.

**Resources** (sampled `docker stats`, 2min-bucketed peaks):

| Container | CPU% peak / p95 / avg | MEM peak / p95 | limit | MEM peak vs limit |
|---|---|---|---|---|
| engine-console | 133 / 88 / 15 | 311M / 218M | 1.25c / 896M | 35% |
| postgres | 55 / 33 / 5 | 176M / 173M | 1.0c / 1024M | 17% |
| rabbitmq | 37 / 34 / 7 | 218M / 194M | 0.75c / 768M | 28% |
| ui-api | 20 / 1 / 0 | 105M / 103M | 1.0c / 640M | 16% |

- Engine CPU briefly touches the 1.25-core cap (133% peak) but p95 is 88% and avg
  15% — the cap trims a transient without a throughput cost.
- **No engine memory leak.** The 311M peak is the Person bulk-upsert (loads the
  existing table into a dictionary); it settles to ~215M for the entire 60-file
  event run and stays flat (per-2min peak series plateaus at 218M). The periodic
  `SaveChangesAsync` bounds change-tracker growth as intended. ~4× headroom to 896M.
- Postgres memory climbs 63M → 176M then plateaus — `shared_buffers=128MB` cache
  warming, expected. RabbitMQ steady well under both the 768M cap and the 477 MiB
  publish watermark; no memory alarm, no blocked connections.
- Peak concurrent footprint across all four containers ≈ 810 MiB, and the peaks
  don't even coincide (engine's 311M spike is during Person, before Postgres warms).
  Steady-state ≈ 686 MiB. A 4 GB Pi (~3.3 GB usable) has large headroom for a
  **serial** 2-season import.

**Caveat:** serial only. Concurrent imports (Step 2) are where memory, the
`max_connections=50` Postgres cap, and RabbitMQ queue depth are actually exercised.

### Step 2 — concurrent event imports under the Pi overlay

**Status**: Not Started
