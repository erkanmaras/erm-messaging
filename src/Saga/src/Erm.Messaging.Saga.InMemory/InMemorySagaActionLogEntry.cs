using System;
using Erm.Messaging;
using Erm.Messaging.Saga;

namespace Erm.Messaging.Saga.InMemory;

internal class InMemorySagaActionLogEntry : ISagaActionLogEntry
{
    public InMemorySagaActionLogEntry(Guid id, DateTimeOffset createdAt, IEnvelope envelope)
    {
        SagaId = id;
        MessageName = envelope.MessageName;
        Envelope = envelope;
        CreatedAt = createdAt;
    }

    public Guid SagaId { get; }
    public string MessageName { get; }
    public IEnvelope Envelope { get; }
    public DateTimeOffset CreatedAt { get; }
}