using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Erm.MessageOutbox;
using Erm.Messaging;

namespace Erm.Messaging.Outbox.MySql;

[PublicAPI]
public class MySqlMessageOutboxEntry : IMessageOutboxEntry
{
    public MySqlMessageOutboxEntry(Guid id, IMessageEnvelope envelope)
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
        ReplyTo = envelope.ReplyTo;
        ExtendedProperties = envelope.ExtendedProperties.Count == 0 ? null : envelope.ExtendedProperties;
        MessageName = envelope.MessageName;
        MessageContentType = envelope.MessageContentType;
        Message = envelope.Message;
    }

    internal MySqlMessageOutboxEntry(
        Guid id,
        Guid messageId,
        string? groupId,
        Guid? correlationId,
        string? destination,
        DateTimeOffset? time,
        int? timeToLive,
        Guid? requestId,
        string? source,
        string? replyTo,
        Dictionary<string, string>? extendedProperties,
        string messageName,
        string messageContentType,
        byte[] message,
        DateTimeOffset? createdAt)
    {
        Id = id;
        MessageId = messageId;
        GroupId = groupId;
        CorrelationId = correlationId;
        Destination = destination;
        Time = time;
        TimeToLive = timeToLive;
        RequestId = requestId;
        Source = source;
        ReplyTo = replyTo;
        ExtendedProperties = extendedProperties;
        MessageName = messageName;
        MessageContentType = messageContentType;
        Message = message;
        CreatedAt = createdAt;
    }

    public Guid Id { get; internal init; }
    public Guid MessageId { get; internal init; }
    public string? GroupId { get; internal init; }
    public Guid? CorrelationId { get; internal init; }
    public string? Destination { get; internal init; }
    public DateTimeOffset? Time { get; internal init; }
    public int? TimeToLive { get; internal init; }
    public Guid? RequestId { get; internal init; }
    public string? Source { get; internal init; }
    public string? ReplyTo { get; internal init; }
    public Dictionary<string, string>? ExtendedProperties { get; internal init; }
    public string MessageName { get; internal init; }
    public string MessageContentType { get; internal init; }
    public byte[] Message { get; internal init; }
    public DateTimeOffset? CreatedAt { get; internal init; }
}