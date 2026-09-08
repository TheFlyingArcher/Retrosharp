# Bulk Import of Retrosheet Game Events

## Overview

The 2024 season's game events were imported from Retrosheet one file at a time through
`POST /api/gameevent/import`. It worked, but it was an exercise in tedium. Retrosheet has
game event files dating back to 1871; even constraining to 1998 onward (the first year MLB
had 30 teams) is 30 teams x 28 seasons = **840** individual event files, each needing its
own API call. This document specifies a bulk import process that imports a whole season's
worth of game event files from a single zip archive with one API call, orchestrated
asynchronously by a new saga in `Retrosharp.Engine.Console`.

Bulk import is part of **Phase One** — it is the ETL tooling that makes
[phase-1-build-plan.md](./phase-1-build-plan.md)'s Step 9 ("a full season across every
team") practical to run.

> **Step 11 update ([retrosheet-auto-download.md](./retrosheet-auto-download.md)):** the
> request no longer carries a zip path. It carries a **season year**; the engine downloads
> `{season}eve.zip` from Retrosheet into a per-run working directory (under the system temp
> dir by default) and validates it before any file is queued. Everything below about
> extraction, batching, per-file status, rerun-skip, and cleanup is unchanged except that
> the downloaded zip is always deleted when the run finishes, and `SourceZipPath` records
> the download URL. Where the old text says "the path supplied in the request" / "the
> `.zip` on the mounted volume", read "the archive downloaded for the requested season".

## Considerations

### Bulk import is orchestration, not a new parser

The per-file work — parsing a `.EVN`/`.EVA` file, populating `GameEvent` and its supporting
tables, deriving `Batting`/`Pitching`/`Fielding`, reconciling against the Game Log Parser's
`Game*Statistics` — is already built and already correct (see [game-event.md](./game-event.md)
and [phase-1-build-plan.md](./phase-1-build-plan.md) Step 6). Bulk import does not
reimplement any of it. It extracts the archive, decides which files to process and in what
batches, drives the existing Game Event saga once per file, records per-file status, and
cleans up. All per-file parsing, idempotency, atomicity, retry/backoff, and reconciliation
behavior is inherited unchanged from [game-event.md](./game-event.md) and
[parser.md](parser.md).

### The archive is downloaded by the engine, not supplied

The endpoint accepts a **season year**, not a path or an upload. `Retrosharp.UI.Api` only
range-checks the year and places a message on the bus; the saga in
`Retrosharp.Engine.Console` downloads `{season}eve.zip` from Retrosheet (see
[retrosheet-auto-download.md](./retrosheet-auto-download.md)) and does all reading,
extraction, and validation. A season Retrosheet has no archive for surfaces as a `Failed`
run on the status endpoint, not a `400` from the POST; a transient download failure is
retried and does not create a run row. (Originally the endpoint took a `.zip` path on a
volume mounted into the engine — Step 11 replaced that.)

### Extraction target and season scoping

The saga downloads and extracts the archive into a per-run working directory named by the
tracking id. The parent is `RetrosheetSource__WorkingRoot` when configured, otherwise a
`retrosharp-import/` folder under the system temp directory. Only entries whose name matches
a Retrosheet event file —
`20YYTTT.EVN` (National League) or `20YYTTT.EVA` (American League), year-first, per
[game-event.md](./game-event.md#considerations) — are extracted and considered; anything
else in the archive is ignored and logged.

The season comes from the request (it is what builds the download URL). The saga still
parses the season out of the downloaded archive's file names and rejects the run if they
disagree with the requested season, or span more than one season — this now doubles as a
check that Retrosheet served the archive that was actually asked for.

### Game Log must already be imported for the season

The Game Event Parser requires a `Game` row to exist before it can attach play-by-play to it
(see [game-event.md](./game-event.md#prerequisites)); a whole season of event files against
an unimported Game Log would be 30 files each failing the same way. Bulk import validates
**once, up front**, that at least one `Game` row exists for the target season (via
`IGameRepository.GetBySeasonAsync`). If none does, the bulk import is rejected immediately
with an error telling the operator to import that season's Game Log first — no files are
extracted or queued, and the `BulkImport` row is recorded with status `Failed`.

This is a deliberately coarse check — it confirms the Game Log Parser has run for the
season, not that every individual game is present. A genuine per-game gap (an event file
referencing a game still missing from `Game`) remains a retryable condition handled by the
Game Event saga's existing retry/backoff policy, exactly as it is for a single-file import.

### Orchestration mechanism: one child Game Event saga per file

The bulk saga drives the existing Game Event saga by sending it a normal `GameEventStart`
per file, carrying a new optional `BulkImportId` correlation value. `GameEventSaga` stores
`BulkImportId` in its saga data and echoes it back on the `GameEventComplete` it already
sends on success. `BulkGameEventImportSaga` also handles `GameEventComplete`, correlated on
`BulkImportId` — NServiceBus delivers that one message to both saga instances (the per-file
`GameEventSaga` marking itself complete, and the bulk saga recording the file's outcome).
When `BulkImportId` is absent (a plain single-file import through the existing endpoint), no
bulk saga instance matches and nothing changes.

### A failed file must not stall the batch

On the success path the bulk saga learns a file is done from `GameEventComplete`. On the
failure path `GameEventSaga` sends nothing — the exception propagates to the endpoint's
recoverability policy and, after any retries, the message is moved to the error queue (see
[game-event.md](./game-event.md) Acceptance Criteria and [defects.md](./defects.md),
"Needless Retrying"; the saga deliberately does not catch-and-complete its own failures).

So the bulk saga gets its failure signal from an **error-queue hook**
(`recoverability.Failed().OnMessageSentToErrorQueue(...)` in `Retrosharp.Engine.Console`):
when a message that carries a `BulkImportId` is moved to the error queue, the hook sends a
`GameEventImportFailed` (`BulkImportId`, file name, exception summary) that the bulk saga
handles — marking that file `Failed` and starting the next one. The failed message still
lands on the error queue with its full exception and headers, so an operator can fix the
underlying data/parser issue and retry it later exactly as today. A generous overall
**watchdog timeout** on the bulk saga is the backstop for the cases no hook covers (the
engine process crashing mid-file, a transient error retrying indefinitely): when it fires,
every still-unfinished file is recorded `Failed` with a "no completion signal" note and the
bulk import is closed out.

### Batching

At most `BatchSize` files are in flight at once (default **10**, configurable). The saga
keeps that many child `GameEventStart` messages outstanding; each time a file finishes
(`GameEventComplete` or `GameEventImportFailed`), the next `Pending` file is dispatched,
until the archive is exhausted. Batching bounds concurrent load on Postgres and RabbitMQ —
the same shared-row contention between overlapping event files that
[stress-testing.md](./stress-testing.md) Step 2 tuned the deadlock/retry behavior for — so
that importing a 30-file season (or an 840-file backfill) degrades gracefully instead of
stampeding.

### Rerun skips files that already succeeded

Re-running bulk import for a season that was partly imported before must not redo work.
Before dispatching, for each event file in the archive the saga looks for the most recent
`BulkImportFile` row for that `(SeasonYear, FileName)`:

- most recent status `Success` **or `Skipped`** → the new row is created as `Skipped`,
  never dispatched. (`Skipped` on an earlier rerun still means the file's games are already
  imported — a rerun does not un-import them.)
- most recent status `Failed`, or no prior row → created as `Pending` and processed.

Skipped files are counted and reported in the completion summary. (Per-game idempotency
inside a reprocessed file is still enforced by the Game Event Parser's `GameEventGameStatus`
claim — see [game-event.md](./game-event.md#gameeventgamestatus) — so reprocessing a
partially-applied file never double-counts.)

### Cleanup removes only what succeeded

When the bulk import finishes, each extracted file whose `BulkImportFile` status is
`Success` is deleted from the working directory, and the downloaded `{season}eve.zip` is
**always** deleted (it is re-downloadable). Files that ended `Failed` are left in place for
investigation. The working directory is removed only if it ends up empty — so a fully
successful run leaves nothing behind, and a run with failures leaves only the failed
`.EVN`/`.EVA` files. A startup failure removes the whole working directory.

## Data Model

Two new tables, both owned entirely by the bulk import saga. Enums are stored as their
`int` value (no lookup table), matching the `GameEventType` convention in
[game-event.md](./game-event.md).

### `BulkImport`

One row per bulk import request — the unit the tracking identifier refers to.

| Column | Type | Notes |
|---|---|---|
| `Id` | int, PK identity | Internal key. |
| `TrackingId` | Guid, unique | The identifier returned to the caller and used by the status endpoint. Also the saga's `BulkImportId` correlation value. |
| `SeasonYear` | short | Parsed from the archive's file names. |
| `SourceZipPath` | string | The Retrosheet URL the archive was downloaded from. |
| `WorkingDirectory` | string | Where the archive was downloaded and extracted. |
| `BatchSize` | int | Effective batch size for this run. |
| `Status` | enum | `Pending`, `InProgress`, `Completed`, `CompletedWithFailures`, `Failed`. |
| `FailureReason` | string, null | Set when `Status` is `Failed` (e.g. Game Log not imported, unreadable archive, mixed seasons). |
| `CreatedUtc` | DateTime | |
| `CompletedUtc` | DateTime, null | |

`Status` semantics: `Failed` = the run never started processing files (a prerequisite or
archive problem). `Completed` = every file ended `Success` or `Skipped`.
`CompletedWithFailures` = the run finished but at least one file ended `Failed`.

### `BulkImportFile`

One row per event file discovered in the archive.

| Column | Type | Notes |
|---|---|---|
| `Id` | int, PK identity | |
| `BulkImportId` | int, FK -> `BulkImport.Id` | |
| `FileName` | string | Event file name, e.g. `2024SDN.EVN`. |
| `Status` | enum | `Pending`, `InProgress`, `Success`, `Failed`, `Skipped`. |
| `ErrorMessage` | string, null | Exception summary when `Status` is `Failed`. |
| `GamesInserted` | int, null | From `GameEventComplete` on success. |
| `GamesSkipped` | int, null | From `GameEventComplete` on success. |
| `StartedUtc` | DateTime, null | |
| `ProcessedUtc` | DateTime, null | |

Index on `(BulkImportId, FileName)` unique. Index on `(FileName)` (or a `SeasonYear` copy)
to support the rerun lookup for a file's most recent outcome.

## API

Both routes live on the existing `GameEventController` in `Retrosharp.UI.Api` (the spec
previously referred to a `Retrosharp.Engine.Api` project; the API project is
`Retrosharp.UI.Api`).

### `POST /api/gameevent/bulkimport`

Request body:

```json
{ "seasonYear": 2024, "batchSize": 10 }
```

`seasonYear` is required and range-checked (`1871`..current year + 1); `batchSize` is
optional and defaults to the configured value. On success returns `202 Accepted` with
`{ "trackingId": "<guid>" }` and places a `BulkGameEventImportStart` message on the bus.
Returns `400 Bad Request` if `seasonYear` is missing or out of range, or `batchSize` is not
positive.

Prerequisite failures (Game Log for the season not imported, the season's archive not
published on Retrosheet, a corrupt download, mixed or mismatched seasons, no event files in
the archive) are detected inside the saga, not by the endpoint: the endpoint still returns
`202` with a tracking id, and the `BulkImport` row is recorded with `Status = Failed` and a
`FailureReason` the status endpoint surfaces. A transient download failure is the exception
— it is retried and no `BulkImport` row is written until the archive is in hand.

### `GET /api/gameevent/bulkimport/{trackingId}`

Returns `200 OK` with the `BulkImport` row and its `BulkImportFile` rows:

```json
{
  "trackingId": "…", "seasonYear": 2024, "status": "CompletedWithFailures",
  "batchSize": 10, "createdUtc": "…", "completedUtc": "…", "failureReason": null,
  "counts": { "total": 30, "success": 28, "failed": 1, "skipped": 1, "pending": 0, "inProgress": 0 },
  "files": [
    { "fileName": "2024SDN.EVN", "status": "Success", "gamesInserted": 81, "gamesSkipped": 0, "processedUtc": "…" },
    { "fileName": "2024ARI.EVN", "status": "Failed", "errorMessage": "PlayCodeParseException: …", "processedUtc": "…" }
  ]
}
```

Returns `404 Not Found` if no `BulkImport` has that tracking id. Read-only; anonymous, per
[api.md](./api.md#no-authentication-in-phase-1).

## Prerequisites

1. Everything the Game Event Parser itself requires — `Person`, `League`, `Franchise`,
   `Ballpark` populated (see [game-event.md](./game-event.md#prerequisites)).
1. The target season's **Game Log** has been imported (`Game` has at least one row for the
   season). Bulk import validates this up front and refuses to proceed otherwise.
1. `Retrosharp.Engine.Console` has outbound HTTPS to `www.retrosheet.org` (or a mirror set
   via `RetrosheetSource__BaseUrl`) and a writable working root (`RetrosheetSource__WorkingRoot`,
   default: the system temp directory). No host bind mount is required. See
   [retrosheet-auto-download.md](./retrosheet-auto-download.md).

## Requirements

1. Bulk import accepts a **season year**, downloads that season's Retrosheet event archive
   (`{season}eve.zip`), and extracts the event files (`20YYTTT.EVN`/`20YYTTT.EVA`) from it
   inside `Retrosharp.Engine.Console`. Non-event entries are ignored and logged.
1. The number of files processed concurrently is configurable, default **10**.
1. Bulk import is asynchronous: the endpoint returns immediately with a unique tracking
   identifier, and processing continues in the background.
1. Bulk import is a new saga (`BulkGameEventImportSaga`) in `Retrosharp.Engine.Console`.
1. The bulk saga orchestrates the existing per-file Game Event saga — one `GameEventStart`
   per file, correlated back to the bulk run by `BulkImportId` — rather than reimplementing
   any parsing, derivation, or reconciliation.
1. Bulk import records the status of every discovered file in `BulkImportFile`. All files
   are inserted as `Pending` (or `Skipped`, per Requirement 8) before processing begins;
   each moves to `InProgress` when dispatched and to `Success` or `Failed` when it finishes,
   with the failure's exception summary stored on the row.
1. A file failing to import does not stop the run. The remaining files are still processed,
   and every failure is reported in the completion summary and on the status endpoint. The
   failed child message is still delivered to the error queue for later manual retry.
1. Re-running bulk import for a season skips files whose most recent `BulkImportFile`
   outcome for that `(SeasonYear, FileName)` is `Success` (recorded as `Skipped`), and
   reprocesses files whose most recent outcome is `Failed` or which have no prior row.
   Skipped files are reported in the completion summary.
1. Before processing, bulk import validates that the target season's Game Log is imported.
   If not, it does not proceed: the `BulkImport` row is recorded `Failed` with a
   `FailureReason` indicating the Game Log must be imported first.
1. The endpoints are `POST /api/gameevent/bulkimport` (initiate; returns the tracking id)
   and `GET /api/gameevent/bulkimport/{trackingId}` (overall status plus per-file rows), on
   `Retrosharp.UI.Api`.
1. Bulk import is part of Phase One.
1. After the run completes, each extracted file whose import succeeded is deleted from the
   working directory, and the downloaded `{season}eve.zip` is always deleted; files whose
   import failed are left in place. The working directory is removed if empty.
1. Per-file parsing, record-level idempotency, atomicity, retry/backoff, and
   `Game*Statistics`/`EarnedRuns` reconciliation are inherited unchanged from
   [game-event.md](./game-event.md) and [parser.md](parser.md); bulk import adds no new
   parsing behavior.

## Acceptance Criteria

1. A single `POST /api/gameevent/bulkimport` with `{ "seasonYear": Y }` downloads that
   season's event archive and imports every event file in it, and
   `GET /api/gameevent/bulkimport/{trackingId}` reports one `Success` row per file with
   per-file `GamesInserted`/`GamesSkipped`.
1. The batch size is honoured: with `batchSize` = N, no more than N child `GameEventStart`
   messages are outstanding at once (verifiable from `BulkImportFile.StartedUtc`/`ProcessedUtc`
   overlap and the engine logs).
1. Initiating a bulk import returns `202 Accepted` with a tracking id before any file is
   processed.
1. The run uses the existing `GameEventSaga`/`GameEventImportService` per file — no
   duplicate parsing/derivation code paths are introduced.
1. Every discovered file has a `BulkImportFile` row that progresses
   `Pending`/`Skipped` -> `InProgress` -> `Success`/`Failed`, and a `Failed` row carries the
   exception summary.
1. An archive containing one deliberately corrupt event file completes: the other files end
   `Success`, the corrupt one ends `Failed` with its error recorded, the corrupt child
   message is on the error queue, and the `BulkImport` ends `CompletedWithFailures`.
1. Re-running the same archive after a fully successful run processes zero files — every row
   is `Skipped` — and re-running after a partial run reprocesses only the previously
   `Failed` files.
1. Bulk importing a season whose Game Log has not been imported does not extract or queue
   any file; the `BulkImport` row is `Failed` with a Game-Log-first `FailureReason` visible
   on the status endpoint.
1. After a run, the working directory contains exactly the event files that ended `Failed`
   (or is gone if there were none), and the downloaded `{season}eve.zip` has been deleted.
1. A game's statistics are applied **at most once** regardless of how the archive is
   processed — the Game Event Parser's `GameEventGameStatus` claim (see
   [game-event.md](./game-event.md)) is unaffected by running under the bulk orchestrator.
   (Modern Retrosheet team files are home-games-only, so a game appears in just one file and
   the claim never actually contends; the guarantee still holds if that ever changes.)
