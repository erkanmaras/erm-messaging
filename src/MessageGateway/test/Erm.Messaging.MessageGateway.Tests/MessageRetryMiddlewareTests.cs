using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Erm.Messaging.MessageGateway.Tests;

public class MessageRetryMiddlewareTests
{
    [Fact]
    public async Task WhenNextMiddlewareThrowError_MessageShouldRetry()
    {
        const byte retryCount = 2;
        var retryConfiguration = new MessageRetryConfiguration(retryCount, 1);
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message);
        var middleware = new MessageRetryMiddleware(retryConfiguration, Mock.Of<ILogger<MessageRetryMiddleware>>());
        var nextCounter = 0;
        await middleware.Invoking(async retryMiddleware => await retryMiddleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) =>
        {
            nextCounter++;
            throw new InvalidOperationException("Lets retry!");
        })).Should().ThrowAsync<AggregateException>();

        nextCounter.Should().Be(retryCount + 1);
    }


    [Fact]
    public async Task WhenPolicyReturnFalse_MessageShouldNotRetry()
    {
        var retryConfiguration = new MessageRetryConfiguration(3, 1, (_, _, _) => false);
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message);
        var middleware = new MessageRetryMiddleware(retryConfiguration, Mock.Of<ILogger<MessageRetryMiddleware>>());
        var nextCounter = 0;
        await middleware.Invoking(async retryMiddleware => await retryMiddleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) =>
        {
            nextCounter++;
            throw new InvalidOperationException("Lets retry!");
        })).Should().ThrowAsync<InvalidOperationException>();

        nextCounter.Should().Be(1);
    }

    [Fact]
    public async Task WhenRetryCountIsZero_MessageShouldNotRetry()
    {
        var retryConfiguration = new MessageRetryConfiguration(0, 1);
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message);
        var middleware = new MessageRetryMiddleware(retryConfiguration, Mock.Of<ILogger<MessageRetryMiddleware>>());
        var nextCounter = 0;
        await middleware.Invoking(async retryMiddleware => await retryMiddleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) =>
        {
            nextCounter++;
            throw new InvalidOperationException("Lets retry!");
        })).Should().ThrowAsync<InvalidOperationException>();

        nextCounter.Should().Be(1);
    }

    [Fact]
    public async Task WhenRetryCountIsOne_MessageShouldRetryOnce()
    {
        const byte retryCount = 1;
        var retryConfiguration = new MessageRetryConfiguration(retryCount, 1);
        var message = new DoSomething();
        var envelope = new Envelope<DoSomething>(message);
        var middleware = new MessageRetryMiddleware(retryConfiguration, Mock.Of<ILogger<MessageRetryMiddleware>>());
        var nextCounter = 0;
        await middleware.Invoking(async retryMiddleware => await retryMiddleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>()), envelope, (_, _) =>
        {
            nextCounter++;
            throw new InvalidOperationException("Lets retry!");
        })).Should().ThrowAsync<InvalidOperationException>();

        nextCounter.Should().Be(retryCount + 1);
    }


    private class DoSomething
    {
    }
}