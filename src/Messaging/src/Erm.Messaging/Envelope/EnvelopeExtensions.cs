using System;
using System.Threading.Tasks;
using Erm.Messaging.Serialization;

namespace Erm.Messaging;

public static class EnvelopeExtensions
{
    public static async Task<IMessageEnvelope> ToEncoded(this IEnvelope envelope, IMessageSerializer messageSerializer)
    {
        var message = await messageSerializer.Serialize(envelope.Message);
        var messageName = envelope.MessageName;
        var messageContentType = messageSerializer.ContentType;

        //source generator ile  mapper yaratılabilir.
        MessageEnvelope result = new(envelope.MessageId, messageName, messageContentType, message, CopyProperties(envelope.ExtendedProperties))
        {
            RequestId = envelope.RequestId,
            Destination = envelope.Destination,
            Source = envelope.Source,
            ReplyTo = envelope.ReplyTo,
            CorrelationId = envelope.CorrelationId,
            Time = envelope.Time,
            TimeToLive = envelope.TimeToLive,
            GroupId = envelope.GroupId,
        };

        return result;

        EnvelopeProperties CopyProperties(EnvelopeProperties sourceProperties)
        {
            var targetProperties = new EnvelopeProperties(sourceProperties);
            //ContentType is only meaningful for typed-envelope
            targetProperties.Remove(WellKnownEnvelopeProperties.ContentType);
            return targetProperties;
        }
    }

    public static async Task<IEnvelope> ToTyped(this IMessageEnvelope envelope, Type messageType, IMessageSerializer messageSerializer)
    {
        var message = await messageSerializer.Deserialize(envelope.Message, messageType);
        var typedEnvelope = EnvelopeFactory.CreateInstance(messageType, envelope.MessageId, message, envelope.ExtendedProperties);
        typedEnvelope.Destination = envelope.Destination;
        typedEnvelope.ReplyTo = envelope.ReplyTo;
        typedEnvelope.Source = envelope.Source;
        typedEnvelope.CorrelationId = envelope.CorrelationId;
        typedEnvelope.RequestId = envelope.RequestId;
        typedEnvelope.Time = envelope.Time;
        typedEnvelope.TimeToLive = envelope.TimeToLive;
        typedEnvelope.GroupId = envelope.GroupId;
        return typedEnvelope;
    }

    public static bool IsExpired(this IEnvelope envelope, DateTimeOffset currentTime)
    {
        return TimeToLiveExpired(envelope, currentTime);
    }

    public static bool IsExpired(this IMessageEnvelope envelope, DateTimeOffset currentTime)
    {
        return TimeToLiveExpired(envelope, currentTime);
    }

    private static bool TimeToLiveExpired(this IEnvelopeHeader envelopeHeader, DateTimeOffset currentTime)
    {
        if (envelopeHeader.TimeToLive.GetValueOrDefault() > 0)
        {
            return envelopeHeader.Time?.AddSeconds(envelopeHeader.TimeToLive!.Value) < currentTime;
        }

        return false;
    }
}