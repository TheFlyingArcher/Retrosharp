# Retrosharp Defects

I, the human, have been running Retrosharp in Visual Studio and using the product like the end user would. This spec details defects and bugs found along the way.

## Exception on Game Event import

Actual
This exception appeared while trying to import `D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2025SDN.EVN` on game ID 7.
I initiated the request by sending a POST to `https://localhost:7017/api/gameevent/import` with body

```json
{
    "filePath":"D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2025SDN.EVN"
}
```

```text
An exception occurred in the database while saving changes for context type 'Retrosharp.Data.Context.RetrosharpContext'.
      Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
       ---> System.ArgumentException: Cannot write DateTime with Kind=UTC to PostgreSQL type 'timestamp without time zone', consider using 'timestamp with time zone'. Note that it's not possible to mix DateTimes with different Kinds in an array, range, or multirange. (Parameter 'value')
         at Npgsql.Internal.Converters.DateTimeConverterResolver`1.Get(DateTime value, Nullable`1 expectedPgTypeId, Boolean validateOnly)
         at Npgsql.Internal.PgConverterResolver`1.GetAsObjectInternal(PgTypeInfo typeInfo, Object value, Nullable`1 expectedPgTypeId)
         at Npgsql.NpgsqlParameterCollection.ProcessParameters(ReloadableState reloadableState, Boolean validateValues, CommandType commandType)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         --- End of inner exception stack trace ---
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
      Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
       ---> System.ArgumentException: Cannot write DateTime with Kind=UTC to PostgreSQL type 'timestamp without time zone', consider using 'timestamp with time zone'. Note that it's not possible to mix DateTimes with different Kinds in an array, range, or multirange. (Parameter 'value')
         at Npgsql.Internal.Converters.DateTimeConverterResolver`1.Get(DateTime value, Nullable`1 expectedPgTypeId, Boolean validateOnly)
         at Npgsql.Internal.PgConverterResolver`1.GetAsObjectInternal(PgTypeInfo typeInfo, Object value, Nullable`1 expectedPgTypeId)
         at Npgsql.NpgsqlParameterCollection.ProcessParameters(ReloadableState reloadableState, Boolean validateValues, CommandType commandType)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
         at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         --- End of inner exception stack trace ---
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
         at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
```

Expected:
Game Event imports should be able to handle datetimes without throwing exception.

Level: Critical (this is a blocker)

Notes:
Do `select * from GameEvent`, there are records that have been entered into the database already (97 of them) and add that to your evaluation.

Status: **Resolved**

Root cause:
`GameEventModel` itself has no `DateTime` properties -- the throw was one step later, in the
game-statistics claim mechanism. `GameStatisticsRepository.TryApplyGameStatisticsAsync`
(`src/lib/Retrosharp.Data/GameStatisticsRepository.cs:32`) set `ProcessedUtc = DateTime.UtcNow`
on `GameEventGameStatusModel`, which always produces `DateTimeKind.Utc`. The project's EF Core
context maps every `DateTime` column to Postgres `timestamp without time zone`
(`src/lib/Retrosharp.Data/Context/RetrosharpContext.cs:50-64`, added when the project converted
from SQL Server to Postgres), which requires `Kind=Unspecified`. This was the one call site
missed during that conversion -- every other `DateTime`-writing code path in the project already
produced `Kind=Unspecified` values.

Because `BulkInsertAsync` (`src/lib/Retrosharp.Data/GameEventRepository.cs`) commits each game's
event rows in their own transaction *before* attempting that game's statistics claim in a
separate transaction, the 97 `GameEvent` rows for game 7 had already committed successfully by
the time the statistics-claim step threw. This confirmed the exception was deterministic, not
data-dependent: since the claim insert always throws on `Kind=Utc`, no game's statistics had
successfully applied via this path for any import since the Postgres conversion.

Fix:
Changed the assignment to
`DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)`
(`src/lib/Retrosharp.Data/GameStatisticsRepository.cs:32`).

Verification:
Re-ran the import for `2025SDN.EVN` against game 7 after the fix. `BulkInsertAsync`'s existing
per-game idempotency check correctly skipped re-inserting game 7's 97 already-committed
`GameEvent` rows, and the statistics claim succeeded for the first time -- `GameEventGameStatus`
now has a row for game 7 (`ProcessedUtc = 2026-08-16 17:08:13`). Full re-import of the file
completed with zero exceptions: "80 games inserted, 1 games skipped, 81 games' statistics
applied, 0 games' statistics already claimed."

Follow-up audit (completed):
Checked `GameEventGameStatus` coverage against all imported `GameEvent` game IDs. 81 distinct
games have event data, 81 have status rows, 0 missing and 0 orphaned. Only one import file
(`docs/csv/2025SDN.EVN`) exists in the repo and it was the one already re-run above, so no
further action is needed.

## Unable to parse a play code in event file

Actual:
Attempting to import `D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2025ARI.EVN` via the Retrosharp API yielded this exception:

```text
Retrosharp.Format.PlayByPlay.PlayCodeParseException: Fielded-out code has no trajectory modifier (G/L/F/P/BG/BP) to determine GroundOut vs FlyOut. Raw play code: '1/BL1S'.
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.Parse(String rawEventText, String countField, String pitchSequence) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs:line 57
   at Retrosharp.Format.PlayByPlay.GameEventResolver.ResolvePlay(Int32 gameId, Int32 sequence, Int32 recordIndex, PlayRecord play, TeamLineupState visitingTeam, TeamLineupState homeTeam, IReadOnlyDictionary`2 personIdsByRetrosheetId, Dictionary`2 baserunners) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\GameEventResolver.cs:line 107
   at Retrosharp.Format.PlayByPlay.GameEventResolver.Resolve(Int32 gameId, EventFileGame game, IReadOnlyDictionary`2 personIdsByRetrosheetId) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\GameEventResolver.cs:line 81
   at Retrosharp.Service.GameEventImportService.MapToGameEventRecordAsync(EventFileGame game) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp.Service\GameEventImportService.cs:line 105
   at Retrosharp.Service.GameEventImportService.ImportAsync(String filePath) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp.Service\GameEventImportService.cs:line 56
   at Retrosharp.Engine.Console.Saga.GameEventSaga.Handle(GameEventStart message, IMessageHandlerContext context) in D:\Code\TheFlyingArcher\Retrosharp\src\engine\Retrosharp.Engine.Console\Saga\GameEventSaga.cs:line 49
   at NServiceBus.InvokeHandlerTerminator.Terminate(IInvokeHandlerContext context) in /_/src/NServiceBus.Core/Pipeline/Incoming/InvokeHandlerTerminator.cs:line 31
   at NServiceBus.SagaPersistenceBehavior.Invoke(IInvokeHandlerContext context, Func`2 next) in /_/src/NServiceBus.Core/Sagas/SagaPersistenceBehavior.cs:line 95
   at NServiceBus.LoadHandlersConnector.Invoke(IIncomingLogicalMessageContext context, Func`2 stage) in /_/src/NServiceBus.Core/Pipeline/Incoming/LoadHandlersConnector.cs:line 59
   at NServiceBus.LoadHandlersConnector.Invoke(IIncomingLogicalMessageContext context, Func`2 stage) in /_/src/NServiceBus.Core/Pipeline/Incoming/LoadHandlersConnector.cs:line 80
   at NServiceBus.DeserializeMessageConnector.Invoke(IIncomingPhysicalMessageContext context, Func`2 stage) in /_/src/NServiceBus.Core/Pipeline/Incoming/DeserializeMessageConnector.cs:line 38
   at NServiceBus.InvokeAuditPipelineBehavior.Invoke(IIncomingPhysicalMessageContext context, Func`2 next) in /_/src/NServiceBus.Core/Audit/InvokeAuditPipelineBehavior.cs:line 21
   at NServiceBus.ProcessingStatisticsBehavior.Invoke(IIncomingPhysicalMessageContext context, Func`2 next) in /_/src/NServiceBus.Core/Performance/Statistics/ProcessingStatisticsBehavior.cs:line 27
   at NServiceBus.TransportReceiveToPhysicalMessageConnector.Invoke(ITransportReceiveContext context, Func`2 next) in /_/src/NServiceBus.Core/Pipeline/Incoming/TransportReceiveToPhysicalMessageConnector.cs:line 39
   at NServiceBus.TransportReceiveToPhysicalMessageConnector.Invoke(ITransportReceiveContext context, Func`2 next) in /_/src/NServiceBus.Core/Pipeline/Incoming/TransportReceiveToPhysicalMessageConnector.cs:line 45
   at NServiceBus.RetryAcknowledgementBehavior.Invoke(ITransportReceiveContext context, Func`2 next) in /_/src/NServiceBus.Core/ServicePlatform/Retries/RetryAcknowledgementBehavior.cs:line 25
   at NServiceBus.MainPipelineExecutor.Invoke(MessageContext messageContext, CancellationToken cancellationToken) in /_/src/NServiceBus.Core/Pipeline/MainPipelineExecutor.cs:line 54
   at NServiceBus.MainPipelineExecutor.Invoke(MessageContext messageContext, CancellationToken cancellationToken) in /_/src/NServiceBus.Core/Pipeline/MainPipelineExecutor.cs:line 82
   at NServiceBus.LogWrappedMessageReceiver.<>c__DisplayClass12_0.<<Initialize>g__ScopedOnMessage|0>d.MoveNext() in /_/src/NServiceBus.Core/Receiving/LogWrappedMessageReceiver.cs:line 36
```

Expected:
All play codes should be able to be parsed. If a play code cannot be parsed because of erroneous input from Retrosheet, it should silently fail, be logged and the parsing should continue.

Status: **Resolved**

Level: High (prevents some parsing, not all)

Notes: Examine, Retrosheet's description of how to parse the event file and see if this particular play code was missed: https://www.retrosheet.org/eventfile.htm

Root cause:
Confirmed against Retrosheet's own event-file spec (retrosheet.org/eventfile.htm): `BL` ("line
drive bunt") is a documented batted-ball trajectory modifier, the same category as `BG` (bunt
grounder) and `BP` (bunt pop up). `PlayCodeParser.ApplyModifiers`
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`) explicitly handled `BG` and `BP` but
never `BL`, so a fielded out whose only modifier was a `BL...` code (e.g. `1/BL1S`) left
`battedBallType` unset and fell through to the "no trajectory modifier" exception. This was a
parser gap, not erroneous Retrosheet data -- `docs/csv/2025ARI.EVN` has 50 `/BG` occurrences and
exactly 1 `/BL` (the reported play), so the code path was simply never exercised before.

Fix:
Added a `BL` branch mapping to `BattedBallType.LineDrive`, mirroring the existing `BG`/`BP`
handling, and included `BL` in the exception message's list of recognized modifiers
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Added a regression test,
`Parse_LineDriveBunt_ResolvesToFlyOutWithLineDriveBattedBallType`, using the real play from
`2025ARI.EVN` line 2428 (`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

Verification:
All 40 tests in `Retrosharp.Format.Tests` pass. Re-ran the full import of `2025ARI.EVN` end to
end (API -> NServiceBus -> engine saga -> Postgres): "81 games inserted, 0 games skipped, 81
games' statistics applied" with zero exceptions. Confirmed in the database that the exact
reported play (`RawEventText = '1/BL1S'`, game 389, inning 9) now has `EventType = FlyOut` and
`BattedBallType = LineDrive`, matching Retrosheet's documented semantics for a caught bunted
line drive.

## Needless Retrying

Actual:
Posting to `https://localhost:7017/api/gamelog/import`, I mistyped the filename. Naturally this produced a `FileNotFoundException` which is reasonable. What it kept doing was retrying the same mistyped file path. It was never going to succeed, just annoy me with constant hitting the same breakpoint over and over.

Expected:
Sagas should gracefully fail on unrecoverable errors like file not found exceptions. Recoverable errors are those like connection timeouts.

Status: **Re-opened**

Level: Medium

Root cause:
`Retrosharp.Engine.Console\Program.cs`'s recoverability policy (`ExponentialBackoffWithJitterPolicy`)
applies uniformly to every exception from every message handler: 3 immediate retries, then 5
delayed retries with exponential backoff + jitter (roughly a minute total), then `MoveToError`.
It has no concept of "this exception can never succeed no matter how many times we retry" --
a mistyped file path was treated identically to a transient DB/broker blip. All three import
sagas (`GameLogSaga`, `GameEventSaga`, `PersonSaga`) share the exact same shape: a `Handle(...Start)`
method that calls into an ETL service which throws `FileNotFoundException` for a missing file
(confirmed consistent across `GameLogFileService.cs:17`, `BioFileService.cs:16`, and
`EventFileReader.cs:26`), with the exception left to propagate straight into that global retry
pipeline.

Fix:
Added `ImportFailureClassifier.IsUnrecoverable(Exception)`
(`src/engine/Retrosharp.Engine.Console/Saga/ImportFailureClassifier.cs`), currently classifying
`FileNotFoundException`/`DirectoryNotFoundException` as unrecoverable. Each saga's `Handle(...Start)`
method now wraps its import call in a `try/catch` on that classifier: an unrecoverable failure is
logged as a warning and the saga calls `MarkAsComplete()` immediately, never reaching NServiceBus's
recoverability pipeline at all. Any other exception (a dropped DB connection, RabbitMQ hiccup, etc.)
is left unhandled and still flows through the existing immediate/delayed retry policy in
`Program.cs`, completely untouched -- this only changes behavior for the specific class of error
that retrying can never fix.

Verification:
Rebuilt and restarted the engine and API, then POSTed a mistyped path to
`/api/gamelog/import` (the exact repro from this report). The engine logged the
`FileNotFoundException` exactly once as a warning ("...failed with an unrecoverable error; not
retrying.") with no immediate or delayed retry attempts over the following 20+ seconds (previously
this would have retried 3 times immediately plus 5 more times over about a minute). Confirmed in
`GameLogSaga`'s NServiceBus persistence table that no orphaned/incomplete saga row was left behind
for the failed request -- `MarkAsComplete()` cleaned it up the same as a normal successful run,
i.e. a genuine graceful failure rather than a silently swallowed retry loop.

Re-opening reason:
Exceptions thrown like `InvalidOperationException` in `Retrosharp.Format.PlayByPlay.GameEventResolver.ResolvePlay` will still cause the saga to needlessly retry on unrecoverable errors. `Retrosharp.Engine.Console.Saga.ImportFailureClassifier.IsUnrecoverable` is far too narrow in scope. `FileNotFoundException` are not the only exceptions that are unrecoverable.

Re-opening expected:
`InvalidOperationException` is added as "unrecoverable" as is `PlayCodeParseException`. That exception is the result of bad Retrosheet data and cannot be recovered.

Status: **Resolved**

Fix:
Widened `ImportFailureClassifier.IsUnrecoverable`
(`src/engine/Retrosharp.Engine.Console/Saga/ImportFailureClassifier.cs`) to also match
`InvalidOperationException` and `PlayCodeParseException`, as requested. Verified every
`InvalidOperationException` throw site in the codebase first (11 across `GameLogImportService`,
`GameEventImportService`, `GameEventResolver`, `GameContextResolver`, `PersonImportService` via
`BaseRepository`) -- all are deterministic "no matching franchise/game/person/lineup slot for
this input" conditions, none transient. The one exception that ISN'T one of these,
`FailingPingMessageHandler`'s deliberate diagnostic failure (used to exercise the retry/backoff
path itself), is unaffected: it's a plain message handler, not one of the three sagas this
classifier is scoped to, so widening it has no effect there.

Verification:
Added `IsUnrecoverable_KnownUnrecoverableTypes_ReturnsTrue`/`IsUnrecoverable_PlayCodeParseException_ReturnsTrue`
to `ImportFailureClassifierTests.cs`, plus a new saga-level regression test,
`Handle_Start_InvalidOperationException_MarksSagaCompleteWithoutSendingComplete`, in
`GameEventSagaTests.cs`. All pass. This also fixed the retry storm for the
"InvalidOperationException on base runners" defect below once that defect's own root cause was
fixed -- see that entry for the full end-to-end verification (81 games imported from
`2025ATH.EVA` with zero exceptions and zero retries).

Re-opening reason (observability):
The catch-and-`MarkAsComplete()` fix stopped the retry storm but swallowed the failure. Found
during stress-test Run 1 (`spec/stress-testing.md`): `2024MIA.EVN`'s first play was a bare `2`
(unassisted catcher putout, no trajectory modifier), `PlayCodeParser` threw, and the saga
logged one `warn:` line and completed. Result: all 81 Marlins home games silently absent from
`GameEvent`/`GameEventGameStatus`/`Batting`/`Pitching`/`Fielding`, **nothing on the error
queue**, the saga looked successful, and the API's 202 was indistinguishable from a real
import. The gap was only caught by reconciling `GameEventGameStatus` (2,348) against the
game-log count (2,429). For an ETL system, a wholesale file failure must be durably visible,
not just logged.

Re-opening expected:
An unrecoverable import failure lands in `Retrosharp.Engine.Errors` (the configured error
queue) with its full exception and headers, is logged by NServiceBus at error level, and is
operator-retryable once the underlying data/parser issue is fixed -- while still doing **zero**
retries (the "Needless Retrying" requirement is unchanged).

Status: **Resolved**

Fix:
Moved the unrecoverable-vs-transient decision out of each saga's `Handle(...Start)` try/catch
and into the endpoint's single recoverability policy. New
`EngineRecoverabilityPolicy.Decide(...)`
(`src/engine/Retrosharp.Engine.Console/Saga/EngineRecoverabilityPolicy.cs`, extracted from the
old `Program.cs` `ExponentialBackoffWithJitterPolicy` and now a pure, unit-testable function):
if `ImportFailureClassifier.IsUnrecoverable(exception)` it returns
`RecoverabilityAction.MoveToError(errorQueue)` on the first failure -- no immediate or delayed
retries -- otherwise it applies the existing immediate -> exponential-backoff-with-jitter ->
error-queue ladder unchanged. `PersonSaga`/`GameLogSaga`/`GameEventSaga` no longer catch
anything: every exception propagates, NServiceBus's recoverability pipeline consults the policy,
and the failed message is moved to the error queue with a matching error-level log line. The
message transaction (saga-data write included) rolls back on the exception, so no orphaned saga
row is left behind and a later retry of the same file starts fresh -- the same clean-failure
property the original fix verified, now with the failure actually visible.

Verification:
`dotnet test` green (227 tests). New `EngineRecoverabilityPolicyTests.cs` (9 cases): each
unrecoverable exception type (`FileNotFoundException`, `DirectoryNotFoundException`,
`InvalidOperationException`, `PlayCodeParseException`) -> `MoveToError` on the first failure
with zero retries and priority over the retry ladder; transient (`TimeoutException`) ->
`ImmediateRetry` within budget, then `DelayedRetry` with a `2^n` base delay plus <=20% jitter,
then `MoveToError`. The three saga tests that asserted catch-and-complete now assert the
exception propagates without the saga completing or sending its `Complete` message. Live
end-to-end verification is folded into stress-test Run 1's backfill (`spec/stress-testing.md`).

Re-opening reason (over-broad, the other direction):
Found in stress-test Step 2 (`spec/stress-testing.md`): 30 Game Event files imported
concurrently, 6 failed with `Npgsql.PostgresException 40P01: deadlock detected`, each landing
in the error queue **immediately with zero retries** and leaving its file partially imported
(7-76 of 81 games). Root cause of the deadlock itself is separate (concurrent upserts of the
shared `Batting`/`Pitching`/`Fielding` season rows -- see `spec/stress-testing.md` Step 2
findings 2-3, deferred). The recoverability defect: EF Core / Npgsql surface a Postgres
deadlock as a `System.InvalidOperationException` ("An exception has been raised that is likely
due to a transient failure"), and `ImportFailureClassifier`'s blanket `InvalidOperationException
=> unrecoverable` rule sent it straight to the error queue. A deadlock victim is the textbook
*retryable* failure -- it should ride the retry ladder and would almost certainly succeed once
the contending imports finish.

Re-opening expected:
A transient database error -- deadlock (`40P01`), serialization failure (`40001`), dropped or
timed-out connection -- is treated as recoverable regardless of what exception wraps it.

Status: **Resolved**

Fix:
`ImportFailureClassifier.IsUnrecoverable` now walks the whole exception chain first and returns
`false` if any link is an `NpgsqlException` with `IsTransient == true` (Npgsql's own curated
set: deadlock, serialization failure, connection failures, resource-limit errors --
`PostgresException` derives from it and checks the SQLSTATE) or a `TimeoutException`. Only if
no transient link is found does the existing type check (`FileNotFoundException`,
`DirectoryNotFoundException`, `InvalidOperationException`, `PlayCodeParseException`) apply. A
non-transient `PostgresException` (e.g. `23503` foreign-key violation) wrapped in an
`InvalidOperationException` stays unrecoverable. The deadlocked message now flows through
`EngineRecoverabilityPolicy`'s 3-immediate + 5-delayed-backoff ladder; `GameEventImportService`
is idempotent per game (`GameEventGameStatus` claim), so a retry finishes the partial file.

Verification:
`dotnet test` green (231). 4 new tests: `ImportFailureClassifierTests` -- a `40P01`
`PostgresException` bare and wrapped in `InvalidOperationException` -> recoverable; a `23503`
wrapped the same way -> still unrecoverable. `EngineRecoverabilityPolicyTests` -- the wrapped
deadlock -> `ImmediateRetry` then `DelayedRetry`, never `MoveToError` on the first failure.
Verified live by re-running Step 2 with the fix: engine log `grep -c 40P01` -> 0, zero retries, error queue clean, row counts identical to a serial import. See `spec/stress-testing.md` Step 2c.

## Deadlock under concurrent Game Event import

Actual:
Stress-test Step 2 (`spec/stress-testing.md`) fired 30 Game Event files concurrently. Six
failed with `Npgsql.PostgresException 40P01: deadlock detected` in
`GameStatisticsRepository.TryApplyGameStatisticsAsync`. Serial import: zero.

Status: **Resolved**

Level: Medium

Root cause:
`GameStatisticsRepository` carried a comment asserting row-level races on a player's
`Batting`/`Pitching`/`Fielding` season row "can't actually happen here: every game a player
plays for a given franchise lives in that franchise's own event file, processed sequentially
by one saga." That holds only for **home** games. A player's road games appear in the host
team's event file, so franchise X's players' season rows are written by every other team's
file that hosted X. Under concurrent import, two files' per-game transactions each held one
player's row lock (`ExecuteUpdateAsync` -> `UPDATE ... WHERE Id = n`, held to commit) and
waited on another player's row locked by the other transaction, in the opposite order --
a textbook lock-order-inversion deadlock. The `GameEventGameStatus` claim only serialises a
single *game*'s statistics; it does nothing for the *season rows* shared across games.

Fix:
Acquire the row locks in one deterministic order across every transaction.
`TryApplyGameStatisticsAsync` now applies each delta group sorted by its natural key
(`PersonId`, `FranchiseId`, `SeasonYear`, and `Position` for fielding) and keeps the fixed
group order batting -> pitching -> fielding. With a global acquisition order no lock cycle can
form; two files touching the same rows queue instead of deadlocking. The insert path (a
brand-new season row, before any row exists) is savepoint-guarded via a new
`TrySaveNewSeasonRowAsync`: a losing insert race rolls back to the savepoint -- so the outer
per-game transaction isn't left in Postgres's aborted state -- detaches the entity, and falls
through to the additive update. No file-level or global lock (per `project.md`'s explicit
prohibition). The transient-error reclassification above is retained as a backstop but is no
longer expected to fire for this path.

Verification:
`dotnet test` green (231 -- this data-layer path has no unit-test harness; `GameStatisticsRepository`
takes `RetrosharpContext` and uses `ExecuteUpdateAsync`/transactions/savepoints, none of which
the EF in-memory provider supports). Verified live by re-running Step 2: see
`spec/stress-testing.md` for the zero-deadlock, zero-retry concurrent run.

## InvalidOperationException on base runners

Actual:
Importing `D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2025ATH.EVA` produced this exception

```text
System.InvalidOperationException: Play 'S7/L7S.3-H(UR);2-H(UR);1-3' (inning 5) references a runner on Third that the resolver has no record of -- a preceding play or substitution was missed. Current baserunners: [Second=dubom001(slot8), First=penaj004(slot1)]
   at Retrosharp.Format.PlayByPlay.GameEventResolver.ResolvePlay(Int32 gameId, Int32 sequence, Int32 recordIndex, PlayRecord play, TeamLineupState visitingTeam, TeamLineupState homeTeam, IReadOnlyDictionary`2 personIdsByRetrosheetId, Dictionary`2 baserunners)
   at Retrosharp.Format.PlayByPlay.GameEventResolver.Resolve(Int32 gameId, EventFileGame game, IReadOnlyDictionary`2 personIdsByRetrosheetId)
   at Retrosharp.Service.GameEventImportService.MapToGameEventRecordAsync(EventFileGame game) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp.Service\GameEventImportService.cs:line 105
   at Retrosharp.Service.GameEventImportService.ImportAsync(String filePath) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp.Service\GameEventImportService.cs:line 56
   at Retrosharp.Engine.Console.Saga.GameEventSaga.Handle(GameEventStart message, IMessageHandlerContext context) in D:\Code\TheFlyingArcher\Retrosharp\src\engine\Retrosharp.Engine.Console\Saga\GameEventSaga.cs:line 52
   at NServiceBus.InvokeHandlerTerminator.Terminate(IInvokeHandlerContext context) in /_/src/NServiceBus.Core/Pipeline/Incoming/InvokeHandlerTerminator.cs:line 31
   at NServiceBus.SagaPersistenceBehavior.Invoke(IInvokeHandlerContext context, Func`2 next) in /_/src/NServiceBus.Core/Sagas/SagaPersistenceBehavior.cs:line 95
   at NServiceBus.LoadHandlersConnector.Invoke(IIncomingLogicalMessageContext context, Func`2 stage) in /_/src/NServiceBus.Core/Pipeline/Incoming/LoadHandlersConnector.cs:line 59
   at NServiceBus.LoadHandlersConnector.Invoke(IIncomingLogicalMessageContext context, Func`2 stage) in /_/src/NServiceBus.Core/Pipeline/Incoming/LoadHandlersConnector.cs:line 80
   at NServiceBus.DeserializeMessageConnector.Invoke(IIncomingPhysicalMessageContext context, Func`2 stage) in /_/src/NServiceBus.Core/Pipeline/Incoming/DeserializeMessageConnector.cs:line 38
   at NServiceBus.InvokeAuditPipelineBehavior.Invoke(IIncomingPhysicalMessageContext context, Func`2 next) in /_/src/NServiceBus.Core/Audit/InvokeAuditPipelineBehavior.cs:line 21
   at NServiceBus.ProcessingStatisticsBehavior.Invoke(IIncomingPhysicalMessageContext context, Func`2 next) in /_/src/NServiceBus.Core/Performance/Statistics/ProcessingStatisticsBehavior.cs:line 27
   at NServiceBus.TransportReceiveToPhysicalMessageConnector.Invoke(ITransportReceiveContext context, Func`2 next) in /_/src/NServiceBus.Core/Pipeline/Incoming/TransportReceiveToPhysicalMessageConnector.cs:line 39
   at NServiceBus.TransportReceiveToPhysicalMessageConnector.Invoke(ITransportReceiveContext context, Func`2 next) in /_/src/NServiceBus.Core/Pipeline/Incoming/TransportReceiveToPhysicalMessageConnector.cs:line 45
   at NServiceBus.RetryAcknowledgementBehavior.Invoke(ITransportReceiveContext context, Func`2 next) in /_/src/NServiceBus.Core/ServicePlatform/Retries/RetryAcknowledgementBehavior.cs:line 25
   at NServiceBus.MainPipelineExecutor.Invoke(MessageContext messageContext, CancellationToken cancellationToken) in /_/src/NServiceBus.Core/Pipeline/MainPipelineExecutor.cs:line 54
   at NServiceBus.MainPipelineExecutor.Invoke(MessageContext messageContext, CancellationToken cancellationToken) in /_/src/NServiceBus.Core/Pipeline/MainPipelineExecutor.cs:line 82
   at NServiceBus.LogWrappedMessageReceiver.<>c__DisplayClass12_0.<<Initialize>g__ScopedOnMessage|0>d.MoveNext() in /_/src/NServiceBus.Core/Receiving/LogWrappedMessageReceiver.cs:line 36
```

Expected:
Possible Retrosheet erroneous data entry. However, because game plays determine the statistics, this cannot be ignored otherwise statistics will not be accurate to what is published. Parsing is halted and rolled back.

Status: **Resolved**

Level: High (blocks parsing)

Root cause:
Not erroneous Retrosheet data -- a confirmed parser bug, the same class of gap as the resolved
"Unable to parse a play code" defect. Traced the actual top of the 5th inning in
`docs/csv/2025ATH.EVA` (lines 6250-6257) play by play up to the failing play:

```
S5/G56S            smitc010 -> 1st
W.1-2              dubom001 walk, smitc010 -> 2nd        [1st=dubom001, 2nd=smitc010]
K+CS3(2E5).1-2     strikeout + caught-stealing at 3rd, fielder chain "2E5" (catcher throws, 3B *errors*)
```

`K+CS3(2E5)` routes through `ParseCaughtStealingLike`
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`), which unconditionally set
`runner.IsOut = true` without ever inspecting the fielder chain. But the chain ends in an
*error* (`E5`) -- the relay throw was misplayed, so per Retrosheet convention the runner is
actually safe at third. The parser dropped `smitc010` from the baserunner tracker entirely
instead of placing him on third. The exact same "an out chain ending in an error means the
runner is actually safe" rule already existed elsewhere in the same file --
`ApplyAdvanceSegment` explicitly checks for a trailing `Error` credit and flips `IsOut = false`
(confirmed against a real play, `1X2(4E6)`, in `2025SDN.EVN`) -- but `ParseCaughtStealingLike`
(shared by both `CS` and `POCS`) never got the same treatment.

Two plays later, `S7/L7S.3-H(UR);2-H(UR);1-3` explicitly requires a runner on Third and threw
the reported exception with baserunners `[Second=dubom001, First=penaj004]`, matching this trace
exactly. The `(UR)` unearned-run tags on both scoring advances corroborate this: Retrosheet's
own official scoring already reflects a fielding error upstream, consistent with that runner
having reached safely rather than being retired.

On "Parsing is halted and rolled back": this already held structurally, no change needed.
`GameEventImportService.ImportAsync` parses every game in the file into an in-memory list
*before* calling `BulkInsertAsync`, so this exception always fired during in-memory resolution,
before any transaction opened or any row was written. Nothing to roll back because nothing was
ever written.

Fix:
In `ParseCaughtStealingLike`, after adding the fielder credits, apply the same check
`ApplyAdvanceSegment` already uses: if the last credit is an `Error`, set `IsOut = false`
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Added a regression test,
`Parse_CaughtStealingWithErrorOnRelay_RunnerSafeNotOut`, using the real play from
`2025ATH.EVA` line 6255 (`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

Verification:
All 154 tests in `Retrosharp.Format.Tests` pass. Re-ran the full import of `2025ATH.EVA` end to
end (API -> NServiceBus -> engine saga -> Postgres): "81 games inserted, 0 games skipped, 81
games' statistics applied" with zero exceptions. Confirmed in the database that
`GameEventRunner` for the `K+CS3(2E5).1-2` play now shows `smitc010` with `StartBase=Second`,
`EndBase=Third`, `IsOut=false` -- exactly the fix -- and the following `S7/L7S...` play resolved
without error since the runner was correctly on Third.

Related gap noted, not in scope here: `PlayCodeParseException`'s own doc comment says callers
should "catch this per-play, log it, and continue with the rest of the file" (per spec/parser.md),
but no code anywhere actually does that -- an unparseable code still aborts the whole file's
import rather than skipping just that one play. That's a real gap against the original
"Unable to parse a play code" defect's expected behavior, but a larger architectural change than
either item resolved in this entry.

## Missing catcher putout on strikeouts

Actual:
Importing `D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2025HOU.EVA` produced reconciliation
warnings (not exceptions -- the parse succeeded) for every game in the file:

```text
Retrosharp.Service.GameEventImportService: Warning: Game '2251', Franchise '122': GameFieldingStatistics.Putouts = 24, but play-by-play derives 19.
Retrosharp.Service.GameEventImportService: Warning: Game '2266', Franchise '122': GameFieldingStatistics.Putouts = 24, but play-by-play derives 18.
Retrosharp.Service.GameEventImportService: Warning: Game '2266', Franchise '58': GameFieldingStatistics.Putouts = 27, but play-by-play derives 16.
Retrosharp.Service.GameEventImportService: Warning: Game '2281', Franchise '58': GameFieldingStatistics.Putouts = 27, but play-by-play derives 22.
Retrosharp.Service.GameEventImportService: Warning: Game '2281', Franchise '122': GameFieldingStatistics.Putouts = 24, but play-by-play derives 15.
Retrosharp.Service.GameEventImportService: Warning: Game '2305', Franchise '58': GameFieldingStatistics.Putouts = 27, but play-by-play derives 19.
Retrosharp.Service.GameEventImportService: Warning: Game '2305', Franchise '107': GameFieldingStatistics.Putouts = 27, but play-by-play derives 16.
Retrosharp.Service.GameEventImportService: Warning: Game '2320', Franchise '58': GameFieldingStatistics.Putouts = 27, but play-by-play derives 18.
Retrosharp.Service.GameEventImportService: Warning: Game '2320', Franchise '107': GameFieldingStatistics.Putouts = 27, but play-by-play derives 16.
Retrosharp.Service.GameEventImportService: Warning: Game '2336', Franchise '58': GameFieldingStatistics.Putouts = 27, but play-by-play derives 20.
Retrosharp.Service.GameEventImportService: Warning: Game '2336', Franchise '107': GameFieldingStatistics.Putouts = 27, but play-by-play derives 22.
```

Expected:
Determine whether this is missing/erroneous Retrosheet data or a Retrosharp defect, and whether
it affects downstream statistics.

Status: **Resolved**

Level: Medium (no exception, data quietly wrong -- not a blocker, but affects a persisted stat)

Root cause:
Not missing or erroneous Retrosheet data -- a confirmed parser gap. `PlayCodeParser.ParseSingleCode`'s
`"K"` case (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`) marks the batter out but
never assigns a `FieldingCredit`. Standard scoring credits the catcher (position 2) with the
putout on an uncomplicated strikeout, the same way "63" credits the shortstop and second
baseman on a ground out -- but a bare `"K"` carries no fielder digits in Retrosheet's own
notation, so nothing in the code was filling that credit in.

Verified quantitatively against every warning in this report, not just inferred: joined
`GameEventFieldingCredit` (derived) against `GameFieldingStatistics` (Game Log Parser's
independently-sourced totals) and against a count of `GameEvent` rows with `EventType = 7`
(Strikeout) per fielding side, for all six games/franchise pairs in the warning list. 9 of 11
matched the reported gap exactly, one-for-one. The two that didn't were fully explained, not
loose ends:
- Game 2305, Franchise 58: gap was 8, bare-K count was 10 -- 2 of those 10 were actually
  `K.BX1(23)` (a dropped third strike where the batter is thrown out at first by the catcher-to-
  first-baseman relay), which already carries its own correct fielder-chain credit via
  `ApplyAdvanceSegment`. Only the other 8 bare `K`s were genuinely creditless.
- Game 2336, Franchise 107: gap was 5, bare-K count was 6 -- 1 of those 6 was `K+WP.1-2;B-1` (a
  strikeout on a wild pitch where the batter reaches first safely), which has no putout at all
  since nobody is actually out on that play.

Both of those already-correct cases (thrown out elsewhere via an explicit fielder chain; safe
via an explicit advance overriding `IsOut`) confirmed the fix needs to check the *final* resolved
state of the batter's runner, not just "was the primary code a K".

Downstream impact (this is real, not cosmetic):
`GameStatisticsResolver` sums `Fielding.Putouts` directly from `GameEventFieldingCredit` rows
(`src/lib/Retrosharp/Format/PlayByPlay/GameStatisticsResolver.cs:140`), and
`GameStatisticsRepository.ApplyFieldingDeltaAsync` accumulates that delta straight into the
persisted **season** `Fielding` table -- this isn't confined to the per-game reconciliation
warning. Queried the database directly: as of this report, **729 games** across every file
imported so far (not just `2025HOU.EVA`) have at least one affected strikeout, totaling
**12,274 missing `GameEventFieldingCredit` Putout rows**. Every catcher's season `Fielding.Putouts`
total is undercounted by however many strikeouts they caught. Confirmed this is scoped
precisely to catcher Putouts: `Batting.Strikeouts` and `Pitching.Strikeouts` are computed
independently from `EventType` directly (not from `FieldingCredits`), so those, `Assists`,
`Errors`, and every other stat are unaffected.

Fix:
In `PlayCodeParser.Parse`, after modifiers and advances are fully applied (so the batter's
runner reflects its final resolved state, not just what the primary "K" code implied), credit
the catcher (position 2) with a Putout if the event is a Strikeout, the batter's own runner is
still out, and it has no fielding credits yet -- i.e., nothing else (a dropped-third-strike
throw-out, a wild-pitch reached-base override) already gave it a different disposition
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Added three regression tests using
real plays from `2025HOU.EVA`: a bare `"K"` gets the catcher putout; `"K.BX1(23)"` does *not*
get a second, phantom putout on top of its real fielder-chain credit; `"K+WP.1-2;B-1"` gets no
putout at all, since the batter reached safely
(`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

Verification:
All 157 tests in `Retrosharp.Format.Tests` pass (190 across the full solution).

Backfill (completed):
Considered and rejected delete-and-reimport: `GameStatisticsRepository.ApplyBattingDeltaAsync`/
`ApplyPitchingDeltaAsync`/`ApplyFieldingDeltaAsync` are pure additive accumulators (`SetProperty(f
=> f.Putouts, f => f.Putouts + delta.Putouts)`) with no per-game ledger, and the only thing gating
re-application is `GameEventGameStatus.GameId`'s uniqueness. Deleting a game's event data and its
`GameEventGameStatus` claim row and re-importing would have re-applied the *entire* statistics
delta a second time -- doubling Hits, AtBats, Runs, Strikeouts, Assists, Errors, etc. for every
player in every affected game, not just fixing Putouts. Since the bug is scoped exclusively to
catcher Putouts, did a targeted backfill instead: a one-off tool
(`PutoutBackfill`, not part of the repo -- built in scratch, referencing the real
`Retrosharp`/`Retrosharp.Data` assemblies so it reuses the actual fixed `GameEventResolver`/
`PlayCodeParser` rather than reimplementing "who was catching" logic) re-derived play-by-play for
every event file, matched each Strikeout's batter runner to its persisted `GameEventRunner` row
by `(GameId, Sequence)`, and inserted a `GameEventFieldingCredit` **only** where the DB row
currently had zero credits -- `GameEvent`, `GameEventRunner`, `Batting`, `Pitching`, and
`GameEventGameStatus` were never touched.

Discovered along the way: the original 7-file list (used for this defect's earlier investigation)
was incomplete -- `docs/csv/` also has `2025ANA.EVA`, `2025SEA.EVA`, `2025TEX.EVA`, each a team's
own *home*-game file (a specific game is recorded once, under the home team's file only, not
duplicated across both participants' files -- confirmed empirically: zero shared-game double
encounters across all 10 files). Re-deriving from only 7 files found 567 games / 9,340 credits;
adding all 10 found exactly 729 games / 12,274 credits -- a 1:1 match against the independently-
run SQL baseline (`SELECT COUNT(DISTINCT "GameId")... WHERE "EventType" = 7 AND "IsOut" = true AND
NOT EXISTS (...credit...)`), confirmed as a hard safety gate in the tool before allowing any write.

Also surfaced, out of scope, not touched: re-deriving all 10 files hit 1,329 plays across 80 other
games with no matching persisted `GameEvent` row at the expected `(GameId, Sequence)` (their
source files were very likely edited/regenerated after their original import, so current content
no longer lines up with what's persisted), one unrelated already-correct play misclassified by a
too-narrow tool heuristic (a caught-stealing error credit, not a strikeout gap), and one genuinely
new parser gap (`PlayCodeParseException: Unrecognized advance annotation '(WP)'` on
`SB3.1-2(WP)` in `2025TEX.EVA`, game 2357 -- an advance annotation shape `ApplyAdvanceSegment`
doesn't recognize). Confirmed **zero overlap** between these 82 games and the 729 verified-affected
games before proceeding, so none of this affected the backfill's correctness -- flagging the
`(WP)` annotation gap here for future investigation, not fixing it as part of this defect.

Post-backfill verification: `GameEventFieldingCredit` row count increased by exactly 12,274
(39,650 -> 51,924); season `Fielding.Putouts` sum increased by exactly 12,274 (26,588 -> 38,862);
the original SQL "affected games" query now returns 0 games / 0 missing credits; re-ran the exact
reconciliation check for all 6 games/11 franchise-pairs from the original warning report --
every gap is now 0.

## Unrecognized "(WP)"/"(PB)" advance annotation

Actual:
Surfaced while re-deriving play-by-play for the "Missing catcher putout on strikeouts" backfill
(not via a live import through the API): parsing game 2357's play-by-play from
`D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2025TEX.EVA` throws

```text
Retrosharp.Format.PlayByPlay.PlayCodeParseException: Unrecognized advance annotation '(WP)' in '1-2(WP)'. Raw play code: 'SB3.1-2(WP)'.
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.ApplyAdvanceSegment(String segment, String rawEventText, IDictionary`2 runners) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.Parse(String rawEventText, String countField, String pitchSequence) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs
   at Retrosharp.Format.PlayByPlay.GameEventResolver.ResolvePlay(...) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\GameEventResolver.cs
```

The exact source line: `docs/csv/2025TEX.EVA:12992` --
`play,9,0,lee-b002,22,BFB2C>B,SB3.1-2(WP)`.

Expected:
All documented Retrosheet advance annotations should parse without throwing. If a play code
cannot be parsed because of genuinely erroneous Retrosheet data, it should be handled per the
existing policy for that case (see "Unable to parse a play code in event file" and "Needless
Retrying" above), not crash the whole file's import.

Status: **Resolved**

Level: Low (exactly 1 occurrence across all 10 currently-imported event files, confirmed via
`grep -c "(WP)\|(PB)" docs/csv/*.EV{N,A}` -- every file returns 0 except `2025TEX.EVA`, which
returns 1; this is the only known real-world occurrence, and it doesn't overlap with any
already-imported game's data)

Root cause (same class of gap as "BL" in the resolved play-code defect -- a documented Retrosheet
shape the parser was never extended to cover, not erroneous source data):
`ApplyAdvanceSegment`'s annotation loop (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`)
recognizes `E$` (error), `NR`/`NORBI` (deny RBI), `UR`/`TUR` (deny earned run), and a bare digit
prefix (already-consumed fielder chain for an out) -- anything else throws
`PlayCodeParseException`. Confirmed against Retrosheet's own event-file documentation
(retrosheet.org/eventfile.htm): "Advance parameters provide an alternative way of indicating wild
pitches and passed balls" -- `(WP)` and `(PB)` are both documented, legitimate advance
annotations (`1-2(WP)` and `1-2(E5/TH)` are both given as example syntax in Retrosheet's own
spec), purely informational tags on *why* the runner advanced. They don't change `IsRBI`,
`IsEarnedRun`, or fielding credits -- the same "no-op, already consumed" treatment the code
already gives a bare digit-prefixed annotation.

Fix:
Added `WP`/`PB` as recognized annotations that fall through without altering the runner, mirroring
the existing digit-prefix case's `// Already consumed...` branch
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Added two regression tests: the real
play from `2025TEX.EVA:12992` for `(WP)`, and a synthetic `(PB)` case since no real occurrence
exists in the currently-imported files but it shares the exact same code path
(`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

Verification:
All 159 tests in `Retrosharp.Format.Tests` pass (192 across the full solution). `2025TEX.EVA` had
never been successfully imported before (its own home games were absent from the DB -- it only
showed up indirectly as an opponent in other teams' home-game files during the putout backfill).
Imported it for real end to end (API -> NServiceBus -> engine saga -> Postgres): "81 games
inserted, 0 games skipped, 81 games' statistics applied" with zero exceptions. Confirmed in the
database that game 2357's `SB3.1-2(WP)` play resolved correctly: the stolen-base runner
(Second -> Third) and the `(WP)`-annotated advance (First -> Second) are both present with
`IsOut = false`, matching Retrosheet's documented semantics.

## PlayCodeParseException: 2025BAL.EVA

Actual:

```text
Retrosharp.Format.PlayByPlay.PlayCodeParseException: Unexpected character '/' in fielder chain 'E1/TH'. Raw play code: 'CS2(E1/TH).1-3'.
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.ParseFielderChain(String chain, String rawEventText) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs:line 256
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.ParseCaughtStealingLike(String code, String prefix, String rawEventText, IDictionary`2 runners) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs:line 586
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.ParseSingleCode(String code, String rawEventText, IDictionary`2 runners) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs:line 462
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.ParsePrimaryCode(String primaryCode, String rawEventText, IDictionary`2 runners) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs:line 346
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.Parse(String rawEventText, String countField, String pitchSequence) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs:line 38
   at Retrosharp.Format.PlayByPlay.GameEventResolver.ResolvePlay(Int32 gameId, Int32 sequence, Int32 recordIndex, PlayRecord play, TeamLineupState visitingTeam, TeamLineupState homeTeam, IReadOnlyDictionary`2 personIdsByRetrosheetId, Dictionary`2 baserunners) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\GameEventResolver.cs:line 107
   at Retrosharp.Format.PlayByPlay.GameEventResolver.Resolve(Int32 gameId, EventFileGame game, IReadOnlyDictionary`2 personIdsByRetrosheetId) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\GameEventResolver.cs:line 81
   at Retrosharp.Service.GameEventImportService.MapToGameEventRecordAsync(EventFileGame game) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp.Service\GameEventImportService.cs:line 105
   at Retrosharp.Service.GameEventImportService.ImportAsync(String filePath) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp.Service\GameEventImportService.cs:line 56
   at Retrosharp.Engine.Console.Saga.GameEventSaga.Handle(GameEventStart message, IMessageHandlerContext context) in D:\Code\TheFlyingArcher\Retrosharp\src\engine\Retrosharp.Engine.Console\Saga\GameEventSaga.cs:line 52
```

Expected:
The play code is parsed correctly. Sounds like has something to do with an error on the pitcher

Status: **Resolved**

Level: High (blocks parsing this file)

Root cause:
Same class of gap as the already-fixed `K+CS3(2E5)` issue, in the same method.
`ParseCaughtStealingLike` (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`)
unconditionally treated the parenthetical as a raw fielder-digit chain and handed it to
`ParseFielderChain`, which throws on the non-digit `/`, `T`, `H` characters in `E1/TH`. The `PO`
(pickoff) code path already handles this exact shape correctly -- it checks whether the
parenthetical starts with `E<digit>` and, if so, treats it as a structured error annotation
(runner safe, single error credit to that fielder) rather than a raw chain. `ParseCaughtStealingLike`
(shared by `CS` and `POCS`) never got the same treatment. Confirms the user's own hunch ("Sounds
like has something to do with an error on the pitcher") exactly -- `E1` is the pitcher.

Fix:
Applied the same structured-error-annotation check `PO` already uses to `ParseCaughtStealingLike`,
checked before falling back to the raw fielder-chain parse (which still handles the pre-existing
"K+CS3(2E5)" error-negates-out case). Added a regression test using the real play from
`2025BAL.EVA:6476` (`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

Verification:
All 163 tests in `Retrosharp.Format.Tests` pass (196 across the full solution). `2025BAL.EVA` had
never been successfully imported before (0 games -- the exception aborted the whole file before
any writes, per the same "parse everything first" architecture as every other `PlayCodeParseException`
defect). Imported it for real end to end: "81 games inserted, 0 games skipped, 81 games'
statistics applied" with zero exceptions. Confirmed in the database that game `CS2(E1/TH).1-3`
resolved correctly: `StartBase=First`, `EndBase=Third`, `IsOut=false`, with an Error credit to
position 1 (the pitcher).

## Discrepancy Issues in 2025BOS.EVA

Actual:

```text
warn: Retrosharp.Service.GameEventImportService[0]
      Game '151', Franchise '25': GameFieldingStatistics.Errors = 2, but play-by-play derives 1.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '559', Franchise '122': GameBattingStatistics.GroundedIntoDoublePlay = 2, but play-by-play derives 1.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '772', PersonId '6286': Pitching.EarnedRuns = 4 (from 'data,er,...'), but play-by-play independently derives 3 earned runs.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '772', PersonId '17591': Pitching.EarnedRuns = 1 (from 'data,er,...'), but play-by-play independently derives 2 earned runs.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '1236', Franchise '25': GameBattingStatistics.StolenBases = 2, but play-by-play derives 3.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '1370', PersonId '171': Pitching.EarnedRuns = 1 (from 'data,er,...'), but play-by-play independently derives 0 earned runs.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '2093', Franchise '25': GameFieldingStatistics.Errors = 1, but play-by-play derives 0.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '2210', Franchise '25': GameFieldingStatistics.Errors = 3, but play-by-play derives 2.
```

Expected:
Possibly related to the [Missing catcher putout on strikeouts](#missing-catcher-putout-on-strikeouts). Needs evaluation. Possibly a missed error/earned run in parsing the play codes. May need retroactive backfilling as before

Status: **Resolved** (Errors and StolenBases discrepancies; GIDP and EarnedRuns are separate, see below)

Level: Medium (parses successfully, but data discrepancies)

Evaluation:
Traced each discrepancy against the real source data individually rather than assuming a single
shared cause -- they turned out to be two distinct, unrelated bugs, plus two categories that
aren't bugs at all:

- **Errors undercount (games 151, 2093, 2210) -- confirmed root cause.** Every case traces to a
  `"C/E2..."` play (Catcher's Interference where the catcher also commits a subsequent throwing
  error recovering the ball, e.g. `C/E2.2-3;1-2;B-1`). `E2` here is a bare **modifier** (after
  `/`), not a primary code or an advance annotation -- `ApplyModifiers`
  (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`) had no case for `E<digit>` at all, so
  the credit was silently dropped. Verified directly: game 151's `C/E2...` play had zero fielding
  credits in the database before the fix, while the game's other error plays (primary-code `E7`,
  `E5`) were already correctly recorded.

- **StolenBases overcount (game 1236) -- confirmed root cause, different file location entirely.**
  `GameStatisticsResolver.cs` credited a `StolenBase` to *every non-batter runner present in a
  play whose `EventType` is `StolenBase`*, not just the runner who actually stole. Play
  `SB2.3-H(E2/TH)(NR)(UR);1-3` has two runners: the actual stealer (First->Third) and a runner
  who merely *scored* off the ensuing error. Both were credited, overcounting by 1.

- **GIDP undercount (game 559) -- investigated, not a bug.** Confirmed the two ground-ball-DP
  plays: the first is a genuine batter GIDP (correctly counted). The second
  (`54(1)6(2)/GDP/G56.B-1`) forces out two *other* runners while the batter reaches safely on the
  fielder's choice -- by the standard rule (GIDP requires the batter himself to be retired),
  that's correctly *not* a batter GIDP. Left open as a separate note, not re-opened here, since
  it isn't part of the two confirmed bugs and the "official" Game Log figure may be using a
  different definition -- not chasing further without more evidence.

- **EarnedRuns mismatches (games 772, 1370) -- not a bug.** Same "official `data,er` figure vs.
  independently-derived" category seen as an expected, accepted discrepancy in nearly every game
  imported this session -- genuine subjective official-scorer judgment that can't be mechanically
  re-derived from play-by-play.

Fix (Errors):
Added a case to `ApplyModifiers` for a bare `E<digit>` modifier, crediting the batter's own
runner row (guaranteed to exist on a `C` play) with an Error at that position
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`).

Fix (StolenBases):
Added `IsStolenBase` to `MutableRunner`/`ParsedRunnerAdvance` (parser-internal) and to the
`GameEventRunner` contract type (transient -- not a persisted column; `GameEventRunnerModel` has
no matching property), set `true` only where `ParseSingleCode`'s `"SB"` case itself creates or
updates a runner -- including multi-steal (`;`) and bundled (`+`) combinators, which route
through the same code path. `GameStatisticsResolver`'s `StolenBases` counting now checks
`runner.IsStolenBase` directly instead of inferring from the whole play's `EventType`
(`src/lib/Retrosharp/Format/PlayByPlay/GameStatisticsResolver.cs`).

Tests:
Three new `PlayCodeParserTests` (real plays from `2025BOS.EVA:625` and `2025BOS.EVA` line for the
`SB2.3-H(E2/TH)...` play) plus a new `GameStatisticsResolverTests` case exercising a
StolenBase-typed play with a bystander runner. All 163 tests in `Retrosharp.Format.Tests` pass
(196 across the full solution).

Backfill (completed):
`2025BOS.EVA` was already imported (81 games) before both fixes landed. Built a one-off tool
(`BosBackfill`, scratch-only, not part of the repo -- same pattern as the earlier putout
backfill: re-derives play-by-play with the fixed parser, reusing the real `GameEventResolver`/
`PlayCodeParser` rather than reimplementing lineup/error logic) to correct exactly what each fix
touches, nothing else:
- **Errors**: purely additive, same pattern as the putout backfill -- insert the missing
  `GameEventFieldingCredit` row only where the DB row currently has zero credits, increment
  season `Fielding.Errors` by the same count.
- **StolenBases**: *not* a season recompute-and-overwrite -- first attempt at that produced
  nonsense deltas (e.g. persisted=19/correct=2) because a season `Batting.StolenBases` row is fed
  by every team's file a player appears in, not just BOS's own, so "recompute from BOS's file
  alone" is comparing a partial total against a season-wide one. Corrected to a **precise
  subtractive delta**: for each `StolenBase`-typed play, identify exactly which specific runner
  the *old* buggy logic would have phantom-credited (non-batter, `IsStolenBase = false`) and
  subtract exactly 1 from that runner's own season row -- safe regardless of how many other files
  also feed that row, since it only removes exactly what BOS's own import incorrectly added.
- Hard safety gate before any write: dry-run counts had to match the independently-verified
  baseline (grepped directly against `docs/csv/2025BOS.EVA`: exactly 3 `"C/E2"` occurrences,
  exactly 1 `SB` play with a secondary advance) before `--commit` was allowed to proceed.

Verification:
`GameEventFieldingCredit` row count increased by exactly 3 (63,587 -> 63,590); the phantom
stolen-base credit for the one affected player (PersonId 573, Franchise 25/BOS, season 2025) went
from 5 to 4. Re-ran the exact reconciliation check for all three Errors-discrepancy games -- every
gap is now 0.

## PlateAppearances/AtBats overcounted on a foul ball dropped for an error

Actual:
Importing `D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2025NYA.EVA` produced these warnings:

```text
warn: Retrosharp.Service.GameEventImportService[0]
      Game '1257', Franchise '87': GameBattingStatistics.PlateAppearances = 44, but play-by-play derives 45.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '1257', Franchise '87': GameBattingStatistics.AtBats = 34, but play-by-play derives 35.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '1257', Franchise '94': GameFieldingStatistics.Errors = 3, but play-by-play derives 2.
warn: Retrosharp.Service.GameEventImportService[0]
      Game '2355', Franchise '28': GameFieldingStatistics.Assists = 10, but play-by-play derives 11.
```

Expected:
PlateAppearances/AtBats should match. Errors treated as likely official-scorer-judgment noise
per the earlier "EarnedRuns" precedent, not investigated here. Assists discrepancy (game 2355,
Franchise 28/CHA) checked for the same error-annotation patterns behind prior Assists/Errors
bugs and found none -- doesn't look related to the bug below; needs its own separate
investigation, not chased further per priority.

Status: **Resolved**

Level: Medium (parses successfully; affects a persisted stat used in statistical calculations,
confirmed to already exist in 20 already-imported games, not an isolated occurrence)

Root cause:
`GameEventType.Error` is overloaded to mean two structurally different things.
`PlayCodeParser.ParseSingleCode`'s `FLE<n>` case (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`)
-- a foul ball dropped for an error -- returns `GameEventType.Error` with its own comment stating
"the plate appearance continues... the batter never becomes a runner at all." But
`GameStatisticsResolver.PlateAppearanceEndingEvents`
(`src/lib/Retrosharp/Format/PlayByPlay/GameStatisticsResolver.cs`) includes `Error`
unconditionally, with no way to distinguish "the play code that returned Error" -- so every
`FLE$` gets counted as a full plate appearance (and, since `Error` isn't in `NonAtBatEvents`
either, a full at-bat too), on top of whatever event *actually* ends that same plate appearance
a few pitches later.

Traced game 1257 exactly: NYA's `goldp001` fouls one off for an error (`FLE2`, confirmed zero
`GameEventRunner` rows for that play -- the batter never reached base), then the same plate
appearance genuinely ends two pitches later with a flyout (`7/F7`). Both get counted, double-
counting that one plate appearance -- exactly matching the reported gap (PA 44->45, AB 34->35,
both off by exactly 1).

Not isolated to this game: queried every currently-imported `GameEvent` row with
`RawEventText LIKE 'FLE%'` -- **20 occurrences across 18 games**, every single one with zero
`GameEventRunner` rows, all hitting the same overcounting bug. Every game with a foul-ball-error
in it has (or will have, once reconciliation runs) an inflated PlateAppearances/AtBats total for
that batter.

Also affected, not yet confirmed against a live example: `PitcherEventAggregateResolver.IsAtBat`
(`src/lib/Retrosharp/Format/PlayByPlay/PitcherEventAggregateResolver.cs`) explicitly mirrors
`GameStatisticsResolver`'s at-bat rule ("Mirrors GameStatisticsResolver.ApplyBatterEvent's at-bat
rule exactly") against the *same* `PlateAppearanceEndingEvents`/`NonAtBatEvents` sets -- but it
operates on `PitcherGameEventRecord`, a flat DB projection with no runner information at all, so
the same "does the batter have a runner row" check that would fix `GameStatisticsResolver` isn't
directly available there.

Fix:
Added a distinct `GameEventType.FoulBallError` (appended after `OtherAdvance`, so every existing
persisted row's integer value is unaffected) instead of reusing `Error`
(`src/lib/Retrosharp/Contract/GameEvent/GameEventType.cs`). `PlayCodeParser`'s `FLE<n>` case now
returns it (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Neither
`GameStatisticsResolver.PlateAppearanceEndingEvents` nor `PitcherEventAggregateResolver.IsAtBat`
needed any code change -- `FoulBallError` is excluded by construction simply by never being added
to that set, which also confirms the earlier suspicion that `PitcherEventAggregateResolver` shared
the same bug: it's fixed by the same one-line root change, no separate fix needed. Audited every
other `GameEventType.Error` reference in the codebase (`PlayCodeParser`'s two genuine-error sites,
`GameStatisticsResolver`'s `PlateAppearanceEndingEvents` set, and three test assertions) --
confirmed none of them needed to change; only the `FLE<n>` site was reclassified. Updated one
test in `GameEventResolverTests.cs` whose zero-runner allowlist referenced `Error` (only true
because of `FLE` plays, which no longer map to it) to reference `FoulBallError` instead, and
`PlayCodeParserTests.cs`'s existing FLE test to assert the new type. Added a new
`GameStatisticsResolverTests` case modeling the exact bug shape (an `FLE`-typed play followed by
the real event that actually ends the same plate appearance) confirming only one PA/AB is
counted.

Backfill (completed):
`2025NYA.EVA` (and 17 other files) were already imported with the old code. Applied a precise,
fully-verified SQL transaction (not a C# tool, given the small bounded scope of 20 already-
identified rows): updated all 20 already-persisted `FLE` `GameEvent` rows' `EventType` from
`Error` (10) to `FoulBallError` (verified programmatically via a throwaway console app rather
than trusting manual enum counting: `23`), then subtracted exactly 1 `PlateAppearances`/`AtBats`
per affected `(BatterId, battingFranchiseId, SeasonYear)` from the season `Batting` table (2 for
one batter who had two separate `FLE` plays that season) -- confirmed all 20 rows have
`IsSacHit = IsSacFly = false`, so every one was a clean +1/+1 overcount with no sacrifice
exclusion to account for.

Verification:
All 164 tests in `Retrosharp.Format.Tests` pass (197 across the full solution). Snapshotted
before/after: `GameEvent` rows with `RawEventText LIKE 'FLE%'` went from 20-at-`EventType=10` to
0-at-`EventType=10`/20-at-`EventType=23`; all 19 affected `Batting` rows decreased by exactly the
expected delta. Re-ran the exact per-game reconciliation for game 1257/Franchise 87: `PlateAppearances`
44/44, `AtBats` 34/34 -- exact match. Also imported `2025NYN.EVN` fresh (never previously
imported, contains 4 of its own `FLE` plays) end to end: "81 games inserted, 0 games skipped, 81
games' statistics applied" with zero exceptions and, notably, zero `PlateAppearances`/`AtBats`
reconciliation warnings anywhere in the whole file (only the already-known Errors/EarnedRuns/
Assists noise categories) -- confirmed in the database that all 4 fresh `FLE` plays landed with
`EventType = 23` and zero runners, exactly as designed.

## Spec/triple-play audit: five parser gaps

Actual:
Proactive audit, not triggered by an import warning or exception. Fetched Retrosheet's full
event-file specification (retrosheet.org/eventfile.htm) and its complete 2000-2025 triple-play
log (retrosheet.org/TriplePlays.htm, ~90 real plays) and cross-referenced both against
`PlayCodeParser.cs` line by line. Five distinct gaps found, none triggered by any file in
`docs/csv/` today (confirmed by grep -- none of the five patterns below occur in any currently
available file), so none of these were caught by the reconciliation-warning mechanism the way
every earlier defect in this document was.

Expected:
Log all five; fix the ones with real stat/crash impact first.

Status: **3 Resolved (items 1, 2, 4), 2 open (items 3, 5)**

Level: Mixed -- item 1 is Critical (aborts the whole file), items 2 and 4 are Medium
(stat-corrupting, no crash), items 3 and 5 are Low (item 3 is rare; item 5 has no stat impact,
only a descriptive-data one -- see each item).

### Item 1 (Resolved): Runner-interference advance annotation crashes the parser

Retrosheet's own documented example: `S/L9S.3-H;2X3(5/INT);1-2` -- "Interference can be indicated
with an advance parameter... An alternative way of writing this is (5/INT)."
`ApplyAdvanceSegment` (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`) picked `"5/INT"`
as the out's fielder chain and handed it straight to `ParseFielderChain`, which throws on the
`/` (`"Unexpected character '/' in fielder chain"`). Any file containing this documented shape
would abort entirely (parse-everything-then-write architecture -- zero games from that file
would import). Same class of gap as the already-fixed `(E5/TH)` case, one directory over
(`/INT` instead of `/TH`), never covered.

Fix: strip a trailing `/<tag>` suffix from the fielder-chain annotation before parsing it,
mirroring the error annotation's existing `slash >= 0 ? annotation[1..slash] : annotation[1..]`
handling (`ApplyAdvanceSegment`, `src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Added
`Parse_RunnerInterferenceAdvanceAnnotation_DoesNotThrow` using Retrosheet's own documented
example verbatim (`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

### Item 2 (Resolved): `K##` dropped-third-strike putout misattributed to catcher

Retrosheet's own documented example, verbatim: "A dropped third strike with a putout at first
base is given by the event `K23`." `ParseSingleCode`'s `"K"` branch
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`) matched on `code.StartsWith("K")`,
which is also true for `"K23"`, and never looked at the trailing digits -- so `K23` was treated
identically to a bare `K`. The fallback logic from the earlier "Missing catcher putout on
strikeouts" fix then unconditionally credited the catcher with an *unassisted* putout, when the
real play is catcher **assist** + first baseman **putout**. Inflated catcher Putouts and
undercounted the relay fielder's Putouts on every dropped-third-strike-thrown-elsewhere play.

Fix: the `"K"` branch now parses any trailing digit suffix as a real fielder chain via the
existing `ParseFielderChain`, only falling back to the bare-K unassisted-catcher-putout default
(unchanged) when there is no suffix (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`).
Added `Parse_DroppedThirdStrikeThrownToFirst_CreditsCatcherAssistAndFirstBasePutout` (Retrosheet's
own `K23` example) and a regression guard, `Parse_BareStrikeout_StillCreditsCatcherUnassistedPutout`,
confirming the existing bare-`K` behavior is unchanged
(`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

### Item 3 (Open, not fixed): Phantom assist on unassisted multi-out plays

Real example, 5/29/2000 (the first entry in Retrosheet's own triple-play log, flagged `[1]`
unassisted): `4(B)4(2)4(1)/LTP` -- Randy Velarde (2B) catches a liner and retires all three
runners himself, no throw ever happens. `AssignFieldedOutGroup`'s carry-over logic
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`) -- designed for genuine relay plays
like `64(1)3`, where the previous group's finishing fielder really did throw the ball to enable
the next out -- has no way to tell that apart from the same fielder appearing again with no
throw involved, so it manufactures a phantom assist for him on group 2 and group 3, inflating his
season Assists by 2 for this one play. Lower priority than items 1/2/4: genuinely unassisted
double/triple plays are rare, and this specific primary-code-parenthetical-group notation for
them is rarer still (most modern unassisted plays in Retrosheet's own log are instead recorded
via the advance section, which doesn't have this bug -- see item 5 below). Not fixed in this
pass; logged for future work.

### Item 4 (Resolved): `"99"` unknown-play placeholder miscredited to right fielder

Retrosheet's own documented text, verbatim: "the double digit combination 99, which cannot arise
in play, is used to code unknown plays including forms that otherwise describe force outs and
the double plays... No assist or putout credits are given." `ParseFieldedOutGroups` had no
special case for it -- `"99"` ran through the ordinary digit-chain path and generated a real
assist + putout credited to **position 9 (right field)**, even though `9` here is explicitly a
"fielder unknown" placeholder, not an actual position. Would corrupt the right fielder's Putouts
and Assists whenever this placeholder appears (chiefly older/incomplete games; not present in
any currently-imported 2025 file).

Fix: `AssignFieldedOutGroup` now skips generating fielding credits when a group's own digit
chain is exactly `"99"`, still marking the runner out but with zero credits, per Retrosheet's
own "no assist or putout credits are given" rule
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Scoped narrowly to a group's own raw
digits (not the carry-over-prefixed chain), since carry-over represents a real fielder from an
earlier group in the same play, unrelated to an unconnected group happening to be the `"99"`
placeholder -- noted as a residual, unconfirmed edge case (a `"99"` group immediately followed by
a real fielder group in the same play) not chased further since no real occurrence of even the
simple case exists yet to validate against. Added
`Parse_UnknownPlayPlaceholder99_ProducesNoFieldingCredits`
(`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

### Item 5 (Open, not fixed): Wrong `EndBase` on non-forced primary-code out groups

`AssignFieldedOutGroup` always sets `EndBase = NextBase(StartBase)` for a primary-code
parenthetical out-group, which is correct for a genuine force (confirmed by the `/FO` modifier's
own documented meaning) but wrong when the group represents a runner doubled/tripled off *their
own* base on a caught line drive or fly -- no force exists there. Confirmed by two real games
using the identical string `3(B)3(1)5(3)/LTP` (07/29/2020 CHN@CIN, 04/17/2021 CLE@CIN): the `(1)`
group's fielder is `3` (first baseman) -- physically standing at first base, so that runner is
retired *at* first, not advanced to second; likewise the `(3)` group's fielder `5` (third
baseman) retires the runner *at* third, not home. Retrosheet's notation doesn't encode an ending
base for these groups at all (only who's out and who fielded it) -- the true ending base is
context-dependent (which base the credited fielder plausibly covers) and isn't mechanically
derivable from the digits the way a `"startXend"` advance-section entry is. Checked
`GameStatisticsResolver` (`src/lib/Retrosharp/Format/PlayByPlay/GameStatisticsResolver.cs:108`):
`EndBase` is only consulted for `EndBase == Home && !IsOut` (run scoring), which never fires for
an out, so this corrupts the stored play-by-play detail (`GameEventRunner.EndBase`) but not any
counting stat. Lower priority than items 1/2/4 for that reason. Not fixed in this pass -- would
need the same "defer the decision until enough context is known" pattern already used for
GroundOut-vs-FlyOut (`isFieldedOutPendingTrajectory`), keyed off the `/FO` modifier's presence,
and the correct default for the non-forced case is still an open question (same-base is the
overwhelmingly common real pattern, but not universal -- see the McGwire triple play
`8(B)2(3)6(2)/TP` from 5/31/2000, where the `(3)` group's fielder is the catcher, which can only
mean the runner from third was retired at home, not doubled off at third). Logged for future
work, not chased to a fix without more real examples to validate against.

Verification (items 1, 2, 4):
All 168 tests in `Retrosharp.Format.Tests` pass (201 across the full solution: 168 + 10
`Retrosharp.Service.Tests` + 23 `Retrosharp.Engine.Console.Tests`). No live-import verification
was possible or necessary: none of the three fixed patterns occur in any currently-available
event file (confirmed by grep across `docs/csv/*.EV*` before writing the fixes), so there is
nothing to backfill and no real file to re-import -- these are implemented per Retrosheet's own
documented spec ahead of ever being observed in real data, the same way the bare `"C"` (catcher's
interference) case was implemented earlier this session.

## PlayCodeParseException: 2025TBA.EVA

Actual:

```text
Retrosharp.Format.PlayByPlay.PlayCodeParseException: Fielded-out code has no trajectory modifier (G/L/F/P/BG/BP/BL) to determine GroundOut vs FlyOut. Raw play code: '2/BINT'.
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.Parse(String rawEventText, String countField, String pitchSequence) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs:line 57
   at Retrosharp.Format.PlayByPlay.GameEventResolver.ResolvePlay(...) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\GameEventResolver.cs:line 107
   at Retrosharp.Format.PlayByPlay.GameEventResolver.Resolve(...) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\GameEventResolver.cs:line 81
   at Retrosharp.Service.GameEventImportService.MapToGameEventRecordAsync(EventFileGame game) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp.Service\GameEventImportService.cs:line 105
   at Retrosharp.Service.GameEventImportService.ImportAsync(String filePath) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp.Service\GameEventImportService.cs:line 56
   at Retrosharp.Engine.Console.Saga.GameEventSaga.Handle(GameEventStart message, IMessageHandlerContext context) in D:\Code\TheFlyingArcher\Retrosharp\src\engine\Retrosharp.Engine.Console\Saga\GameEventSaga.cs:line 52
```

Expected:
All documented Retrosheet play codes should parse without throwing.

Status: **Resolved**

Level: High (blocks parsing this file)

Root cause:
`2/BINT` is a batter-interference out (`BINT`, "batter interference" -- a documented modifier):
the batter is called out for interfering with a fielder, with no ball ever put in play. Every
other digit-led primary code in `PlayCodeParser` (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`)
is a genuine fielded out, where trajectory (ground/line/fly/pop) is always determinable from a
modifier -- the pending-trajectory resolution in `Parse()` correctly treats a missing one as a
data gap for those. But `BINT` has no batted ball at all, so requiring a trajectory modifier for
it is simply wrong, not a missing-modifier gap. Confirmed against every real `BINT` play
currently available (`grep -n BINT docs/csv/*.EV*`): three of the four already carry a real
trajectory modifier alongside `BINT` (`2/P2F/FL/BINT`, `2/G2/BINT`, `13/BG1S/BINT`) and parse
fine; only the reported `2/BINT` -- and one more in the same file, `13/BG1S/BINT`'s sibling in
`2025TBA.EVA` line 6616, which actually already has a `BG` trajectory and was unaffected -- has
no trajectory modifier at all. So this was narrowly the bare-`BINT` case, not `BINT` in general.

Fix:
`ApplyModifiers` now recognizes a bare `"BINT"` modifier and sets a new `isBatterInterference`
flag; the pending-trajectory switch in `Parse()` falls back to `GameEventType.GroundOut` only
when no real trajectory modifier was found *and* `isBatterInterference` is set -- a genuine
trajectory modifier (as in the other three real `BINT` plays) still takes priority, unchanged
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). `GroundOut` was chosen deliberately,
not a new `GameEventType`: offensive interference is scored as an ordinary out for statistical
purposes (it ends the plate appearance and counts as an at-bat, unlike catcher's interference or
a sacrifice), which is exactly what `GroundOut` already means to `GameStatisticsResolver`. Added
`Parse_BareBatterInterference_ResolvesToGroundOutWithNoTrajectoryModifier` (the exact reported
play) and `Parse_BatterInterferenceWithRealTrajectory_TrajectoryTakesPriority` (confirming the
three already-working real `BINT` plays stay unaffected)
(`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

Verification:
All 170 tests in `Retrosharp.Format.Tests` pass (203 across the full solution). `2025TBA.EVA` had
never been successfully imported before (the exception aborted the whole file before any writes).
Imported it for real end to end (API -> NServiceBus -> engine saga -> Postgres): "81 games
inserted, 0 games skipped, 81 games' statistics applied" with zero exceptions. Confirmed in the
database that the exact reported play (`RawEventText = '2/BINT'`, game 257) resolved to
`EventType = GroundOut` (8), `BattedBallType = null`, with a single fielding credit -- an
unassisted Putout to position 2 (catcher), no phantom entries. Also confirmed the file's other
`BINT` plays and the four already-imported `BINT` plays from other files (`2025ATL.EVN`,
`2025PHI.EVN`) are unaffected: all still resolve with their real trajectory-derived
`BattedBallType`.

Noted, not investigated (out of scope for this defect): `2025TBA.EVA` also produced five
unrelated reconciliation warnings (GroundedIntoDoublePlay, StolenBases, TimesCaughtStealing,
PitchersUsed, and Assists discrepancies across five different games, all Franchise 119). Not
chased here since none relate to `BINT` parsing; flagging for a future defect if they turn out
not to be official-scorer-judgment noise.

## Partial import: 2025PHI.EVN (18 of 81 games)

Actual:
Asked to list franchises with 2025 game events imported. PHI (Philadelphia Phillies, Franchise
99) showed only 18 of 81 home games with `GameEvent` data -- every other franchise with any home
games imported had all 81. The 18 successful games were exactly the file's first 18 by date
(chronological order, matching how Retrosheet orders games within a team's own file); game 489
(the 18th, May 3rd) had `GameEvent` rows but no `GameEventGameStatus` claim row; every game after
it (503 onward) had neither.

Expected:
Either all-or-nothing for a successfully-parsed file, or a clear, diagnosable reason for a
partial result.

Status: **Resolved**

Level: High (blocks recovery from any interrupted import, not just this one file)

Root cause:
Initially assumed to be a simple interrupted process (crash, kill, restart) mid-file, since
`GameEventRepository.BulkInsertAsync` commits each game's events in its own transaction --
consistent with "some prefix of games fully done, the rest untouched." Re-running the import to
test that theory instead surfaced a **second, distinct, and more serious bug**: the re-run made
*zero* additional progress and, after exhausting NServiceBus's full retry policy, moved the
message to the `Retrosharp.Engine.Errors` queue.

Root cause of the re-run's failure: `GameStatisticsRepository.TryApplyGameStatisticsAsync`
(`src/lib/Retrosharp.Data/GameStatisticsRepository.cs`) adds a `GameEventGameStatusModel`,
calls `SaveChangesAsync()`, and on a unique-constraint violation (meaning the game was already
claimed -- the expected, designed-for "already done" case) calls
`RollbackTransactionAsync()` and returns `false`. But `RollbackTransactionAsync()` only undoes
the *database* transaction -- it does nothing to EF Core's in-memory change tracker, which still
holds that `GameEventGameStatusModel` as `Added`. `GameEventRepository.BulkInsertAsync` reuses
one `DbContext` across every game in the file (`TryApplyGameStatisticsAsync` is called once per
game inside its loop), so that stale entity sits in the tracker until the *next* unrelated
`SaveChangesAsync()` call on the same context -- which turned out to be `BulkInsertAsync`'s own
per-game event-insert step for whatever game came next. That flush re-submits the same already-
rejected insert, fails with the identical `PK_GameEventGameStatus` violation, but this time with
no catch block expecting it -- crashing the entire import.

Confirmed by direct trace of the re-run's log: 68 occurrences of the `PK_GameEventGameStatus`
violation (one EF diagnostic-log entry per already-claimed game hit, all otherwise harmless), but
the *escaped, uncaught* copy of the exception's stack trace ran through
`Retrosharp.Data.GameEventRepository.BulkInsertAsync` -- not
`GameStatisticsRepository.TryApplyGameStatisticsAsync` at all -- exactly matching this mechanism.
This isn't specific to PHI or to this particular partial-import scenario: it means **any** re-run
of a partially-completed file, or re-POSTing an already-fully-imported file by accident, was
guaranteed to crash the same way. The original interruption that first left PHI at 18/81 games is
still unexplained (no log survives from whatever process ran it), but is no longer the operative
problem -- this change-tracker bug is what actually blocked recovery.

Fix:
`TryApplyGameStatisticsAsync` now keeps a reference to the `GameEventGameStatusModel` it adds and,
in the "already claimed" catch block, explicitly sets `_context.Entry(statusModel).State =
EntityState.Detached` after the transaction rollback -- undoing the `Add` from the change
tracker's point of view, not just the database's
(`src/lib/Retrosharp.Data/GameStatisticsRepository.cs`). No dedicated test project exists for the
repository/EF layer (`Retrosharp.Format.Tests`, `Retrosharp.Service.Tests`, and
`Retrosharp.Engine.Console.Tests` are the only test projects, none of which exercise a real
Postgres unique-constraint violation), and this bug specifically requires one to trigger the
`Npgsql.PostgresException` the catch filter matches on -- so this was verified via a live
end-to-end re-import rather than a synthetic unit test, the same way the very first defect in
this document (the `DateTime` `Kind=UTC` repository bug) was verified.

Verification:
All 203 tests across the solution still pass (no regression). Rebuilt and re-ran the full import
of `2025PHI.EVN` end to end (API -> NServiceBus -> engine saga -> Postgres): "63 games inserted,
18 games skipped, 64 games' statistics applied, 17 games' statistics already claimed" -- the
previously-stuck game 489 and all 63 never-attempted games (503 onward) completed successfully,
with zero uncaught exceptions and zero retries this time (confirmed
`GameEventRepository.BulkInsertAsync` no longer appears in any exception stack trace in the new
run's log). Confirmed in the database: all 81 of PHI's 2025 home games now have both `GameEvent`
rows and a `GameEventGameStatus` claim row.

Noted, not investigated (out of scope for this defect): the completed import produced four
unrelated reconciliation warnings (Assists x2, Errors, GroundedIntoDoublePlay across four
different games). Not chased here for the same reason as the `2025TBA.EVA` note above.

## Batch import of remaining 2025 franchises: two crashes

Actual:
Imported every remaining not-yet-imported `.EVN`/`.EVA` file in `docs/csv/` one at a time via the
API, as an end user would (ATL, CHA, CHN, CIN, CLE, DET, KCA, MIL, MIN, PIT, SLN). 9 of 11
completed cleanly (81 games each, zero exceptions): ATL, CHA, CIN, CLE, DET, MIL, MIN, PIT, SLN.
Two crashed and imported 0 games (per the usual parse-everything-first architecture): `2025CHN.EVN`
and `2025KCA.EVA`.

Expected:
All documented Retrosheet play codes should parse without throwing.

Status: **Resolved (both)**

Level: High (blocks parsing the respective file) for both.

### Crash 1: `2025CHN.EVN` -- `InvalidOperationException`, runner on Second not found

```text
System.InvalidOperationException: Play 'S7/L7.3-H(UR);2-H(UR);BX2(754)' (inning 8) references a runner on Second that the resolver has no record of -- a preceding play or substitution was missed. Current baserunners: [First=swand001(slot5), Third=bertj001(slot4)]
   at Retrosharp.Format.PlayByPlay.GameEventResolver.ResolvePlay(...) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\GameEventResolver.cs:line 145
```

Source: `docs/csv/2025CHN.EVN:1137`.

Root cause: **not erroneous Retrosheet data -- a confirmed parser bug**, traced by manually
replaying the actual half-inning (`docs/csv/2025CHN.EVN:1128-1137`) against the real baserunner
sequence:

- Line 1131 (`FC5/G56.2-3;1-2(E5);B-1`): `happi001` (2nd->3rd), `tuckk001` (1st->2nd via error),
  `bertj001` reaches 1st. State: 1st=`bertj001`, 2nd=`tuckk001`, 3rd=`happi001`.
- Line 1132 (`S9/L89+.3-H(UR);2-H(UR);1-2`): both `happi001` and `tuckk001` score, `bertj001`
  advances 1st->2nd. State: 2nd=`bertj001`.
- Line 1133 (`K+SB3;SB2`): a strikeout **bundled with a double steal** -- `bertj001` (2nd->3rd)
  and `swand001` (1st->2nd, from the previous line's own single, `S9`... actually from
  `swand001`'s own at-bat two lines earlier) should both move. State should become: 2nd=`swand001`,
  3rd=`bertj001`.
- Line 1137 (the crashing play): advances `3-H(UR)` and `2-H(UR)` reference exactly those two
  runners -- and match the manually-traced state exactly. But the resolver's own snapshot at
  crash time shows `swand001` still on **First**, not Second -- meaning the `SB2` half of line
  1133's double steal never actually moved him.

Confirmed by reading `PlayCodeParser.ParsePrimaryCode`
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`): the `"+"` combinator (`plusIndex >= 0`,
handling `K+<event>`) is checked *before* the `;`-joined multi-steal combinator (`primaryCode.Contains(';')`)
and returns immediately, calling `ParseSingleCode` directly on the right-hand side
(`right = primaryCode[(plusIndex + 1)..]`) without ever routing it back through the `;`-splitting
logic. For `"K+SB3;SB2"`, `right` is `"SB3;SB2"` -- handed whole to `ParseSingleCode`, which has
no semicolon-awareness of its own. `ParseSingleCode`'s `"SB"` branch reads only `code[2]` (`'3'`)
to determine the stolen base, silently ignoring everything from the `;` onward -- so `SB3` (the
first steal) is processed correctly, and `SB2` (the second) is dropped entirely, leaving that
runner's tracked position stale. This is a real interaction gap between two already-existing,
independently-working features: the `"+"` bundling combinator (works standalone, e.g. `K+SB3`)
and the `";"`-joined multi-steal combinator (works standalone, e.g. `SB3;SB2` -- the existing code
comment even cites a real confirmed example, `docs/csv/2025SDN.EVN`) -- just never exercised
together (`K+SB3;SB2`) until this real play surfaced it. Not isolated notation either: the
double-steal codes `SB2;SBH` and `SB3;SB2` are both documented directly in Retrosheet's own spec
("show double steals, second and third in one case, second and home in the other"), and bundling
onto a `K`/`W`/`PO`-type primary code via `"+"` is equally standard, so this combination is
expected to recur.

Fix: extracted the `;`-splitting logic (previously inline in `ParsePrimaryCode`, only reached
when there's no `"+"`) into a shared `ParseSingleOrMultiCode` helper, and route the right-hand
side of a `"+"` bundle through it instead of calling `ParseSingleCode` directly
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). The top-level (no `"+"`) case is
unchanged in behavior -- same splitting logic, just shared. Added
`Parse_StrikeoutBundledWithDoubleSteal_BothRunnersMove`, using the exact real play from
`2025CHN.EVN:1133` (`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

### Crash 2: `2025KCA.EVA` -- unrecognized `(SB<base>)` advance annotation

```text
Retrosharp.Format.PlayByPlay.PlayCodeParseException: Unrecognized advance annotation '(SB3)' in '2-3(SB3)'. Raw play code: 'BK.2-3(SB3);1-2'.
   at Retrosharp.Format.PlayByPlay.PlayCodeParser.ApplyAdvanceSegment(String segment, String rawEventText, IDictionary`2 runners) in D:\Code\TheFlyingArcher\Retrosharp\src\lib\Retrosharp\Format\PlayByPlay\PlayCodeParser.cs:line 961
```

Source: `docs/csv/2025KCA.EVA:5871` -- `play,3,1,pasqv001,22,SFBFBB,BK.2-3(SB3);1-2`.

Root cause: same class of gap as the already-fixed `(WP)`/`(PB)` advance-annotation defect --
`ApplyAdvanceSegment`'s annotation loop
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`) recognizes `E$`, `NR`/`NORBI`,
`UR`/`TUR`, a bare digit-prefixed fielder chain, and `WP`/`PB` -- anything else throws. `(SB3)`
here is a balk (`BK`) where the runner on second was mid-steal-attempt to third when the balk was
called -- the informational annotation notes that the advance coincided with (or was really
caused by) a stolen-base attempt, the same purely-descriptive role `(WP)`/`(PB)` already play on
an advance. Checked scope across every currently-available file
(`grep -c "(SB[23H])" docs/csv/*.EV*`): exactly 1 occurrence, this one -- same narrow-but-real
pattern as the `(WP)`/`(PB)` precedent.

Fix: added an `(SB<base>)` branch to `ApplyAdvanceSegment`'s annotation loop, mirroring the
existing `WP`/`PB` no-op treatment exactly -- purely informational, doesn't touch `IsRBI`,
`IsEarnedRun`, or fielding credits
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Added
`Parse_BalkAdvanceWithStolenBaseAnnotation_DoesNotThrow`, using the exact real play from
`2025KCA.EVA:5871` (`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`).

Verification:
All 172 tests in `Retrosharp.Format.Tests` pass (205 across the full solution). Rebuilt the
engine and API and re-imported both files end to end (API -> NServiceBus -> engine saga ->
Postgres): `2025CHN.EVN` -- "81 games inserted, 0 games skipped, 81 games' statistics applied"
with zero exceptions; `2025KCA.EVA` -- same, "81 games inserted, 0 games skipped, 81 games'
statistics applied" with zero exceptions. Confirmed in the database: the `K+SB3;SB2` play in the
actual crashing game (GameId 157) now produces all three expected runner rows, including the
previously-dropped steal (First->Second); the very next play in that same half-inning,
`S7/L7.3-H(UR);2-H(UR);BX2(754)` (the one that originally threw), now resolves cleanly and
correctly scores both runners. The `BK.2-3(SB3);1-2` play in `2025KCA.EVA` (GameId 1053) resolved
with both runners safely advanced (Second->Third, First->Second), matching a balk's semantics
exactly.

With both fixed, every currently-available 2025 `.EVN`/`.EVA` file in `docs/csv/` has now been
successfully imported -- all 30 MLB franchises have their full 2025 home schedule (81 games each)
in the database.

## Discrepancy warnings from remaining 2025 imports

Actual:
Evaluated every reconciliation warning saved (not logged at the time) from the recent import
batch -- `2025TBA.EVA`, `2025PHI.EVN`, and the 11-file batch (ATL/CHA/CHN/CIN/CLE/DET/KCA/MIL/
MIN/PIT/SLN) -- 209 warnings total. Investigated each category against real data.

Expected:
Determine which are real bugs vs. official-scorer-judgment noise; fix the highest-confidence,
highest-impact ones first.

Status: **Assists and Errors Resolved; GIDP and EarnedRuns confirmed not bugs (same class as
earlier-established precedents); StolenBases/TimesCaughtStealing, WildPitches, PitchersUsed, and
Putouts evaluated and logged, not yet fixed**

### Assists (18 of 209 warnings, always exactly "+1" in derived) -- Resolved

Root cause: two independent bugs, both in the fielder-chain "carry-over" mechanism
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`):

1. **Carry-over self-throw artifact.** When a primary-code group's own putout was unassisted
   (a bare single digit, e.g. `3(B)3(1)/GDP/G3` -- game 330) and the *next* group's own digit is
   the *same* fielder, `AssignFieldedOutGroup`'s carry-over logic still prepended him as an
   assist onto the next group -- crediting a fielder with "throwing to himself." Same shape via
   the implicit trailing-digit form too (`4(1)4/GDP/G34` -- games 270/1411): 2B forces the runner
   unassisted, then separately (no throw) also retires the batter, and the trailing bare "4"
   still inherited the carry-over even though it's identical to its own digit.
2. **Repeated-fielder-in-one-chain over-crediting.** Real rundown chains such as
   `POCS2(134634)` (docs/csv/2025TBA.EVA, game 2133: pitcher to 1B to 2B to SS to 1B to 2B)
   credited the *same* fielder a separate assist for *every* touch. Official scoring credits a
   fielder at most one assist per play no matter how many times he touches the ball during a
   single rundown.

Confirmed against real data: the persisted (Game Log Parser) Assists total was always exactly 1
lower than what the buggy parser derived, in every one of the 18 affected games, and the two
mechanisms above account for all 18 exactly (11 rundown-repeat cases, 7 self-throw cases) --
verified game by game before writing any fix.

Fix:
1. `AssignFieldedOutGroup` now detects when the carry-over fielder digit exactly matches the
   current group's own (single-digit) leading digit and, in that case, skips the carry-over
   prefix entirely and returns no carry-forward value (since no throw happened in this group
   either). Return type changed from `char` to `char?` to represent "nothing to carry forward."
2. `ParseFielderChain` now tracks which positions have already been credited an assist within
   the current chain (a `HashSet<byte>`) and skips crediting a repeat -- the chain's *final*
   token is unaffected regardless of whether its position repeats an earlier one (a fielder who
   touches the ball early and *also* completes the final putout himself legitimately keeps both
   credits; only repeated *non-final* touches by the same fielder are deduplicated).

Added four regression tests using the exact real plays above
(`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`):
`Parse_UnassistedBatterThenUnassistedRunner_NoPhantomAssist`,
`Parse_UnassistedForceThenSameFielderTrailingPutout_NoPhantomAssist`,
`Parse_PickoffCaughtStealingRundownWithRepeatedFielder_OnlyOneAssistPerFielder`. A pre-existing
test, `Parse_RundownStyleChain_RepeatedFielderProducesRepeatedAssist` (`POCS2(1341)`), already
covered the "touches the ball early AND completes the final putout" case correctly -- confirmed
it still passes unchanged, since that position only appears once among the chain's *non-final*
tokens.

### Errors (11 of 209 warnings, always exactly "+1" in derived) -- Resolved

Root cause: every one of the 11 affected games has an `FLE<n>` (foul ball dropped for an error)
play. This is the exact gap flagged but explicitly deferred when `FoulBallError` was split out
earlier this session ("the missing fielding-Error-credit gap for `FLE$` plays... would require a
schema change since `GameEventFieldingCredit.GameEventRunnerId` is NOT NULL and FLE plays have no
runner row -- explicitly scoped out"). Now confirmed with 11 real examples of its actual,
consistent impact.

Fix:
The `FLE<n>` case in `ParseSingleCode` now creates a runner row for the batter with
`StartBase == EndBase == BattersBox` (no actual base movement, `IsOut = false`) purely to attach
the error credit to, satisfying the NOT NULL constraint without implying the batter reached base
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`). Verified this doesn't affect base-
occupancy tracking or GIDP/at-bat logic: `GameEventResolver` only adds a runner to the baserunner
tracker when `EndBase` is First/Second/Third, and `GameStatisticsResolver`'s GIDP check is gated
on `EventType == GroundOut` (FLE's EventType is `FoulBallError`), so neither is affected. Updated
the existing test (renamed `Parse_DroppedFoulError_NoRunnerRecorded` ->
`Parse_DroppedFoulError_NoBaseMovementButCreditsFielderError`) and the `GameEventResolverTests.cs`
zero-runner allowlist, which no longer needs to exempt `FoulBallError`
(`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`,
`src/lib/Retrosharp.Format.Tests/GameEventResolverTests.cs`).

Verification (both fixes):
All 175 tests in `Retrosharp.Format.Tests` pass (208 across the full solution).

Backfill (completed):
Both fixes affect games imported throughout this entire session, not just the 11-file batch that
surfaced the warnings -- a full-database sweep was needed, not a per-file one. Built a one-off
tool (scratch-only, not part of the repo) that re-derived `PlayCodeParser.Parse` output directly
from each already-persisted `GameEvent.RawEventText` (not the original source files -- most of
the earlier-imported files had already been replaced on disk by the time of this backfill, but
`RawEventText` is exactly what was originally parsed, so no source file is needed to re-derive
correctly) and diffed it against currently-persisted `GameEventRunner`/`GameEventFieldingCredit`
rows:
- **Assists**: for every existing runner row, compared the freshly-derived fielding-credit
  multiset against the persisted one; every difference found was confirmed to be an Assist-type
  removal (never an addition, never a Putout/Error) before deleting anything -- a hard safety
  gate that aborts on any other kind of mismatch. Found and correctly *excluded* 24 unrelated
  plays needing an *additional* Error credit (all `"C/E2..."` catcher's-interference-plus-error
  plays) -- a separate, pre-existing gap from the already-shipped "Errors undercount from C/E2
  modifier" fix (see "Discrepancy Issues in 2025BOS.EVA" above), whose own backfill only ever
  covered BOS's specific games. Flagged as its own follow-up, not touched here.
- **Errors (FLE)**: for every `FoulBallError` event with zero runner rows, resolved the error
  fielder's identity by finding the nearest-by-sequence *other* fielding credit in the same game,
  same fielding side, same position, then confirming via `GameSubstitution` that no substitution
  at that position occurred between the two plays before trusting it -- all 35 resolved this way
  with zero ambiguous cases requiring manual review.
- A genuine bug caught by the tool's own safety gate before any write: the season `Fielding`
  table is keyed by `(PersonId, FranchiseId, SeasonYear, Position)`, not just the first three --
  an early version of the backfill's `UPDATE` omitted `Position` and was caught immediately (the
  assertion "expected exactly 1 row affected" found 4), rolling back cleanly with no data written.

Post-backfill verification: `GameEventFieldingCredit` row count unchanged at 172,637 (35 deleted +
35 inserted); season `Fielding.Assists` sum decreased by exactly 35 (41,023 -> 40,988);
`Fielding.Errors` sum increased by exactly 35 (2,390 -> 2,425); zero `FoulBallError` events remain
without a runner row. Spot-checked game 330's `3(B)3(1)/GDP/G3` (now exactly 2 Putout credits, no
Assist) and one backfilled FLE play (`FLE3`, game 315 -- new runner at `BattersBox->BattersBox`,
`IsOut=false`, one Error credit at the correct fielder).

### GIDP (12 of 209 warnings, mixed direction) -- confirmed not a bug

Every affected game's discrepancy traces to a `/GDP/`-tagged play where the batter's own
`IsOut` is `false` (a force double play on *other* runners with the batter safe on the fielder's
choice, e.g. `75(2)4(1)/GDP/G7.B-1`) -- the exact same class already investigated and ruled not a
bug in "Discrepancy Issues in 2025BOS.EVA" above (game 559: "GIDP requires the batter himself to
be retired... that's correctly not a batter GIDP"). Mixed-direction discrepancies (sometimes
persisted higher, sometimes derived higher) are consistent with this being a genuine, pre-existing
scoring-convention edge case rather than a parser bug. Not chased further.

### StolenBases / TimesCaughtStealing (11 of 209 warnings) -- three distinct causes, all Resolved

- **`K+SB3;SB2` / `K+SBH;SB2` in other already-imported games -- Resolved.** The bundled-double-
  steal bug was already fixed in the parser (see the `2025CHN.EVN` crash entry above); this is
  its backfill for every *other* already-imported game sharing the same literal play text. A
  full-database sweep (`RawEventText ~ '\+SB[23H];SB[23H]'`) found exactly six occurrences total:
  game 157 (`2025CHN.EVN`), already correct from the live re-import; and five more needing a
  backfill --
  game 73 (`2025SLN.EVN`), game 107 (`2025PIT.EVN`), game 2288 (`2025MIL.EVN`),
  game 2427 (`2025CLE.EVA`), and game 905 (`2025TBA.EVA`).
  For the first four, the source file was still available on disk, so a one-off tool
  (scratch-only, not part of the repo) re-derived the *entire* game via the real
  `GameEventResolver.Resolve` (not just an isolated re-parse of the one play -- baserunner
  identity depends on the whole half-inning's preceding plays, unlike fielder identity, which was
  resolvable from position stability alone in the earlier FLE backfill) and inserted the missing
  `GameEventRunner` row for the dropped `SB2` steal, matched against the persisted play by exact
  `RawEventText` and `Sequence` before touching anything. Game 905's source file (`2025TBA.EVA`)
  was no longer on disk (already replaced), but needed no re-derivation at all: its play also has
  an explicit advance segment (`.3-H(NR);1-3(E2/TH)`) that independently creates the same runner
  row as a side effect regardless of the `SB2` bug, so the row already existed correctly --
  only the `Batting.StolenBases` *credit* (a transient, non-persisted flag on the parser's own
  runner object, never itself a column) was missing for that already-existing row's player.
  `Batting.StolenBases` incremented by exactly 1 for all 5 affected players, confirmed
  individually before and after. All 6 target plays now show 3 runner rows each, matching
  `2025CHN.EVN`'s already-correct state exactly.
- **Game 1053 (`2025KCA.EVA`) -- Resolved.** `BK.2-3(SB3);1-2`'s `(SB3)` annotation now also
  flags that runner's advance as `IsStolenBase = true`
  (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`) -- unlike `(WP)`/`(PB)`, which stay
  purely informational, `(SB<base>)` means the runner really was stealing, confirmed by the
  persisted Game Log crediting the steal (persisted 1, derived 0). The *other* runner in the same
  play (a plain `1-2` balk advance, no `(SB..)` annotation) correctly stays `IsStolenBase = false`.
  Updated the existing test (renamed
  `Parse_BalkAdvanceWithStolenBaseAnnotation_DoesNotThrow` ->
  `Parse_BalkAdvanceWithStolenBaseAnnotation_CreditsOnlyThatRunnerAStolenBase`) to assert both
  runners' `IsStolenBase` explicitly
  (`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`). All 175 tests in
  `Retrosharp.Format.Tests` pass (208 across the full solution). Checked the whole database for
  every occurrence of the `(SB<base>)` pattern before backfilling -- confirmed exactly one
  (game 1053's own play, the only one that ever existed) -- and applied a single-row backfill:
  `Batting.StolenBases` for the runner (PersonId 8427, Franchise 60/KCA, season 2025) incremented
  by exactly 1 (22 -> 23), matching the persisted Game Log's own count exactly.
- **Caught-stealing attempts negated by an error (e.g. `CS2(2E4)`, `POCS2(13E6)`) -- Resolved.**
  Official scoring charges a caught stealing whenever the runner "is put out, or would have been
  put out by errorless play" -- so an error negating the out (`IsOut = false`) shouldn't also
  negate the *attempt*. Added `IsCaughtStealingAttempt`, mirroring the existing `IsStolenBase`
  pattern end to end (`MutableRunner` -> `ParsedRunnerAdvance` -> `GameEventRunner`, all
  transient -- not a persisted column), set unconditionally in `ParseCaughtStealingLike` before
  any error-negation adjustment
  (`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`,
  `src/lib/Retrosharp/Format/PlayByPlay/ParsedRunnerAdvance.cs`,
  `src/lib/Retrosharp/Contract/GameEvent/GameEventRunner.cs`,
  `src/lib/Retrosharp/Format/PlayByPlay/GameEventResolver.cs`). `GameStatisticsResolver`'s
  `TimesCaughtStealing` counting now keys off this flag directly instead of the old
  `isCaughtStealingEvent (play-level) && runner.IsOut && StartBase != BattersBox` combination --
  which, being play-level rather than runner-level, had the *same* class of imprecision
  `IsStolenBase` was created to fix for `StolenBases`: a bundled or multi-out play could credit a
  runner who wasn't actually the one attempting the steal
  (`src/lib/Retrosharp/Format/PlayByPlay/GameStatisticsResolver.cs`). Added four regression
  tests: two at the parser level asserting `IsCaughtStealingAttempt = true` on the two existing
  error-negated-CS tests, and two at `GameStatisticsResolver`'s level (error-negated attempt
  still counts; a runner out for an unrelated reason on the same play does not)
  (`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`,
  `src/lib/Retrosharp.Format.Tests/GameStatisticsResolverTests.cs`). All 177 tests in
  `Retrosharp.Format.Tests` pass (210 across the full solution).

  Backfill (completed): a full-database sweep of every `GameEventRunner` row on a play whose
  `EventType`/`SecondaryEventType` is `CaughtStealing` or `PickoffCaughtStealing` (1,143 rows),
  computing both the old and new crediting decision for each and diffing them, found 9 total
  mismatches -- 8 needing a `+1` (error-negated attempts that were silently uncredited) and,
  confirming the imprecision concern above was real and not just theoretical, exactly 1 needing a
  `-1`: game 369's `CS3(15)/DP/RINT.1X2(54)` had *two* non-batter runners, only one of whom
  (`PersonId` 632, the actual `CS3` steal) was a real caught-stealing attempt -- the other
  (`PersonId` 10441, out via the play's own separate `1X2(54)` advance segment, unrelated to the
  steal) had been wrongly credited by the old play-level check. `Batting.TimesCaughtStealing`
  updated by exactly the expected `+1`/`-1` for all 9 affected `(PersonId, FranchiseId,
  SeasonYear)` rows, confirmed individually before and after.

### WildPitches (2 of 209 warnings) -- Resolved

Both affected games record their wild pitch only via the `(WP)` advance annotation (e.g.
`SB2.1-3(WP)`), not as its own primary `WildPitch` event -- since that annotation was previously
a pure no-op (see the earlier `(WP)`/`(PB)` fix), nothing incremented
`GamePitchingStatistics.WildPitches` for it.

Fix:
Added `CausedWildPitch`, mirroring the `IsStolenBase`/`IsCaughtStealingAttempt` pattern end to
end (`MutableRunner` -> `ParsedRunnerAdvance` -> `GameEventRunner`, all transient, not persisted
columns), set only for the `"WP"` annotation specifically -- `"(PB)"` stays a pure no-op, since
`Fielding.PassedBalls` is out of scope for this project entirely (explicit exclusion in
spec/phase-1-build-plan.md Step 6d), so there's nothing to flag it toward
(`src/lib/Retrosharp/Format/PlayByPlay/PlayCodeParser.cs`,
`src/lib/Retrosharp/Format/PlayByPlay/ParsedRunnerAdvance.cs`,
`src/lib/Retrosharp/Contract/GameEvent/GameEventRunner.cs`,
`src/lib/Retrosharp/Format/PlayByPlay/GameEventResolver.cs`). Unlike the batter-level
`IsStolenBase`/`IsCaughtStealingAttempt` flags, this credits the *pitcher* -- `GameStatisticsResolver`
checks `play.Runners.Any(r => r.Runner.CausedWildPitch)` **once per play** (not once per
annotated runner, since a single wild pitch can move more than one runner but must still only
count once), and only when the play's own `EventType`/`SecondaryEventType` isn't already
`WildPitch` -- a defensive guard against double-counting if a runner's advance were ever
redundantly annotated `"(WP)"` on a play whose primary event already is one (not observed in real
data, but cheap to guard against)
(`src/lib/Retrosharp/Format/PlayByPlay/GameStatisticsResolver.cs`). Added five regression tests
covering the annotation flagging (`(WP)` sets it, `(PB)` doesn't), the pitcher-crediting logic
(single annotated runner, two runners from the same wild pitch counting once, and the
primary-event double-count guard)
(`src/lib/Retrosharp.Format.Tests/PlayCodeParserTests.cs`,
`src/lib/Retrosharp.Format.Tests/GameStatisticsResolverTests.cs`). All 180 tests in
`Retrosharp.Format.Tests` pass (213 across the full solution).

Backfill (completed): a full-database sweep (`RawEventText LIKE '%(WP)%'`) found exactly 5
occurrences total -- the 2 originally reported plus 3 more from games imported earlier in the
session that were never surfaced as reconciliation warnings in this specific evaluation. None had
`EventType`/`SecondaryEventType` already `WildPitch`, so none were already counted.
`Pitching.WildPitches` incremented by exactly 1 for each of the 5 affected `(PersonId,
FranchiseId, SeasonYear)` rows (resolved directly from each play's own persisted `PitcherId` --
no identity-resolution heuristic needed, unlike the earlier FLE backfill), confirmed individually
before and after.

### PitchersUsed (2 of 209 warnings) and Putouts (1 of 209 warnings) -- not conclusively diagnosed

Investigated but didn't converge on a clear cause; each is a single/double occurrence. For
Putouts specifically, confirmed every out-runner in the affected game already has a putout
credit, so it isn't a missing-credit gap of the kind found elsewhere in this entry. Not chased
further without more evidence -- flagged here for whenever another occurrence surfaces.
in the database.