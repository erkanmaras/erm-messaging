using System;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Erm.Core;

namespace Erm.Messaging;

[PublicAPI]
public class Envelope<TMessage> : IEnvelope<TMessage> where TMessage : class
{
    private EnvelopeProperties? _extendedProperties;

    public Envelope(TMessage message, EnvelopeProperties? extendedProperties = null) : this(Uuid.Next(), message, extendedProperties)
    {
    }

    [JsonConstructor]
    public Envelope(Guid messageId, TMessage message, EnvelopeProperties? extendedProperties = null)
    {
        if (messageId == Guid.Empty)
        {
            throw new ArgumentException("MessageId is null!");
        }

        MessageId = messageId;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        _extendedProperties = extendedProperties;
    }

    public Guid MessageId { get; }
    public TMessage Message { get; }
    public Guid? RequestId { get; set; }
    public string? Destination { get; set; }
    public string? ReplyTo { get; set; }
    public DateTimeOffset? Time { get; set; }

    /// <inheritdoc />
    public int? TimeToLive { get; set; }

    public Guid? CorrelationId { get; set; }
    public string? GroupId { get; set; }
    public string? Source { get; set; }

    public EnvelopeProperties ExtendedProperties
    {
        get
        {
            _extendedProperties ??= new EnvelopeProperties();
            return _extendedProperties;
        }
        protected set => _extendedProperties = value;
    }

    public string MessageName => Message.GetType().FullName!;
    object IEnvelope.Message => Message;
}

public static class EnvelopeFactory
{
    private static readonly Type OpenEnvelopeType;

    static EnvelopeFactory()
    {
        OpenEnvelopeType = typeof(Envelope<>);
    }

    public static IEnvelope CreateInstance(Type messageType, Guid messageId, object message, EnvelopeProperties? properties)
    {
        Type[] typeArgs = { messageType };
        var envelopeType = OpenEnvelopeType.MakeGenericType(typeArgs);
        return (IEnvelope)Activator.CreateInstance(envelopeType, messageId, message, properties)!;
    }
}