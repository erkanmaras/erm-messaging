using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Erm.Core;
using Erm.Messaging.Serialization.Json;
using Erm.Messaging.KafkaTransport;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Erm.Messaging.KafkaTransport.Tests;

public class KafkaMessageTests
{
    [Fact]
    public async Task FromEnvelope_ToEnvelope()
    {
        var envelope = await CreateValidEnvelope();
        var kafkaMessage = KafkaMessage.FromEnvelope(envelope);
        kafkaMessage.Topic.Should().Be(envelope.Destination);
        Encoding.UTF8.GetString(kafkaMessage.MessageKey).Should().Be(envelope.GroupId);
        Encoding.UTF8.GetString(kafkaMessage.MessageValue).Should().Be(Encoding.UTF8.GetString(envelope.Message));

        var deserializedEnvelope = kafkaMessage.ToEnvelope();
        deserializedEnvelope.Should().BeEquivalentTo(envelope);
    }

    [Fact]
    public async Task EmptyDestination_ShouldThrow_Exception()
    {
        var envelope = await CreateValidEnvelope();
        envelope.Destination = string.Empty;
        Action act = () => KafkaMessage.FromEnvelope(envelope);
        act.Should().Throw<ArgumentException>();
    }


    private static async Task<MessageEnvelope> CreateValidEnvelope()
    {
        var properties = new EnvelopeProperties { { "Key1", "value1" }, { "key2", "value2" } };
        var message = new Message
        {
            Data = Uuid.Next()
        };

        var serializer = new JsonMessageSerializer();
        var serializedMessage = await serializer.Serialize(message);

        return new MessageEnvelope(Uuid.Next(), message.GetType().FullName!, MessageContentTypes.Json, serializedMessage, properties)
        {
            Destination = "destination",
            RequestId = Uuid.Next(),
            Time = new DateTimeOffset(1984, 11, 11, 1, 1, 1, 1, TimeSpan.Zero),
            CorrelationId = Uuid.Next(),
            ReplyTo = "reply-here",
            GroupId = "group",
            Source = "source",
            TimeToLive = 360
        };
    }

    private class Message
    {
        public Guid Data { get; set; }
    }
}