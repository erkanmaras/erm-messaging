using System;
using Microsoft.Extensions.Logging;

namespace Erm.Messaging.KafkaTransport;

public static partial class LogMessages
{
    public static IDisposable BeginMessageReceiveScope(this ILogger logger, string messageId, string messageName)
    {
        return logger.BeginScope("Message {MessageName}[{MessageId}]", messageName, messageId);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Message receive started.")]
    public static partial void MessageReceiveStart(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Message received:{Succeeded}, reason:{ReceiveFailReason}.")]
    public static partial void MessageReceiveEnd(this ILogger logger, bool succeeded, string receiveFailReason);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Message receive failed.")]
    public static partial void MessageReceiveFailed(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Message {MessageName} skipped.")]
    public static partial void MessageSkipped(this ILogger logger, string messageName);
}