using Microsoft.Extensions.Logging;

namespace Erm.Messaging.TypedMessageHandler;

public static partial class LogMessages
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "No MessageHandler found.")]
    public static partial void NoMessageHandlerFound(this ILogger logger);
}