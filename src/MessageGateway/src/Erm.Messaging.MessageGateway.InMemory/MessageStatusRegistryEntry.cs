using System;
using Erm.Messaging.MessageGateway;

namespace Erm.Messaging.MessageGateway.InMemory;

public class InMemoryMessageStatusRegistryEntry : IMessageStatusRegistryEntry
{
    public InMemoryMessageStatusRegistryEntry(Guid messageId, MessageStatus status, DateTimeOffset createdAt, MessageFaultDetails? faultDetails = default)
    {
        MessageId = messageId;
        MessageStatus = status;
        CreatedAt = createdAt;
        FaultDetails = faultDetails;
    }

    public Guid MessageId { get; }
    public MessageStatus MessageStatus { get; }
    public DateTimeOffset CreatedAt { get; }
    public MessageFaultDetails? FaultDetails { get; }
}