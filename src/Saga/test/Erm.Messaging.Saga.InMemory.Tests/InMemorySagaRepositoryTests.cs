using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Erm.Core;
using Xunit;

namespace Erm.Messaging.Saga.InMemory.Tests;

public class InMemorySagaRepositoryTests
{
    [Fact]
    public async Task GetActionLogs_ShouldReturn_SavedEntry()
    {
        var repository = new InMemorySagaRepository();
        var correlationId = Uuid.Next();
        var sagaEvent = new SagaEvent
        {
            Data = "42"
        };

        const string destination = "black-hole";
        var createdAt = new DateTimeOffset(1984, 11, 11, 1, 1, 1, TimeSpan.Zero);
        var headers = new EnvelopeProperties { { "headerKey", "headerValue" } };
        const string source = "bigBang";
        const string groupId = "metallica";

        var saveEnvelope = new Envelope<SagaEvent>(sagaEvent, headers)
        {
            Destination = destination,
            Time = createdAt,
            CorrelationId = correlationId,
            GroupId = groupId,
            Source = source
        };

        var logEntry = repository.CreateActionLogEntry(correlationId, DateTimeOffset.UtcNow, saveEnvelope);
        await repository.SaveActionLog(logEntry);

        var logs = await repository.GetActionLogs(correlationId);
        var entry = logs.FirstOrDefault();
        entry.Should().NotBeNull();
        var getEnvelope = (Envelope<SagaEvent>)entry!.Envelope;
        getEnvelope.Should().Be(saveEnvelope);
        getEnvelope.Message.Data.Should().Be(sagaEvent.Data);
    }

    [Fact]
    public async Task GetState_ShouldReturn_SavedState()
    {
        var repository = new InMemorySagaRepository();
        var correlationId = Uuid.Next();
        var state = new SagaState { MessageHandled = true };
        const SagaStatus sagaStatus = SagaStatus.Rejected;
        var saveStateEntry = repository.CreateStateEntry(correlationId, typeof(SagaWithState), sagaStatus, state);
        await repository.SaveState(saveStateEntry);

        var getStateEntry = await repository.GetState(correlationId, typeof(SagaWithState));

        getStateEntry!.Should().NotBeNull();

        getStateEntry.Should().Be(saveStateEntry);
        ((SagaState)getStateEntry!.Data)!.MessageHandled.Should().Be(true);
        ((SagaState)getStateEntry.Data).Compensated.Should().Be(false);
    }

    private class SagaWithState : Saga<SagaState>, ISagaStartAction<SagaEvent>
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
        public string Data { get; init; }
    }

    private class SagaState
    {
        public bool MessageHandled { get; set; }
        public bool Compensated { get; set; }
    }
}