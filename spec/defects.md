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