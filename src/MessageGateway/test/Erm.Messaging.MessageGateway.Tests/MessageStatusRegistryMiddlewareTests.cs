using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Erm.Core;
using Erm.Messaging.MessageGateway.InMemory;
using Xunit;

namespace Erm.Messaging.MessageGateway.Tests;

public class MessageStatusRegistryMiddlewareTests
{
    [Fact]
    public async Task WhenNextMiddlewareExecuted_RegistryLastState_MustBeSucceeded()
    {
        var registry = new InMemoryMessageStatusRegistry(new Clock());
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message);
        var middleware = new MessageStatusMiddleware(registry, new Clock(), Mock.Of<ILogger<MessageStatusMiddleware>>());
        await middleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) => Task.CompletedTask);
        var entry = await registry.GetLastEntry(envelope.MessageId);
        entry.Should().NotBeNull();
        entry!.MessageStatus.Should().Be(MessageStatus.Succeeded);
    }

    [Fact]
    public async Task WhenNextMiddlewareThrowEx_RegistryLastState_MustBeFaulted()
    {
        var registry = new InMemoryMessageStatusRegistry(new Clock());
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message);
        var middleware = new MessageStatusMiddleware(registry, new Clock(), Mock.Of<ILogger<MessageStatusMiddleware>>());
        await middleware.Invoking(async m => await m.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) => throw new InvalidOperationException())).Should().ThrowAsync<InvalidOperationException>();
        var entry = await registry.GetLastEntry(envelope.MessageId);
        entry.Should().NotBeNull();
        entry!.MessageStatus.Should().Be(MessageStatus.Faulted);
    }

    [Fact]
    public async Task WhenDuplicateMessageDetected_NextMiddleware_ShouldNotExecute()
    {
        var registry = new InMemoryMessageStatusRegistry(new Clock());
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message);
        var middleware = new MessageStatusMiddleware(registry, new Clock(), Mock.Of<ILogger<MessageStatusMiddleware>>());
        await registry.MarkAsProcessing(envelope.MessageId);
        await middleware.Invoking(async m => await m.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) => throw new InvalidOperationException())).Should().NotThrowAsync();
        var entry = await registry.GetLastEntry(envelope.MessageId);
        entry.Should().NotBeNull();
        entry!.MessageStatus.Should().Be(MessageStatus.Processing);
    }


    [Fact]
    public async Task WhenTTLExpired_NextMiddleware_ShouldNotExecute()
    {
        var registry = new InMemoryMessageStatusRegistry(new Clock());
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message)
        {
            TimeToLive = 1, //seconds;
            Time = SystemClock.UtcNow.AddHours(-1)
        };
        var middleware = new MessageStatusMiddleware(registry, new Clock(), Mock.Of<ILogger<MessageStatusMiddleware>>());
        var nextExecuted = false;
        await middleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) =>
        {
            nextExecuted = true;
            return Task.CompletedTask;
        });
        var entry = await registry.GetLastEntry(envelope.MessageId);
        nextExecuted.Should().BeFalse();
        entry!.MessageStatus.Should().Be(MessageStatus.Faulted);
        entry.FaultDetails?.Type.Should().Be("Skipped");
    }

    [Fact]
    public async Task WhenTTLValid_NextMiddleware_ShouldExecute()
    {
        var registry = new InMemoryMessageStatusRegistry(new Clock());
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message)
        {
            TimeToLive = 120, //seconds;
            Time = SystemClock.UtcNow
        };
        var middleware = new MessageStatusMiddleware(registry, new Clock(), Mock.Of<ILogger<MessageStatusMiddleware>>());
        var nextExecuted = false;
        await middleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) =>
        {
            nextExecuted = true;
            return Task.CompletedTask;
        });
        var entry = await registry.GetLastEntry(envelope.MessageId);
        nextExecuted.Should().BeTrue();
        entry!.MessageStatus.Should().Be(MessageStatus.Succeeded);
    }

    private class DoSomething
    {
    }
}