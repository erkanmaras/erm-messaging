using System;

namespace Erm.Messaging.Saga;

public interface ISagaStateEntry
{
    Guid SagaId { get; }
    Type SagaType { get; }
    SagaStatus Status { get; }
    object? Data { get; }
    void Update(SagaStatus status, object? data = null);
}