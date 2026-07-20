namespace Retrosharp.Service.Interface
{
    /// <summary>
    /// Parses a Retrosheet play-by-play event file (.EVN/.EVA) and populates GameEvent,
    /// GameEventRunner, and GameEventFieldingCredit for every game in it. See
    /// spec/game-event.md and spec/phase-1-build-plan.md Step 6b.
    /// </summary>
    public interface IGameEventImportService
    {
        Task<GameEventImportResult> ImportAsync(string filePath);
    }
}
