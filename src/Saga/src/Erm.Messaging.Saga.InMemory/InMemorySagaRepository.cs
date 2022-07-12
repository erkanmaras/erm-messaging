using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Erm.Messaging;
using Erm.Messaging.Saga;

namespace Erm.Messaging.Saga.InMemory;

public class InMemorySagaRepository : ISagaRepository
{
    private readonly List<ISagaStateEntry> _repository;
    private readonly List<ISagaActionLogEntry> _sagaLog;

    public InMemorySagaRepository()
    {
        _repository = new List<ISagaStateEntry>();
        _sagaLog = new List<ISagaActionLogEntry>();
    }

    public Task<IEnumerable<ISagaActionLogEntry>> GetActionLogs(Guid sagaId)
    {
        var result = new List<ISagaActionLogEntry>();
        for (var index = 0; index < _sagaLog.Count; index++)
        {
            var entry = _sagaLog[index];
            if (entry.SagaId == sagaId)
            {
                result.Add(entry);
            }
        }

        return Task.FromResult((IEnumerable<ISagaActionLogEntry>)result);
    }

    public Task SaveActionLog(ISagaActionLogEntry logEntry)
    {
        _sagaLog.Add(logEntry);
        return Task.CompletedTask;
    }

    public Task<ISagaStateEntry?> GetState(Guid sagaId, Type sagaType)
    {
        return Task.FromResult(_repository.FirstOrDefault(s => s.SagaId == sagaId));
    }

    public async Task SaveState(ISagaStateEntry stateEntry)
    {
        var sagaDataToUpdate = await GetState(stateEntry.SagaId, stateEntry.SagaType).ConfigureAwait(false);

        if (sagaDataToUpdate != null)
        {
            _repository.Remove(sagaDataToUpdate);
        }

        _repository.Add(stateEntry);
        await Task.CompletedTask;
    }

    public ISagaStateEntry CreateStateEntry(Guid sagaId, Type sagaType, SagaStatus status, object? data = null)
    {
        return new InMemorySagaStateEntry(sagaId, sagaType, status, data);
    }

    public ISagaActionLogEntry CreateActionLogEntry(Guid sagaId, DateTimeOffset createdAt, IEnvelope envelope)
    {
        return new InMemorySagaActionLogEntry(sagaId, createdAt, envelope);
    }
}