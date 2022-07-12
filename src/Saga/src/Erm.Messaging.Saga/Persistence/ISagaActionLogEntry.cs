using System;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

public interface ISagaActionLogEntry
{
    Guid SagaId { get; }
    string MessageName { get; }
    DateTimeOffset CreatedAt { get; }
    IEnvelope Envelope { get; }
}