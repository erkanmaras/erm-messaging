using System;
using Erm.Messaging;
using Erm.Messaging.Saga;

namespace Erm.Messaging.Saga.MySql;

internal class MySqlSagaActionLogEntry : ISagaActionLogEntry
{
    public MySqlSagaActionLogEntry(Guid id, DateTimeOffset createdAt, IEnvelope envelope)
    {
        SagaId = id;
        MessageName = envelope.MessageName;
        Envelope = envelope;
        CreatedAt = createdAt;
    }

    public Guid SagaId { get; }
    public string MessageName { get; }
    public DateTimeOffset CreatedAt { get; }
    public IEnvelope Envelope { get; }
}