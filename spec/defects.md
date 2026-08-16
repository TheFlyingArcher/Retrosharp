# Retrosharp Defects

I, the human, have been running Retrosharp in Visual Studio and using the product like the end user would. This spec details defects and bugs found along the way.

## Exception on Game Event import

Actual
This exception appeared while trying to import `D:\Code\TheFlyingArcher\Retrosharp\docs\csv\2025SDN.EVN` on game ID 7.
I initiated the request by sending a POST to `https://localhost:7017/api/gameevent/import` with body

```json
{
    "filePath":"D:\\Code\\TheFlyingArcher\\Retrosharp\\docs\\csv\\2025SDN.EVN"
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

Status: **Resolved**

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