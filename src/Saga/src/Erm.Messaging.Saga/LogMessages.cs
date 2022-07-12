using Microsoft.Extensions.Logging;

namespace Erm.Messaging.Saga;

public static partial class LogMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "No saga found.")]
    public static partial void NoSagaFound(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Saga processing.")]
    public static partial void SagaProcessing(this ILogger logger);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "Saga already completed.")]
    public static partial void SagaAlreadyCompleted(this ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Saga rejected.")]
    public static partial void SagaRejected(this ILogger logger);

    [LoggerMessage(EventId = 5, Level = LogLevel.Debug, Message = "Saga completed.")]
    public static partial void SagaCompleted(this ILogger logger);
}