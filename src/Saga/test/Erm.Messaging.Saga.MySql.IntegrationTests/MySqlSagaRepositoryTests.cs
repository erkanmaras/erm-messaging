using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Erm.Core;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
namespace Erm.Messaging.Saga.MySql.IntegrationTests;

public class MySqlSagaRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;

    public MySqlSagaRepositoryTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task GetActionLogs_ShouldReturn_SavedEntries()
    {
        var metadataProvider = new MetadataProvider();
        metadataProvider.AddEventType<SagaEvent>();
        var repository = new MySqlSagaRepository(new MySqlSagaConfiguration(() => _databaseFixture.ConnectionString), metadataProvider);
        var correlationId = Uuid.Next();
        var sagaEvent = new SagaEvent
        {
            Data = "42"
        };

        const string destination = "black-hole";
        var createdAt = new DateTimeOffset(1984, 11, 11, 1, 1, 1, TimeSpan.Zero);
        var properties = new EnvelopeProperties { { "headerKey", "headerValue" } };
        const string source = "bigBang";
        const string groupId = "milkyWay";

        var saveEnvelope = new Envelope<SagaEvent>(sagaEvent, properties)
        {
            Destination = destination,
            Time = createdAt,
            CorrelationId = correlationId,
            GroupId = groupId,
            Source = source
        };

        var logEntry = new MySqlSagaActionLogEntry(correlationId, DateTimeOffset.UtcNow, saveEnvelope);
        await repository.SaveActionLog(logEntry);
        var logs = await repository.GetActionLogs(correlationId);
        var entry = logs.FirstOrDefault();
        entry.Should().NotBeNull();
        var getEnvelope = entry!.Envelope;
        getEnvelope.Should().BeEquivalentTo(saveEnvelope);
    }

    [Fact]
    public async Task GetState_ShouldReturn_SavedState()
    {
        var metadataProvider = new MetadataProvider();
        metadataProvider.AddEventType<SagaEvent>();
        var repository = new MySqlSagaRepository(new MySqlSagaConfiguration(() => _databaseFixture.ConnectionString), metadataProvider);
        var correlationId = Uuid.Next();
        var saveState = new SagaData { MessageHandled = true };
        const SagaStatus sagaStatus = SagaStatus.Rejected;
        var stateEntry = new MySqlSagaStateEntry(correlationId, typeof(SagaWithState), sagaStatus, saveState, rowVersion: 1);
        await repository.SaveState(stateEntry);

        var getState = await repository.GetState(correlationId, typeof(SagaWithState));
        getState!.Should().NotBeNull();
        getState!.SagaId.Should().Be(correlationId);
        getState.SagaType.Should().Be(typeof(SagaWithState));
        getState.Status.Should().Be(sagaStatus);
        getState.Data.Should().BeOfType<SagaData>();
        ((SagaData)getState.Data)!.MessageHandled.Should().Be(true);
        ((SagaData)getState.Data).Compensated.Should().Be(false);
    }

    [Fact]
    public async Task SaveState_ShouldThrowConcurrencyException_WhenUpdateNotEffectAnyRow()
    {
        var metadataProvider = new MetadataProvider();
        metadataProvider.AddEventType<SagaEvent>();
        var repository = new MySqlSagaRepository(new MySqlSagaConfiguration(() => _databaseFixture.ConnectionString), metadataProvider);
        var correlationId = Uuid.Next();
        var sagaData = new SagaData { MessageHandled = true };
        var stateEntry = repository.CreateStateEntry(correlationId, typeof(SagaWithState), SagaStatus.Pending, sagaData);
        await repository.SaveState(stateEntry);
        var getState = await repository.GetState(correlationId, typeof(SagaWithState));
        getState!.Should().NotBeNull();
        await repository.SaveState(getState!);
        await repository.Invoking(repo => repo.SaveState(getState)).Should().ThrowAsync<SagaStateConcurrencyException>();
    }

    [Fact]
    public async Task SaveState_ShouldIncreaseRowVersion_WhenUpdate()
    {
        var metadataProvider = new MetadataProvider();
        metadataProvider.AddEventType<SagaEvent>();
        var repository = new MySqlSagaRepository(new MySqlSagaConfiguration(() => _databaseFixture.ConnectionString), metadataProvider);
        var correlationId = Uuid.Next();
        var sagaData = new SagaData { MessageHandled = true };
        var stateEntry = (MySqlSagaStateEntry)repository.CreateStateEntry(correlationId, typeof(SagaWithState), SagaStatus.Pending, sagaData);
        stateEntry.RowVersion.Should().Be(1);
        await repository.SaveState(stateEntry); // Insert -> RowVersion=1;
        var updatedState = (MySqlSagaStateEntry)await repository.GetState(correlationId, typeof(SagaWithState));

        updatedState!.Should().NotBeNull();
        updatedState!.RowVersion.Should().Be(1);
        await repository.SaveState(updatedState); // Update -> RowVersion+1;

        updatedState = (MySqlSagaStateEntry)await repository.GetState(correlationId, typeof(SagaWithState));
        updatedState!.Should().NotBeNull();
        updatedState!.RowVersion.Should().Be(2);
    }

    private class SagaWithState : Saga<SagaData>, ISagaStartAction<SagaEvent>
    {
        public Task Handle(IReceiveContext context, IEnvelope<SagaEvent> envelope)
        {
            Data.MessageHandled = true;
            return Task.CompletedTask;
        }

        public Task Compensate(IReceiveContext context, IEnvelope<SagaEvent> envelope)
        {
            Data.Compensated = true;
            return Task.CompletedTask;
        }
    }

    private class SagaEvent
    {
        public string Data { get; set; }
    }

    private class SagaData
    {
        public bool MessageHandled { get; set; }
        public bool Compensated { get; set; }
    }
}