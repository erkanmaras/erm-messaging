using System;
using System.Threading.Tasks;
using FluentAssertions;
using Erm.Core;
using Erm.Messaging.Serialization.Json;
using Xunit;

namespace Erm.Messaging.Tests;

public class MessageEnvelopeTests
{
    [Fact]
    public async Task CannotCreateEnvelopeWithEmptyMessageId()
    {
        var message = new Message
        {
            Data = Uuid.Next()
        };
        var messageBytes = await new JsonMessageSerializer().Serialize(message);
        FluentActions.Invoking(() => new MessageEnvelope(Guid.Empty, message.GetType().FullName!, MessageContentTypes.Json, messageBytes)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task CannotCreateEnvelopeWithEmptyMessageTypeName()
    {
        var message = new Message
        {
            Data = Uuid.Next()
        };
        var messageBytes = await new JsonMessageSerializer().Serialize(message);
        FluentActions.Invoking(() => new MessageEnvelope(Uuid.Next(), "", MessageContentTypes.Json, messageBytes)).Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task CannotCreateEnvelopeWithEmptyMessageContentType()
    {
        var message = new Message
        {
            Data = Uuid.Next()
        };
        var messageBytes = await new JsonMessageSerializer().Serialize(message);
        FluentActions.Invoking(() => new MessageEnvelope(Uuid.Next(), message.GetType().FullName!, "", messageBytes)).Should().Throw<ArgumentException>();
    }

    private class Message
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public Guid Data { get; set; }
    }
}