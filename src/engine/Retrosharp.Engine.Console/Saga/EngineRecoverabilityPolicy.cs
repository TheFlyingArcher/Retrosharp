using NServiceBus;

using Retrosharp.Configuration;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// The endpoint's single recoverability decision point (wired up in <c>Program.cs</c> via
    /// <c>Recoverability().CustomPolicy(...)</c>). Two responsibilities:
    ///
    /// 1. <b>Unrecoverable failures fail fast, but visibly.</b> An exception that retrying can
    ///    never fix (see <see cref="ImportFailureClassifier"/> -- a missing file, a deterministic
    ///    "no matching franchise/game/person" resolution failure, an unparseable Retrosheet play
    ///    code) is routed straight to the error queue on the first failure, with zero retries.
    ///    This used to be handled by a try/catch in every import saga's Start handler that
    ///    logged a warning and called <c>MarkAsComplete()</c> -- which stopped the retry storm
    ///    (spec/defects.md, "Needless Retrying") but also meant a whole file could fail to
    ///    import with nothing on the error queue and a saga that looked like it succeeded. The
    ///    failed message, its full exception, and its headers now land in
    ///    <c>Retrosharp.Engine.Errors</c>, NServiceBus logs the move at error level, and an
    ///    operator can retry the message once the underlying data/parser issue is fixed.
    ///
    /// 2. <b>Transient failures get exponential backoff with jitter.</b> NServiceBus's built-in
    ///    delayed recoverability only supports a linear <c>TimeIncrease</c>, so parser.md's
    ///    "exponential backoff with jitter" requirement is implemented here: immediate retries
    ///    first, then delayed retries with a <c>2^n</c> base delay plus up to 20% jitter, then
    ///    the error queue.
    ///
    /// Kept as a pure function of the values it needs (not <c>ErrorContext</c> directly) so it
    /// is straightforward to unit test -- see <c>EngineRecoverabilityPolicyTests</c>.
    /// </summary>
    internal static class EngineRecoverabilityPolicy
    {
        public static RecoverabilityAction Decide(
            MessagingConfiguration messagingConfig,
            string errorQueueAddress,
            Exception exception,
            int immediateProcessingFailures,
            int delayedDeliveriesPerformed)
        {
            if (ImportFailureClassifier.IsUnrecoverable(exception))
                return RecoverabilityAction.MoveToError(errorQueueAddress);

            if (immediateProcessingFailures < messagingConfig.ImmediateRetries)
                return RecoverabilityAction.ImmediateRetry();

            if (delayedDeliveriesPerformed < messagingConfig.DelayedRetries)
            {
                var attempt = delayedDeliveriesPerformed + 1;
                var baseDelaySeconds = messagingConfig.InitialRetryDelaySeconds * Math.Pow(2, attempt - 1);
                var jitterMilliseconds = Random.Shared.Next(0, (int)(baseDelaySeconds * 1000 * 0.2));
                var delay = TimeSpan.FromSeconds(baseDelaySeconds) + TimeSpan.FromMilliseconds(jitterMilliseconds);

                return RecoverabilityAction.DelayedRetry(delay);
            }

            return RecoverabilityAction.MoveToError(errorQueueAddress);
        }
    }
}
