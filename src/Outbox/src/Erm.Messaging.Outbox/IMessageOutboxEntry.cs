using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Erm.MessageOutbox;

[PublicAPI]
public interface IMessageOutboxEntry
{
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
    public DateTimeOffset? CreatedAt { get; }
}