using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Erm.Core;
using Erm.Messaging.Saga.InMemory;
using Xunit;

namespace Erm.Messaging.Saga.Tests;

public class SagaCoordinatorTests
{
    [Fact]
    public async Task Process_ShouldExecute_HandleAndPersistState()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMessaging(messaging => messaging.AddSaga(saga => saga.UseInMemoryPersistence()));

        var sagaMock = new Mock<SagaWithState>
        {
            CallBase = true
        };

        sagaMock.Setup(saga => saga.Handle(It.IsAny<IReceiveContext>(), It.IsAny<IEnvelope<SagaEvent>>()));
        serviceCollection.AddSingleton<ISagaAction<SagaEvent>>(_ => sagaMock.Object);

        using var scope = serviceCollection.BuildServiceProvider().CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var coordinator = serviceProvider.GetRequiredService<ISagaCoordinator>();
        var message = new SagaEvent();
        var envelope = new Envelope<SagaEvent>(message);

        await coordinator.Process(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), serviceProvider), envelope);
        sagaMock.Verify(sampleSaga => sampleSaga.Handle(It.IsAny<IReceiveContext>(), It.IsAny<IEnvelope<SagaEvent>>()), Times.Once());

        var sagaRepository = serviceProvider.GetRequiredService<ISagaRepository>();
        var state = await sagaRepository.GetState(envelope.MessageId, typeof(SagaWithState));
        state.Should().NotBeNull();
        state!.Data.Should().NotBeNull();
        state.Data!.Should().BeOfType<SagaState>();
        ((SagaState)state.Data)?.Completed.Should().BeTrue();
    }

    [Fact]
    public void Process_ShouldExecute_Compensate_WhenHandleThrowException()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMessaging(messaging => messaging.AddSaga(saga => saga.UseInMemoryPersistence()));
        var sagaMock = new Mock<SagaWithState>
        {
            CallBase = true
        };

        sagaMock.Setup(saga => saga.Handle(It.IsAny<IReceiveContext>(), It.IsAny<IEnvelope<SagaEvent>>())).Throws(new Exception());
        serviceCollection.AddSingleton<ISagaAction<SagaEvent>>(_ => sagaMock.Object);

        using var scope = serviceCollection.BuildServiceProvider().CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var coordinator = serviceProvider.GetRequiredService<ISagaCoordinator>();
        var message = new SagaEvent();
        var envelope = new Envelope<SagaEvent>(message);

        coordinator.Process(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), serviceProvider), envelope);
        sagaMock.Verify(sampleSaga => sampleSaga.Handle(It.IsAny<IReceiveContext>(), It.IsAny<IEnvelope<SagaEvent>>()), Times.Once());
        sagaMock.Verify(sampleSaga => sampleSaga.Compensate(It.IsAny<IReceiveContext>(), It.IsAny<IEnvelope<SagaEvent>>()), Times.Once());
    }

    [Fact]
    public async Task Process_ShouldNotThrowException_WhenSagaAllReadyCompleted()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMessaging(messaging => messaging.AddSaga(saga => saga.UseInMemoryPersistence()));

        var sagaMock = new Mock<SagaWithState>
        {
            CallBase = true
        };

        serviceCollection.AddSingleton<ISagaAction<SagaEvent>>(_ => sagaMock.Object);
        using var scope = serviceCollection.BuildServiceProvider().CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var coordinator = serviceProvider.GetRequiredService<ISagaCoordinator>();
        var message = new SagaEvent();
        var envelope = new Envelope<SagaEvent>(message);
        var sagaRepository = serviceProvider.GetRequiredService<ISagaRepository>();
        var stateEntry = sagaRepository.CreateStateEntry(envelope.MessageId, typeof(SagaWithState), SagaStatus.Completed, new SagaState());
        await sagaRepository.SaveState(stateEntry);
        await coordinator.Invoking(sagaCoordinator => sagaCoordinator.Process(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), serviceProvider), envelope)).Should().NotThrowAsync();
    }

    [Fact]
    public async Task Process_ShouldNotThrowException_WhenSagaNotFound()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMessaging(messaging => messaging.AddSaga(saga => saga.UseInMemoryPersistence()));

        var sagaMock = new Mock<SagaWithState>
        {
            CallBase = true
        };

        serviceCollection.AddSingleton<ISagaAction<SagaEvent>>(_ => sagaMock.Object);
        using var scope = serviceCollection.BuildServiceProvider().CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var coordinator = serviceProvider.GetRequiredService<ISagaCoordinator>();

        var unknownEvent = new UnknownEvent();
        var unknownEnvelope = new Envelope<UnknownEvent>(unknownEvent);
        await coordinator.Invoking(sagaCoordinator => sagaCoordinator.Process(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), serviceProvider), unknownEnvelope)).Should().NotThrowAsync();
    }

    public class SagaEvent
    {
        public SagaEvent()
        {
            Version = "1";
            Id = Uuid.Next();
        }

        public string Version { get; }
        public Guid Id { get; }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public class UnknownEvent
    {
        public UnknownEvent()
        {
            Version = "1";
            Id = Uuid.Next();
        }

        public string Version { get; }
        public Guid Id { get; }
    }


    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    // ReSharper disable once MemberCanBePrivate.Global
    public class SagaWithState : Saga<SagaState>, ISagaStartAction<SagaEvent>
    {
        protected override Guid ResolveId<TMessage>(IReceiveContext context, IEnvelope<TMessage> envelope)
        {
            return envelope.MessageId;
        }

        public virtual Task Handle(IReceiveContext context, IEnvelope<SagaEvent> envelope)
        {
            Data.Completed = true;
            Complete();
            return Task.CompletedTask;
        }

        public virtual Task Compensate(IReceiveContext context, IEnvelope<SagaEvent> envelope)
        {
            return Task.CompletedTask;
        }
    }

    public class SagaState
    {
        public bool Completed { get; set; }
    }
}