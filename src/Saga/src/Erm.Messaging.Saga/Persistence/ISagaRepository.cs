using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

public interface ISagaRepository
{
    Task<IEnumerable<ISagaActionLogEntry>> GetActionLogs(Guid sagaId);
    Task SaveActionLog(ISagaActionLogEntry logEntry);
    Task<ISagaStateEntry?> GetState(Guid sagaId, Type sagaType);
    Task SaveState(ISagaStateEntry stateEntry);

    ISagaStateEntry CreateStateEntry(Guid sagaId, Type sagaType, SagaStatus status, object? data = null);
    ISagaActionLogEntry CreateActionLogEntry(Guid sagaId, DateTimeOffset createdAt, IEnvelope envelope);
}