# Bulk Import QA — 2023 Season

## Overview

Manual acceptance pass for bulk Game Event import (`spec/bulk-import.md`), exercised end
to end against the 2023 Retrosheet season through the public API, the way a user would.

"Bulk import" = `POST /api/gameevent/bulkimport` with a path to a season's zip archive of
team-season event files. It returns immediately with a tracking id; the archive is read,
validated, and processed asynchronously by `BulkGameEventImportSaga` in
`Retrosharp.Engine.Console`, which drives the existing per-file Game Event saga in batches.

## Prerequisites

1. **Postgres and RabbitMQ running** (the standalone dev containers are fine).
1. **Schema current.** Run `Retrosharp.Data.Migration` against the target database — this
   applies the `AddBulkImport` migration (`BulkImport` / `BulkImportFile` tables) and the
   `BulkGameEventImportSaga` NServiceBus persistence table, and seeds `Franchise` / `Ballpark`.
1. **Person data covering 2023 personnel imported** via `POST /api/person/import` with the
   Retrosheet biofile path. A dev database that already imported 2024/2025 very likely
   already has it; a `Person` the Game Log or Game Event parser can't resolve fails that
   game's import.
1. **`Retrosharp.Engine.Console` and `Retrosharp.UI.Api` running with default configuration**
   (locally via `dotnet run --no-launch-profile` with `DOTNET_ENVIRONMENT=Development` /
   `ASPNETCORE_ENVIRONMENT=Development`, or via `docker compose up`).

Default config in play: batch size **10**, watchdog timeout **6 h**, and the archive is
extracted into an `_bulk-import/<trackingId>/` folder next to the source zip
(`BulkImport__ExtractionRoot` unset).

## Data files

| Purpose | Path |
|---|---|
| Game log | `D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2023\gamelog\gl2023.txt` |
| Event archive | `D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2023\gameevent\2023eve.zip` |

These host paths are passed straight to the endpoints when the engine runs **locally**.
Under **docker-compose** the engine container only mounts `./data/retrosheet`, so copy the
files under `data/retrosheet/2023/…` on the host and pass the corresponding
`/data/retrosheet/2023/…` container paths instead.

`docs/csv/**` is git-ignored; the files above must be downloaded from retrosheet.org first.

## How to drive a case

All steps are plain HTTP:

- **Start bulk import** — `POST /api/gameevent/bulkimport`
  `{ "zipPath": "<archive path>" }` (optionally `"seasonYear"`, `"batchSize"`).
  Response: `202 Accepted` `{ "trackingId": "<guid>" }`.
- **Poll** — `GET /api/gameevent/bulkimport/{trackingId}` until `status` is one of
  `Completed`, `CompletedWithFailures`, `Failed`.
- **Import the game log** (between Case 1 and the rest) — `POST /api/gamelog/import`
  `{ "filePath": "<gl2023.txt path>", "seasonYear": 2023 }`; it is asynchronous, so confirm
  from the engine log or from `Game` rows for 2023 before continuing.

## How to observe

- **Status endpoint** — overall `status`, `failureReason`, `counts`
  (`total`/`pending`/`inProgress`/`success`/`failed`/`skipped`), and a `files[]` row per
  event file with `status`, `errorMessage`, `gamesInserted`, `gamesSkipped`, timestamps.
- **Engine console log** — per-file start/finish, the run summary line, and
  `"Notified bulk import … failed and was moved to the error queue"` when the error-queue
  hook fires.
- **Database** — `BulkImport` / `BulkImportFile`; and `GameEvent` / `Batting` / `Pitching` /
  `Fielding` / `GameEventGameStatus` for successfully imported games.
- **RabbitMQ** — `Retrosharp.Engine.Errors`: a failed per-file `GameEventStart` lands here
  (with its full exception) for a manual retry; bulk import does not consume it.
- **Working directory** — `…/_bulk-import/<trackingId>/`: files that imported successfully
  are deleted, failed files are left in place, and the folder is removed once empty.

## Test sequence

The cases are ordered. **Case 1 must run before the 2023 game log is imported**; import the
game log after it; run Cases 2–5 after that. Cases 2 and 3 use a **small hand-built
archive** (a handful of 2023 event files, one corrupted) so they stay fast and leave the
full season available for Case 4's clean happy-path run.

### Case 1 — Game Log not yet imported

- **Input:** `POST /api/gameevent/bulkimport` with `2023eve.zip`, before `gl2023.txt` has
  been imported.
- **Expected:** the POST returns `202` with a tracking id. The run ends **`status: "Failed"`**
  with `failureReason` ≈ *"The Game Log for season 2023 has not been imported. Import it
  first, then retry the bulk import."*; `files` is empty and every count is `0`. Nothing is
  extracted and no `GameEventStart` is dispatched. (The failure is reported on the run, not
  as an error from the POST — consistent with the other ETL endpoints.)

> After Case 1, import the 2023 game log (`POST /api/gamelog/import`, see above) and wait
> for it to finish. This is a prerequisite for Cases 2–5.

### Case 2 — Isolated import failure

- **Input:** a small archive built from ~3–4 real 2023 event files with **one file
  corrupted** so its import is guaranteed to fail (e.g. delete the game's `info` records, or
  truncate the file mid-game). `POST /api/gameevent/bulkimport` with that archive.
- **Expected:** the run ends **`status: "CompletedWithFailures"`**. The corrupted file's row
  is `Failed` with a populated `errorMessage`; every other file is `Success` with
  `gamesInserted` / `gamesSkipped`. The corrupted file's `GameEventStart` is on the
  `Retrosharp.Engine.Errors` queue. In the working directory the successful files' extracted
  copies are deleted and the corrupted file remains. `GameEvent` / `Batting` / … rows exist
  for the successful games only.
- **Note:** a malformed event file currently runs the full immediate + delayed retry ladder
  before reaching the error queue. For a quicker run set `Messaging__DelayedRetries=0`
  (and optionally `Messaging__ImmediateRetries=1`) on the engine; otherwise expect a few
  minutes per corrupted file.

### Case 3 — Recovering from a failure

- **Input:** replace the corrupted file in Case 2's archive with the correct version and
  `POST /api/gameevent/bulkimport` again (same season).
- **Expected:** the run ends **`status: "Completed"`**. The previously-`Failed` file is
  re-processed and ends `Success`; the previously-`Success` files are `Skipped`
  (`counts.skipped == counts.total - 1`). No duplicate `GameEvent` rows — the per-game
  `GameEventGameStatus` claim makes re-processing idempotent. The stale error-queue message
  from Case 2 is now orphaned (its run is finished) and can be deleted.

### Case 4 — Game Log imported, happy path

- **Input:** the unmodified full `2023eve.zip`, `POST /api/gameevent/bulkimport`, against a
  2023 season with no event data yet imported. (If Cases 2/3 imported part of 2023, reset it
  first — see below — or treat those files as expected `Skipped` in this run.)
- **Expected:** the run ends **`status: "Completed"`**; every file is `Success` with counts;
  all extracted files are deleted and the working directory is removed. The Game Event
  Parser may log reconciliation *warnings* (its derived team totals vs. the Game Log
  Parser's `Game*Statistics`, per `spec/game-event.md`) — these are warnings, not failures.
  A game that appears in two teams' files has its statistics applied exactly once.

### Case 5 — Re-import the same season's game event data

- **Input:** `2023eve.zip` again, after a successful full import (Case 4).
- **Expected:** the run ends **`status: "Completed"`** essentially immediately; every
  `BulkImportFile` row is `Skipped`; `counts.skipped == counts.total`; no `GameEventStart`
  is dispatched; `GameEvent` / `Batting` / `Pitching` / `Fielding` row counts are unchanged.

## Resetting between runs

To re-run a case from a clean slate:

1. Delete the season's `BulkImport` rows (`BulkImportFile` rows cascade).
1. Purge the `Retrosharp.Engine` and `Retrosharp.Engine.Errors` queues.
1. For a genuine happy-path re-run of an already-imported season, also clear that season's
   `GameEventGameStatus` rows **and** the `Batting` / `Pitching` / `Fielding` contributions
   those games made (clearing only `GameEventGameStatus` would let a re-run double-count).
   Otherwise just expect `Skipped` results on re-runs.

## Revision notes

Changes from the original draft, and why:

- **Case 1 expected output corrected.** The endpoint does not "error out" — it returns
  `202` + a tracking id like the other ETL endpoints; the prerequisite failure surfaces as
  a `Failed` run with a `failureReason` on the status endpoint.
- **Cases 2 & 4 wording corrected.** A run with a failed file ends `CompletedWithFailures`,
  not "without any errors" — the failure is isolated per file and recorded, and the failed
  child message is kept on the error queue.
- **Added the game-log import step** between Case 1 and the rest — Case 1 requires it
  absent, Cases 4–5 require it present, and it is itself an API call.
- **Added prerequisites** (schema migration for the new tables + saga persistence, Person
  data, running services, default config values).
- **Scoped Cases 2 & 3 to a small custom archive** so they don't import most of the season
  and invalidate Case 4's clean happy-path run; added a reset section.
- **Added "How to drive a case" / "How to observe"** so results are checked against the
  status endpoint, engine log, DB tables, error queue, and working directory rather than
  "without any errors".
- **Clarified the file paths** for local vs. docker-compose runs (only `data/retrosheet` is
  mounted into the engine container) and that `docs/csv/**` is git-ignored.
- **Noted** that a malformed event file currently retries the full recoverability ladder
  before failing (`ImportFailureClassifier` does not yet treat `EventFileParseException` as
  unrecoverable), so Case 2 is slow unless retry counts are lowered.
