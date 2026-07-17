using NServiceBus;

namespace Retrosharp.Engine.Console.Saga
{
    public abstract class BaseSagaData : ContainSagaData
    {
        /// <summary>
        /// Unique identifier for the saga instance.
        /// This is used to correlate messages and events with the correct saga instance.
        /// </summary>
        public Guid RequestId { get; set; }
    }
}
