using NServiceBus;

using Retrosharp.Configuration;
using Retrosharp.Engine.Console.Saga;
using Retrosharp.Format.PlayByPlay;

namespace Retrosharp.Engine.Console.Tests
{
    public class EngineRecoverabilityPolicyTests
    {
        private const string ErrorQueue = "Retrosharp.Engine.Errors";

        // Defaults: ImmediateRetries = 3, DelayedRetries = 5, InitialRetryDelaySeconds = 2.
        private static readonly MessagingConfiguration Config = new();

        public static IEnumerable<object[]> UnrecoverableExceptions() => new[]
        {
            new object[] { new FileNotFoundException("bad path") },
            new object[] { new DirectoryNotFoundException("bad dir") },
            new object[] { new InvalidOperationException("no matching franchise for this input") },
            new object[] { new PlayCodeParseException("2", "no trajectory modifier") },
        };

        [Theory]
        [MemberData(nameof(UnrecoverableExceptions))]
        public void Decide_UnrecoverableException_MovesToErrorOnFirstFailureWithNoRetries(Exception exception)
        {
            var action = EngineRecoverabilityPolicy.Decide(
                Config, ErrorQueue, exception, immediateProcessingFailures: 0, delayedDeliveriesPerformed: 0);

            var moveToError = Assert.IsType<MoveToError>(action);
            Assert.Equal(ErrorQueue, moveToError.ErrorQueue);
        }

        [Fact]
        public void Decide_UnrecoverableException_TakesPriorityOverTheRetryLadder()
        {
            // Even if it had somehow accrued retries, an unrecoverable exception still goes to
            // the error queue -- the classifier check is first.
            var action = EngineRecoverabilityPolicy.Decide(
                Config, ErrorQueue, new FileNotFoundException("bad path"),
                immediateProcessingFailures: 3, delayedDeliveriesPerformed: 5);

            Assert.IsType<MoveToError>(action);
        }

        [Fact]
        public void Decide_TransientException_WithinImmediateBudget_ImmediateRetry()
        {
            var action = EngineRecoverabilityPolicy.Decide(
                Config, ErrorQueue, new TimeoutException("connection reset"),
                immediateProcessingFailures: 0, delayedDeliveriesPerformed: 0);

            Assert.IsType<ImmediateRetry>(action);
        }

        [Fact]
        public void Decide_TransientException_ImmediateExhausted_DelayedRetryWithExponentialBackoffPlusJitter()
        {
            var action = EngineRecoverabilityPolicy.Decide(
                Config, ErrorQueue, new TimeoutException("connection reset"),
                immediateProcessingFailures: 3, delayedDeliveriesPerformed: 0);

            var delayed = Assert.IsType<DelayedRetry>(action);
            // First delayed attempt: base = 2s * 2^0 = 2s, plus up to 20% jitter (<= 400ms).
            Assert.InRange(delayed.Delay, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(2400));
        }

        [Fact]
        public void Decide_TransientException_DelayGrowsExponentiallyPerDelayedAttempt()
        {
            var second = Assert.IsType<DelayedRetry>(EngineRecoverabilityPolicy.Decide(
                Config, ErrorQueue, new TimeoutException("x"), 3, delayedDeliveriesPerformed: 1));
            var third = Assert.IsType<DelayedRetry>(EngineRecoverabilityPolicy.Decide(
                Config, ErrorQueue, new TimeoutException("x"), 3, delayedDeliveriesPerformed: 2));

            // attempt 2: base 2s * 2^1 = 4s (<= 4.8s); attempt 3: base 2s * 2^2 = 8s (<= 9.6s).
            Assert.InRange(second.Delay, TimeSpan.FromSeconds(4), TimeSpan.FromMilliseconds(4800));
            Assert.InRange(third.Delay, TimeSpan.FromSeconds(8), TimeSpan.FromMilliseconds(9600));
        }

        [Fact]
        public void Decide_TransientException_AllRetriesExhausted_MovesToError()
        {
            var action = EngineRecoverabilityPolicy.Decide(
                Config, ErrorQueue, new TimeoutException("connection reset"),
                immediateProcessingFailures: 3, delayedDeliveriesPerformed: 5);

            var moveToError = Assert.IsType<MoveToError>(action);
            Assert.Equal(ErrorQueue, moveToError.ErrorQueue);
        }
    }
}
