using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Tests.Fakes
{
    /// <summary>
    /// Hand-rolled test double rather than a mocking library, matching the rest of the
    /// codebase's test conventions (see Retrosharp.Service.Tests). Configurable to either
    /// return a canned result or throw, and records whether it was invoked so duplicate-request
    /// suppression can be asserted without inspecting saga internals.
    /// </summary>
    public class FakeGameLogImportService : IGameLogImportService
    {
        public GameLogImportResult? ResultToReturn { get; set; }
        public Exception? ExceptionToThrow { get; set; }
        public bool WasCalled { get; private set; }
        public string? LastFilePath { get; private set; }
        public int? LastSeasonYear { get; private set; }

        public Task<GameLogImportResult> ImportAsync(string filePath, int seasonYear)
        {
            WasCalled = true;
            LastFilePath = filePath;
            LastSeasonYear = seasonYear;

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.FromResult(ResultToReturn ?? new GameLogImportResult());
        }
    }
}
