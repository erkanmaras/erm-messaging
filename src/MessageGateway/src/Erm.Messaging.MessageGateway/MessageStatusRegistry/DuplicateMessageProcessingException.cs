using System;

namespace Erm.Messaging.MessageGateway;

public class DuplicateMessageProcessingException : Exception
{
    public DuplicateMessageProcessingException(Guid messageId, Exception? innerException = null)
        : base($"Message[{messageId}] already processing or processed!", innerException)
    {
    }
}