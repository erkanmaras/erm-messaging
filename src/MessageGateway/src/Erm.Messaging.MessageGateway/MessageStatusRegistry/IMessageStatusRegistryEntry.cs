using System;

namespace Erm.Messaging.MessageGateway;

public interface IMessageStatusRegistryEntry
{
    Guid MessageId { get; }
    MessageStatus MessageStatus { get; }
    DateTimeOffset CreatedAt { get; }
    MessageFaultDetails? FaultDetails { get; }
}