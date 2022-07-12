using System;
using Erm.Messaging.Saga;

namespace Erm.Messaging.Saga.InMemory;

internal class InMemorySagaStateEntry : ISagaStateEntry
{
    public InMemorySagaStateEntry(Guid id, Type sagaType, SagaStatus status, object? data)
    {
        SagaId = id;
        SagaType = sagaType;
        Status = status;
        Data = data;
    }

    public Guid SagaId { get; }
    public Type SagaType { get; }
    public SagaStatus Status { get; private set; }
    public object? Data { get; private set; }

    public void Update(SagaStatus status, object? data)
    {
        Status = status;
        Data = data;
    }
}