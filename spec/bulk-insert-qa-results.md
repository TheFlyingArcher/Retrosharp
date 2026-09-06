# Bulk Import QA — 2023 Season — Results

Executed per [bulk-insert-qa.md](./bulk-insert-qa.md) on 2026-09-06.

## Environment

- `Retrosharp.UI.Api` and `Retrosharp.Engine.Console` run locally (`dotnet run`,
  `DOTNET_ENVIRONMENT=Development` / `ASPNETCORE_ENVIRONMENT=Development`) against the
  dockerised `retrosharp-postgres` and `rabbitmq` dev containers.
- Engine started with `Messaging__DelayedRetries=0` / `Messaging__ImmediateRetries=1` so the
  corrupted-file cases fail fast (per the QA doc's note). All other config default:
  `BulkImport__DefaultBatchSize=10`, `BulkImport__WatchdogTimeoutHours=6`,
  `BulkImport__ExtractionRoot` unset.
- Schema current (`Retrosharp.Data.Migration` applied — `AddBulkImport` +
  `BulkGameEventImportSaga` persistence table). `Person` already held 26,961 rows from prior
  2024/2025 imports; no separate 2023 biofile import was needed (coverage was complete).
- Data: `docs/csv/2023/gamelog/gl2023.txt` (2430 game rows),
  `docs/csv/2023/gameevent/2023eve.zip` (30 team event files + 30 `.ROS` files, which the
  saga correctly ignores).

## Result summary

| Case | As run | After remediation |
|---|---|---|
| 1 — Game Log not imported | **PASS** | — |
| Prereq — import 2023 Game Log | PASS (2430 added, 0 skipped, no `Person` resolution failures) | — |
| 2 — Isolated import failure | **PASS** | — |
| 3 — Recovering from a failure | **PASS** | — |
| 4 — Happy path, full season | **FAIL** — findings A + B | **PASS** — full import ends `Completed`, every file `Success`, 2430/2430 games have play-by-play |
| 5 — Re-import an already-imported season | **FAIL** — findings A + B | **PASS** — all 30 files `Skipped`, `Completed`, error queue empty |

Three findings, all remediated:

- **Finding B** (bulk import, in scope) — rerun-skip logic; fixed and re-verified during the run.
- **Finding A** (Game Event Parser, out of scope) — `(RBI)` play-code annotation; analysed
  and remediated below, then verified end to end.
- **Finding D** (spec accuracy, out of scope) — team event files are home-games-only; the
  spec text corrected.

All 276 unit tests pass. The dev database now holds a complete 2023 season
(2430 games, all with `GameEvent` play-by-play).

## Case results

### Case 1 — Game Log not yet imported — PASS

`POST /api/gameevent/bulkimport` `{ "zipPath": ".../2023eve.zip" }` before importing
`gl2023.txt`.

- `202 Accepted` `{ "trackingId": "79fc8866-…" }`.
- Run ended `status: "Failed"`, `failureReason`:
  *"The Game Log for season 2023 has not been imported. Import it first, then retry the bulk
  import."*
- `files: []`, every count `0`, `completedUtc` set, `batchSize: 10` (the configured default).
- No extraction, no `GameEventStart` dispatched.

Matches the expected output as revised (the failure is reported on the run, not as an error
from the POST).

### Prerequisite — import the 2023 Game Log — PASS

`POST /api/gamelog/import` `{ "filePath": ".../gl2023.txt", "seasonYear": 2023 }` →
engine log: *"Game log import for season 2023: 2430 games added, 0 games skipped."* No
`Person` / `Franchise` / `Ballpark` resolution failures. `Game` rows for 2023: 2430.

### Case 2 — Isolated import failure — PASS

Custom archive of `2023SDN.EVN`, `2023SEA.EVA`, `2023ARI.EVN`, with `2023ARI.EVN` corrupted
by stripping every `info,` record (2592 → 0).

- Run ended `status: "CompletedWithFailures"`.
- `2023ARI.EVN` → `Failed`, `errorMessage`:
  *"EventFileParseException: Game 'ARI202304060' is missing one or more required 'info'
  records (hometeam/visteam/date/number)."*
- `2023SDN.EVN`, `2023SEA.EVA` → `Success`, `gamesInserted: 81` each.
- `counts`: total 3, success 2, failed 1.
- `Retrosharp.Engine.Errors` depth increased by 1 (the failed `GameEventStart`).
- Working directory: only `2023ARI.EVN` left; the two successful files' extracted copies
  deleted.
- Engine log: *"Notified bulk import 0165a664-… that '2023ARI.EVN' failed and was moved to
  the error queue."* then the run summary *"2 succeeded, 1 failed, 0 skipped (of 3)."*

### Case 3 — Recovering from a failure — PASS

Same three files, `2023ARI.EVN` restored to the correct version, re-`POST`ed.

- Run ended `status: "Completed"`.
- `2023ARI.EVN` → `Success`, `gamesInserted: 81` (the previously-failed file, reprocessed).
- `2023SDN.EVN`, `2023SEA.EVA` → `Skipped` (`counts`: total 3, success 1, skipped 2,
  failed 0).
- `GameEvent` for 2023 held 243 distinct games = 3 × 81 — no duplicates despite `2023ARI`
  being processed twice (`GameEventGameStatus` per-game claim).

### Case 4 — Happy path, full season — FAIL

`POST` the unmodified full `2023eve.zip`. (`2023SDN` / `2023SEA` / `2023ARI` were already
imported by Cases 2–3, so `Skipped` results for those three were expected.)

- Run ended `status: "CompletedWithFailures"`. `counts`: total 30, **success 28, skipped 1,
  failed 1**.
- **`2023BOS.EVA` → `Failed`**, `errorMessage`:
  *"PlayCodeParseException: Unrecognized advance annotation '(RBI)' in '3-H(RBI)(UR)'. Raw
  play code: 'C/E2.3-H(RBI)(UR);2-3;1-2;B-1'."* — **Finding A**.
- Only **`2023ARI.EVN` was `Skipped`**; `2023SDN.EVN` and `2023SEA.EVA` were **reprocessed**
  (`Success`, `gamesInserted: 0`, `gamesSkipped: 81` — idempotent, no data change) instead
  of skipped — **Finding B**.
- Post-run DB: 2430 `Game` rows for 2023; **2349** have `GameEvent` rows; 2349
  `GameEventGameStatus` rows. The 81 games without play-by-play are **all `2023BOS` home
  games** — a direct consequence of Finding A (a Retrosheet team file contains only that
  team's home games, so `2023BOS.EVA` failing loses all 81 BOS home games with no
  redundancy from other files). See **Finding D**.
- `Batting` rows for 2023: 737; `Pitching`: 961.
- Bulk-import behaviour otherwise correct: batch window held (max 10 `inProgress` observed),
  failed file isolated and left on disk, successful files deleted, error-queue hook fired
  for `2023BOS.EVA`, run summary logged.

### Case 5 — Re-import an already-imported season — FAIL first run; PASS after Finding B fix

**First run** (before Finding B remediation): `POST` full `2023eve.zip`.

- `status: "CompletedWithFailures"`; `counts`: total 30, **skipped 28, success 1,
  failed 1**.
- `2023ARI.EVN` → `Success` (`gamesInserted: 0`, `gamesSkipped: 81`) — reprocessed instead
  of skipped, **Finding B** again (its most-recent prior outcome was `Skipped`, from Case 4).
- `2023BOS.EVA` → `Failed`, same `(RBI)` error — **Finding A**.

**Re-run after Finding B remediation** (see below), stale error-queue messages purged first:

- `status: "CompletedWithFailures"`; `counts`: total 30, **skipped 29, success 0,
  failed 1**.
- All 29 importable files `Skipped` — including `2023SDN` / `2023SEA` / `2023ARI`, which
  previously had a most-recent outcome of `Skipped`. **Finding B fixed.**
- `2023BOS.EVA` → `Failed` (`(RBI)`), still **Finding A** — correctly reprocessed because
  its most-recent outcome is `Failed`, and it fails identically. With `2023BOS.EVA` fixed
  (Finding A remediation) this case would end `Completed` with all 30 `Skipped`.

## Findings

### Finding B — rerun-skip only recognised `Success`, not `Skipped` (bulk import; in scope; FIXED during QA)

**Symptom.** On the third+ bulk import of a season, files that were imported (`Success`) on
run 1 and `Skipped` on run 2 were **reprocessed** on run 3 instead of skipped — visible in
Cases 4 and 5 as `Success` rows with `gamesInserted: 0` / `gamesSkipped: 81` where `Skipped`
was expected, and an understated `counts.skipped`.

**Root cause.** `BulkGameEventImportSaga.Handle` seeded a file as `Skipped` only when
`GetMostRecentFileOutcomeAsync` returned exactly `BulkImportFileStatus.Success`. After a
rerun, a file's most-recent `BulkImportFile` row is `Skipped`, not `Success`, so the check
failed and the file was queued for processing. No data was corrupted — the Game Event
Parser's per-game `GameEventGameStatus` claim made the reprocess a no-op — but it did
unnecessary work and misreported the skip count.

**Impact.** Low (no incorrect data; wasted parsing on rerun; wrong `skipped`/`success`
tallies). Would have made Case 5 permanently `CompletedWithFailures`/partial even once
Finding A is fixed.

**Fix.** `spec/bulk-import.md` §"Rerun skips files that already succeeded" and the saga now
skip when the most-recent outcome is `Success` **or `Skipped`** (both mean "already imported
for this season"; only `Failed` or no prior row is reprocessed):

```csharp
var status = priorOutcome is BulkImportFileStatus.Success or BulkImportFileStatus.Skipped
    ? BulkImportFileStatus.Skipped
    : BulkImportFileStatus.Pending;
```

`BulkGameEventImportSagaTests.Start_RerunSkipsFilesWhoseMostRecentOutcomeIsSuccessOrSkipped`
updated to cover a prior-`Skipped` file. Full suite 275 pass. Case 5 re-verified live
(29 `Skipped`, above).

### Finding A — `PlayCodeParser` rejects the `(RBI)` advance annotation (Game Event Parser; out of scope; remediated below)

**Symptom.** `2023BOS.EVA` fails its entire import (Cases 4 and 5) with
`PlayCodeParseException: Unrecognized advance annotation '(RBI)' in '3-H(RBI)(UR)'`.

**Occurrence.** Exactly one play in the whole 2023 archive — `2023BOS.EVA` line 6527:
`play,4,1,mcgur002,00,X,C/E2.3-H(RBI)(UR);2-3;1-2;B-1` (a catcher's-interference play where
the run is unearned but still credited as an RBI). `spec/phase-1-build-plan.md` Step 6a and
`PlayCodeParser` comments explicitly assumed `(RBI)` "never appears in modern data" — 2023
Boston disproves it.

**Why it is not a bulk-import defect.** Bulk import handled it exactly as designed — the one
bad file was isolated, its message was preserved on the error queue, the run continued, and
the other 28 files imported cleanly. This is a pre-existing limitation of the play-by-play
grammar that real 2023 data happened to expose.

**Consequence.** Until fixed, `2023BOS.EVA` cannot import, so 81 Boston home games have no
`GameEvent` / `Batting` / `Pitching` / `Fielding` play-by-play data (team event files are
home-games-only — see Finding D — so no other file covers them).

**Remediation.** Recognise `(RBI)` as an explicit RBI affirmation (see
[Remediation](#remediation-finding-a) below).

### Finding D — spec describes cross-file game duplication that does not occur (documentation; out of scope; deferred)

`spec/game-event.md` §Considerations states a game "will be in both the `2025SDN.EVN` and
`2025PHI.EVN` files" and requires handling "duplicate games ... ensuring that each game is
only represented once", and `spec/bulk-import.md` Acceptance Criterion 10 exercises "two
files … that share a physical game".

In the real 2023 data, **team event files contain only that team's home games** (verified:
`2023SEA.EVA` has 81 `id` records, all `info,hometeam,SEA`, 22 distinct visiting teams).
A given game therefore appears in exactly **one** team file, its home team's. Post-Case-4 the
DB shows 2349 games with events and 2349 `GameEventGameStatus` rows — a strict 1:1, i.e. no
game was claimed twice. The `GameEventGameStatus` atomic-claim mechanism (Step 6d) is
harmless but, with modern Retrosheet team files, never actually contends.

**Deferred.** Suggested: correct `spec/game-event.md` §Considerations to describe home-only
team files, note that the "shared game" / double-count scenario the claim mechanism guards
against does not arise for modern single-season team files (keep the mechanism as
defence-in-depth), and soften `spec/bulk-import.md` AC 10 to "…is applied at most once (the
`GameEventGameStatus` claim is unaffected by the bulk orchestrator)".

### Observation — concurrency retries under batch size 10 (not a finding)

Case 4 logged ~488 recoverable `Microsoft.EntityFrameworkCore.DbUpdateException`s (Postgres
detail redacted), which the endpoint's recoverability policy retried and cleared — 28 of 30
files still completed. This is the `Batting` / `Pitching` / `Fielding` upsert contention
between concurrently-processing files documented in `spec/stress-testing.md` Step 2, not a
regression. A smaller `BulkImport__DefaultBatchSize` reduces it at the cost of throughput.

## Remediation

### Finding A — recognise the `(RBI)` advance annotation

`Retrosharp.Format.PlayByPlay.PlayCodeParser.ApplyAdvanceSegment` now handles `(RBI)` as an
explicit RBI affirmation (`runner.IsRBI = true`), alongside the existing `(NR)`/`(NORBI)`
(deny RBI) and `(UR)`/`(TUR)` (deny earned run) annotations, instead of throwing
`PlayCodeParseException`. A scored run already defaults to `IsRBI = true`, so this only
matters where the play type would otherwise suppress it; setting it explicitly is correct
either way. The three doc comments that asserted `(RBI)` "never appears in modern data" were
corrected.

- Test: `PlayCodeParserTests.Parse_ExplicitRbiAnnotationOnAnUnearnedRun_IsRbiButNotEarned`
  uses the real play `C/E2.3-H(RBI)(UR);2-3;1-2;B-1` (2023BOS.EVA:6527) and asserts the
  scoring runner is `IsRBI == true`, `IsEarnedRun == false`. `Retrosharp.Format.Tests`:
  185 → 186 pass; full solution 276 pass.
- Live verification: re-`POST`ed the full `2023eve.zip`. `2023BOS.EVA` → `Success`,
  `gamesInserted: 81`; run ended **`Completed`** (29 `Skipped` + 1 `Success`, 0 `Failed`).
  2023 event coverage went from 2349/2430 to **2430/2430** games with `GameEvent` rows; 0
  Boston home games without play-by-play.

### Finding B — see [above](#finding-b--rerun-skip-only-recognised-success-not-skipped-bulk-import-in-scope-fixed-during-qa)

Fixed and re-verified during the run.

### Finding D — correct the spec

`spec/game-event.md` §Considerations rewritten to state that modern Retrosheet single-season
team files are home-games-only (so a game appears in exactly one file, and a failed team
file has no fallback), with the `GameEventGameStatus` claim retained as defence-in-depth
that never contends in practice. `spec/bulk-import.md` Acceptance Criterion 10 reworded from
"two files that share a physical game … exactly once" to "applied at most once regardless of
how the archive is processed".

## Post-remediation state

- Cases 1–5 all pass (see the [summary table](#result-summary)).
- Findings A, B, D all remediated; 276 unit tests pass; full solution builds clean.
- Dev database left with a complete 2023 season: 2430 `Game` rows, all 2430 with `GameEvent`
  play-by-play, plus the derived `Batting`/`Pitching`/`Fielding` rows. Test `BulkImport`
  rows, extraction folders, and queue residue cleaned up.
- The engine was run with reduced retry counts for the corrupted-file cases; production
  defaults (`Messaging__ImmediateRetries=3`, `Messaging__DelayedRetries=5`) are unchanged in
  config. A malformed event file under those defaults still runs the full retry ladder
  before reaching the error queue — see the separately-tracked
  `ImportFailureClassifier` / `EventFileParseException` item (not a bulk-import defect).
