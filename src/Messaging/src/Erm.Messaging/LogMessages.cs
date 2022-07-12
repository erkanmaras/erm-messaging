using System;
using Microsoft.Extensions.Logging;

namespace Erm.Messaging;

public static partial class LogMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Trace, Message = "Message send start.")]
    public static partial void MessageSendStart(this ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Trace, Message = "Message send end:{Succeeded}.")]
    public static partial void MessageSendEnd(this ILogger logger, bool succeeded);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Message send failed.")]
    public static partial void MessageSendFailed(this ILogger logger, Exception exception);
}