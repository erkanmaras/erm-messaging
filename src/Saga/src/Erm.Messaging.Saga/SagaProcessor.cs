using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Erm.Messaging;

namespace Erm.Messaging.Saga;

internal sealed class SagaProcessor : ISagaProcessor
{
    private readonly ISagaRepository _repository;
    private readonly ILogger<SagaProcessor> _logger;

    public SagaProcessor(ISagaRepository repository, ILogger<SagaProcessor> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Process<TMessage>(
        ISaga saga,
        IReceiveContext context,
        IEnvelope<TMessage> envelope,
        ISagaStateEntry stateEntry) where TMessage : class
    {
        var action = (ISagaAction<TMessage>)saga;

        try
        {
            await action.Handle(context, envelope).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (saga.Status is not SagaStatus.Rejected)
            {
                saga.Reject(ex);
            }

            throw;
        }
        finally
        {
            await UpdateSaga(envelope, saga, stateEntry).ConfigureAwait(false);
        }
    }

    private async Task UpdateSaga(IEnvelope envelope, ISaga saga, ISagaStateEntry stateEntry)
    {
        //TODO: TransactionScope gereklimi yoksa global bir tane zaten olacak deyip  geçelim mi?
        stateEntry.Update(saga.Status, ((Saga)saga).GetData());
        var logEntry = _repository.CreateActionLogEntry(saga.SagaId, DateTimeOffset.UtcNow, envelope);

        var persistenceTasks = new[]
        {
            _repository.SaveState(stateEntry),
            _repository.SaveActionLog(logEntry)
        };

        await Task.WhenAll(persistenceTasks).ConfigureAwait(false);
    }


    public async Task PostProcess<TMessage>(
        ISaga saga,
        IReceiveContext context,
        IEnvelope<TMessage> envelope,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onCompleted,
        Func<IReceiveContext, IEnvelope<TMessage>, Task>? onRejected) where TMessage : class
    {
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch (saga.Status)
        {
            case SagaStatus.Rejected:
                if (onRejected != null)
                {
                    await onRejected(context, envelope).ConfigureAwait(false);
                }

                _logger.SagaRejected();
                await Compensate(saga, context).ConfigureAwait(false);
                break;
            case SagaStatus.Completed:
                if (onCompleted != null)
                {
                    await onCompleted(context, envelope).ConfigureAwait(false);
                }

                _logger.SagaCompleted();
                break;
        }
    }

    private async Task Compensate(ISaga saga, IReceiveContext context)
    {
        var actionLogs = await _repository.GetActionLogs(saga.SagaId).ConfigureAwait(false);
        var sortedActionLogs = actionLogs.OrderByDescending(entry => entry.CreatedAt).ToList();
        foreach (var actionLog in sortedActionLogs)
        {
            await Invoke(actionLog.Envelope).ConfigureAwait(false);
        }

        Task Invoke(object envelope)
        {
            return (Task)saga.InvokeGeneric(nameof(ISagaAction<object>.Compensate), context, envelope)!;
        }
    }
}