using System;
using System.Text;
using Erm.KafkaClient;
using Erm.Core;
using Erm.Serialization.Json;

namespace Erm.Messaging.KafkaTransport;

public class KafkaMessage
{
    public KafkaMessage(string topic, byte[] messageKey, byte[] messageValue, IKafkaMessageHeaders headers)
    {
        Topic = topic;
        MessageKey = messageKey;
        MessageValue = messageValue;
        Headers = headers;
    }

    public string Topic { get; }
    public byte[] MessageKey { get; }
    public byte[] MessageValue { get; }
    public IKafkaMessageHeaders Headers { get; }

    public static KafkaMessage FromEnvelope(IMessageEnvelope envelope)
    {
        if (string.IsNullOrEmpty(envelope.Destination))
        {
            throw new ArgumentException($"Message:{envelope.MessageName} destination empty!");
        }

        KafkaMessageHeaders headers = new();
        headers.AddStr(nameof(IMessageEnvelope.MessageId), envelope.MessageId.ToString());
        headers.AddStr(nameof(IMessageEnvelope.MessageName), envelope.MessageName);
        headers.AddStr(nameof(IMessageEnvelope.MessageContentType), envelope.MessageContentType);
        headers.AddStr(nameof(IMessageEnvelope.Time), (envelope.Time ?? SystemClock.UtcNow).ToIsoString());
        headers.AddStr(nameof(IMessageEnvelope.CorrelationId), envelope.CorrelationId.ToString());

        if (envelope.RequestId.HasValue)
        {
            headers.AddStr(nameof(IMessageEnvelope.RequestId), envelope.RequestId.ToString());
        }

        if (envelope.ReplyTo != null)
        {
            headers.AddStr(nameof(IMessageEnvelope.ReplyTo), envelope.ReplyTo);
        }

        if (envelope.TimeToLive.HasValue)
        {
            headers.AddStr(nameof(IMessageEnvelope.TimeToLive), envelope.TimeToLive.Value.ToString());
        }

        if (envelope.GroupId != null)
        {
            headers.AddStr(nameof(IMessageEnvelope.GroupId), envelope.GroupId);
        }

        if (envelope.Source != null)
        {
            headers.AddStr(nameof(IMessageEnvelope.Source), envelope.Source);
        }

        if (envelope.ExtendedProperties.Count > 0)
        {
            headers.AddStr(nameof(IMessageEnvelope.ExtendedProperties), JsonSerde.Serialize(envelope.ExtendedProperties));
        }

        // TODO : Check is there any differance between null and empty array !
        var messageKey = !string.IsNullOrWhiteSpace(envelope.GroupId) ? Encoding.UTF8.GetBytes(envelope.GroupId) : Array.Empty<byte>();
        var message = envelope.Message;

        return new KafkaMessage(envelope.Destination, messageKey, message, headers);
    }

    public IMessageEnvelope ToEnvelope()
    {
        var messageId = Headers.GetGuid(nameof(IMessageEnvelope.MessageId))!;
        if (!messageId.HasValue)
        {
            throw new InvalidOperationException($"MessageId can't be empty!");
        }

        var messageType = Headers.GetStr(nameof(IMessageEnvelope.MessageName));
        if (string.IsNullOrEmpty(messageType))
        {
            throw new InvalidOperationException("MessageName can't be empty!");
        }

        var messageContentType = Headers.GetStr(nameof(IMessageEnvelope.MessageContentType));
        if (string.IsNullOrEmpty(messageContentType))
        {
            throw new InvalidOperationException("MessageContentType can't be empty!");
        }

        var envelope = new MessageEnvelope(
            messageId.Value,
            messageType,
            messageContentType,
            MessageValue,
            Headers.GetExtendedProperties(nameof(IMessageEnvelope.ExtendedProperties)))
        {
            Destination = Topic,
            Time = Headers.GetDateTimeOffset(nameof(IMessageEnvelope.Time)),
            CorrelationId = Headers.GetGuid(nameof(IMessageEnvelope.CorrelationId)),
            ReplyTo = Headers.GetStr(nameof(IMessageEnvelope.ReplyTo)),
            RequestId = Headers.GetGuid(nameof(IMessageEnvelope.RequestId)),
            TimeToLive = Headers.GetInt(nameof(IMessageEnvelope.TimeToLive)),
            GroupId = Headers.GetStr(nameof(IMessageEnvelope.GroupId)),
            Source = Headers.GetStr(nameof(IMessageEnvelope.Source))
        };

        return envelope;
    }
}