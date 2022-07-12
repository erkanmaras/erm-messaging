using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Erm.Core;
using Erm.Messaging.Pipeline;
using Erm.Messaging.Serialization;
using Erm.Messaging.Serialization.Json;
using Xunit;
using HandlerMiddleware = Erm.Messaging.Pipeline.IMessagePipelineMiddleware<Erm.Messaging.IReceiveContext>;

namespace Erm.Messaging.Tests;

public class MessageHandlerTransportTests
{
    [Fact]
    public async Task Receive_ShouldExecutePipeline()
    {
        var serviceCollection = new ServiceCollection();
        var metadataProvider = new MetadataProvider();

        List<IEnvelope> handledEnvelopes = new();
        List<HandlerMiddleware> middlewares = new()
        {
            new Middleware(handledEnvelopes)
        };

        var serializer = new JsonMessageSerializer();
        var sender = new Mock<IMessageSender>().Object;
        var serializerFactory = new Mock<IMessageSerializerFactory>();
        serializerFactory.Setup(factory => factory.GetSerializer(It.IsAny<string>())).Returns(serializer);
        MessagePipeline<IReceiveContext> pipeline = new(middlewares);
        MessagePipelineExecutor<IReceiveContext> pipelineExecutor = new(pipeline);
        var transport = new ReceiveEndpoint(
            pipelineExecutor,
            metadataProvider,
            sender,
            serializerFactory.Object,
            serviceCollection.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());

        var message = new Message
        {
            Data = Uuid.Next()
        };

        metadataProvider.AddEventType<Message>();
        var messageBytes = await serializer.Serialize(message);
        var envelope = new MessageEnvelope(Uuid.Next(), message.GetType().FullName!, MessageContentTypes.Json, messageBytes);
        await transport.Receive(envelope);
        handledEnvelopes.Should().HaveCount(1);
        var handledEnvelope = await handledEnvelopes.First().ToEncoded(serializer);
        handledEnvelope.Should().BeEquivalentTo(envelope);
    }

    [SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Local")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    private class Message
    {
        public Guid Data { get; set; }
    }

    private class Middleware : HandlerMiddleware
    {
        private readonly List<IEnvelope> _handledEnvelopes;

        public Middleware(List<IEnvelope> handledEnvelopes)
        {
            _handledEnvelopes = handledEnvelopes;
        }

        public Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
        {
            _handledEnvelopes.Add(envelope);
            return next(context, envelope);
        }
    }
}