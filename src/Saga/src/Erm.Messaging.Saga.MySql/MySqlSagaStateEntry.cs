using System;
using Erm.Messaging.Saga;

namespace Erm.Messaging.Saga.MySql;

internal class MySqlSagaStateEntry : ISagaStateEntry
{
    public MySqlSagaStateEntry(ulong? id, Guid sagaId, Type sagaType, SagaStatus status, object? data, int rowVersion)
    {
        Id = id;
        SagaId = sagaId;
        SagaType = sagaType;
        Status = status;
        Data = data;
        RowVersion = rowVersion;
    }

    public MySqlSagaStateEntry(Guid sagaId, Type sagaType, SagaStatus status, object? data, int rowVersion)
        : this(null, sagaId, sagaType, status, data, rowVersion)
    {
    }

    public ulong? Id { get; }
    public Guid SagaId { get; }
    public Type SagaType { get; }
    public SagaStatus Status { get; private set; }
    public object? Data { get; private set; }
    public int RowVersion { get; }

    public void Update(SagaStatus status, object? data)
    {
        Status = status;
        Data = data;
    }
}