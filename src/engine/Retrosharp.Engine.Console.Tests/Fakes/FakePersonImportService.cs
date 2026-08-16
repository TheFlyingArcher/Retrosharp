using Retrosharp.Service.Interface;

namespace Retrosharp.Engine.Console.Tests.Fakes
{
    public class FakePersonImportService : IPersonImportService
    {
        public PersonImportResult? ResultToReturn { get; set; }
        public Exception? ExceptionToThrow { get; set; }
        public bool WasCalled { get; private set; }
        public string? LastFilePath { get; private set; }

        public Task<PersonImportResult> ImportAsync(string filePath)
        {
            WasCalled = true;
            LastFilePath = filePath;

            if (ExceptionToThrow is not null)
                throw ExceptionToThrow;

            return Task.FromResult(ResultToReturn ?? new PersonImportResult());
        }
    }
}
