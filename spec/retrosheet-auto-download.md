# Automatic Retrosheet Download for the Import Process

## Overview

Every ETL entry point today takes a **file path** to data an operator has already
downloaded from Retrosheet and dropped onto a volume the engine container can read
(`POST /api/person/import`, `POST /api/gamelog/import`, `POST /api/gameevent/import`,
`POST /api/gameevent/bulkimport`). That is a long chain of manual prerequisite steps and a
non-starter for a web-facing application — a user would have to upload multi-megabyte
archives to the server before anything could run.

This document specifies removing the manual download step: the engine fetches the required
archive from Retrosheet itself, given only the **season year**. It extends
[game-log.md](./game-log.md) and [bulk-import.md](./bulk-import.md); the per-file parsing,
idempotency, atomicity, retry/backoff, and reconciliation behavior in those specs and in
[parser.md](parser.md) is inherited unchanged. Nothing about *how* a game log or an event
file is parsed changes — only *how the bytes arrive*.

It is tracked as **Step 11** in [phase-1-build-plan.md](./phase-1-build-plan.md) and depends
on Step 5 (Game Log Parser) and Step 10 (Bulk Game Event Import).

## Goals

1. The engine downloads the required archive from Retrosheet automatically; the caller
   supplies a season year, not a path.
2. `glYYYY.zip` and `YYYYeve.zip` are extracted into a working directory and imported
   exactly as a hand-placed file is today.
3. The working directory is configurable and defaults to a per-run subdirectory of the
   system temp directory, cleaned up when the run finishes.
4. No API request body carries a file path any more.
5. The Game-Log-before-Game-Event ordering requirement is unchanged: bulk Game Event import
   still refuses to run until the season's Game Log has been imported.

## Retrosheet source URLs

| Archive | URL (default) | Zip contents used |
|---|---|---|
| Season game log | `https://www.retrosheet.org/gamelogs/gl{season}.zip` | `GL{season}.TXT` (CSV despite the extension; see [game-log.md](./game-log.md)) |
| Season event archive | `https://www.retrosheet.org/events/{season}eve.zip` | `{season}TTT.EVN` / `{season}TTT.EVA` per team (see [bulk-import.md](./bulk-import.md)); all other entries ignored |
| Biofile | `https://www.retrosheet.org/downloads/biodata.zip` | `biofile0.csv` (newer 32-column format). The archive also holds the legacy `biofile.csv` and six unrelated files (coaches, umpires, managers, teams, ballparks, relatives), all ignored. The `biofile.htm` page's "Download biofile.zip" link actually points at `downloads/biodata.zip`. |

The draft used the bare `retrosheet.org` host; the site 301-redirects that to `www.`, so
the default is written with `www.` and redirects are followed regardless. The base URL and
the two path templates are configurable (see [Configuration](#configuration)) so a mirror,
an on-disk caching proxy, or a local fixture server can be substituted — this is also how
the download is tested without hitting the network.

## Considerations

### The download happens in the engine, never in the API

The existing architecture keeps `Retrosharp.UI.Api` free of all file and network I/O — it
validates the request shape and puts a message on the bus; the saga in
`Retrosharp.Engine.Console` does everything else (see [bulk-import.md](./bulk-import.md),
"The archive is delivered by path, not uploaded"). That is preserved. The API now validates
only that `seasonYear` is present and in a plausible range; the HTTP GET to Retrosheet, the
extraction, and the cleanup all happen inside the sagas. A season Retrosheet has no data for
surfaces the same way a bad path does today — a `Failed` bulk-import run on the status
endpoint, or a message on the error queue for the game log — not a `400` from the POST.

### One download client, shared by both sagas

A new `IRetrosheetArchiveClient` (in `Retrosharp.Service`, implemented over a named
`HttpClient`) is the single transport seam:

```csharp
Task<string> DownloadAsync(Uri archiveUrl, string targetDirectory, CancellationToken ct = default);
// returns the local path of the downloaded .zip
```

It streams the response to a file in `targetDirectory`, verifies the payload begins with the
ZIP local-file-header magic (`PK\x03\x04`), and maps failures to two exception types:

| Condition | Exception | Recoverability |
|---|---|---|
| `404 Not Found` (season not published) | `RetrosheetArchiveNotFoundException` | **unrecoverable** — straight to error queue / `Failed` run |
| `408`, `429`, `5xx`, connection reset, DNS failure, timeout | `RetrosheetArchiveUnavailableException` | **transient** — normal immediate + delayed retry ladder |
| body is not a zip | `InvalidDataException` | unrecoverable (already classified) |

The `HttpClient` is registered only in the engine's `Program.cs`
(`builder.Services.AddHttpClient<IRetrosheetArchiveClient, RetrosheetArchiveClient>(...)`),
not in the shared `Retrosharp.Service` `IocRegistrations` — the API host has no need of it,
the same way `BulkImportConfiguration` is an engine-only singleton. It sets a real
`User-Agent`, a configurable timeout, and `BaseAddress` from configuration.

No new retry library is introduced. A transient download failure throws; NServiceBus's
existing recoverability policy (`EngineRecoverabilityPolicy` — exponential backoff with
jitter) retries the message, which re-downloads. `ImportFailureClassifier` gains
`RetrosheetArchiveNotFoundException` in its unrecoverable set;
`RetrosheetArchiveUnavailableException` is deliberately absent so it falls through to
"retryable".

### Working directory, default, and cleanup

A per-run working directory holds the downloaded zip and everything extracted from it:

- Bulk Game Event import: `<WorkingRoot>/<trackingId>/`
- Game Log import: `<WorkingRoot>/gamelog/<requestId>/`
- Biofile import (if in scope): `<WorkingRoot>/person/<requestId>/`

`WorkingRoot` is `RetrosheetSource__WorkingRoot` when configured, otherwise
`Path.Combine(Path.GetTempPath(), "retrosharp-import")`. Under the container that is `/tmp`,
which needs no mount — a season event archive is a few MB zipped and ~40 MB extracted, and
it is deleted on a clean run.

Cleanup rules, per import type:

- **Game Log**: the working directory is deleted in a `finally` — unconditionally, on
  success or failure. Game log import is a single atomic batch with no partial state to
  investigate on disk; a failed attempt's message carries its own exception to the error
  queue. A retry re-downloads.
- **Bulk Game Event**: on finish, the downloaded zip is **always** deleted; each extracted
  event file that ended `Success` is deleted (unchanged from [bulk-import.md](./bulk-import.md));
  event files that ended `Failed` are **left in place** for investigation, and the working
  directory is removed only if it ends up empty. So a fully successful run leaves nothing;
  a run with failures leaves only the failed `.EVN`/`.EVA` files (no zip). A startup
  failure (`FailStartupAsync`) deletes the whole working directory.

This reconciles the draft's "cleaned up after the import process is complete" with
[bulk-import.md](./bulk-import.md)'s existing "cleanup removes only what succeeded": the
re-downloadable archive is always removed, failed inputs are still preserved.

### Season year is now required, and cross-checked

The season is what builds the URL, so it is mandatory on both endpoints (it was optional on
`bulkimport` before, derived from the archive's filenames). The API rejects a missing or
out-of-range year (`< 1871` or `> currentYear + 1`) with `400`. The bulk saga still runs
`EventFileArchive.TryResolveSeason` over the downloaded archive's entries and fails the run
if the season encoded in the filenames does not match the requested season — this now
doubles as a guard that Retrosheet served the archive that was actually asked for.

### Bulk import: transient download failure at startup does not fail the run

[bulk-import.md](./bulk-import.md) fails the whole run on any archive problem found in
`Handle(BulkGameEventImportStart)`. That stays true for *unrecoverable* problems (season not
published, corrupt zip, mixed seasons, Game Log not imported). A *transient* download error
is different: `RetrosheetArchiveUnavailableException` is allowed to propagate out of the
handler so recoverability retries `BulkGameEventImportStart`. The existing
`Data.ProcessingStarted` guard already makes that handler idempotent, and no `BulkImport`
row is written until the download and validation succeed, so a retried start is clean.

### `SourceZipPath` now holds the source URL

The `BulkImport.SourceZipPath` column previously recorded the operator-supplied path. It now
records the Retrosheet URL the archive was fetched from. The column is reused as-is (no
migration, no rename) — it is a provenance/audit field on the row, and a URL is the natural
value for it now. It is not exposed by `GET /api/gameevent/bulkimport/{trackingId}`
(`BulkImportStatusResponse` has never carried it); it is visible only in the database.

## Configuration

New `RetrosheetSourceConfiguration` in `Retrosharp.Configuration`, bound from a
`RetrosheetSource` section (and `RetrosheetSource__*` environment variables), following the
`MessagingConfiguration.Instance()` / `BulkImportConfiguration.Instance()` pattern:

| Key | Default | Meaning |
|---|---|---|
| `BaseUrl` | `https://www.retrosheet.org/` | Root for all archive fetches. |
| `GameLogArchivePath` | `gamelogs/gl{season}.zip` | Path template; `{season}` is substituted. |
| `EventArchivePath` | `events/{season}eve.zip` | Path template. |
| `BioFileArchivePath` | `downloads/biodata.zip` | Path (no `{season}`). |
| `WorkingRoot` | `""` → `%TEMP%/retrosharp-import` | Parent of every per-run working directory. |
| `HttpTimeoutSeconds` | `100` | `HttpClient.Timeout`. |

`BulkImportConfiguration.ExtractionRoot` is **removed**; `WorkingRoot` replaces it. The
`DefaultBatchSize` and `WatchdogTimeoutHours` keys on `BulkImportConfiguration` are
unchanged. `appsettings.json`, `.env.example`, and `docs/deployment.md` are updated
accordingly.

## Operational change: outbound network

The engine container now needs egress to `www.retrosheet.org:443` (or whatever `BaseUrl`
points at). Previously the ETL path was fully air-gappable once files were staged. Deployments
that restrict outbound traffic must allow that host, or point `BaseUrl` at an internal
mirror. This is called out in `docs/deployment.md`.

The `./data/retrosheet` bind mount on `retrosharp-engine-console` in `docker-compose.yml`
is **removed** — there are no operator-supplied input files any more. An operator who wants
downloads to persist across restarts or to pre-seed an offline cache can mount a volume and
set `RetrosheetSource__WorkingRoot` to it, but nothing requires it. The
`data/retrosheet/.gitkeep` placeholder and the matching `.gitignore` entries are removed.

## API changes

### `POST /api/gamelog/import`

Request body becomes:

```json
{ "seasonYear": 2024 }
```

`FilePath` is removed from `GameLogImportRequest` and from the `GameLogStart` message.
`seasonYear` is required and range-checked (`400` otherwise). Response is unchanged
(`202 Accepted` with `{ requestId }`).

### `POST /api/gameevent/bulkimport`

Request body becomes:

```json
{ "seasonYear": 2024, "batchSize": 10 }
```

`zipPath` is removed from `BulkGameEventImportRequest` and from the `BulkGameEventImportStart`
message. `seasonYear` is required and range-checked (`400`); `batchSize` stays optional.
The `202` response and `GET /api/gameevent/bulkimport/{trackingId}` payload are unchanged —
the download URL is recorded on the `BulkImport` row (`SourceZipPath`) but not surfaced by
the status endpoint.

### `POST /api/gameevent/import` (single file)

**Retired.** See [Open Decisions](#open-decisions) — Retrosheet publishes no per-team file,
so there is no season-less download equivalent, and re-importing one failed file is already
covered by re-running the bulk import (it reprocesses only `Failed` and new files). The
`GameEventStart` message and `GameEventSaga` are unchanged — the bulk saga still drives them
per extracted file. Only the controller action and `GameEventImportRequest` are removed.

### `POST /api/person/import`

Request body removed entirely. The engine downloads `downloads/biodata.zip`, extracts
`biofile0.csv`, imports it, and cleans up. `FilePath` is removed from
`PersonImportRequest` (deleted), `PersonStart`, and `PersonSagaData`.

## Data Model

No schema change. `BulkImport.SourceZipPath` is repurposed to hold the source URL (see
Considerations). Saga-data classes lose their `FilePath` fields (`GameLogSagaData.FilePath`);
saga data is a JSONB blob, so there is no persistence migration, matching how
`GameEventSagaData.BulkImportId` was added in Step 10.

## Requirements

1. Given only a season year, the Game Log import downloads `gl{season}.zip` from Retrosheet,
   extracts `GL{season}.TXT`, and imports it exactly as [game-log.md](./game-log.md)
   specifies.
2. Given only a season year (plus optional batch size), the bulk Game Event import downloads
   `{season}eve.zip` from Retrosheet, extracts the team event files, and imports them exactly
   as [bulk-import.md](./bulk-import.md) specifies.
3. The download client resolves URLs from configurable templates, follows redirects, sets a
   `User-Agent`, and maps a `404` to an unrecoverable error and transient HTTP/network
   failures to a retryable error.
4. Downloading and extraction happen only in `Retrosharp.Engine.Console`. The API validates
   `seasonYear` presence and range and returns `400` on failure; every other failure is
   surfaced through the existing async status/error-queue mechanisms.
5. The working directory is `RetrosheetSource__WorkingRoot` when set, otherwise a per-run
   subdirectory of the system temp directory.
6. Game Log import deletes its working directory unconditionally when the handler finishes.
7. Bulk Game Event import always deletes the downloaded zip on finish, deletes successfully
   imported event files, keeps failed event files, and removes the working directory only if
   empty; a startup failure deletes the working directory entirely.
8. The Game-Log-imported precheck in the bulk saga is unchanged and still runs before any
   file is queued.
9. A transient download failure in `Handle(BulkGameEventImportStart)` propagates for retry
   and does not create a `Failed` `BulkImport` row; an unrecoverable one creates a `Failed`
   row with a `FailureReason`, as today.
10. No request body accepted by any controller contains a file path. (Subject to the Person
    endpoint decision.)
11. `docker-compose.yml` no longer bind-mounts a host directory into the engine for input
    files; `docs/deployment.md` documents the new outbound-network requirement.

## Acceptance Criteria

1. `POST /api/gamelog/import` with `{ "seasonYear": <Y> }` and no file staged anywhere
   imports season `<Y>`'s game log, and the working directory does not exist afterward.
2. `POST /api/gameevent/bulkimport` with `{ "seasonYear": <Y> }` and no file staged imports
   every team event file for `<Y>` in batches, with the same per-file `Success` rows and
   counts [bulk-import.md](./bulk-import.md) requires.
3. A season Retrosheet has no game log for produces a message on the error queue whose
   exception is `RetrosheetArchiveNotFoundException`; no retry storm precedes it.
4. A season Retrosheet has no event archive for produces a `BulkImport` row with
   `status: "Failed"` and a `failureReason` naming the missing archive, and zero
   `BulkImportFile` rows.
5. A simulated `503` from the archive host causes the import message to be retried on the
   normal delayed-retry ladder and to succeed once the host recovers, with no operator
   action.
6. After a fully successful bulk run the working directory is gone (no zip, no event files);
   after a bulk run with one corrupt file, the working directory contains exactly that one
   `.EVN`/`.EVA` file and no zip.
7. `POST /api/gameevent/bulkimport` with `{ "seasonYear": 1300 }` returns `400`; with a body
   that omits `seasonYear`, returns `400`.
8. Requesting season `<Y>` but with the archive host configured to return season `<Y-1>`'s
   archive fails the run with a season-mismatch `failureReason`.
9. No controller request DTO in `Retrosharp.UI.Api` has a `FilePath`/`ZipPath` property
   (subject to the Person decision), and `grep` for staged-file instructions in
   `docs/deployment.md` finds none.
10. The full existing unit-test suite passes, plus new coverage for the download client, the
    game-log archive extraction, the classifier entries, and the two sagas' download paths.

## Open Decisions

**All three resolved (2026-09-07): D1 confirmed (retire), D2 confirmed (add Person
download), D3 confirmed (keep the season cross-check).** The plan below is authoritative.

### D1 — `POST /api/gameevent/import` (single event file) — CONFIRMED: retire

**Recommendation: retire it.** Retrosheet ships event files only inside the season
`YYYYeve.zip`, so there is no path-free single-file download. Re-importing an individual
failed file is already served by re-running `POST /api/gameevent/bulkimport` for the season
— [bulk-import.md](./bulk-import.md)'s rerun rules reprocess only `Failed` and previously
unseen files. Keeping a `{ seasonYear, teamCode }` variant that downloads the whole season
zip to extract one file is possible but wasteful and adds a second code path; not
recommended.

### D2 — `POST /api/person/import` (biofile) — CONFIRMED: add download

**Confirmed and implemented in 11f.** Goal 4 ("no request body carries a file path") covers
it, it is the same ~30-line pattern, and removing it clears the last manual-staging step so
a fresh environment is seeded with three argument-light calls. Archive layout was verified
against `retrosheet.org/biofile.htm`: the "Download biofile.zip" link points at
`https://www.retrosheet.org/downloads/biodata.zip`, which contains eight files; only
`biofile0.csv` (the newer 32-column format the parser already targets) is extracted, the
legacy `biofile.csv` and the six unrelated files are ignored — so `BioFileService` needs no
change, it still parses one CSV.

### D3 — Keep `EventFileArchive.TryResolveSeason` season-mismatch rejection — CONFIRMED: keep

**Recommendation: keep it.** It is cheap and now also detects a wrong archive served by the
host. No change needed; noted only because the season is no longer *derived* from filenames,
only *checked* against them.

---

# Implementation Plan

Ordered so the solution builds and every existing test passes after each step. New tests
land with the step that introduces the behavior. "Live E2E" is deferred to Step 11h.

### 11a — Configuration and exception types

- **Add** `Retrosharp.Configuration/RetrosheetSourceConfiguration.cs` — properties and
  `Instance()` per the table in [Configuration](#configuration), plus helpers
  `Uri GameLogUri(int season)`, `Uri EventArchiveUri(int season)`, `Uri BioFileUri()` that
  compose `BaseUrl` + the substituted template.
- **Defer** removing `ExtractionRoot` from `BulkImportConfiguration` to 11e (the step that
  switches `BulkGameEventImportSaga` to `RetrosheetSourceConfiguration.WorkingRoot`), so the
  solution keeps building after every intervening step. 11a only *adds*.
- **Add** `Retrosharp.Service.Interface/ETL/RetrosheetArchiveExceptions.cs` —
  `RetrosheetArchiveNotFoundException`, `RetrosheetArchiveUnavailableException` (both
  `: Exception`, neither derived from `IOException`/`InvalidOperationException`).
- **Update** `src/engine/Retrosharp.Engine.Console/appsettings.json`: drop
  `BulkImport.ExtractionRoot`, add a `RetrosheetSource` section with the defaults.
- **Update** `.env.example`: replace the `BulkImport__ExtractionRoot` note with
  `RetrosheetSource__BaseUrl` / `RetrosheetSource__WorkingRoot` / `RetrosheetSource__HttpTimeoutSeconds`.
- Tests: `RetrosheetSourceConfigurationTests` (URL composition for a few seasons; template
  override; `WorkingRoot` empty → temp path).

### 11b — `IRetrosheetArchiveClient` + `RetrosheetArchiveClient`

- **Add** `Retrosharp.Service.Interface/ETL/IRetrosheetArchiveClient.cs` — the
  `DownloadAsync(Uri, string targetDirectory, CancellationToken)` signature above.
- **Add** `Retrosharp.Service/ETL/RetrosheetArchiveClient.cs` — constructor takes
  `HttpClient` + `ILogger`. Streams to `Path.Combine(targetDirectory, <fileName from URL>)`,
  creating the directory; checks the `PK\x03\x04` magic; throws the mapped exception on
  non-success / network fault / non-zip.
- **Wire** in `src/engine/Retrosharp.Engine.Console/Program.cs`:
  `builder.Services.AddSingleton(RetrosheetSourceConfiguration.Instance());` and
  `builder.Services.AddHttpClient<IRetrosheetArchiveClient, RetrosheetArchiveClient>(c => { c.BaseAddress = cfg.BaseUrl; c.Timeout = ...; c.DefaultRequestHeaders.UserAgent.ParseAdd("Retrosharp/1.0 (+https://github.com/…)"); });`
  Do **not** add it to `Retrosharp.Service/IocRegistrations.cs` (shared with the API).
- Tests: `RetrosheetArchiveClientTests` with a stub `HttpMessageHandler` — 200+zip → file
  written, returned path exists; 404 → `RetrosheetArchiveNotFoundException`; 503/timeout/
  `HttpRequestException` → `RetrosheetArchiveUnavailableException`; 200 + non-zip body →
  `InvalidDataException`.

### 11c — Game-log archive extraction helper

- **Add** `src/engine/Retrosharp.Engine.Console/Saga/GameLogArchive.cs` (mirrors
  `EventFileArchive`, `internal static`): `string Extract(string zipPath, string targetDir)`
  — finds the single entry matching `^gl\d{4}\.txt$` (case-insensitive), extracts it
  (flattened, overwrite), returns the local path; throws `InvalidDataException` if there is
  not exactly one match.
- Tests: `GameLogArchiveTests` — extracts `GL2024.TXT` / `gl2024.txt`; ignores sibling
  entries; missing / multiple → `InvalidDataException`.

### 11d — Game Log saga: download, extract, clean up

- **`GameLogController.Import`**: request DTO → `{ int SeasonYear }`; validate presence and
  `1871 <= SeasonYear <= DateTime.UtcNow.Year + 1` → `BadRequest`; send
  `GameLogStart { RequestId, SeasonYear }`.
- **`GameLogStart`**: remove `FilePath`.
- **`GameLogSagaData`**: remove `FilePath`; keep `SeasonYear`.
- **`GameLogSaga.Handle(GameLogStart)`**: inject `IRetrosheetArchiveClient` +
  `RetrosheetSourceConfiguration`. Resolve `workingDir = <WorkingRoot>/gamelog/<RequestId:N>`.
  `try { zip = await client.DownloadAsync(cfg.GameLogUri(season), workingDir); txt = GameLogArchive.Extract(zip, workingDir); result = await _gameLogImportService.ImportAsync(txt, season); await context.SendLocal(new GameLogComplete{…}); } finally { TryDeleteDirectory(workingDir); }`.
  A download/parse exception propagates out of `try` (cleanup still runs in `finally`) into
  the recoverability policy, exactly as a bad path does today.
- Add a private recursive `TryDeleteDirectory` (log-and-swallow), or a shared
  `Saga/WorkingDirectory.cs` helper used here and by the bulk saga.
- Tests: update `GameLogSagaTests` — fake `IRetrosheetArchiveClient` (returns a fixture zip
  path) and a fake/real `GameLogArchive` input; assert the URI requested matches the season,
  `GameLogComplete` is sent with the service's counts, and the working directory is deleted.
  Add: download throws `RetrosheetArchiveNotFoundException` → propagates (no
  `GameLogComplete`); `finally` still deletes the directory.

### 11e — Bulk Game Event saga: download first, then the existing pipeline

- **`GameEventController.BulkImport`**: request DTO → `{ int SeasonYear, int? BatchSize }`;
  validate `SeasonYear` presence + range and `BatchSize` positivity → `BadRequest`; send
  `BulkGameEventImportStart { RequestId, BulkImportId, SeasonYear, BatchSize }` (no
  `ZipPath`).
- **`BulkGameEventImportStart`**: remove `ZipPath`; `SeasonYear` stays `int?` on the message
  but is always populated by the controller (keep nullable to avoid churning the message
  contract / serializer; the saga treats absence as a validation failure defensively).
- **`BulkGameEventImportSaga`**: inject `IRetrosheetArchiveClient` +
  `RetrosheetSourceConfiguration`; drop `BulkImportConfiguration.ExtractionRoot` usage.
  - `ResolveWorkingDirectory(trackingId)` → `<WorkingRoot>/<trackingId:N>`.
  - New first action in `Handle(BulkGameEventImportStart)` (after the `ProcessingStarted`
    guard, before `ListEventFiles`):
    ```
    string localZip;
    try { localZip = await _archiveClient.DownloadAsync(_source.EventArchiveUri(season), workingDirectory, ct); }
    catch (RetrosheetArchiveNotFoundException ex) { await FailStartupAsync(…, $"The event archive for season {season} could not be downloaded: {ex.Message}"); return; }
    catch (InvalidDataException ex)              { await FailStartupAsync(…, $"The downloaded event archive for season {season} is not a valid zip: {ex.Message}"); return; }
    // RetrosheetArchiveUnavailableException is NOT caught -> propagates -> message retried
    ```
    `season` here comes from `message.SeasonYear` (validate it is present/plausible first;
    fail startup if not). The subsequent `EventFileArchive.ListEventFiles` / `TryResolveSeason`
    / requested-season cross-check / Game-Log check / seed / extract all run against
    `localZip` unchanged.
  - `CreateAsync(new ContractBulkImport { … SourceZipPath = _source.EventArchiveUri(season).ToString(), WorkingDirectory = workingDirectory, … })`.
  - `FinishAsync`: before the existing `TryRemoveEmptyDirectory`, add
    `TryDelete(localZipPathStoredInSagaData)` — always. (Store the local zip path in
    `BulkGameEventImportSagaData` so `FinishAsync`/`Timeout` can find it.)
  - `FailStartupAsync`: add `TryDeleteDirectory(workingDirectory)` at the end.
- **`BulkGameEventImportSagaData`**: add `string DownloadedArchivePath`.
- Tests: update `BulkGameEventImportSagaTests` — inject a fake `IRetrosheetArchiveClient`.
  New/changed cases: happy path downloads from `EventArchiveUri(season)` then behaves as the
  existing happy-path test; `RetrosheetArchiveNotFoundException` → `Failed` row + reason, no
  files; `RetrosheetArchiveUnavailableException` → exception propagates, `CreateAsync` never
  called; `FinishAsync` deletes the downloaded zip; season in filenames ≠ requested season →
  `Failed`. Keep all existing dispatch/rerun/watchdog tests (now with the fake client
  returning a fixture zip).

### 11f — Person biofile download *(Open Decision D2 — implement or drop as a unit)*

- **`PersonController.Import`**: remove `FilePath`; accept an empty body; send
  `PersonStart { RequestId }` (drop `FilePath`).
- **`PersonStart` / `PersonSagaData`**: remove `FilePath`.
- **`PersonSaga.Handle(PersonStart)`**: download `cfg.BioFileUri()` into
  `<WorkingRoot>/person/<RequestId:N>`, extract the `biofile*.csv` entry/entries, import,
  `finally`-delete the directory.
- **Confirm** `biofile.zip`'s internal layout; if multiple CSVs, extend `BioFileService` /
  the saga to parse each and concatenate the record sets.
- **Update** `docs/deployment.md` to drop the `POST /api/Person/import` file-path
  instruction.
- Tests: `PersonSagaTests` updated the same way as `GameLogSagaTests`.

### 11g — Recoverability classification

- **`ImportFailureClassifier.IsUnrecoverable`**: add `RetrosheetArchiveNotFoundException` to
  the unrecoverable set; extend the XML-doc comment to explain that
  `RetrosheetArchiveUnavailableException` is intentionally omitted (transient host/network
  failure, retry can fix it).
- Tests: `ImportFailureClassifierTests` — `RetrosheetArchiveNotFoundException` →
  unrecoverable; `RetrosheetArchiveUnavailableException` (bare and wrapped) → recoverable.
  `EngineRecoverabilityPolicyTests` — a matching pair asserting `MoveToError` vs the
  immediate/delayed ladder.

### 11h — Compose, deployment docs, spec cross-references

- **`docker-compose.yml`**: remove the `volumes: - ./data/retrosheet:/data/retrosheet`
  block and its comment from `retrosharp-engine-console`. Confirm `docker compose config`
  still validates. Check `docker-compose.pi.yml` for the same mount.
- **Remove** `data/retrosheet/.gitkeep` and the `/data/retrosheet/*` /
  `docs/csv/**/*eve.zip` etc. entries in `.gitignore` that only existed for staged inputs
  (leave anything still referenced by tests).
- **`docs/deployment.md`**: rewrite the "Retrosheet source files" and "Bulk Game Event
  import" sections — no staging, request bodies are `{ "seasonYear": … }`, the engine needs
  outbound HTTPS to `www.retrosheet.org`, `RetrosheetSource__*` tuning.
- **`spec/game-log.md`**: note the file arrives by download; the request is a season year.
- **`spec/bulk-import.md`**: update "The archive is delivered by path" → "by download";
  "Extraction target" → temp-dir default; `SourceZipPath` holds the URL; the POST body and
  Acceptance Criteria 1/8 wording.
- **`spec/api.md`**: adjust the ETL-endpoint mentions (`PersonController`/`GameLogController`/
  `GameEventController`) and remove the retired single-import route.
- **`spec/phase-1-build-plan.md`**: add the **Step 11** entry (objective, deliverables,
  dependencies on Steps 5 and 10, definition of done = this document's Acceptance Criteria),
  and add the line to the step-ordering note near the top.

### 11i — Verification

- Full solution build; entire existing unit suite green plus the new tests from 11a–11g.
- Migrations unaffected (no schema change) — a quick `Retrosharp.Data.Migration` run against
  a scratch DB to confirm nothing regressed.
- **Live E2E** against real Retrosheet, real Postgres + RabbitMQ:
  1. `POST /api/gamelog/import { "seasonYear": 2023 }` with nothing staged → season 2023
     game log imported; temp working dir gone afterward.
  2. `POST /api/gameevent/bulkimport { "seasonYear": 2023 }` → 202 + trackingId; poll to
     `Completed`; 30 `Success` rows; temp dir gone; `sourceZipPath` shows the URL.
  3. `POST /api/gameevent/bulkimport { "seasonYear": 1876 }` (no event archive) → `Failed`
     row with a missing-archive `failureReason`.
  4. `POST /api/gamelog/import { "seasonYear": 2099 }` → error queue,
     `RetrosheetArchiveNotFoundException`, no retry storm.
  5. Point `RetrosheetSource__BaseUrl` at a local server that first returns `503` then
     proxies through → confirm delayed-retry recovery.
  6. Re-run (2) for 2023 → every file `Skipped` (rerun rules intact); temp dir cleaned.
- Update the memory note ([[bulk-import-implementation]]) and mark build-plan Step 11
  complete.

## Files touched (summary)

**New**
- `src/lib/Retrosharp/Configuration/RetrosheetSourceConfiguration.cs`
- `src/lib/Retrosharp.Service.Interface/ETL/IRetrosheetArchiveClient.cs`
- `src/lib/Retrosharp.Service.Interface/ETL/RetrosheetArchiveExceptions.cs`
- `src/lib/Retrosharp.Service/ETL/RetrosheetArchiveClient.cs`
- `src/engine/Retrosharp.Engine.Console/Saga/GameLogArchive.cs`
- `src/engine/Retrosharp.Engine.Console/Saga/WorkingDirectory.cs` *(optional shared cleanup helper)*
- Test files: `RetrosheetSourceConfigurationTests`, `RetrosheetArchiveClientTests`,
  `GameLogArchiveTests` (+ edits to `GameLogSagaTests`, `BulkGameEventImportSagaTests`,
  `ImportFailureClassifierTests`, `EngineRecoverabilityPolicyTests`, `PersonSagaTests`).

**Modified**
- `src/lib/Retrosharp/Configuration/BulkImportConfiguration.cs` (drop `ExtractionRoot`)
- `src/lib/Retrosharp/Message/GameLog/GameLogStart.cs` (drop `FilePath`)
- `src/lib/Retrosharp/Message/GameEvent/BulkGameEventImportStart.cs` (drop `ZipPath`)
- `src/lib/Retrosharp/Message/Person/PersonStart.cs` (drop `FilePath` — D2)
- `src/engine/Retrosharp.Engine.Console/Program.cs` (`AddHttpClient`, config singleton)
- `src/engine/Retrosharp.Engine.Console/Saga/GameLogSaga.cs` / `GameLogSagaData.cs`
- `src/engine/Retrosharp.Engine.Console/Saga/BulkGameEventImportSaga.cs` / `BulkGameEventImportSagaData.cs`
- `src/engine/Retrosharp.Engine.Console/Saga/PersonSaga.cs` / `PersonSagaData.cs` (D2)
- `src/engine/Retrosharp.Engine.Console/Saga/ImportFailureClassifier.cs`
- `src/engine/Retrosharp.Engine.Console/appsettings.json`
- `src/ui/Retrosharp.UI.Api/Controllers/GameLogController.cs`
- `src/ui/Retrosharp.UI.Api/Controllers/GameEventController.cs` (require season; retire single import)
- `src/ui/Retrosharp.UI.Api/Controllers/PersonController.cs` (D2)
- `docker-compose.yml`, `docker-compose.pi.yml`, `.gitignore`, `.env.example`
- `docs/deployment.md`
- `spec/game-log.md`, `spec/bulk-import.md`, `spec/api.md`, `spec/phase-1-build-plan.md`

**Removed**
- `data/retrosheet/.gitkeep`
- `GameEventController.Import` action + `GameEventImportRequest` (D1)
