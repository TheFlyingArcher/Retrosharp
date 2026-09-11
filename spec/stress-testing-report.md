# Retrosharp Stress Testing — Final Report

**Status: Closed 2026-09-11. Steps 1-6 complete and passing. Step 7 not scoped —
plan closed by decision.**

This is a short, standalone summary of the stress-testing pass. The full
procedure, environment setup, and per-step evidence live in
[stress-testing.md](./stress-testing.md); defect write-ups live in
[defects.md](./defects.md). This document exists to answer "what happened and
where does it stand" without reading either in full.

## What was tested

The Phase 1 stack (ASP.NET Core API, NServiceBus/RabbitMQ ETL engine,
PostgreSQL) had only ever been exercised with serial, single-file imports.
This pass pushed it under concurrent load, Pi-scale memory/CPU limits,
injected infrastructure failure, and a horizontally-scaled engine — six
scenarios run against a Mac host (arm64, matching the Raspberry Pi 4/5
deployment target's architecture) constrained to Pi-sized resource limits.

Real Pi hardware, SD-card/USB I/O behavior, thermal throttling, and data
volume beyond a handful of seasons were explicitly out of scope — see
*Deferred to a future pass* below.

## Results

| Step | Scenario | Result |
|---|---|---|
| 1 | Serial baseline import, Pi-scale limits | ✅ Pass — matches known-good row counts, no OOM |
| 2 | 30 concurrent event-file imports | ✅ Pass — after fixing 2 defects (below); now 3.75-3.8× faster than serial with zero deadlocks |
| 3 | Duplicate-import / idempotency race | ✅ Pass — no code changes needed; three independent layers of defense confirmed under load |
| 4 | Sustained API read load (k6, 2→150 VUs) | ✅ Pass — graceful degradation, no crash, no 5xx storm, full recovery; one slow endpoint improved ~1.9s → ~1.0s |
| 5 | Infrastructure failure injection | ✅ Pass (Postgres-restart case) — 30/30 messages recovered via NServiceBus retry, 0 data loss |
| 6 | Competing consumers (2 engine replicas) | ✅ Pass — after fixing 1 defect (below); verified with a full season re-import, zero duplicates |

**Bottom line: no correctness defect survived the pass.** Every defect found was
fixed and re-verified live against the running stack, not just unit-tested.

## Defects found and fixed

| # | Defect | Severity | Fix |
|---|---|---|---|
| 1 | Play-code parser choked on bare fielded-out codes, silently aborting an import | Medium | Added the missing fallback classification |
| 2 | A Postgres deadlock was misclassified as unrecoverable and sent straight to the error queue with zero retries | High | `ImportFailureClassifier` now walks the exception chain and treats any transient Npgsql error / timeout as retryable |
| 3 | Concurrent imports could genuinely deadlock on shared player season-stat rows | High | Deterministic lock ordering across every stat-update transaction, plus a savepoint-guarded insert path |
| 4 | `/seasons/{year}/teams/stats` was ~1.9s due to an N+1 query pattern (30× per-franchise scans) | Low (performance) | Batched the two heaviest per-franchise scans into one season-wide query each — down to ~1.0s, output byte-identical |
| 5 | Bulk import silently dropped 25 of 30 files when the engine was scaled to 2 replicas (each extracted archive lived only on the replica that downloaded it) | High — data-loss risk on an otherwise-supported-looking scaling operation | Shared Docker volume for the engine's working directory, so every replica sees the same extracted files regardless of which one did the download |

Full root-cause analysis, evidence, and considered-and-rejected alternatives for
each are in [defects.md](./defects.md).

## Capacity findings (Mac host at Pi-scale limits — not real Pi numbers)

- A serial two-season import uses ≈810 MiB peak / ≈686 MiB steady-state across
  all four containers — comfortable headroom on a 4 GB Pi.
- Concurrent imports don't cost more memory, only more CPU (the engine pegs its
  capped core) — expect this to be *slower*, not *less reliable*, on real
  (slower) Pi cores.
- Sustained API read load saturates on Postgres's single capped core well
  before the API or engine become the bottleneck; latency degrades gracefully,
  nothing crashes.
- A mid-import Postgres restart is fully absorbed by NServiceBus's retry
  policy with zero data loss.
- Two engine replicas share a bulk import correctly with no measurable
  coordination overhead once given shared storage.

## Deferred to a future pass, by design

- **Real Raspberry Pi hardware.** This pass validated correctness and
  approximate capacity on emulated resource limits; absolute Pi
  throughput/latency, SD-card/USB-SSD I/O behavior, and thermal throttling
  need a physical-Pi pass.
- **Data volume beyond a handful of seasons.** True volume-scale behavior
  (a multi-decade backfill) is untested.
- **Two Step 4 recommendations, not shipped as fixes**: add request-duration /
  slow-query logging to the API (there was none to diagnose the load-test
  meltdown by), and cap the API's Npgsql pool size below Postgres's
  `max_connections` so it degrades by queuing instead of erroring.
- **Precomputing `/seasons/{year}/teams/stats`** into a table (the way
  standings already are) would take it from ~1.0s to ~20ms, but is a feature-
  sized change, not a stress-test fix.
- RabbitMQ-pause and engine-restart failure injection (part of Step 5) were
  attempted but absorbed too fast to observe in isolation at the polling
  granularity used; accepted as adequately covered by the Postgres-restart
  case plus existing unit tests rather than re-run with finer instrumentation.

## Where things stand now

The environment has been returned to its documented Phase 1 baseline: a
single `retrosharp-engine-console` replica (confirmed via
`rabbitmqctl list_queues` — `consumers=1`), empty error queue, no leftover
test overrides. The permanent Pi-sizing overlay (`docker-compose.pi.yml`)
remains in the repo; the temporary Step 6 replica-count override was Mac-local
only and has been deleted.

**Recommendation: the plan is closed at Step 6.** Step 7 ("analysis and
tuning loop") was written into the original plan as a catch-all, but every
finding it would have addressed was already fixed inline as each step ran.
The three deferred items above belong on the ordinary feature/tech-debt
backlog, not as unfinished stress-test work.
