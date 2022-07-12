using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Erm.Core;
using Erm.Messaging.Pipeline;
using Erm.Messaging.Pipeline.Middleware;
using Xunit;
using ReceiveMiddleware = Erm.Messaging.Pipeline.IMessagePipelineMiddleware<Erm.Messaging.IReceiveContext>;
using SendMiddleware = Erm.Messaging.Pipeline.IMessagePipelineMiddleware<Erm.Messaging.ISendContext>;

namespace Erm.Messaging.Tests;

public class MessagePipelineTests
{
    [Fact]
    public async Task Middlewares_ShouldExecute_CorrectOrder()
    {
        List<ReceiveMiddleware> executedMiddlewares = new();
        List<ReceiveMiddleware> middlewares = new()
        {
            new Middleware1(executedMiddlewares),
            new Middleware2(executedMiddlewares),
            new Middleware3(executedMiddlewares)
        };

        MessagePipeline<IReceiveContext> pipeline = new(middlewares);

        var context = new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>());
        var envelope = new Envelope<SomethingHappened>(new SomethingHappened(Uuid.Next()));

        await pipeline.Invoke(context, envelope);
        executedMiddlewares.Count.Should().Be(middlewares.Count);
        executedMiddlewares.Should().ContainInOrder(middlewares);
    }

    [Fact]
    public async Task MessageHandlerLamdaMiddlewares_ShouldExecute_CorrectOrder()
    {
        List<int> executedMiddlewares = new();
        List<ReceiveMiddleware> middlewares = new()
        {
            new LamdaMiddleware<IReceiveContext>(async (handlerContext, envelope, next) =>
            {
                executedMiddlewares.Add(1);
                await next(handlerContext, envelope);
            }),
            new LamdaMiddleware<IReceiveContext>(async (handlerContext, envelope, next) =>
            {
                executedMiddlewares.Add(2);
                await next(handlerContext, envelope);
            }),
            new LamdaMiddleware<IReceiveContext>(async (handlerContext, envelope, next) =>
            {
                executedMiddlewares.Add(3);
                await next(handlerContext, envelope);
            })
        };

        MessagePipeline<IReceiveContext> pipeline = new(middlewares);

        var context = new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>());
        var envelope = new Envelope<SomethingHappened>(new SomethingHappened(Uuid.Next()));

        await pipeline.Invoke(context, envelope);
        executedMiddlewares.Count.Should().Be(middlewares.Count);
        executedMiddlewares.Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public async Task SendLamdaMiddlewares_ShouldExecute_CorrectOrder()
    {
        List<int> executedMiddlewares = new();
        List<SendMiddleware> middlewares = new()
        {
            new LamdaMiddleware<ISendContext>(async (sendContext, envelope, next) =>
            {
                executedMiddlewares.Add(1);
                await next(sendContext, envelope);
            }),
            new LamdaMiddleware<ISendContext>(async (sendContext, envelope, next) =>
            {
                executedMiddlewares.Add(2);
                await next(sendContext, envelope);
            }),
            new LamdaMiddleware<ISendContext>(async (sendContext, envelope, next) =>
            {
                executedMiddlewares.Add(3);
                await next(sendContext, envelope);
            })
        };

        MessagePipeline<ISendContext> pipeline = new(middlewares);

        var context = new SendContext(Mock.Of<IServiceProvider>());
        var envelope = new Envelope<SomethingHappened>(new SomethingHappened(Uuid.Next()));

        await pipeline.Invoke(context, envelope);
        executedMiddlewares.Count.Should().Be(middlewares.Count);
        executedMiddlewares.Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public async Task Pipeline_ShouldNotShallow_MiddlewareException()
    {
        List<ReceiveMiddleware> executedMiddlewares = new();
        List<ReceiveMiddleware> middlewares = new()
        {
            new Middleware1(executedMiddlewares),
            new ThrowExMiddleware(executedMiddlewares),
            new Middleware3(executedMiddlewares)
        };

        MessagePipeline<IReceiveContext> pipeline = new(middlewares);
        var context = new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), Mock.Of<IServiceProvider>());
        var envelope = new Envelope<SomethingHappened>(new SomethingHappened(Uuid.Next()));
        await pipeline.Invoking(async r => await r.Invoke(context, envelope)).Should().ThrowAsync<Exception>();
        executedMiddlewares.Should().HaveCount(middlewares.Count - 1);
        executedMiddlewares.Should().BeEquivalentTo(new List<ReceiveMiddleware> { middlewares[0], middlewares[1] });
    }
}

public class Middleware1 : ReceiveMiddleware
{
    private readonly List<ReceiveMiddleware> _executedMiddlewares;

    public Middleware1(List<ReceiveMiddleware> executedMiddlewares)
    {
        _executedMiddlewares = executedMiddlewares;
    }

    public Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
    {
        _executedMiddlewares.Add(this);
        return next(context, envelope);
    }
}

public class Middleware2 : ReceiveMiddleware
{
    private readonly List<ReceiveMiddleware> _executedMiddlewares;

    public Middleware2(List<ReceiveMiddleware> executedMiddlewares)
    {
        _executedMiddlewares = executedMiddlewares;
    }

    public Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
    {
        _executedMiddlewares.Add(this);
        return next(context, envelope);
    }
}

public class Middleware3 : ReceiveMiddleware
{
    private readonly List<ReceiveMiddleware> _executedMiddlewares;

    public Middleware3(List<ReceiveMiddleware> executedMiddlewares)
    {
        _executedMiddlewares = executedMiddlewares;
    }

    public Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
    {
        _executedMiddlewares.Add(this);
        return next(context, envelope);
    }
}

public class ThrowExMiddleware : ReceiveMiddleware
{
    private readonly List<ReceiveMiddleware> _executedMiddlewares;

    public ThrowExMiddleware(List<ReceiveMiddleware> executedMiddlewares)
    {
        _executedMiddlewares = executedMiddlewares;
    }

    public Task Invoke(IReceiveContext context, IEnvelope envelope, NextDelegate<IReceiveContext> next)
    {
        _executedMiddlewares.Add(this);
        throw new Exception("Exception from ThrowExMiddleware!");
    }
}

public class SomethingHappened
{
    public SomethingHappened(Guid id)
    {
        Id = id;
    }

    public string Version => "2";
    public Guid Id { get; }
}