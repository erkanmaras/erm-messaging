using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Erm.Messaging.Saga;

internal sealed class SagaInitializer : ISagaInitializer
{
    private readonly ISagaRepository _repository;
    private readonly ILogger<SagaInitializer> _logger;

    public SagaInitializer(ISagaRepository repository, ILogger<SagaInitializer> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<(bool, ISagaStateEntry?)> TryInitialize<TMessage>(ISaga saga, Guid sagaId) where TMessage : class
    {
        //var action = (ISagaAction<TMessage>)saga;
        var sagaType = saga.GetType();
        var dataType = saga.GetSagaDataType();
        var state = await _repository.GetState(sagaId, sagaType).ConfigureAwait(false);

        if (state is null)
        {
            //TODO:Saga ISagaStartAction mesajı ile mi başlamalı. ISagaAction olan bir mesaj önce geldi ise(unordered) başlamasının nasıl bir etkisi olur ?
            // if (action is not ISagaStartAction<TMessage>)
            // {
            //     throw new SagaException("Saga must start ISagaStartAction");
            // }
            state = CreateSagaState(sagaId, sagaType, dataType);
        }
        else if (state.Status is SagaStatus.Rejected or SagaStatus.Completed)
        {
            _logger.SagaAlreadyCompleted();
            return (false, null);
        }

        InitializeSaga(saga, sagaId, state);
        return (true, state);
    }

    private ISagaStateEntry CreateSagaState(Guid sagaId, Type sagaType, Type? dataType)
    {
        var data = dataType != null ? Activator.CreateInstance(dataType) : null;
        return _repository.CreateStateEntry(sagaId, sagaType, SagaStatus.Pending, data);
    }

    private static void InitializeSaga(ISaga saga, Guid id, ISagaStateEntry stateEntry)
    {
        saga.Initialize(id, stateEntry.Status, stateEntry.Data);
    }
}