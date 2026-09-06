using System.Text.Json;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NServiceBus;
using NServiceBus.Faults;

using Retrosharp.Message.GameEvent;

namespace Retrosharp.Engine.Console.Saga
{
    /// <summary>
    /// Bridges "a <see cref="GameEventStart"/> failed and was moved to the error queue" to
    /// "<see cref="BulkGameEventImportSaga"/> is told that file failed". <see cref="GameEventSaga"/>
    /// deliberately does not report its own failures (see spec/defects.md, "Needless
    /// Retrying"), so for a file that belongs to a bulk run this -- wired into the endpoint's
    /// <c>Recoverability().Failed().OnMessageSentToErrorQueue(...)</c> -- sends the saga a
    /// <see cref="GameEventImportFailed"/> once retries are exhausted. A standalone single-file
    /// import (<c>BulkImportId == Guid.Empty</c>) is left alone. The failed message still lands
    /// on the error queue with its full exception for an operator to retry later. See
    /// spec/bulk-import.md, "A failed file must not stall the batch".
    /// </summary>
    internal static class BulkImportFailureNotifier
    {
        private const int MaxErrorLength = 2000;

        public static async Task OnMessageSentToErrorQueueAsync(
            IServiceProvider? runtimeServices,
            FailedMessage failedMessage,
            CancellationToken cancellationToken)
        {
            // Set once the host is built; this callback only runs at message-processing time,
            // long after startup, so it is always populated by then.
            if (runtimeServices is null)
                return;

            failedMessage.Headers.TryGetValue(Headers.EnclosedMessageTypes, out var enclosedMessageTypes);
            if (!IsGameEventStart(enclosedMessageTypes))
                return;

            if (!TryReadChildFields(failedMessage.Body, out var bulkImportId, out var filePath) || bulkImportId == Guid.Empty)
                return;

            var session = runtimeServices.GetRequiredService<IMessageSession>();
            var error = failedMessage.Exception is { } ex
                ? Truncate($"{ex.GetType().Name}: {ex.Message}", MaxErrorLength)
                : "The import failed and was moved to the error queue.";

            // SendLocal: GameEventImportFailed is handled by BulkGameEventImportSaga in this
            // same endpoint, so no routing configuration is needed.
            await session.SendLocal(new GameEventImportFailed
            {
                RequestId = Guid.NewGuid(),
                BulkImportId = bulkImportId,
                FileName = Path.GetFileName(filePath),
                Error = error
            }, cancellationToken);

            runtimeServices.GetService<ILoggerFactory>()?.CreateLogger(typeof(BulkImportFailureNotifier))
                .LogWarning(
                    "Notified bulk import {BulkImportId} that '{FileName}' failed and was moved to the error queue.",
                    bulkImportId, Path.GetFileName(filePath));
        }

        /// <summary>
        /// True when the <c>NServiceBus.EnclosedMessageTypes</c> header names
        /// <see cref="GameEventStart"/> as (one of) the message type(s) -- matched exactly
        /// against each entry's type name, so it never matches <see cref="GameEventImportFailed"/>
        /// and cannot feed itself.
        /// </summary>
        internal static bool IsGameEventStart(string? enclosedMessageTypes)
        {
            if (string.IsNullOrEmpty(enclosedMessageTypes))
                return false;

            var expected = typeof(GameEventStart).FullName;
            return enclosedMessageTypes
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(t => t.Split(',', 2)[0].Trim())
                .Any(t => t == expected);
        }

        /// <summary>
        /// Pulls <c>BulkImportId</c> and <c>FilePath</c> out of a serialized
        /// <see cref="GameEventStart"/> body. Returns false only if the body is not parseable
        /// JSON; a body missing either field yields <see cref="Guid.Empty"/> / <c>""</c>.
        /// </summary>
        internal static bool TryReadChildFields(ReadOnlyMemory<byte> body, out Guid bulkImportId, out string filePath)
        {
            bulkImportId = Guid.Empty;
            filePath = string.Empty;

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.TryGetProperty("BulkImportId", out var idElement) && idElement.TryGetGuid(out var id))
                    bulkImportId = id;

                if (root.TryGetProperty("FilePath", out var pathElement))
                    filePath = pathElement.GetString() ?? string.Empty;

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength];
    }
}
