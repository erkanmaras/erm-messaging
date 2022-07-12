using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Erm.Core;
using Xunit;

namespace Erm.Messaging.Saga.MySql.IntegrationTests;

public class SagaCoordinatorTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public SagaCoordinatorTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task Process_MultipleEvent_ValidateState()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMessaging(configuration => { configuration.AddSaga(builder => builder.UseMySqlPersistence(() => _databaseFixture.ConnectionString)); });
        serviceCollection.AddTransient<ISagaAction<PlusEvent>, SagaWithState>();
        serviceCollection.AddTransient<ISagaAction<MinusEvent>, SagaWithState>();
        var metadataProvider = new MetadataProvider();
        metadataProvider.AddEventType<PlusEvent>();
        metadataProvider.AddEventType<MinusEvent>();
        serviceCollection.AddSingleton<IMetadataProvider>(metadataProvider);
        using var scope = serviceCollection.BuildServiceProvider().CreateScope();

        var serviceProvider = scope.ServiceProvider;
        var coordinator = serviceProvider.GetRequiredService<ISagaCoordinator>();
        var correlationId = Uuid.Next();
        var plusEvent = new PlusEvent { Increment = 42 };
        var plusEventEnvelope = new Envelope<PlusEvent>(plusEvent)
        {
            CorrelationId = correlationId
        };
        await coordinator.Process(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), serviceProvider), plusEventEnvelope);
        await coordinator.Process(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), serviceProvider), plusEventEnvelope);

        var minusEvent = new MinusEvent { Decrement = 4 };
        var minusEventEnvelope = new Envelope<MinusEvent>(minusEvent)
        {
            CorrelationId = correlationId
        };
        await coordinator.Process(new ReceiveContext(Mock.Of<IMessageEnvelope>(), Mock.Of<IMessageSender>(), serviceProvider), minusEventEnvelope);

        var repository = serviceProvider.GetRequiredService<ISagaRepository>();
        var state = await repository.GetState(correlationId, typeof(SagaWithState));
        state!.Data.Should().NotBeNull();
        state.Data.Should().BeOfType<SagaState>();
        ((SagaState)state.Data)!.Value.Should().Be(80);
    }


    private class SagaWithState : Saga<SagaState>, ISagaStartAction<PlusEvent>, ISagaStartAction<MinusEvent>
    {
        public Task Handle(IReceiveContext context, IEnvelope<PlusEvent> envelope)
        {
            Data.Value += envelope.Message.Increment;
            return Task.CompletedTask;
        }

        public Task Compensate(IReceiveContext context, IEnvelope<PlusEvent> envelope)
        {
            Data.Value -= envelope.Message.Increment;
            return Task.CompletedTask;
        }

        public Task Handle(IReceiveContext context, IEnvelope<MinusEvent> envelope)
        {
            Data.Value -= envelope.Message.Decrement;
            return Task.CompletedTask;
        }

        public Task Compensate(IReceiveContext context, IEnvelope<MinusEvent> envelope)
        {
            Data.Value += envelope.Message.Decrement;
            return Task.CompletedTask;
        }
    }

    private class PlusEvent
    {
        public int Increment { get; init; }
    }

    private class MinusEvent
    {
        public int Decrement { get; init; }
    }

    private class SagaState
    {
        public int Value { get; set; }
    }
}