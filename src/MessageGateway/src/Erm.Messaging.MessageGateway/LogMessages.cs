using System;
using Microsoft.Extensions.Logging;

namespace Erm.Messaging.MessageGateway;

public static partial class LogMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Message {MessageStatus}.")]
    public static partial void MessageStatusChanged(this ILogger logger, MessageStatus messageStatus);

    [LoggerMessage(EventId = 2, Level = LogLevel.Critical, Message = "MarkAsSucceed failed!")]
    public static partial void MarkAsSucceedFailed(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Error occurred retrying, attempt {Attempt}")]
    public static partial void ErrorOccurredRetrying(this ILogger logger, Exception exception, int attempt);
}