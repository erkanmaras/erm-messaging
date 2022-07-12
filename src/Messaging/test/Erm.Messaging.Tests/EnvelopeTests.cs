using System;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Erm.Core;
using Erm.Messaging.Serialization.Json;
using Erm.Serialization.Json;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Erm.Messaging.Tests;

public class EnvelopeTests
{
    [Fact]
    public async Task EnsureSerializable()
    {
        var message = new Message { Data = Uuid.Next() };
        var properties = new EnvelopeProperties { { "key", "value" } };
        var envelope = new Envelope<Message>(message, properties)
        {
            Destination = "destination",
            Time = new DateTimeOffset(1984, 11, 11, 1, 1, 1, 1, TimeSpan.Zero),
            CorrelationId = Uuid.Next(),
            GroupId = "group",
            Source = "source"
        };

        var serializer = new JsonMessageSerializer();
        var encodedEnvelope = await envelope.ToEncoded(serializer);
        var encodedEnvelopeSerialized = JsonSerializer.Serialize(encodedEnvelope, encodedEnvelope.GetType());
        var encodedEnvelopeDeserialized = JsonSerde.Deserialize<MessageEnvelope>(encodedEnvelopeSerialized)!;
        encodedEnvelopeDeserialized.Should().BeEquivalentTo(encodedEnvelope);
        var typedEnvelope = await encodedEnvelopeDeserialized.ToTyped(typeof(Message), serializer);
        typedEnvelope.Should().BeEquivalentTo(envelope);
    }

    [Fact]
    public void CreateInstance()
    {
        var message = new Message { Data = Uuid.Next() };
        var properties = new EnvelopeProperties { { "key", "value" } };
        var expected = new Envelope<Message>(message, properties);
        var envelope = EnvelopeFactory.CreateInstance(typeof(Message), expected.MessageId, expected.Message, expected.ExtendedProperties);
        envelope.Should().BeEquivalentTo(expected);
    }

    private class Message
    {
        public Guid Data { get; set; }
    }
}