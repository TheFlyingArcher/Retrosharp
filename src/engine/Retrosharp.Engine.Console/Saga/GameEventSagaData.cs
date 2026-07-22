namespace Retrosharp.Engine.Console.Saga
{
    public class GameEventSagaData : BaseSagaData
    {
        /// <summary>
        /// The game event file path being processed -- the saga's own correlation key (not
        /// RequestId, unlike PersonSagaData/GameLogSagaData), so that a second GameEventStart
        /// for the same file routes to this same instance instead of starting an independent
        /// duplicate import. See spec/game-event.md Requirements 169-170 and
        /// spec/phase-1-build-plan.md Step 6f.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// True from the moment Handle(GameEventStart) begins the import until
        /// Handle(GameEventComplete) marks the saga complete. A duplicate GameEventStart that
        /// finds this true is a file already in progress -- logged and ignored rather than
        /// starting a second concurrent import of the same file.
        /// </summary>
        public bool IsRunning { get; set; }
    }
}
