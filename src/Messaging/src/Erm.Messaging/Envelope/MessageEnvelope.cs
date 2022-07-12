using System;
using JetBrains.Annotations;

namespace Erm.Messaging;

[PublicAPI]
public class MessageEnvelope : IMessageEnvelope
{
    private Guid? _correlationId;
    private EnvelopeProperties? _extendedProperties;

    public MessageEnvelope(
        Guid messageId,
        string messageName,
        string messageContentType,
        byte[] message,
        EnvelopeProperties? extendedProperties = null)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("MessageId is empty!");
        }

        if (string.IsNullOrWhiteSpace(messageName))
        {
            throw new ArgumentException("MessageName is null or empty!");
        }

        if (string.IsNullOrWhiteSpace(messageContentType))
        {
            throw new ArgumentException("MessageContentType is null or empty!");
        }

        MessageId = messageId;
        MessageName = messageName;
        MessageContentType = messageContentType;
        Message = message;
        _extendedProperties = extendedProperties;
    }

    public Guid MessageId { get; set; }
    public Guid? RequestId { get; set; }
    public string? Destination { get; set; }
    public string? ReplyTo { get; set; }
    public DateTimeOffset? Time { get; set; }

    /// <inheritdoc />
    public int? TimeToLive { get; set; }

    public Guid? CorrelationId
    {
        get => _correlationId ?? MessageId;
        set => _correlationId = value;
    }

    public string? GroupId { get; set; }
    public string? Source { get; set; }
    public string MessageName { get; set; }
    public string MessageContentType { get; set; }
    public byte[] Message { get; set; }

    public EnvelopeProperties ExtendedProperties
    {
        get
        {
            _extendedProperties ??= new EnvelopeProperties();
            return _extendedProperties;
        }
        set => _extendedProperties = value;
    }
}