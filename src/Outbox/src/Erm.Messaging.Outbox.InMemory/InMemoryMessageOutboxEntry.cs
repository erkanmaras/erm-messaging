using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Erm.MessageOutbox;
using Erm.Messaging;

namespace Erm.Messaging.Outbox.InMemory;

[PublicAPI]
public class InMemoryMessageOutboxEntry : IMessageOutboxEntry
{
    public InMemoryMessageOutboxEntry(Guid id, IMessageEnvelope envelope)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException($"{nameof(id)} is empty!");
        }

        ArgumentNullException.ThrowIfNull(envelope);

        Id = id;
        MessageId = envelope.MessageId;
        GroupId = envelope.GroupId;
        CorrelationId = envelope.CorrelationId;
        Destination = envelope.Destination;
        Time = envelope.Time;
        TimeToLive = envelope.TimeToLive;
        Source = envelope.Source;
        ExtendedProperties = envelope.ExtendedProperties;
        MessageName = envelope.MessageName;
        MessageContentType = envelope.MessageContentType;
        Message = envelope.Message;
    }

    public Guid Id { get; }
    public Guid MessageId { get; }
    public string? GroupId { get; }
    public Guid? CorrelationId { get; }
    public string? Destination { get; }
    public DateTimeOffset? Time { get; }
    public int? TimeToLive { get; }
    public Guid? RequestId { get; }
    public string? Source { get; }
    public string? ReplyTo { get; }
    public Dictionary<string, string>? ExtendedProperties { get; }
    public string MessageName { get; }
    public string MessageContentType { get; }
    public byte[] Message { get; }
    public DateTimeOffset? CreatedAt { get; internal set; }
}