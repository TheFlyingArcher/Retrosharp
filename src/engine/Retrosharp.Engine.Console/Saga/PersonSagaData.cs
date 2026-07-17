namespace Retrosharp.Engine.Console.Saga
{
    public class PersonSagaData : BaseSagaData
    {
        /// <summary>
        /// The biofile path being processed, kept for logging when the saga completes.
        /// </summary>
        public string FilePath { get; set; }
    }
}
