using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Erm.Messaging.TypedMessageHandler;
using Xunit;

// ReSharper disable All
namespace Erm.Messaging.Tests;

public class MessageHandlerTypeMapTests
{
    [Fact]
    public void AddFromAssemblies()
    {
        var serviceCollection = new ServiceCollection();
        var typeMap = new MessageHandlerTypeMap(serviceCollection)
        {
            HandlerType = typeof(IMessageHandlerTest<>)
        };

        typeMap.AddFromAssemblies(new[] { GetType().Assembly });

        typeMap.GetMessageHandlerTypes(typeof(Event1)).Should().HaveCount(1);
        typeMap.GetMessageHandlerTypes(typeof(Event1)).First().UnderlyingSystemType.Should().Be(typeof(EventHandler));

        typeMap.GetMessageHandlerTypes(typeof(Event2)).Should().HaveCount(1);
        typeMap.GetMessageHandlerTypes(typeof(Event2)).First().UnderlyingSystemType.Should().Be(typeof(EventHandler));

        typeMap.GetMessageHandlerTypes(typeof(Command1)).Should().HaveCount(1);
        typeMap.GetMessageHandlerTypes(typeof(Command1)).First().UnderlyingSystemType.Should().Be(typeof(CommandHandler1));

        typeMap.GetMessageHandlerTypes(typeof(Command2)).Should().HaveCount(1);
        typeMap.GetMessageHandlerTypes(typeof(Command2)).First().UnderlyingSystemType.Should().Be(typeof(CommandHandler2));

        serviceCollection.Should().Contain(s => s.ServiceType == typeof(EventHandler) &&
                                                s.Lifetime == ServiceLifetime.Transient);

        serviceCollection.Should().Contain(s => s.ServiceType == typeof(CommandHandler1) &&
                                                s.Lifetime == ServiceLifetime.Transient);

        serviceCollection.Should().Contain(s => s.ServiceType == typeof(CommandHandler2) &&
                                                s.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void Add()
    {
        var serviceCollection = new ServiceCollection();
        var typeMap = new MessageHandlerTypeMap(serviceCollection)
        {
            HandlerType = typeof(IMessageHandlerTest<>)
        };

        typeMap.Add(typeof(EventHandler));

        typeMap.GetMessageHandlerTypes(typeof(Event1)).Should().HaveCount(1);
        typeMap.GetMessageHandlerTypes(typeof(Event1)).First().UnderlyingSystemType.Should().Be(typeof(EventHandler));

        typeMap.GetMessageHandlerTypes(typeof(Event2)).Should().HaveCount(1);
        typeMap.GetMessageHandlerTypes(typeof(Event2)).First().UnderlyingSystemType.Should().Be(typeof(EventHandler));

        serviceCollection.Should().Contain(s => s.ServiceType == typeof(EventHandler) &&
                                                s.Lifetime == ServiceLifetime.Transient);
    }


    public interface IMessageHandlerTest<in TMessage> where TMessage : class
    {
        public Task Handle(IReceiveContext context, IEnvelope<TMessage> envelope);
    }

    public class EventHandler : IMessageHandlerTest<Event1>, IMessageHandlerTest<Event2>
    {
        public Task Handle(IReceiveContext context, IEnvelope<Event1> envelope)
        {
            return Task.CompletedTask;
        }

        public Task Handle(IReceiveContext context, IEnvelope<Event2> envelope)
        {
            return Task.CompletedTask;
        }
    }

    public class CommandHandler1 : IMessageHandlerTest<Command1>
    {
        public Task Handle(IReceiveContext context, IEnvelope<Command1> envelope)
        {
            return Task.CompletedTask;
        }
    }

    public class CommandHandler2 : IMessageHandlerTest<Command2>
    {
        public Task Handle(IReceiveContext context, IEnvelope<Command2> envelope)
        {
            return Task.CompletedTask;
        }
    }

    public class Event1
    {
        public string Version { get; } = "1";
        public Guid Id { get; }
    }

    public class Event2
    {
        public string Version { get; } = "1";
        public Guid Id { get; }
    }

    public class Command1
    {
        public string Version { get; } = "1";
        public Guid Id { get; }
    }

    public class Command2
    {
        public string Version { get; } = "1";
        public Guid Id { get; }
    }
}