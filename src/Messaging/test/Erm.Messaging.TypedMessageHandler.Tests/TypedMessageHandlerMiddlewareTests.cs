using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Erm.Messaging.TypedMessageHandler.Middleware;
using Xunit;

namespace Erm.Messaging.TypedMessageHandler.Tests;

public class TypedMessageHandlerMiddlewareTests
{
    private readonly IList<IMessageHandler<TypedEvent1>> _executedHandlers;

    public TypedMessageHandlerMiddlewareTests()
    {
        _executedHandlers = new List<IMessageHandler<TypedEvent1>>();
    }

    [Fact]
    public async Task Invoke_ShouldExecute_Handler()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(_ => _executedHandlers);
        var typeMap = new MessageHandlerTypeMap(serviceCollection);

        typeMap.Add(typeof(TypedMessageHandler));
        var message = new TypedEvent1();
        var envelope = new Envelope<TypedEvent1>(message);

        using var scope = serviceCollection.BuildServiceProvider().CreateScope();

        var middleware = new TypedMessageHandlerMiddleware(typeMap, Mock.Of<ILogger<TypedMessageHandlerMiddleware>>());
        await middleware.Invoke(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), scope.ServiceProvider), envelope, (_, _) => Task.CompletedTask);
        _executedHandlers.Should().HaveCount(1);
        _executedHandlers.First().Should().BeOfType<TypedMessageHandler>();
    }

    private class TypedMessageHandler : IMessageHandler<TypedEvent1>, IMessageHandler<TypedEvent2>
    {
        private readonly IList<IMessageHandler<TypedEvent1>> _executedHandlers;

        public TypedMessageHandler(IList<IMessageHandler<TypedEvent1>> executedHandlers)
        {
            _executedHandlers = executedHandlers;
        }

        public Task Handle(IReceiveContext context, IEnvelope<TypedEvent1> envelope)
        {
            _executedHandlers.Add(this);
            return Task.CompletedTask;
        }

        public Task Handle(IReceiveContext context, IEnvelope<TypedEvent2> envelope)
        {
            _executedHandlers.Add(this);
            return Task.CompletedTask;
        }
    }

    private class TypedEvent1
    {
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class TypedEvent2
    {
    }
}